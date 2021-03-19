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

        public Bitmap ImageRes; // temporary
    }

    public class APT
    {
        public static (IDataTreatmentUnit treatUnit, object[] args)[] TreatmentUnits =
        {
            (new SquareRootDemodulator(), new object[] { }),
            (new Upsampler(), new object[] { 57}),
            (new Downsampler(), new object[] { 150}),
        };

        private static Bitmap CurrentImage = null;

        private static Point CurrentPoint = new Point(0, 0);

        private static float MaxSampleValue = 0;

        public static APTData ParseAPTFile(string wavPath)
        {
            APTData ret = new APTData();
            AudioFileReader reader = new AudioFileReader(wavPath);
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 2];

            
            isp.Read(buffer, 0, buffer.Length);
            ret.TotalSeconds = (long)Math.Ceiling(reader.TotalTime.TotalSeconds);
            ret.TotalLines = ret.TotalSeconds * 2;
            ret.ImageRes = new Bitmap(2080, (int)ret.TotalLines * 10);
            for (int y = 0; y < ret.ImageRes.Height; y++)
                for (int x = 0; x < ret.ImageRes.Width; x++)
                    ret.ImageRes.SetPixel(x, y, Color.Black);
            CurrentImage = ret.ImageRes;
            foreach (var pair in TreatmentUnits) {
                buffer = pair.treatUnit.Treat(buffer, pair.args);
            }
            var syncer = new Syncer(buffer);

            syncer.OnSyncA += Syncer_OnSyncA;
            syncer.OnSyncB += Syncer_OnSyncB;
            syncer.OnSample += Syncer_OnSample;
            syncer.Synchronize();
            return (ret);
        }

        private static void Syncer_OnSample(float sample)
        {
            int sampleValue = 0;

            MaxSampleValue = Math.Max(MaxSampleValue, sample);
            MaxSampleValue = MaxSampleValue == 0 ? 0.1f : MaxSampleValue;
            sampleValue = (int)(sample / MaxSampleValue * 255.0f);
            CurrentImage.SetPixel(CurrentPoint.X, CurrentPoint.Y, Color.FromArgb(sampleValue, sampleValue, sampleValue));
            CurrentPoint.X++;
            if (CurrentPoint.X > 2040)
            {
                CurrentPoint.X = 0;
                CurrentPoint.Y++;
            }
        }

        private static void Syncer_OnSyncB(float sample)
        {
            // A Sync B represents the middle of a line, so we go there
            CurrentPoint.X = 2040 / 2;
        }

        private static void Syncer_OnSyncA(float sample)
        {
            // A Sync A represents the beginning of a line, so we go there
            CurrentPoint.X = 0;
        }
    }
}
