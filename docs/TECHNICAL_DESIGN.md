# Technical Design

## CP Barcode Manager for Vigo v0.2.0

### Objective
Prepare Customer Paperwork PDFs for Vigo by extracting a customer reference from the filename and adding a Code 128 barcode to every page.

### Platform
- C#
- .NET 8
- Windows Forms
- ZXing.Net
- PDFsharp

### Key Files
- `Models/AppSettings.cs`
- `Models/ProcessResult.cs`
- `Services/BarcodeImageService.cs`
- `Services/BarcodeValueExtractor.cs`
- `Services/CsvLogService.cs`
- `Services/PdfBarcodeService.cs`
- `Services/SettingsService.cs`
- `MainForm.cs`
- `Program.cs`

### Filename Extraction
Defaults:
- `SkipCharacters = 2`
- `TakeCharacters = 8`

Example:
`0080001234202606011125.pdf` -> `80001234`

The extracted value must contain digits only.

### Barcode
- Code 128
- applied to every page
- Bottom Left / Bottom Centre / Bottom Right
- default 50 mm x 12 mm
- default bottom margin 20 mm

### Settings
`AppSettings` stores source/output paths, extraction settings, barcode placement and dimensions, bottom margin, monitoring enabled state and interval.

Persisted at:
`%LOCALAPPDATA%\CP Barcode Manager for Vigo\settings.json`

### PDF Safety
`PdfBarcodeService` writes to a temporary PDF, verifies existence, non-zero size and page count, then promotes it to the final output.

The source is removed only after successful output verification.

### Automatic Monitoring
The WinForms timer tracks file length, last-write UTC and stable checks. Files must remain unchanged across scans and be available with `FileShare.None`.

A second readiness check runs before processing.

### Result Model
User-facing statuses:
- Ready for Vigo
- Needs Attention
- Critical Error

### Error Workflow
On normal processing failure, the original is copied to Output using an `ERROR_` prefix, the copy is verified, then the source is removed. If safe cleanup cannot complete, the result becomes Critical Error.

### Logging
The Desktop app shows a processing log and manual batch mode writes a CSV log to Output.

### Planned Architecture
Future structure:
Shared Processing Core -> WinForms Desktop + Windows Service

The shared core should own filename extraction, barcode generation, PDF processing, verification, readiness rules and result models.

### Windows Service Considerations
Service account, least-privilege permissions, persistent logging, recovery, configuration, approved UNC/network/SharePoint paths, and IT deployment approval.

v0.2.0 implements the Desktop application only. The Windows Service remains planned.
