namespace CPBarcodeManagerForVigo.Models;

public sealed class ProcessResult
{
    public string FileName { get; set; } = string.Empty;
    public string CustomerReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
}
