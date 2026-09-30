using CPBarcodeManager.Core.Models;

namespace CPBarcodeManager.Core.Services;

public sealed class ProcessingWorkflowService
{
    public ProcessResult ProcessFileForVigo(
        string sourcePdfPath,
        AppSettings settings)
    {
        if (!WaitUntilFileReady(sourcePdfPath))
        {
            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                Status = "Critical Error",
                Message =
                    "Source PDF is still locked or changing. It was left in the Import Folder."
            };
        }

        var pdfService = new PdfBarcodeService();
        var result = pdfService.ProcessPdf(sourcePdfPath, settings);

        if (result.Status == "Success")
        {
            if (TryDeleteSourceWithRetry(sourcePdfPath))
            {
                result.Status = "Ready for Vigo";
                result.Message +=
                    " Output verified. Source PDF removed from Import Folder.";

                return result;
            }

            result.Status = "Critical Error";
            result.Message +=
                " Output was created and verified, but the source PDF could not be removed. " +
                "The source was left in the Import Folder to prevent silent data loss.";

            return result;
        }

        var processingError = result.Message;

        try
        {
            var errorOutputPath =
                MoveOriginalToNeedsAttention(
                    sourcePdfPath,
                    settings.OutputFolder);

            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                CustomerReference = result.CustomerReference,
                OutputPath = errorOutputPath,
                Status = "Needs Attention",
                Message =
                    $"Barcode processing failed: {processingError} " +
                    $"Original PDF moved to Output Folder as {Path.GetFileName(errorOutputPath)}."
            };
        }
        catch (Exception moveEx)
        {
            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                CustomerReference = result.CustomerReference,
                Status = "Critical Error",
                Message =
                    $"Barcode processing failed: {processingError} " +
                    $"The original PDF could not be moved to Output Folder: {moveEx.Message} " +
                    "The source was left in the Import Folder."
            };
        }
    }

    public static bool CanOpenForExclusiveRead(string filePath)
    {
        try
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);

            return stream.Length >= 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool WaitUntilFileReady(string filePath)
    {
        long? previousLength = null;
        DateTime? previousWriteUtc = null;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                var info = new FileInfo(filePath);

                if (!info.Exists)
                    return false;

                var length = info.Length;
                var writeUtc = info.LastWriteTimeUtc;

                if (previousLength == length &&
                    previousWriteUtc == writeUtc &&
                    CanOpenForExclusiveRead(filePath))
                {
                    return true;
                }

                previousLength = length;
                previousWriteUtc = writeUtc;
            }
            catch
            {
                // Retry below.
            }

            Thread.Sleep(500);
        }

        return CanOpenForExclusiveRead(filePath);
    }

    private static bool TryDeleteSourceWithRetry(string sourcePdfPath)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (!File.Exists(sourcePdfPath))
                    return true;

                File.Delete(sourcePdfPath);

                if (!File.Exists(sourcePdfPath))
                    return true;
            }
            catch
            {
                // Retry below.
            }

            Thread.Sleep(500);
        }

        return !File.Exists(sourcePdfPath);
    }

    private static string MoveOriginalToNeedsAttention(
        string sourcePdfPath,
        string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        var originalName = Path.GetFileName(sourcePdfPath);
        var destinationPath =
            GetUniqueErrorOutputPath(
                outputFolder,
                $"ERROR_{originalName}");

        File.Copy(
            sourcePdfPath,
            destinationPath,
            overwrite: false);

        var sourceInfo = new FileInfo(sourcePdfPath);
        var destinationInfo = new FileInfo(destinationPath);

        if (!destinationInfo.Exists ||
            destinationInfo.Length != sourceInfo.Length)
        {
            try
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
            }
            catch
            {
                // Best-effort cleanup only.
            }

            throw new IOException(
                "Error-file copy verification failed.");
        }

        if (!TryDeleteSourceWithRetry(sourcePdfPath))
        {
            throw new IOException(
                $"The file was copied to {destinationPath}, but the source could not be removed.");
        }

        return destinationPath;
    }

    private static string GetUniqueErrorOutputPath(
        string outputFolder,
        string fileName)
    {
        var path = Path.Combine(outputFolder, fileName);

        if (!File.Exists(path))
            return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        for (int i = 1; i < 10000; i++)
        {
            var candidate =
                Path.Combine(
                    outputFolder,
                    $"{name}_{i}{ext}");

            if (!File.Exists(candidate))
                return candidate;
        }

        throw new IOException(
            "Unable to create a unique ERROR output file name.");
    }
}