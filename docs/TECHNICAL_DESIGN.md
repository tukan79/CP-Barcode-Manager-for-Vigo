# Technical Design

## Main sequence

1. Collect `.xlsx` files from `Config` and require exactly one.
2. Open Excel, read the worksheet into `ExcelData`, and close Excel.
3. Collect PDFs from `SAP_Import`.
4. Initialise `TotalPDFCount`, `SuccessCount`, `NoMappingCount`, `DuplicateCount`, and `InvalidFileNameCount`.
5. Process each PDF.
6. Archive the Excel report.
7. Build and display the final report.

## Filename validation

```regex
^00[0-9]{8}
```

If the match position is `-1`, move the file to `Errors\Invalid_File_Name` and continue to the next PDF.

## Extraction and matching

Extract eight characters from `CurrentPDF.NameWithoutExtension`, beginning at index `2`, into `ConNoFromFile`. Filter `ExcelData` where `Con No` equals `ConNoFromFile`.

If no row is found, move the PDF to `Errors\No_Mapping`. Otherwise set:

```text
ContradoReference = FilteredRows[0]['Reference']
```

If `Contrado_Output\<ContradoReference>.pdf` already exists, move the incoming PDF to `Errors\Duplicate_File`. Otherwise rename and move it to `Contrado_Output`.

## Excel archive

Move the Excel report to `Archive\Excel`, then rename it:

```text
Contrado_Report_dd.MM.yyyy_HH-mm-ss.xlsx
```

## Final invariant

```text
TotalPDFCount = SuccessCount + NoMappingCount + DuplicateCount + InvalidFileNameCount
```
