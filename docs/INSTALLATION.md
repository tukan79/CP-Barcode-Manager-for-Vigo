# Installation Guide

## CP Barcode Manager for Vigo v0.2.0

### Requirements
- Windows 10 or Windows 11
- .NET 8 compatible environment
- Access to Import and Output folders
- Read/write/delete permission for Import
- Read/write permission for Output

For development:
- .NET 8 SDK
- Git
- Visual Studio or VS Code with C# support

### Project
`src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj`

### Build
```powershell
dotnet restore .\src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj
dotnet build .\src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj
```

### Run
```powershell
dotnet run --project .\src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj
```

### First Configuration
In Settings configure:
1. Import Folder
2. Output Folder
3. Barcode position
4. Barcode width
5. Barcode height
6. Bottom margin
7. Automatic Folder Monitoring
8. Scan interval

Select `Save Settings`.

Import and Output must be different.

### Defaults
- Code 128
- Bottom Centre
- 50 mm x 12 mm
- 20 mm bottom margin
- Every page
- Skip 2 filename characters, read next 8 digits

### Settings Location
`%LOCALAPPDATA%\CP Barcode Manager for Vigo\settings.json`

### OneDrive / SharePoint
The Desktop version can use company-approved OneDrive or SharePoint-synchronised Windows folders. Production paths and permissions must be approved by IT.

### Deployment Note
Automatic monitoring works only while the Desktop application is running. A Windows Service version is planned for unattended operation.

### Production Approval
Before production use validate build, permissions, filename convention, barcode readability, page placement, multi-page PDFs, slow-write protection, output verification, error handling, and approved folder configuration.
