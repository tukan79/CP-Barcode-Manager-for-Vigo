# User Guide - CP Barcode Manager for Vigo

## Purpose

The tool adds a Customer Reference barcode to SAP Customer Paperwork PDFs so they can be uploaded into Vigo using the Customer Ref barcode option.

## Standard Process

1. Open **CP Barcode Manager for Vigo**.
2. Select **Source Folder** containing SAP PDF files.
3. Select **Output Folder** where processed PDFs will be saved.
4. Confirm settings:
   - Barcode Type: Code 128
   - Skip first characters: 2
   - Read next digits: 8
   - Apply to: First page only
   - Position: Bottom Center
   - Bottom margin: 20 mm
5. Click **Preview First File** to check the extracted barcode value.
6. Click **Start Processing**.
7. Upload the processed PDFs from the Output folder into Vigo.

## Example

Input file:

```text
0080001234202606011125.pdf
```

Barcode generated:

```text
80001234
```

## Error Handling

The tool skips files that do not match the expected filename format. Errors are shown in the log window and saved to the CSV log file.
