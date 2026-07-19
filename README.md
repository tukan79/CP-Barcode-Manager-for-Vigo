# SAP–Contrado File Renamer

A Microsoft Power Automate Desktop workflow for validating, matching, renaming, routing, and reporting SAP PDF delivery documents against a Contrado Excel export.

> **Project status:** Tested prototype — version 1.3  
> **Platform:** Microsoft Power Automate Desktop on Windows  
> **License:** MIT

## Overview

The workflow processes PDF files placed in a local `SAP_Import` folder. It extracts an eight-digit consignment number from each valid SAP filename, compares it with a Contrado Excel export, and renames and routes the PDF using the matching Contrado reference.

The workflow was created to reduce manual handling and prevent documents from being missed, incorrectly named, or left in the input folder.

## Features

- Requires exactly one Contrado `.xlsx` report in `Config`.
- Reads the Excel report into a Power Automate data table.
- Collects all PDFs from `SAP_Import`.
- Validates SAP filenames using `^00[0-9]{8}`.
- Extracts the eight-digit consignment number.
- Matches it against the Excel column `Con No`.
- Renames matched PDFs using the Excel column `Reference`.
- Moves successful files to `Contrado_Output`.
- Moves unmatched consignments to `Errors/No_Mapping`.
- Moves duplicate output references to `Errors/Duplicate_File`.
- Moves invalid filenames to `Errors/Invalid_File_Name`.
- Displays a final summary in a custom form.
- Archives the used Excel report with a timestamp.
- Documents optional company-approved email reporting; email is not enabled in the public test version.

## Processing outcomes

| Outcome | Destination |
|---|---|
| Successfully matched and renamed | `Contrado_Output` |
| Consignment not found in Contrado | `Errors/No_Mapping` |
| Output reference already exists | `Errors/Duplicate_File` |
| Invalid SAP filename | `Errors/Invalid_File_Name` |

## Expected SAP filename

```text
0087654321202607061800.pdf
```

The workflow extracts:

```text
87654321
```

## Expected Excel columns

```text
Con No
Reference
```

## Test folder structure

```text
C:\SAP-Contrado-Test\
├── Config\
├── SAP_Import\
├── Contrado_Output\
├── Archive\
│   └── Excel\
└── Errors\
    ├── Duplicate_File\
    ├── Invalid_File_Name\
    └── No_Mapping\
```

Paths are currently hard-coded for testing and must be reviewed before production deployment.

## Example final report

```text
SAP–Contrado processing has been completed.

PDF files found: 4
Successfully renamed and moved: 1
Not found in Contrado: 1
Duplicates: 1
Invalid PDF filenames: 1
```

## Documentation

- [Installation Guide](docs/INSTALLATION.md)
- [User Guide](docs/USER_GUIDE.md)
- [Technical Design](docs/TECHNICAL_DESIGN.md)
- [Test Plan](docs/TEST_PLAN.md)
- [Publishing Checklist](docs/PUBLISHING_CHECKLIST.md)
- [AI Assistance and Legal Notice](AI_ASSISTANCE.md)

## Security and privacy

Do not upload production Excel exports, real SAP PDFs, customer information, credentials, private email addresses, internal URLs, or confidential screenshots. The `.gitignore` excludes common Excel, PDF, archive, secret, and temporary files, but every file must still be manually reviewed before publication.

## Optional email notification

Email delivery is intentionally disabled in the public test version. A company may configure an approved Microsoft 365 account, shared mailbox, SMTP service, or cloud connector. Private credentials must never be stored in the distributed workflow.

## AI-assisted development disclosure

This project was designed, built, tested, and documented by **Krzysztof Stolarski**, with assistance from **OpenAI ChatGPT**. ChatGPT was used for planning, documentation, troubleshooting, and workflow design guidance. The author manually reviewed, modified, tested, and approved the final workflow and accepts responsibility for the published project.

OpenAI is not affiliated with, sponsoring, certifying, or endorsing this project.

## Disclaimer

This is an independent automation example and not an official SAP, Microsoft, Contrado, or OpenAI product. Product names may be trademarks of their respective owners. Test with synthetic data and obtain organisational approval before business use.

## License

Copyright (c) 2026 Krzysztof Stolarski. Licensed under the [MIT License](LICENSE).
