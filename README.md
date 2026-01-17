# PDF Generation Spike

> **⚠️ DISCLAIMER:** This is a proof-of-concept spike project for evaluating PDF generation approaches. Not intended for production use. No warranty or security guarantees are provided.

This repo contains two .NET 8 Web API implementations for generating PDFs so you can compare approaches side by side.

## Projects
- [/pdfApi](./pdfApi/) — Playwright/Chromium HTML-to-PDF pipeline. Uses HTML templates and browser rendering.
- [/pdfApiAcroForm](./pdfApiAcroForm/) — AcroForm template filling. Uses prebuilt PDF templates with form fields, no browser.

## Why two projects
- Compare browser-based rendering vs. form-field filling.
- Evaluate Docker image size, dependencies, and reliability tradeoffs.
- Keep the API shape consistent while swapping the PDF engine.

## Start here
- Playwright version: `approaches/pdfApi/src/README.md`
- AcroForm version: `approaches/pdfApiAcroForm/src/README.md`
