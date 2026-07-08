using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace CPBarcodeManagerForVigo.Services;

public static class BarcodeImageService
{
    public static MemoryStream GenerateCode128Png(string value, int widthPx, int heightPx)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = widthPx,
                Height = heightPx,
                Margin = 2,
                PureBarcode = false
            }
        };

        using Bitmap bitmap = writer.Write(value);
        var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        return stream;
    }
}
