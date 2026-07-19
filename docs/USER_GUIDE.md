# User Guide

## Before running

1. Put exactly one Contrado `.xlsx` report in `Config`.
2. Close the Excel report.
3. Put SAP PDFs in `SAP_Import`.
4. Run the flow in Power Automate Desktop.
5. Read exception messages and the final report.

## Review after running

```text
Contrado_Output
Errors\No_Mapping
Errors\Duplicate_File
Errors\Invalid_File_Name
Archive\Excel
```

`SAP_Import` and `Config` should normally be empty after a completed run.

## Exceptions

- **Not found in Contrado:** check VIGO and follow the approved upload, deletion, or escalation process.
- **Duplicate PDF:** review the new file in `Errors\Duplicate_File` before replacing or deleting anything.
- **Invalid filename:** confirm the correct consignment number before renaming.
- **Missing Excel report:** download an approved Contrado report and place it in `Config`.
- **Multiple Excel reports:** remove extra files; exactly one `.xlsx` is required.

Email notification may be enabled only with a company-approved account or connection. Do not store private credentials in the distributed flow.
