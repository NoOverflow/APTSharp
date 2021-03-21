using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace APTSharp
{
    public static class Utils
    {
        /// <summary>
        /// We consider a sample with a value higher than 0.5 to be a "1" (Or true)
        /// </summary>
        /// <param name="value">The analog sample from the WAV file</param>
        /// <returns>True if the value > 0.5</returns>
        public static bool AnalogValueToBool(float value)
        {
            return (value > 0.5f);
        }

        public static (int up, int down) GetSamplingCoeffs(int inRate, int outRate)
        {
            return (0, 0);
        }

        /// <summary>
        /// Modify the bitmap contrast efficiently (Inspired from https://stackoverflow.com/questions/3115076/adjust-the-contrast-of-an-image-in-c-sharp-efficiently)
        /// </summary>
        /// <param name="image">A reference to the image</param>
        /// <param name="contrast">New contrast</param>
        public static void ModifyBitmapContrast(ref Bitmap image, float contrast)
        {
            contrast = (100.0f + contrast) / 100.0f;
            contrast *= contrast;
            
            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                image.PixelFormat);
            int Height = image.Height;
            int Width = image.Width;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;
                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * contrast) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * contrast) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * contrast) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }
            image.UnlockBits(data);
        }
    }
}
