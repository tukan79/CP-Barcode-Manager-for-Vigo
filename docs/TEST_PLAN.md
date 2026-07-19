# Test Plan

Use synthetic data only.

## Successful mapping

```text
PDF: 0087654321202607061800.pdf
Con No: 87654321
Reference: TEST-CONTRADO-001
```

Expected: `TEST-CONTRADO-001.pdf` in `Contrado_Output`.

## No mapping

```text
0099999999202607061800.pdf
```

Expected: moved to `Errors\No_Mapping`.

## Duplicate

Process two PDFs mapping to the same reference or pre-create the target output. Expected: the later PDF goes to `Errors\Duplicate_File`.

## Invalid filename

```text
costam.pdf
```

Expected: moved to `Errors\Invalid_File_Name` without substring or Excel errors.

## Full four-file run

```text
0087654321202607061800.pdf
0087654321202607061801.pdf
0099999999202607061800.pdf
costam.pdf
```

With `Contrado_Output` empty before the test, expected:

```text
PDF files found: 4
Successfully renamed and moved: 1
Not found in Contrado: 1
Duplicates: 1
Invalid PDF filenames: 1
```

Expected final state: one success, one file in each error folder, empty `SAP_Import`, empty `Config`, and one archived Excel report.
