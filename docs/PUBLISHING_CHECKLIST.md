# Publishing Checklist

## CP Barcode Manager for Vigo

### Source Code
Confirm:
- project builds
- no merge conflicts
- correct version
- intended branch
- clean working tree before release

Build:
```powershell
dotnet build .\src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj
```

### Functional Regression
Confirm filename extraction, Code 128, every-page placement, dimensions, settings persistence, manual processing, automatic monitoring, slow-write protection, verified output, safe source removal, Needs Attention and Critical Error handling.

### Documentation
Confirm current content in:
- `README.md`
- `docs/INSTALLATION.md`
- `docs/USER_GUIDE.md`
- `docs/TECHNICAL_DESIGN.md`
- `docs/TEST_PLAN.md`
- `docs/PUBLISHING_CHECKLIST.md`

### Privacy Review
Do not publish production PDFs, customer data, credentials, internal URLs, private SharePoint paths, internal email addresses, confidential screenshots or production configuration.

### Git Review
```powershell
git status
git diff
git diff --staged
```

### Commit
Example:
```powershell
git add README.md docs
git commit -m "Update documentation for CP Barcode Manager v0.2.0"
```

### Push
```powershell
git push origin main
```

Do not use force push in the normal workflow.

### Verify GitHub
Check latest commit, README rendering, documentation links, source files, privacy, and removal of obsolete project descriptions.

### v0.2.0 Release Gate
Confirm Desktop build, automatic monitoring, slow-write test, output verification, source deletion safety, invalid-file routing and synthetic test data.

### Windows Service
Keep status as `Planned` until implementation and testing are complete.
