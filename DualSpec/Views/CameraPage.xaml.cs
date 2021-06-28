using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views;
using System.IO;
using SkiaSharp.Views.Forms;

namespace DualSpec.Views
{
    public partial class CameraPage : ContentPage
    {

        SKImage capturedImage;
        SKBitmap capturedBitmap;
        Stream stream;

        public CameraPage()
        {
            InitializeComponent();
        }

        async void SaveImage_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new SavePhotoPage(capturedImage)));
        }

        async void OpenCamera_Clicked(System.Object sender, System.EventArgs e)
        {

            var result = await MediaPicker.CapturePhotoAsync();


            if (result != null)
            {
                stream = await result.OpenReadAsync();

                capturedImage = SKImage.FromEncodedData(stream);

            }
            canvasView.InvalidateSurface();

        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKCanvas canvas = e.Surface.Canvas;

            canvas.Clear();
            if (capturedImage != null)
            {
                canvas.DrawImage(capturedImage, info.Rect);
            }
        }
    }
}
