namespace CPBarcodeManager.Service;

public sealed class CriticalFileRecord
{
    public string SourcePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public long SourceLength { get; set; }
    public DateTime SourceLastWriteUtc { get; set; }

    public DateTime BlockedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
