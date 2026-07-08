namespace CPBarcodeManagerForVigo.Models;

public sealed class AppSettings
{
    public string SourceFolder { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;
    public int SkipCharacters { get; set; } = 2;
    public int TakeCharacters { get; set; } = 8;
    public int BottomMarginMm { get; set; } = 20;
    public int BarcodeWidthPx { get; set; } = 360;
    public int BarcodeHeightPx { get; set; } = 80;
    public bool FirstPageOnly { get; set; } = true;
}
