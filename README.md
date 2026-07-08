# CP Barcode Manager for Vigo

A Windows desktop utility for adding a Code 128 barcode to SAP Customer Paperwork PDFs before uploading them into Vigo.

## Version 1.0 Scope

- Select a fixed source folder containing PDF files.
- Remember the selected source and output folders.
- Process all `.pdf` files in the source folder.
- Extract barcode value from the PDF filename.
- Default extraction rule:
  - Skip first 2 characters.
  - Read next 8 digits.
  - Example: `0080001234202606011125.pdf` -> `80001234`.
- Generate a Code 128 barcode.
- Insert barcode on the first page only.
- Placement: bottom center.
- Bottom margin: 20 mm.
- Keep original PDFs unchanged.
- Save updated PDFs into the output folder.
- Create a CSV processing log.

## Recommended Technology

- C#
- .NET 8
- Windows Forms
- PDFsharp
- ZXing.Net

## Build Requirements

Install on Windows:

- Visual Studio 2022
- .NET 8 SDK
- Workload: .NET desktop development

## Build Command

From the project folder:

```powershell
cd src\CPBarcodeManagerForVigo
dotnet restore
dotnet build -c Release
```

## Publish Single EXE

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The generated EXE will be in:

```text
src\CPBarcodeManagerForVigo\bin\Release\net8.0-windows\win-x64\publish
```

## IT Approval Notes

This application:

- Runs locally on Windows.
- Does not connect to the internet.
- Does not require administrator rights.
- Does not modify original PDFs.
- Saves processed files to the selected output folder.
- Saves settings locally in the user's AppData folder.
- Creates a CSV log for audit purposes.

Settings location:

```text
%APPDATA%\CPBarcodeManagerForVigo\settings.json
```
