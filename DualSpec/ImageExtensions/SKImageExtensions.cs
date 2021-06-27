using SkiaSharp;

namespace DualSpec.Views
{
    public static class SKImageExtensions
    {

        public static unsafe SKPixmap SKImageToPixmap(this SKImage image)
        {

            SKPixmap pixmap = image.PeekPixels();

            return pixmap;

        }
        
    }
}
