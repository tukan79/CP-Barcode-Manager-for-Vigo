# Test Plan

## CP Barcode Manager for Vigo v0.2.0

Use synthetic PDFs unless an organisation-approved production test is being performed.

### Filename Tests
Valid:
- `0080001234202606011125.pdf` -> `80001234`
- `0087654321202606020930.pdf` -> `87654321`

Invalid:
- `00ABC12345202606051700.pdf` -> Needs Attention
- `00123.pdf` -> Needs Attention

### PDF Tests
Test 1, 2, 3 and 5-page PDFs.
Expected:
- same page count in output
- barcode on every page

### Barcode Tests
Verify Code 128 content and readability.

### Placement Tests
Test Bottom Left, Bottom Centre and Bottom Right.

### Dimension Tests
Change width, height and bottom margin; save, restart, and confirm persistence.

### Folder Validation
Processing must be blocked if:
- Import and Output are the same
- Import does not exist
- Output does not exist

### Success Safety Test
Expected:
1. Output generated.
2. Output verified.
3. Final output exists.
4. Source removed only after verification.
5. Status = Ready for Vigo.

### Invalid File Safety Test
Expected:
1. Processing fails safely.
2. Original copied to Output.
3. Copy verified.
4. Source removed only after verified copy.
5. `ERROR_` prefix used.
6. Status = Needs Attention.

### Automatic Monitoring
Enable monitoring, place a completed PDF in Import, verify stable-file checks, automatic processing, output verification and source removal.

### Slow-Write Test
Write/copy a PDF slowly while holding the write handle.
Expected: no processing until writing is complete, file is stable and exclusive read is available.

This test passed for v0.2.0.

### Batch Test
Mix valid and invalid PDFs. One failure must not stop the batch.

### Settings Persistence
Verify folders, placement, dimensions, bottom margin, monitoring and interval survive restart.

### Build Test
```powershell
dotnet build .\src\CPBarcodeManagerForVigo\CPBarcodeManagerForVigo.csproj
```
Expected: build succeeds with no errors.

### Production Acceptance
Separately validate real filename convention, Vigo barcode recognition, production document layout, approved folders, permissions and support process.
