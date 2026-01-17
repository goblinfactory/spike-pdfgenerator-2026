# Playwright .NET PDF Spike

Windows container notes: see `README-windows.md`.

Minimal .NET 8 Web API that uses Playwright .NET (Chromium headless) to render HTML templates to PDF, all inside a single Docker container.
The interesting part of this project is the Dockerfile to install PowerShell and Playwright.

## Prereqs
- Docker
- .NET 8 SDK (for local build/run)

## Project Layout
- `templates` — HTML templates
- `Program.cs` — API entrypoint

## Build and Run (Docker)
```bash
docker build -t pdfapi .
docker run --rm -p 8080:8080 pdfapi
```

## Build and Run (Docker Compose)
```bash
docker compose up --build
```

## Build and Run (Local)
```bash
dotnet build src/PdfApi/PdfApi.csproj
dotnet run --project src/PdfApi/PdfApi.csproj
```

## Test the API
```bash
curl -X POST "http://localhost:8080/create/pdf/templates/invoice" \
  -H "Content-Type: application/json" \
  -d '{
    "InvoiceNumber":"INV-1001",
    "Date":"2026-01-15",
    "CustomerName":"Acme Corp",
    "Reference":"PO-7788",
    "Subtotal":"100.00",
    "Tax":"15.00",
    "Total":"115.00"
  }' \
  --output invoice.pdf
```

## Notes on Playwright/Chromium
- Playwright .NET is installed via NuGet.
- Chromium is installed during the Docker build using the Playwright CLI:
  `pwsh -File /app/playwright.ps1 install --with-deps chromium`
- Templates are loaded from `/templates` inside the container.

## Testing that Playwright is present

To test that `/app/playwright.ps1` exists in the publish output, run the following from the repo root:

```bash
dotnet publish -c Release -o ./app/publish --project src/PdfApi/PdfApi.csproj
```
