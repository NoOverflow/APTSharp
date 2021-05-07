using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace APTSharp.Enhance
{
    public static class ColorCorrection
    {
        public static void ColorCorrectBitmap(ref Bitmap image, double A, double B)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    byte currentColor = image.GetPixel(x, y).R;
                    int newColor = (int)Math.Ceiling(((currentColor - A) / B));
                    byte newColorByte = 0;

                    Math.Clamp(newColor, 0, 255);
                    newColorByte = (byte)(newColor > 255 ? 255 : newColor);
                    newColorByte = (byte)(newColor < 0 ? 0 : newColorByte);
                    image.SetPixel(x, y, Color.FromArgb(newColorByte, newColorByte, newColorByte));
                }
            }
        }


        public static void Despeckle(ref Bitmap image)
        {

        }

        public static void FalseColors(ref Bitmap imageA, ref Bitmap imageB)
        {
            byte watTreshold = 50;
            byte cldTreshold = 110;
            byte vegTreshold = 140;

            byte r, g, b = 0;

            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 84; x < 994; x++)
                {
                    Color c = imageA.GetPixel(x, y);
                    Color cir = imageB.GetPixel(x, y);

                    r = c.R;
                    g = c.G;
                    b = c.B;

                    if (r < watTreshold) {
                        r = ((byte)(8.0 + r * 0.2));
                        g = ((byte)(20.0 + r * 1.0));
                        b = ((byte)(50.0 + r * 0.75));
                    } else if (cir.R > cldTreshold) {
                        r = (byte)((cir.R + r) * 0.5);
                        g = (byte)((cir.G + g) * 0.5);
                        b = (byte)((cir.B + b) * 0.5);
                    } else if (r < vegTreshold) {
                        r = (byte)(r * 0.8);
                        g = (byte)(g * 0.9);
                        b = (byte)(b * 0.6);
                    } else if (r < cldTreshold) {
                        r = (byte)(r * 1.0);
                        g = (byte)(g * 0.9);
                        b = (byte)(b * 0.7);
                    } else
                    {
                        r = (byte)(r * 1.0);
                        g = (byte)(g * 0.8);
                        b = (byte)(b * 0.5);
                    }
                    imageA.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }
    }
}
