using CPBarcodeManagerForVigo.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CPBarcodeManagerForVigo.Services;

public sealed class PdfBarcodeService
{
    private const double SideMarginMm = 15.0;

    public ProcessResult ProcessPdf(string sourcePdfPath, AppSettings settings)
    {
        var result = new ProcessResult
        {
            FileName = Path.GetFileName(sourcePdfPath)
        };

        string? tempOutputPath = null;

        try
        {
            if (!File.Exists(sourcePdfPath))
                throw new FileNotFoundException(
                    "PDF file not found.",
                    sourcePdfPath);

            ValidateSettings(settings);

            Directory.CreateDirectory(settings.OutputFolder);

            var fileNameNoExt =
                Path.GetFileNameWithoutExtension(sourcePdfPath);

            var customerReference =
                BarcodeValueExtractor.Extract(
                    fileNameNoExt,
                    settings.SkipCharacters,
                    settings.TakeCharacters);

            result.CustomerReference = customerReference;

            var imageWidthPx =
                Math.Max(300, settings.BarcodeWidthMm * 8);

            var imageHeightPx =
                Math.Max(80, settings.BarcodeHeightMm * 8);

            var outputPath =
                GetUniqueOutputPath(
                    settings.OutputFolder,
                    Path.GetFileName(sourcePdfPath));

            tempOutputPath =
                Path.Combine(
                    settings.OutputFolder,
                    $".cpbarcode_{Guid.NewGuid():N}.tmp.pdf");

            int pageCount;

            using (var barcodeStream =
                BarcodeImageService.GenerateCode128Png(
                    customerReference,
                    imageWidthPx,
                    imageHeightPx))
            using (PdfDocument document =
                PdfReader.Open(
                    sourcePdfPath,
                    PdfDocumentOpenMode.Modify))
            {
                if (document.PageCount == 0)
                    throw new InvalidOperationException(
                        "PDF contains no pages.");

                pageCount = document.PageCount;

                AddBarcodeToEveryPage(
                    document,
                    barcodeStream,
                    settings);

                document.Save(tempOutputPath);
            }

            VerifyOutputPdf(
                tempOutputPath,
                pageCount);

            File.Move(
                tempOutputPath,
                outputPath);

            tempOutputPath = null;

            result.OutputPath = outputPath;
            result.Status = "Success";
            result.Message =
                $"Barcode added to every page ({pageCount} page(s)), " +
                $"{settings.BarcodePosition}, " +
                $"{settings.BarcodeWidthMm} x {settings.BarcodeHeightMm} mm.";

            return result;
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(tempOutputPath))
            {
                try
                {
                    if (File.Exists(tempOutputPath))
                        File.Delete(tempOutputPath);
                }
                catch
                {
                    // Best-effort cleanup only.
                }
            }

            result.Status = "Error";
            result.Message = ex.Message;
            return result;
        }
    }

    private static void VerifyOutputPdf(
        string outputPath,
        int expectedPageCount)
    {
        if (!File.Exists(outputPath))
            throw new IOException(
                "Output PDF was not created.");

        var info = new FileInfo(outputPath);

        if (info.Length <= 0)
            throw new IOException(
                "Output PDF is empty.");

        using PdfDocument verificationDocument =
            PdfReader.Open(
                outputPath,
                PdfDocumentOpenMode.Import);

        if (verificationDocument.PageCount != expectedPageCount)
        {
            throw new IOException(
                $"Output PDF verification failed. Expected {expectedPageCount} page(s), " +
                $"but found {verificationDocument.PageCount}.");
        }
    }

    private static void AddBarcodeToEveryPage(
        PdfDocument document,
        Stream barcodePngStream,
        AppSettings settings)
    {
        barcodePngStream.Position = 0;

        using XImage barcodeImage =
            XImage.FromStream(barcodePngStream);

        var barcodeWidth =
            MmToPoint(settings.BarcodeWidthMm);

        var barcodeHeight =
            MmToPoint(settings.BarcodeHeightMm);

        var bottomMargin =
            MmToPoint(settings.BottomMarginMm);

        var sideMargin =
            MmToPoint(SideMarginMm);

        foreach (PdfPage page in document.Pages)
        {
            if (barcodeWidth >= page.Width.Point)
            {
                throw new InvalidOperationException(
                    "Barcode width is too large for one or more PDF pages.");
            }

            if (barcodeHeight + bottomMargin >= page.Height.Point)
            {
                throw new InvalidOperationException(
                    "Barcode height and bottom margin are too large for one or more PDF pages.");
            }

            double x =
                settings.BarcodePosition switch
                {
                    "Bottom Left" =>
                        sideMargin,

                    "Bottom Right" =>
                        page.Width.Point
                        - sideMargin
                        - barcodeWidth,

                    _ =>
                        (page.Width.Point - barcodeWidth) / 2.0
                };

            if (x < 0 ||
                x + barcodeWidth > page.Width.Point)
            {
                throw new InvalidOperationException(
                    "Selected barcode position or size does not fit on one or more PDF pages.");
            }

            double y =
                page.Height.Point
                - bottomMargin
                - barcodeHeight;

            using XGraphics gfx =
                XGraphics.FromPdfPage(
                    page,
                    XGraphicsPdfPageOptions.Append);

            gfx.DrawImage(
                barcodeImage,
                x,
                y,
                barcodeWidth,
                barcodeHeight);
        }
    }

    private static void ValidateSettings(
        AppSettings settings)
    {
        if (settings.BarcodeWidthMm < 25 ||
            settings.BarcodeWidthMm > 100)
        {
            throw new InvalidOperationException(
                "Barcode width must be between 25 mm and 100 mm.");
        }

        if (settings.BarcodeHeightMm < 8 ||
            settings.BarcodeHeightMm > 30)
        {
            throw new InvalidOperationException(
                "Barcode height must be between 8 mm and 30 mm.");
        }

        if (settings.BottomMarginMm < 5 ||
            settings.BottomMarginMm > 50)
        {
            throw new InvalidOperationException(
                "Bottom margin must be between 5 mm and 50 mm.");
        }

        if (settings.BarcodePosition is not
            ("Bottom Left" or
             "Bottom Centre" or
             "Bottom Right"))
        {
            throw new InvalidOperationException(
                "Barcode position must be Bottom Left, Bottom Centre or Bottom Right.");
        }
    }

    private static double MmToPoint(double mm)
    {
        return mm * 72.0 / 25.4;
    }

    private static string GetUniqueOutputPath(
        string outputFolder,
        string fileName)
    {
        var path =
            Path.Combine(
                outputFolder,
                fileName);

        if (!File.Exists(path))
            return path;

        var name =
            Path.GetFileNameWithoutExtension(fileName);

        var ext =
            Path.GetExtension(fileName);

        for (int i = 1; i < 10000; i++)
        {
            var candidate =
                Path.Combine(
                    outputFolder,
                    $"{name}_barcode_{i}{ext}");

            if (!File.Exists(candidate))
                return candidate;
        }

        throw new IOException(
            "Unable to create unique output file name.");
    }
}
