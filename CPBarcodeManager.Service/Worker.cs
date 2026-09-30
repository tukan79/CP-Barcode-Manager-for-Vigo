using CPBarcodeManager.Core.Models;

namespace CPBarcodeManager.Service;

public class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "CP Barcode Manager Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = LoadSettings();

            if (!ConfigurationIsReady(settings))
            {
                logger.LogWarning(
                    "Configuration not ready. " +
                    "SourceFolder or OutputFolder is not configured. " +
                    "No files will be processed.");

                await DelayAsync(
                    settings.MonitorIntervalSeconds,
                    stoppingToken);

                continue;
            }

            logger.LogInformation(
                "Configuration ready. Source: {source}. Output: {output}.",
                settings.SourceFolder,
                settings.OutputFolder);

            // File monitoring and processing will be added
            // only after configuration validation is tested.
            await DelayAsync(
                settings.MonitorIntervalSeconds,
                stoppingToken);
        }
    }

    private AppSettings LoadSettings()
    {
        var section = configuration.GetSection("BarcodeService");

        return new AppSettings
        {
            SourceFolder =
                section["SourceFolder"] ?? string.Empty,

            OutputFolder =
                section["OutputFolder"] ?? string.Empty,

            SkipCharacters =
                GetInt(section, "SkipCharacters", 2),

            TakeCharacters =
                GetInt(section, "TakeCharacters", 8),

            BarcodePosition =
                section["BarcodePosition"] ?? "Bottom Centre",

            BarcodeWidthMm =
                GetInt(section, "BarcodeWidthMm", 50),

            BarcodeHeightMm =
                GetInt(section, "BarcodeHeightMm", 12),

            BottomMarginMm =
                GetInt(section, "BottomMarginMm", 20),

            MonitorIntervalSeconds =
                Math.Clamp(
                    GetInt(section, "ScanIntervalSeconds", 5),
                    2,
                    60),

            AutoMonitorEnabled = true
        };
    }

    private static bool ConfigurationIsReady(
        AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SourceFolder) ||
            string.IsNullOrWhiteSpace(settings.OutputFolder))
        {
            return false;
        }

        var sourceFullPath =
            Path.GetFullPath(settings.SourceFolder)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);

        var outputFullPath =
            Path.GetFullPath(settings.OutputFolder)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);

        return !string.Equals(
            sourceFullPath,
            outputFullPath,
            StringComparison.OrdinalIgnoreCase);
    }

    private static int GetInt(
        IConfigurationSection section,
        string key,
        int defaultValue)
    {
        return int.TryParse(section[key], out var value)
            ? value
            : defaultValue;
    }

    private static async Task DelayAsync(
        int seconds,
        CancellationToken stoppingToken)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(Math.Clamp(seconds, 2, 60)),
            stoppingToken);
    }
}