Template PDFs for AcroForm filling
==================================

Place AcroForm-enabled PDF templates in this folder.

Requirements:
- Each field name must match the JSON property name sent to the API.
- Use text fields for values like InvoiceNumber, Date, Total, etc.
- Keep template names aligned with the route: /create/pdf/templates/{templateName}
  Example: templateName=invoice -> templates-pdf/invoice.pdf
- Optional logo per template:
  - Put a logo image in templates-pdf/logos named {templateName}-logo.png (or .jpg/.jpeg)
  - The logo is drawn at 200x200 with a 24pt margin, top-right of page 1

Note:
- This project sets /NeedAppearances so most PDF viewers render filled values.
- PdfSharpCore does not expose a public "flatten" API; fields may remain editable.
