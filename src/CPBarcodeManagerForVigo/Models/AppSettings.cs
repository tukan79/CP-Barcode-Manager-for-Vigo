namespace CPBarcodeManagerForVigo.Models;

public sealed class AppSettings
{
    public string SourceFolder { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;

    public int SkipCharacters { get; set; } = 2;
    public int TakeCharacters { get; set; } = 8;

    public string BarcodePosition { get; set; } = "Bottom Centre";

    public int BarcodeWidthMm { get; set; } = 50;
    public int BarcodeHeightMm { get; set; } = 12;

    public int BottomMarginMm { get; set; } = 20;

    // Automatic processing is intentionally disabled by default.
    // Enable it in Settings after the Import and Output folders are confirmed.
    public bool AutoMonitorEnabled { get; set; } = false;

    // A PDF must remain unchanged across two scans before automatic processing.
    public int MonitorIntervalSeconds { get; set; } = 5;
}
