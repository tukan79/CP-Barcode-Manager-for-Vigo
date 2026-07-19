# Installation Guide

## Requirements

- Windows 10 or Windows 11
- Microsoft Power Automate Desktop
- Microsoft Excel desktop application
- Permission to create and modify local folders

Email software is not required because email reporting is disabled in the public test version.

## Create folders

```text
C:\SAP-Contrado-Test\
├── Config\
├── SAP_Import\
├── Contrado_Output\
├── Archive\Excel\
└── Errors\
    ├── Duplicate_File\
    ├── Invalid_File_Name\
    └── No_Mapping\
```

## Configure

1. Import the reviewed Power Automate Desktop flow package.
2. Review every hard-coded path.
3. Confirm the variable and Excel column names.
4. Save the flow under a test name.
5. Run the full test plan before production use.

Place exactly one closed `.xlsx` report in `Config`. It must contain `Con No` and `Reference` columns. Place SAP PDFs in `SAP_Import`.
