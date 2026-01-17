# PdfApiAcroForm

AcroForm-based PDF generation using .NET 8 and PdfSharpCore. This project fills form fields in prebuilt PDF templates without using a browser.

## Prereqs
- .NET 8 SDK

## Build and Run (Local)
```bash
dotnet build src/PdfApiAcroForm/PdfApiAcroForm.csproj
dotnet run --project src/PdfApiAcroForm/PdfApiAcroForm.csproj
```

## API
- `POST /create/pdf/templates/{templateName}`
- JSON body keys must match AcroForm field names in the template.

## Templates
- Store templates in `templates-pdf/` and name them `{templateName}.pdf`.
- Example: `invoice.pdf` for `/create/pdf/templates/invoice`.

### Logo per template (optional)
- Put a logo image in `templates-pdf/logos/` named `{templateName}-logo.png` (or `.jpg` / `.jpeg`).
- The logo is drawn at 200x200 with a 24pt margin, top-right of page 1.

## Limitations
- PdfSharpCore does not expose a public flatten API; fields remain editable.
- Best suited for fixed-field layouts; complex line items require a fixed number of fields.
- Appearance rendering depends on PDF viewer support (`/NeedAppearances` is set).
