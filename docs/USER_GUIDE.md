# User Guide

## CP Barcode Manager for Vigo v0.2.0

The application reads a customer reference from the PDF filename and adds a Code 128 barcode to every page.

### Processing Tab
Shows:
- Import Folder
- Output Folder
- Barcode Settings
- Preview
- Start Processing
- Exit
- Progress
- Status
- Summary
- Automation Status
- Processing Log

### Settings Tab
Configure:
- Import Folder
- Output Folder
- Barcode position
- Barcode width
- Barcode height
- Bottom margin
- Automatic Folder Monitoring
- Scan interval

Use `Save Settings` after changes.

### Filename Rule
Example:
`0080001234202606011125.pdf`

Default extraction:
- ignore first 2 characters
- read next 8 digits
- result: `80001234`

### Barcode Placement
- Every page
- Bottom Left / Bottom Centre / Bottom Right
- Default: Bottom Centre, 50 mm x 12 mm, 20 mm bottom margin

### Preview
Shows the first PDF's filename, extracted reference, placement and dimensions. It does not process the PDF.

### Manual Processing
1. Check Import and Output folders.
2. Place PDFs in Import.
3. Select `Start Processing`.
4. Wait for completion.
5. Review status and log.

### Automatic Processing
Enable `Automatically monitor Import Folder`.

A new PDF is processed only after its size and last-write time are stable and exclusive read access is available.

### Statuses
- `Ready for Vigo` — output verified and source removed.
- `Needs Attention` — original routed to Output with `ERROR_` prefix.
- `Critical Error` — safety-critical operation could not be completed; source may remain for protection.

### User Rules
Do not use the same folder for Import and Output. Do not rename or manually delete files during processing. Review Critical Errors before taking manual action.

Automatic monitoring stops when the Desktop application is closed.
