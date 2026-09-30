using CPBarcodeManager.Core.Models;
using CPBarcodeManager.Core.Services;

namespace CPBarcodeManager.Service;

public class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration) : BackgroundService
{
    private readonly Dictionary<string, FileSnapshot> _fileSnapshots =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _blockedFiles =
        new(StringComparer.OrdinalIgnoreCase);

    private sealed record FileSnapshot(
        long Length,
        DateTime LastWriteUtc,
        int StableChecks);

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

            try
            {
                var stableFiles =
                    FindStablePdfFiles(settings.SourceFolder);

                if (stableFiles.Count == 0)
                {
                    logger.LogInformation(
                        "Monitoring Import Folder. No stable PDFs ready.");
                }
                else
                {
                    foreach (var file in stableFiles)
                    {
                        if (!File.Exists(file) ||
                            _blockedFiles.Contains(file))
                        {
                            continue;
                        }

                        logger.LogInformation(
                            "Processing PDF: {file}",
                            Path.GetFileName(file));

                        var result =
                            new ProcessingWorkflowService()
                                .ProcessFileForVigo(
                                    file,
                                    settings);

                        if (result.Status == "Critical Error")
                        {
                            _blockedFiles.Add(file);

                            logger.LogCritical(
                                "CRITICAL ERROR processing {file}: {message} " +
                                "The source file has been blocked from automatic retry.",
                                Path.GetFileName(file),
                                result.Message);
                        }
                        else
                        {
                            _fileSnapshots.Remove(file);

                            if (result.Status == "Ready for Vigo")
                            {
                                logger.LogInformation(
                                    "READY FOR VIGO: {file}. " +
                                    "Customer Reference: {reference}. " +
                                    "Output: {output}",
                                    result.FileName,
                                    result.CustomerReference,
                                    result.OutputPath);
                            }
                            else
                            {
                                logger.LogWarning(
                                    "{status}: {file}. {message}",
                                    result.Status,
                                    result.FileName,
                                    result.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Import Folder scan failed.");
            }

            await DelayAsync(
                settings.MonitorIntervalSeconds,
                stoppingToken);
        }
    }

    private List<string> FindStablePdfFiles(
        string sourceFolder)
    {
        var currentFiles = Directory
            .GetFiles(sourceFolder, "*.pdf")
            .OrderBy(x => x)
            .ToList();

        var existingSet =
            new HashSet<string>(
                currentFiles,
                StringComparer.OrdinalIgnoreCase);

        foreach (var knownPath in _fileSnapshots.Keys.ToList())
        {
            if (!existingSet.Contains(knownPath))
                _fileSnapshots.Remove(knownPath);
        }

        var stableFiles = new List<string>();

        foreach (var file in currentFiles)
        {
            if (_blockedFiles.Contains(file))
                continue;

            var info = new FileInfo(file);

            var currentLength = info.Length;
            var currentWriteUtc = info.LastWriteTimeUtc;

            if (_fileSnapshots.TryGetValue(
                    file,
                    out var previous) &&
                previous.Length == currentLength &&
                previous.LastWriteUtc == currentWriteUtc)
            {
                var updated = previous with
                {
                    StableChecks =
                        previous.StableChecks + 1
                };

                _fileSnapshots[file] = updated;

                if (updated.StableChecks >= 1 &&
                    ProcessingWorkflowService
                        .CanOpenForExclusiveRead(file))
                {
                    stableFiles.Add(file);
                }
            }
            else
            {
                _fileSnapshots[file] =
                    new FileSnapshot(
                        currentLength,
                        currentWriteUtc,
                        0);
            }
        }

        return stableFiles;
    }

    private AppSettings LoadSettings()
    {
        var section =
            configuration.GetSection("BarcodeService");

        return new AppSettings
        {
            SourceFolder =
                section["SourceFolder"] ?? string.Empty,

            OutputFolder =
                section["OutputFolder"] ?? string.Empty,

            SkipCharacters =
                GetInt(
                    section,
                    "SkipCharacters",
                    2),

            TakeCharacters =
                GetInt(
                    section,
                    "TakeCharacters",
                    8),

            BarcodePosition =
                section["BarcodePosition"] ??
                "Bottom Centre",

            BarcodeWidthMm =
                GetInt(
                    section,
                    "BarcodeWidthMm",
                    50),

            BarcodeHeightMm =
                GetInt(
                    section,
                    "BarcodeHeightMm",
                    12),

            BottomMarginMm =
                GetInt(
                    section,
                    "BottomMarginMm",
                    20),

            MonitorIntervalSeconds =
                Math.Clamp(
                    GetInt(
                        section,
                        "ScanIntervalSeconds",
                        5),
                    2,
                    60),

            AutoMonitorEnabled = true
        };
    }

    private static bool ConfigurationIsReady(
        AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(
                settings.SourceFolder) ||
            string.IsNullOrWhiteSpace(
                settings.OutputFolder))
        {
            return false;
        }

        if (!Directory.Exists(settings.SourceFolder) ||
            !Directory.Exists(settings.OutputFolder))
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
        return int.TryParse(
            section[key],
            out var value)
            ? value
            : defaultValue;
    }

    private static async Task DelayAsync(
        int seconds,
        CancellationToken stoppingToken)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(
                Math.Clamp(seconds, 2, 60)),
            stoppingToken);
    }
}