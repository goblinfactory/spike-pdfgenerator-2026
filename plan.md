---
name: AcroForm Template Plan
overview: Evaluate and plan an OSS-based AcroForm template workflow to replace Playwright and fill PDF form fields on the server.
todos:
  - id: inventory-fields
    content: List all fields from templates + payloads.
    status: pending
  - id: oss-lib-eval
    content: Evaluate OSS AcroForm libraries + constraints.
    status: pending
  - id: workflow-design
    content: Define template authoring + mapping conventions.
    status: pending
  - id: api-flow
    content: Sketch endpoint and data flow changes.
    status: pending
  - id: risk-check
    content: Assess limits for tables/line items.
    status: pending
isProject: false
---

## Scope

- Focus on AcroForm PDF templates filled server-side without browser dependencies, using OSS-only libraries.
- Implement the no-Playwright version in a new subfolder under `src` so the two approaches can be compared side by side.
- Keep current API shape in mind for minimal disruption in `src/PdfApi/Program.cs`.

## Discovery

- Review current HTML template usage and token replacement to list required fields and data types in `src/PdfApi/templates/invoice.html` and `src/PdfApi/templates/delivery-note.html`.
- Identify how request payloads map to template tokens in `src/PdfApi/Program.cs` to enumerate the full field list.

## OSS Library Evaluation (AcroForm)

- Compare OSS PDF libraries that can fill AcroForm fields in .NET (e.g., PdfSharpCore with forms support, PdfPig + writer add-ons, or other OSS options).
- Confirm Linux Docker compatibility and any native dependencies (target: none).
- Check support for:
- setting text fields
- flattening filled forms
- embedding fonts (if required for consistent output)

## Template Workflow Design

- Define an authoring workflow for PDF templates with form fields (e.g., create templates in a PDF editor; ensure field names match JSON keys).
- Specify field naming conventions and a mapping strategy (e.g., `{{InvoiceNumber}}` -> `InvoiceNumber` form field).
- Determine how to handle variable-length values (wrapping, truncation, font sizing).

## API and Data Flow Plan

- Outline a new endpoint flow: load PDF template -> fill fields -> optionally flatten -> return bytes.
- Plan backwards compatibility with existing `/create/pdf/templates/{templateName}` route (mapping template name to PDF template file).
- Define how the new no-Playwright project will be hosted under `src` (folder name, project name) for easy comparison and isolation.

## Risk Check

- Identify limitations: complex tables, repeated line items, or layouts that depend on HTML/CSS.
- Define how to handle line-item tables (fixed number of rows vs. separate overlay/additional pages).

## Output Artifacts

- A small proof-of-concept plan: one template (e.g., invoice) as AcroForm with minimal fields.
- A mapping spec for JSON payload to PDF field names.

## Next Steps (after plan approval)

- Prototype fill on a single template and validate output quality.
- Decide if tables/line items require a hybrid approach (AcroForm + overlay or code-first).