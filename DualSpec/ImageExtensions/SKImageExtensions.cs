using System;
using System.Collections.Generic;
using SkiaSharp;

namespace DualSpec
{
    public static class SKImageExtensions
    {

        public static unsafe SKPixmap SKImageToPixmap(this SKImage image)
        {

            SKPixmap pixmap = image.PeekPixels();

            return pixmap;

        }

        public static unsafe void IntensityAtPixel(this SKImage processImage, int pixelCoordinates)
        {

            SKPixmap pixmap = processImage.PeekPixels();
            byte* bmpPtr = (byte*)pixmap.GetPixels().ToPointer();
            int width = processImage.Width;
            int height = processImage.Height;
            byte* tempPtr;

            for (int row = 0; row < height; row++)
            {

                List <double> intensityList = new List <double>();


                for (int column = 0; column < width; column ++)
                {
                    

                    tempPtr = bmpPtr;
                    byte red = *bmpPtr++;
                    byte green = *bmpPtr++;
                    byte blue = *bmpPtr++;
                    byte alpha = *bmpPtr++;

                    double intensity = (0.299 * red + 0.587 * green + 0.114 * blue);

                    if(row == pixelCoordinates)
                    {
                        intensityList.Add(intensity);
                    }
                    

                }

                if(row == pixelCoordinates)
                {
                    double average = 0;

                    for(int i = 0; i < intensityList.Count; i++)
                    {
                        average += intensityList[i];
                    }

                    average = average / (intensityList.Count);

                    Console.WriteLine(average);
                }

            }

        }
        
    }
}
