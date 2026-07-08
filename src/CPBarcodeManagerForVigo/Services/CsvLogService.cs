using System.Text;
using CPBarcodeManagerForVigo.Models;

namespace CPBarcodeManagerForVigo.Services;

public static class CsvLogService
{
    public static string SaveLog(string outputFolder, IReadOnlyList<ProcessResult> results)
    {
        Directory.CreateDirectory(outputFolder);
        var logPath = Path.Combine(outputFolder, $"CP_Barcode_Manager_Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        var sb = new StringBuilder();
        sb.AppendLine("FileName,CustomerReference,Status,Message,OutputPath");

        foreach (var r in results)
        {
            sb.AppendLine(string.Join(",",
                Escape(r.FileName),
                Escape(r.CustomerReference),
                Escape(r.Status),
                Escape(r.Message),
                Escape(r.OutputPath)));
        }

        File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
        return logPath;
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
