﻿using System;
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

                    // We clamp the values just to be sure
                    newColorByte = (byte)(newColor > 255 ? 255 : newColor);
                    newColorByte = (byte)(newColor < 0 ? 0 : newColorByte);
                    image.SetPixel(x, y, Color.FromArgb(newColorByte, newColorByte, newColorByte));
                }
            }
        }
    }
}
