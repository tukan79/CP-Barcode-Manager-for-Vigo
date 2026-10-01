using System.Text.Json;

namespace CPBarcodeManager.Service;

public sealed class CriticalStateStore
{
    private readonly string _stateFilePath;
    private readonly object _sync = new();

    public CriticalStateStore(string stateFilePath)
    {
        _stateFilePath = stateFilePath;
    }

    public IReadOnlyList<CriticalFileRecord> Load()
    {
        lock (_sync)
        {
            return LoadInternal();
        }
    }

    public void AddOrUpdate(
        string sourcePath,
        string message)
    {
        lock (_sync)
        {
            var records = LoadInternal();

            var fullPath = Path.GetFullPath(sourcePath);
            var sourceInfo = new FileInfo(fullPath);

            if (!sourceInfo.Exists)
                throw new FileNotFoundException(
                    "Cannot persist critical state because the source file no longer exists.",
                    fullPath);

            records.RemoveAll(x =>
                string.Equals(
                    Path.GetFullPath(x.SourcePath),
                    fullPath,
                    StringComparison.OrdinalIgnoreCase));

            records.Add(new CriticalFileRecord
            {
                SourcePath = fullPath,
                FileName = Path.GetFileName(fullPath),
                SourceLength = sourceInfo.Length,
                SourceLastWriteUtc = sourceInfo.LastWriteTimeUtc,
                BlockedAtUtc = DateTime.UtcNow,
                Message = message
            });

            SaveInternal(records);
        }
    }

    public void Remove(string sourcePath)
    {
        lock (_sync)
        {
            var records = LoadInternal();
            var fullPath = Path.GetFullPath(sourcePath);

            records.RemoveAll(x =>
                string.Equals(
                    Path.GetFullPath(x.SourcePath),
                    fullPath,
                    StringComparison.OrdinalIgnoreCase));

            SaveInternal(records);
        }
    }

    private List<CriticalFileRecord> LoadInternal()
    {
        if (!File.Exists(_stateFilePath))
            return new List<CriticalFileRecord>();

        var json = File.ReadAllText(_stateFilePath);

        var records =
            JsonSerializer.Deserialize<List<CriticalFileRecord>>(json);

        if (records is null)
        {
            throw new InvalidDataException(
                "Critical-state file contains no valid state data.");
        }

        return records;
    }

    private void SaveInternal(
        List<CriticalFileRecord> records)
    {
        var directory = Path.GetDirectoryName(_stateFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(
            records,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var tempPath = _stateFilePath + ".tmp";

        File.WriteAllText(tempPath, json);
        File.Move(
            tempPath,
            _stateFilePath,
            overwrite: true);
    }
}