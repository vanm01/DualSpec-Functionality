using System;
using Xamarin.Forms;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using TouchTracking;

namespace DualSpec.Views
{
    public partial class SpecIsolationPage : ContentPage
    {

        SKPaint rectPaint = new SKPaint();
        SKRect scaleRect = new SKRect(20, 50, 220, 250);

        SKBitmap bitmap;

        SKPoint topLeftCorner = new SKPoint(20, 50);
        SKPoint bottomRightCorner = new SKPoint(120, 150);


        SKPaint circlePaint = new SKPaint();
        int circleRadius = 10;

        SKMatrix currentMatrix = SKMatrix.CreateIdentity();

        // Information for translating and scaling
        long? touchId = null;
        SKPoint pressedLocation;

        // Information for scaling
        bool isScaling;
        SKPoint pivotPoint;

        public SpecIsolationPage()
        {

            rectPaint.Color = SKColors.Red;
            rectPaint.Style = SKPaintStyle.Stroke;
            rectPaint.StrokeWidth = 10;


            circlePaint.Color = SKColors.Red;
            circlePaint.Style = SKPaintStyle.StrokeAndFill;
            circlePaint.StrokeWidth = 15;


            InitializeComponent();

            CoordinateLabel.Text = "Top Left Corner (" + scaleRect.Left + ", " + scaleRect.Top + ")\tBottom Right Conrner (" + scaleRect.Right + ", " + scaleRect.Bottom + ")";

        }


        async void LoadImage_Clicked(System.Object sender, System.EventArgs e)
        {

            Stream stream = await DependencyService.Get<IPhotoPickerService>().PickPhotoAsync();
            if (stream != null)
            {
                bitmap = SKBitmap.Decode(stream);
            }

            canvasView.InvalidateSurface();
        }

        void picker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {



        }

        void OnCanvasViewPaintSurface(System.Object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;



            canvas.Clear();

            canvas.RotateDegrees(90);

            if(bitmap != null)
            {
                canvas.DrawBitmap(bitmap, canvas.LocalClipBounds, null);
            }

            canvas.SetMatrix(currentMatrix);
            canvas.DrawRect(scaleRect, rectPaint);


            canvas.DrawCircle(scaleRect.Right, scaleRect.Top, circleRadius, circlePaint);
            canvas.DrawCircle(scaleRect.Right, scaleRect.Bottom, circleRadius, circlePaint);
            canvas.DrawCircle(scaleRect.Left, scaleRect.Top, circleRadius, circlePaint);
            canvas.DrawCircle(scaleRect.Left, scaleRect.Bottom, circleRadius, circlePaint);

        }

        void OnTouchEffectAction(System.Object sender, TouchTracking.TouchActionEventArgs args)
        {


            // Convert Xamarin.Forms point to pixels
            Point pt = args.Location;
            SKPoint point =
                new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                            (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));

            //Distance from one of the corners considered to be close enough
            double touchRadius = 60;

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Track only one finger
                    if (touchId.HasValue)
                        return;

                    // Check if the finger is within the boundaries of the bitmap
                    SKRect rect = new SKRect(scaleRect.Left, scaleRect.Top, scaleRect.Right, scaleRect.Bottom);
                    rect = currentMatrix.MapRect(rect);
                    if (!rect.Contains(point))
                        return;

                    // First assume there will be no scaling
                    isScaling = false;


                    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    //Bottom Right corner clicked and dragged
                    if ((Math.Abs((point.X - rect.Right)) < touchRadius) && (Math.Abs((point.Y - rect.Bottom)) < touchRadius))
                    {
                        isScaling = true;
                        float xPivot = rect.Left;
                        float yPivot = rect.Top;
                        pivotPoint = new SKPoint(xPivot, yPivot);

                    }

                    //Top Right corner clicked and dragged
                    else if ((Math.Abs(point.X - rect.Right) < touchRadius) && (Math.Abs(point.Y - rect.Top) < touchRadius))
                    {
                        isScaling = true;
                        float xPivot = rect.Left;
                        float yPivot = rect.Bottom;
                        pivotPoint = new SKPoint(xPivot, yPivot);
                    }

                    //Bottom Left corner clicked and dragged
                    else if ((Math.Abs(point.X - rect.Left) < touchRadius) && (Math.Abs(point.Y - rect.Bottom) < touchRadius))
                    {
                        isScaling = true;
                        float xPivot = rect.Right;
                        float yPivot = rect.Top;
                        pivotPoint = new SKPoint(xPivot, yPivot);
                    }

                    //Top Left corner clicked and dragged
                    else if ((Math.Abs(point.X - rect.Left) < touchRadius) && (Math.Abs(point.Y - rect.Top) < touchRadius))
                    {
                        isScaling = true;
                        float xPivot = rect.Right;
                        float yPivot = rect.Bottom;
                        pivotPoint = new SKPoint(xPivot, yPivot);
                    }

                    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                    // Common for either pan or scale
                    touchId = args.Id;
                    pressedLocation = point;
                    //pressedMatrix = currentMatrix;
                    break;

                case TouchActionType.Moved:
                    if (!touchId.HasValue || args.Id != touchId.Value)
                        return;

                    SKMatrix matrix = SKMatrix.CreateIdentity();

                    // Translating
                    if (!isScaling)
                    {
                        SKPoint delta = point - pressedLocation;
                        matrix = SKMatrix.CreateTranslation(delta.X, delta.Y);
                        CoordinateLabel.Text = "Top Left Corner (" + (scaleRect.Left + delta.X) + ", " + (scaleRect.Top + delta.Y) +
                            ")\tBottom Right Conrner (" + (scaleRect.Right + delta.X) + ", " + (scaleRect.Bottom + delta.Y) + ")";
                    }
                    // Scaling
                    else
                    {
                        //float scaleVectorX = (point.X - pivotPoint.X) / (pressedLocation.X - pivotPoint.X);
                        //float scaleVectorY = (point.Y - pivotPoint.Y) / (pressedLocation.Y - pivotPoint.Y);

                        //matrix = SKMatrix.CreateScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y);
                        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                        if (pivotPoint.X <= point.X)
                        {
                            if (pivotPoint.Y <= point.Y)
                            {
                                scaleRect = new SKRect(pivotPoint.X, point.Y, point.X, pivotPoint.Y);
                            }
                            else
                            {
                                scaleRect = new SKRect(pivotPoint.X, pivotPoint.Y, point.X, point.Y);
                            }
                        }
                        else
                        {
                            if (pivotPoint.Y <= point.Y)
                            {
                                scaleRect = new SKRect(point.X, point.Y, pivotPoint.X, pivotPoint.Y);
                            }
                            else
                            {
                                scaleRect = new SKRect(point.X, pivotPoint.Y, pivotPoint.X, point.Y);
                            }
                        }
                        CoordinateLabel.Text = "Top Left Corner (" + scaleRect.Left + ", " + scaleRect.Top + ")\tBottom Right Conrner (" + scaleRect.Right + ", " + scaleRect.Bottom + ")";
                        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    }

                    // Concatenate the matrices
                    //SKMatrix.PreConcat(ref matrix, pressedMatrix);
                    currentMatrix = matrix;
                    canvasView.InvalidateSurface();
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    touchId = null;
                    break;
            }

        }
    }
}
