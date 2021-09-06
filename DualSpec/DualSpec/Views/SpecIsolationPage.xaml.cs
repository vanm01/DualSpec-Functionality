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
        SKRect scaleRect = new SKRect(20, 50, 420, 450);

        SKBitmap bitmap;

        SKPoint topLeftCorner = new SKPoint(20, 50);
        SKPoint bottomRightCorner = new SKPoint(120, 150);


        SKPaint circlePaint = new SKPaint();
        int circleRadius = 18;

        SKMatrix currentMatrix = SKMatrix.CreateIdentity();

        // Information for translating and scaling
        long? touchId = null;
        SKPoint pressedLocation;

        // Information for scaling
        bool isScaling;
        SKPoint pivotPoint;

        //Device independant pixel coordinate information
        double bitmapWidth;
        double bitmapHeight;
        double canvasWidth;
        double canvasHeight;


        bool scaled = false;

        public SpecIsolationPage()
        {

            rectPaint.Color = SKColors.Red;
            rectPaint.Style = SKPaintStyle.Stroke;
            rectPaint.StrokeWidth = 5;


            circlePaint.Color = SKColors.Red;
            circlePaint.Style = SKPaintStyle.StrokeAndFill;
            circlePaint.StrokeWidth = 8;


            InitializeComponent();

            if(bitmap != null)
            {
                CoordinateLabel.Text = "Top Left Corner ("
                    + (scaleRect.Left * bitmapWidth / canvasWidth) + ", "
                    + (scaleRect.Bottom * bitmapHeight / canvasHeight) + ")\tBottom Right Conrner ("
                    + (scaleRect.Right * bitmapWidth / canvasWidth) + ", "
                    + (scaleRect.Top * bitmapHeight / canvasHeight) + ")";
            }

        }


        async void LoadImage_Clicked(System.Object sender, System.EventArgs e)
        {

            Stream stream = await DependencyService.Get<IPhotoPickerService>().PickPhotoAsync();
            if (stream != null)
            {
                bitmap = SKBitmap.Decode(stream);

                //Because the canvas is rotated 90 degrees height and width change resepectively
                bitmapWidth = bitmap.Height;
                bitmapHeight = bitmap.Width;
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(System.Object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvasWidth = info.Width;
            canvasHeight = info.Height;

            canvas.Clear();

            canvas.RotateDegrees(90);

            if(bitmap != null)
            {
                canvas.DrawBitmap(bitmap, canvas.LocalClipBounds, null);
                Console.WriteLine(canvasView.Width + ", " + canvasView.Height);
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
            double touchRadius = 30;

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


                        if (bitmap != null)
                        {
                            if (scaled != true)
                            {
                                CoordinateLabel.Text = "Top Left Corner ("
                                + ((scaleRect.Left + delta.X) * bitmapWidth / canvasWidth) + ", "
                                + ((scaleRect.Top + delta.Y) * bitmapHeight / canvasHeight) + ")\tBottom Right Conrner ("
                                + ((scaleRect.Right + delta.X) * bitmapWidth / canvasWidth) + ", "
                                + ((scaleRect.Bottom + delta.Y) * bitmapHeight / canvasHeight) + ")";
                            }

                            else
                            {
                                CoordinateLabel.Text = "Top Left Corner ("
                                + ((scaleRect.Left + delta.X) * bitmapWidth / canvasWidth) + ", "
                                + ((scaleRect.Bottom + delta.Y) * bitmapHeight / canvasHeight) + ")\tBottom Right Conrner ("
                                + ((scaleRect.Right + delta.X) * bitmapWidth / canvasWidth) + ", "
                                + ((scaleRect.Top + delta.Y) * bitmapHeight / canvasHeight) + ")";
                            }
                        }

                    }

                    // Scaling
                    else
                    {
                        scaled = true;

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

                        if(bitmap != null)
                        {

                            CoordinateLabel.Text = "Top Left Corner ("
                            + (scaleRect.Left * bitmapWidth / canvasWidth) + ", "
                            + (scaleRect.Bottom * bitmapHeight / canvasHeight) + ")\tBottom Right Conrner ("
                            + (scaleRect.Right * bitmapWidth / canvasWidth) + ", "
                            + (scaleRect.Top * bitmapHeight / canvasHeight) + ")";

                        }
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
