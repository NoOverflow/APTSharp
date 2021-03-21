using APTSharp.Demodulation;
using APTSharp.Sound;
using APTSharp.Units.Demodulation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace APTSharp
{
    public struct APTData
    {
        /// <summary>
        /// The total amount of lines contained in the subimages
        /// </summary>
        public long TotalLines;

        /// <summary>
        /// The total amount of seconds of the sound file
        /// </summary>
        public long TotalSeconds;

        public APTFrame ImageRes; // temporary
    }

    public struct APTFrame
    {
        public Bitmap ImageA;
        public Bitmap ImageB;
    }

    public class APT
    {
        public static (IDataTreatmentUnit treatUnit, object[] args)[] TreatmentUnits =
        {
            (new SquareRootDemodulator(), new object[] { }),
            (new Upsampler(), new object[] { 13}),
            (new Downsampler(), new object[] { 150}),
        };

        private static Bitmap CurrentImage = null;

        private static Point CurrentPoint = new Point(0, 0);

        private static float MaxSampleValue = 0;

        /// <summary>
        /// The current frame being treated, the frame 0 is not really useful as we can't synchronize properly to it
        /// </summary>
        private static int CurrentFrame = 0;

        public static APTData ParseAPTFile(string wavPath)
        {
            APTData ret = new APTData();
            AudioFileReader reader = new AudioFileReader(wavPath);
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 2];

            
            isp.Read(buffer, 0, buffer.Length);
            ret.TotalSeconds = (long)Math.Ceiling(reader.TotalTime.TotalSeconds);
            ret.TotalLines = ret.TotalSeconds * 2;
            CurrentImage = new Bitmap(2080, (int)ret.TotalLines * 10);
            for (int y = 0; y < CurrentImage.Height; y++)
                for (int x = 0; x < CurrentImage.Width; x++)
                    CurrentImage.SetPixel(x, y, Color.Red);
            foreach (var pair in TreatmentUnits) {
                buffer = pair.treatUnit.Treat(buffer, pair.args);
            }
            Array.Resize(ref buffer, Array.FindLastIndex(buffer, x => x != 0.0f) + 1);
            var syncer = new Syncer(buffer);

            syncer.OnSyncA += Syncer_OnSyncA;
            syncer.OnSyncB += Syncer_OnSyncB;
            syncer.OnSample += Syncer_OnSample;
            syncer.Synchronize();
            ret.ImageRes.ImageA = new Bitmap(1040, CurrentPoint.Y);
            ret.ImageRes.ImageB = new Bitmap(1040, CurrentPoint.Y);
            for (int y = 0; y < CurrentPoint.Y; y++)
                for (int x = 0; x < 2080; x++)
                {
                    if (x < 1040)
                        ret.ImageRes.ImageA.SetPixel(x, y, CurrentImage.GetPixel(x, y));
                    else
                        ret.ImageRes.ImageB.SetPixel(x - 1040, y, CurrentImage.GetPixel(x, y));            
                }
            // SUtils.ModifyBitmapContrast(ref ret.ImageRes.ImageA, 20.0f);
            // Utils.ModifyBitmapContrast(ref ret.ImageRes.ImageB, 20.0f);
            // ret.ImageRes.RotateFlip(RotateFlipType.Rotate180FlipNone);
            return (ret);
        }

        private static void Syncer_OnSample(float sample)
        {
            byte sampleValue = 0;

            MaxSampleValue = Math.Max(MaxSampleValue, sample);
            MaxSampleValue = MaxSampleValue == 0 ? 0.1f : MaxSampleValue;
            sampleValue = (byte)((byte)(sample / MaxSampleValue * 255.0f));
            sampleValue = (byte)(sampleValue > 255 ? 255 : sampleValue);
            CurrentImage.SetPixel(CurrentPoint.X, CurrentPoint.Y, Color.FromArgb(sampleValue, sampleValue, sampleValue));
            CurrentPoint.X++;
            if (CurrentPoint.X >= 2080)
            {
                CurrentPoint.X = 0;
                CurrentPoint.Y++;
            }
        }

        private static void Syncer_OnSyncB(float sample)
        {
            int sampleValue = 0;

            MaxSampleValue = Math.Max(MaxSampleValue, sample);
            MaxSampleValue = MaxSampleValue == 0 ? 0.1f : MaxSampleValue;
            sampleValue = (byte)((byte)(sample / MaxSampleValue * 255.0f) + 40);
            sampleValue = (byte)(sampleValue > 255 ? 255 : sampleValue);

            // We are going to go the middle of the line, so fill with the remaining sample
            // This is useful for instrumental (Wedges) values, although not ideal cause this shouldn't happen
            if (CurrentPoint.X < (2080 / 2))
            {
                for (; CurrentPoint.X < (2080 / 2); CurrentPoint.X++)
                {
                    CurrentImage.SetPixel(CurrentPoint.X, CurrentPoint.Y, 
                        Color.FromArgb(sampleValue, sampleValue, sampleValue));
                }
            }
            // A Sync B represents the middle of a line, so we go there
            CurrentPoint.X = 2080 / 2;
        }

        private static void Syncer_OnSyncA(float sample)
        {
            // A Sync A represents the beginning of a line, so we go there
            CurrentPoint.X = 0;
        }
    }
}
