using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using Xamarin.Forms;

namespace DualSpec.Views
{
    public partial class IntensityReadingPage : ContentPage
    {

        SKBitmap bitmap;
        SKImage image;

        public IntensityReadingPage()
        {
            InitializeComponent();
        }

        async void LoadImage_Clicked(System.Object sender, System.EventArgs e)
        {

            Stream stream = await DependencyService.Get<IPhotoPickerService>().PickPhotoAsync();


            if (stream != null)
            {
                bitmap = SKBitmap.Decode(stream);
            }

            image = SKImage.FromBitmap(bitmap);

            canvasView.InvalidateSurface();

        }

        void canvasView_PaintSurface(System.Object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;



            canvas.Clear();

            canvas.RotateDegrees(90);


            if (bitmap != null)
            {
                canvas.DrawBitmap(bitmap, canvas.LocalClipBounds, null);
            }

        }

        void Process_Clicked(System.Object sender, System.EventArgs e)
        {

            image.IntensityAtPixel(2000, 1200, 1400);

        }
    }
}
