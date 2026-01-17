# PDF Generation Spike

This repo contains two .NET 8 Web API implementations for generating PDFs so you can compare approaches side by side.

## Projects
- `src/PdfApi` — Playwright/Chromium HTML-to-PDF pipeline. Uses HTML templates and browser rendering.
- `src/PdfApiAcroForm` — AcroForm template filling. Uses prebuilt PDF templates with form fields, no browser.

## Why two projects
- Compare browser-based rendering vs. form-field filling.
- Evaluate Docker image size, dependencies, and reliability tradeoffs.
- Keep the API shape consistent while swapping the PDF engine.

## Start here
- Playwright version: `src/PdfApi/README.md`
- AcroForm version: `src/PdfApiAcroForm/README.md`
