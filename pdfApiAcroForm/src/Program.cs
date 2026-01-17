using System.Text.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string TemplatesRoot = "/templates-pdf";
const string LogosRoot = "/templates-pdf/logos";
const string OutputRoot = "/data";
const double LogoSize = 200;
const double LogoMargin = 24;

app.MapPost("/create/pdf/templates/{templateName}", async (HttpRequest request, IWebHostEnvironment env, string templateName) =>
{
    JsonDocument payload;
    try
    {
        payload = await JsonDocument.ParseAsync(request.Body);
    }
    catch (JsonException)
    {
        return Results.BadRequest("Invalid JSON payload.");
    }

    var templatePath = ResolveTemplatePath(env, templateName);
    if (!File.Exists(templatePath))
    {
        return Results.NotFound($"Template not found: {templateName}");
    }

    byte[] pdfBytes;
    try
    {
        pdfBytes = FillPdfTemplate(templatePath, templateName, env, payload.RootElement, out var missingFields);
        if (missingFields.Count > 0)
        {
            return Results.BadRequest($"Template is missing fields: {string.Join(", ", missingFields)}");
        }
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(exception.Message);
    }

    var outputPath = await SavePdfAsync(templateName, pdfBytes);
    return Results.File(pdfBytes, "application/pdf", Path.GetFileName(outputPath));
});

app.MapGet("/api/files", () =>
{
    if (!Directory.Exists(OutputRoot))
    {
        return Results.Ok(Array.Empty<string>());
    }

    var files = Directory.GetFiles(OutputRoot, "*.pdf")
        .Select(Path.GetFileName)
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .OrderByDescending(name => name)
        .ToArray();

    return Results.Ok(files);
});

app.MapGet("/data/{fileName}", (string fileName) =>
{
    var safeName = Path.GetFileName(fileName);
    if (!string.Equals(safeName, fileName, StringComparison.Ordinal))
    {
        return Results.BadRequest("Invalid file name.");
    }

    var filePath = Path.Combine(OutputRoot, safeName);
    if (!System.IO.File.Exists(filePath))
    {
        return Results.NotFound();
    }

    return Results.File(filePath, "application/pdf", safeName);
});

app.Run();

static string ResolveTemplatePath(IWebHostEnvironment env, string templateName)
{
    var absolutePath = Path.Combine(TemplatesRoot, $"{templateName}.pdf");
    if (File.Exists(absolutePath))
    {
        return absolutePath;
    }

    return Path.Combine(env.ContentRootPath, "templates-pdf", $"{templateName}.pdf");
}

static byte[] FillPdfTemplate(string templatePath, string templateName, IWebHostEnvironment env, JsonElement data, out List<string> missingFields)
{
    if (data.ValueKind != JsonValueKind.Object)
    {
        throw new InvalidOperationException("JSON payload must be an object.");
    }

    using var document = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify);
    var form = document.AcroForm ?? throw new InvalidOperationException("Template has no AcroForm fields.");
    form.Elements["/NeedAppearances"] = new PdfBoolean(true);

    missingFields = new List<string>();
    foreach (var property in data.EnumerateObject())
    {
        var field = form.Fields[property.Name];
        if (field is null)
        {
            missingFields.Add(property.Name);
            continue;
        }

        if (field is PdfTextField textField)
        {
            textField.Value = new PdfString(property.Value.ToString());
        }
        else
        {
            field.Value = new PdfString(property.Value.ToString());
        }
    }

    ApplyLogoIfPresent(document, ResolveLogoPath(env, templateName));

    using var stream = new MemoryStream();
    document.Save(stream);
    return stream.ToArray();
}

static void ApplyLogoIfPresent(PdfDocument document, string? logoPath)
{
    if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
    {
        return;
    }

    if (document.Pages.Count == 0)
    {
        throw new InvalidOperationException("Template has no pages.");
    }

    var page = document.Pages[0];
    using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
    using var image = XImage.FromFile(logoPath);

    var x = page.Width - LogoMargin - LogoSize;
    var y = LogoMargin;
    graphics.DrawImage(image, x, y, LogoSize, LogoSize);
}

static string? ResolveLogoPath(IWebHostEnvironment env, string templateName)
{
    var baseName = $"{templateName}-logo";
    var extensions = new[] { ".png", ".jpg", ".jpeg" };

    foreach (var extension in extensions)
    {
        var absolutePath = Path.Combine(LogosRoot, baseName + extension);
        if (File.Exists(absolutePath))
        {
            return absolutePath;
        }

        var relativePath = Path.Combine(env.ContentRootPath, "templates-pdf", "logos", baseName + extension);
        if (File.Exists(relativePath))
        {
            return relativePath;
        }
    }

    return null;
}

static async Task<string> SavePdfAsync(string templateName, byte[] pdfBytes)
{
    Directory.CreateDirectory(OutputRoot);
    var safeName = templateName.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');
    var fileName = $"{safeName}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.pdf";
    var outputPath = Path.Combine(OutputRoot, fileName);
    await File.WriteAllBytesAsync(outputPath, pdfBytes);
    return outputPath;
}
