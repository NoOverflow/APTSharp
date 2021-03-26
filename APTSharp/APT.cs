using APTSharp.TreatmentUnits;
using APTSharp.TreatmentUnits.Demodulation;
using APTSharp.TreatmentUnits.Filter;
using APTSharp.TreatmentUnits.Sampling;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace APTSharp
{
    public struct APTData
    {
        public Bitmap ImageA;
        public Bitmap ImageB;
        public Bitmap FullImage;
    }

    public class APT
    {
        public const int SAMPLERATE = 11025;

        /// <summary>
        /// Treatment units, these will be executed in order with the arguments provided (This process is described in 'Process.txt').
        /// </summary>
        private static (ITreatmentUnit unit, dynamic args)[] TreatmentUnits =
        {
            (new SquareRootDemodulator(), null),
            (new FIRLowFilter(), new dynamic[] { SAMPLERATE, 1200, 50}),
            (new Normalizer(), null),
        };

        private (float[] samples, double seconds) ReadWAVData(string path)
        {
            AudioFileReader reader = new AudioFileReader(path);
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 4];
            
            isp.Read(buffer, 0, buffer.Length);
            return (buffer, (reader.TotalTime.TotalSeconds + 1));
        }

        public APTData Parse(string path)
        {
            APTData ret = new APTData();
            var wavData = ReadWAVData(path);
            float[] samples = wavData.samples;
            Bitmap fullImage = new Bitmap(2080, (int)(Math.Floor(samples.Length / (float)(SAMPLERATE / 2))));
            byte grayscaleValue = 0;

            foreach (var pair in TreatmentUnits)
                samples = pair.unit.Treat(ref samples, pair.args);
            var syncResult = Syncer.GetNextSync(ref samples, 0, SAMPLERATE);
            float[] lineData = new float[SAMPLERATE / 2 + 1];
            Downsampler downsampler = new Downsampler();
            FIRLowFilter fIRLowFilter = new FIRLowFilter();
            int syncHolder = syncResult.index;

            for (int yLine = 0; yLine < fullImage.Height; yLine++)
            {
                if (syncHolder + SAMPLERATE / 2 + 1 >= samples.Length)
                    break;
                Array.Copy(samples, syncHolder, lineData, 0, SAMPLERATE / 2 + 1); 
                var downsampledData = downsampler.Treat(ref lineData, new dynamic[] { SAMPLERATE / (float)4160 });
                for (int x = 0; x < fullImage.Width; x++)
                {
                    grayscaleValue = (byte)(downsampledData[x] * 255.0f);
                    fullImage.SetPixel(x, yLine, Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue));
                }
                syncResult = Syncer.GetNextSync(ref samples, syncHolder + (SAMPLERATE / 2) - 20, 40);
                if (syncResult.index < 0)
                    syncHolder += SAMPLERATE / 2;
                else
                    syncHolder = syncResult.index;
            }
            ret.FullImage = fullImage;
            ret.ImageA = new Bitmap(fullImage.Width / 2, fullImage.Height);
            ret.ImageB = new Bitmap(fullImage.Width / 2, fullImage.Height);
            for (int y = 0; y < fullImage.Height; y++) 
                for (int x = 0; x < 1040; x++)
                {
                    if (x == 1040 - 46 || x == 2080 - 46)
                    {
                        ret.ImageA.SetPixel(x, y, Color.Purple);
                        ret.ImageB.SetPixel(x, y, Color.Purple);
                    } 
                    else
                    {
                        ret.ImageA.SetPixel(x, y, fullImage.GetPixel(x, y));
                        ret.ImageB.SetPixel(x, y, fullImage.GetPixel(x + 1040, y));
                    }
                }
            return ret;
        }
    }
}
