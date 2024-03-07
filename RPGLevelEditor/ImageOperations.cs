using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace RPGLevelEditor
{
    public static class ImageOperations
    {
        public static void CopyImage(this WriteableBitmap target, BitmapSource source, int x, int y, int width, int height)
        {
            if (source.PixelWidth != width || source.PixelHeight != height || source.Format != target.Format)
            {
                source = new FormatConvertedBitmap(source, target.Format, target.Palette, 0);
                source = new TransformedBitmap(source,
                    new ScaleTransform(width / (double)source.PixelWidth, height / (double)source.PixelHeight));
            }

            int sourceBytesPerPixel = source.Format.BitsPerPixel / 8;
            int sourceBytesPerLine = source.PixelWidth * sourceBytesPerPixel;

            byte[] sourcePixels = new byte[sourceBytesPerLine * source.PixelHeight];
            source.CopyPixels(sourcePixels, sourceBytesPerLine, 0);

            Int32Rect sourceRect = new(x, y, source.PixelWidth, source.PixelHeight);
            target.WritePixels(sourceRect, sourcePixels, sourceBytesPerLine, 0);
        }
    }
}
