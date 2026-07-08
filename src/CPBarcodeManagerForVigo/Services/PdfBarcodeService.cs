using CPBarcodeManagerForVigo.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CPBarcodeManagerForVigo.Services;

public sealed class PdfBarcodeService
{
    private const double BarcodeWidthMm = 70.0;
    private const double BarcodeHeightMm = 18.0;

    public ProcessResult ProcessPdf(string sourcePdfPath, AppSettings settings)
    {
        var result = new ProcessResult
        {
            FileName = Path.GetFileName(sourcePdfPath)
        };

        try
        {
            if (!File.Exists(sourcePdfPath))
                throw new FileNotFoundException("PDF file not found.", sourcePdfPath);

            Directory.CreateDirectory(settings.OutputFolder);

            var fileNameNoExt = Path.GetFileNameWithoutExtension(sourcePdfPath);
            var customerReference = BarcodeValueExtractor.Extract(
                fileNameNoExt,
                settings.SkipCharacters,
                settings.TakeCharacters);

            result.CustomerReference = customerReference;

            using var barcodeStream = BarcodeImageService.GenerateCode128Png(
                customerReference,
                settings.BarcodeWidthPx,
                settings.BarcodeHeightPx);

            using PdfDocument document = PdfReader.Open(sourcePdfPath, PdfDocumentOpenMode.Modify);

            if (document.PageCount == 0)
                throw new InvalidOperationException("PDF contains no pages.");

            AddBarcodeToFirstPage(document, barcodeStream, settings.BottomMarginMm);

            var outputPath = GetUniqueOutputPath(settings.OutputFolder, Path.GetFileName(sourcePdfPath));
            document.Save(outputPath);

            result.OutputPath = outputPath;
            result.Status = "Success";
            result.Message = "Barcode added to first page only.";
            return result;
        }
        catch (Exception ex)
        {
            result.Status = "Error";
            result.Message = ex.Message;
            return result;
        }
    }

    private static void AddBarcodeToFirstPage(PdfDocument document, Stream barcodePngStream, int bottomMarginMm)
    {
        PdfPage page = document.Pages[0];

        barcodePngStream.Position = 0;
        using XImage barcodeImage = XImage.FromStream(barcodePngStream);
        using XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

        double barcodeWidth = MmToPoint(BarcodeWidthMm);
        double barcodeHeight = MmToPoint(BarcodeHeightMm);
        double bottomMargin = MmToPoint(bottomMarginMm);

        double x = (page.Width.Point - barcodeWidth) / 2.0;
        double y = page.Height.Point - bottomMargin - barcodeHeight;

        gfx.DrawImage(barcodeImage, x, y, barcodeWidth, barcodeHeight);
    }

    private static double MmToPoint(double mm) => mm * 72.0 / 25.4;

    private static string GetUniqueOutputPath(string outputFolder, string fileName)
    {
        var path = Path.Combine(outputFolder, fileName);
        if (!File.Exists(path)) return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        for (int i = 1; i < 10000; i++)
        {
            var candidate = Path.Combine(outputFolder, $"{name}_barcode_{i}{ext}");
            if (!File.Exists(candidate)) return candidate;
        }

        throw new IOException("Unable to create unique output file name.");
    }
}
