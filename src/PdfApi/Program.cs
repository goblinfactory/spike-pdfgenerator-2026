using System.Text.Json;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string TemplatesRoot = "/templates";
const string OutputRoot = "/data";

app.UseStaticFiles();

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

    var html = await File.ReadAllTextAsync(templatePath);
    var mergedHtml = ApplyTemplate(html, payload.RootElement);
    var pdfBytes = await RenderPdfAsync(mergedHtml);
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

app.MapGet("/", (IWebHostEnvironment env) =>
{
    var indexPath = Path.Combine(env.WebRootPath, "index.html");
    return Results.File(indexPath, "text/html");
});

app.Run();

static string ResolveTemplatePath(IWebHostEnvironment env, string templateName)
{
    var absolutePath = Path.Combine(TemplatesRoot, $"{templateName}.html");
    if (File.Exists(absolutePath))
    {
        return absolutePath;
    }

    return Path.Combine(env.ContentRootPath, "templates", $"{templateName}.html");
}

static string ApplyTemplate(string html, JsonElement data)
{
    if (data.ValueKind != JsonValueKind.Object)
    {
        return html;
    }

    var output = html;
    foreach (var property in data.EnumerateObject())
    {
        var token = $"{{{{{property.Name}}}}}";
        output = output.Replace(token, property.Value.ToString());
    }

    return output;
}

static async Task<byte[]> RenderPdfAsync(string html)
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = true
    });

    var page = await browser.NewPageAsync();
    await page.SetContentAsync(html, new PageSetContentOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle
    });

    return await page.PdfAsync(new PagePdfOptions
    {
        Format = "A4",
        PrintBackground = true
    });
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
