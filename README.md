# CP Barcode Manager for Vigo

Windows desktop utility for preparing Customer Paperwork PDF files for upload to Vigo by adding a Code 128 barcode containing the customer reference extracted from the PDF filename.

> Current version: 0.2.0  
> Platform: Windows / .NET 8 / WinForms  
> Status: Desktop version tested successfully

## Purpose
For each PDF the application:
1. Extracts the customer reference from the filename.
2. Generates a Code 128 barcode.
3. Adds it to every page.
4. Verifies the generated output.
5. Removes the source only after successful verification.
6. Routes unsuccessful files for manual review.

## Filename Rule
Example:
`0080001234202606011125.pdf`

Default:
- Skip first characters: 2
- Read next digits: 8

Extracted barcode value: `80001234`

## Barcode
- Type: Code 128
- Apply to: Every page
- Default position: Bottom Centre
- Width: 50 mm
- Height: 12 mm
- Bottom margin: 20 mm
- Positions: Bottom Left / Bottom Centre / Bottom Right

## Statuses
### Ready for Vigo
The PDF was processed, output verified, and the source removed successfully.

### Needs Attention
If normal barcode processing fails, the original PDF is preserved in the Output Folder with an `ERROR_` prefix.

### Critical Error
Used when a safety-critical file operation cannot be completed. The source is preserved where necessary to prevent silent data loss.

## Automatic Folder Monitoring
Version 0.2.0 can automatically monitor the Import Folder. A PDF is processed only after:
- size is unchanged across scans,
- last-write timestamp is unchanged,
- exclusive read access is available.

This protects against partially-written, copied, or synchronising files.

## Settings
Stored in:
`%LOCALAPPDATA%\CP Barcode Manager for Vigo\settings.json`

Settings include folders, filename rule, barcode placement and size, bottom margin, automatic monitoring, and scan interval.

Import and Output folders must be different.

## Safety Design
The application follows a verified-output-first workflow:
Source PDF -> Temporary output -> Verify -> Final output -> Remove source.

Verification includes existence, non-zero file size, and matching page count.

## Test Status
v0.2.0 has been tested with synthetic PDFs for:
- valid and invalid filenames,
- Code 128 generation,
- every-page placement,
- multiple page counts,
- configurable position and dimensions,
- settings persistence,
- batch processing,
- error routing,
- verified output,
- automatic monitoring,
- slow-write and exclusive-lock protection.

## Technology
- C#
- .NET 8
- Windows Forms
- ZXing.Net
- PDFsharp

Source: `src/CPBarcodeManagerForVigo`

## Deployment Models
### Desktop Version
Current implementation. Supports manual processing and automatic monitoring while the app is running.

### Windows Service Version
Planned unattended deployment. It should reuse the same processing engine and will require IT approval for service account, permissions, logging, startup configuration, and production paths.

## Documentation
- `docs/INSTALLATION.md`
- `docs/USER_GUIDE.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/TEST_PLAN.md`
- `docs/PUBLISHING_CHECKLIST.md`

## Security and Privacy
Do not commit production PDFs, customer data, credentials, internal URLs, private SharePoint paths, confidential screenshots, or production configuration. Use synthetic test data.

## Development
Designed, developed and tested by Krzysztof Stolarski with assistance from OpenAI ChatGPT for planning, coding support, troubleshooting and documentation. All changes remain subject to human review and testing.

## Disclaimer
Independent utility. Not an official Vigo, SAP, Optima, Microsoft or OpenAI product.

## License
MIT License.
