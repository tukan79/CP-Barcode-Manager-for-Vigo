# Changelog

## [1.3.0] - 2026-07-19

### Added
- Invalid SAP PDF filename validation
- `InvalidFileNameCount`
- Routing to `Errors/Invalid_File_Name`
- Custom final report form
- Timestamped Excel report archiving
- Full four-scenario functional test

### Changed
- Existing files in error folders are replaced so incoming PDFs do not remain in `SAP_Import`
- Excel archiving moves the file before renaming it
- Final report includes all processing outcomes
