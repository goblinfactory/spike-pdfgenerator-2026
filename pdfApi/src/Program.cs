using System.Text.Json;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
var app = builder.Build();

// Enable detailed error pages in Development mode
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

const string TemplatesRoot = "/templates";
var outputRoot = ResolveOutputRoot(app.Environment);

app.UseStaticFiles();

app.MapPost("/create/pdf/templates/{templateName}", async (HttpRequest request, IWebHostEnvironment env, string templateName, ILogger<Program> logger) =>
{
    JsonDocument payload;
    try
    {
        payload = await JsonDocument.ParseAsync(request.Body);
    }
    catch (JsonException ex)
    {
        logger.LogError(ex, "Invalid JSON payload");
        return Results.BadRequest("Invalid JSON payload.");
    }

    var templatePath = ResolveTemplatePath(env, templateName);
    if (!File.Exists(templatePath))
    {
        logger.LogWarning("Template not found: {TemplateName} at {TemplatePath}", templateName, templatePath);
        return Results.NotFound($"Template not found: {templateName}");
    }

    try
    {
        var html = await File.ReadAllTextAsync(templatePath);
        var mergedHtml = ApplyTemplate(html, payload.RootElement);
        var pdfBytes = await RenderPdfAsync(mergedHtml);
        var outputPath = await SavePdfAsync(templateName, pdfBytes, outputRoot);

        logger.LogInformation("PDF created: {OutputPath}", outputPath);
        return Results.File(pdfBytes, "application/pdf", Path.GetFileName(outputPath));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating PDF for template {TemplateName}", templateName);
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.MapGet("/api/files", () =>
{
    if (!Directory.Exists(outputRoot))
    {
        return Results.Ok(Array.Empty<string>());
    }

    var files = Directory.GetFiles(outputRoot, "*.pdf")
        .Select(Path.GetFileName)
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .OrderByDescending(name => name)
        .ToArray();

    return Results.Ok(files);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/data/{fileName}", (string fileName) =>
{
    // Strip any directory components from the filename
    var safeName = Path.GetFileName(fileName);
    
    // Reject if the cleaned name differs from the original (path traversal attempt)
    if (!string.Equals(safeName, fileName, StringComparison.Ordinal))
    {
        return Results.BadRequest("Invalid file name.");
    }
    
    // Additional validation: only allow PDF files
    if (!safeName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Only PDF files are allowed.");
    }
    
    // Construct the full path and ensure it's within outputRoot
    var filePath = Path.Combine(outputRoot, safeName);
    var fullPath = Path.GetFullPath(filePath);
    var fullOutputRoot = Path.GetFullPath(outputRoot);
    
    // Ensure the resolved path is actually within our output directory
    if (!fullPath.StartsWith(fullOutputRoot, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Invalid file path.");
    }
    
    if (!System.IO.File.Exists(filePath))
    {
        return Results.NotFound();
    }

    return Results.File(filePath, "application/pdf", safeName);
});

app.MapGet("/", () =>
{
    return Results.Ok(new 
    { 
        status = "OK", 
        endpoints = new[] 
        { 
            "POST /create/pdf/templates/{templateName}", 
            "GET /api/files", 
            "GET /data/{fileName}" 
        } 
    });
});

app.Run();

static string ResolveTemplatePath(IWebHostEnvironment env, string templateName)
{
    // Try Docker absolute path first
    var absolutePath = Path.Combine(TemplatesRoot, $"{templateName}.html");
    if (File.Exists(absolutePath))
    {
        return absolutePath;
    }

    // Fall back to local relative path
    var relativePath = Path.Combine(env.ContentRootPath, "templates", $"{templateName}.html");
    return relativePath;
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

static string ResolveOutputRoot(IWebHostEnvironment env)
{
    const string dockerPath = "/data";
    try
    {
        Directory.CreateDirectory(dockerPath);
        return dockerPath;
    }
    catch (Exception)
    {
        var localPath = Path.Combine(env.ContentRootPath, "data");
        Directory.CreateDirectory(localPath);
        return localPath;
    }
}

static async Task<string> SavePdfAsync(string templateName, byte[] pdfBytes, string outputRoot)
{
    Directory.CreateDirectory(outputRoot);
    var safeName = templateName.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');
    var fileName = $"{safeName}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.pdf";
    var outputPath = Path.Combine(outputRoot, fileName);
    await File.WriteAllBytesAsync(outputPath, pdfBytes);
    return outputPath;
}
