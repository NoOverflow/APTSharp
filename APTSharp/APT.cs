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
        public static int Samplerate = 11025;

        /// <summary>
        /// Treatment units, these will be executed in order with the arguments provided (This process is described in 'Process.txt').
        /// </summary>
        private static (ITreatmentUnit unit, dynamic args)[] TreatmentUnits =
        {
            (new SquareRootDemodulator(), null),
            (new FIRLowFilter(), new dynamic[] { Samplerate, 1200, 50}),
            (new Normalizer(), null),
        };

        private (float[] samples, double seconds) ReadWAVData(string path)
        {
            AudioFileReader reader = new AudioFileReader(path);
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 4];
            
            isp.Read(buffer, 0, buffer.Length);
            
            Samplerate = reader.WaveFormat.SampleRate;
            return (buffer, (reader.TotalTime.TotalSeconds + 1));
        }

        public APTData Parse(string path)
        {
            APTData ret = new APTData();
            var wavData = ReadWAVData(path);
            float[] samples = wavData.samples;
            Bitmap fullImage = new Bitmap(2080, (int)(Math.Floor(samples.Length / (float)(Samplerate / 2)) - 1));
            byte grayscaleValue = 0;

            foreach (var pair in TreatmentUnits)
                samples = pair.unit.Treat(ref samples, pair.args);
            var syncResult = Syncer.GetNextSync(ref samples, 0, Samplerate * 10);
            float[] lineData = new float[Samplerate / 2 + 1];
            Downsampler downsampler = new Downsampler();
            FIRLowFilter fIRLowFilter = new FIRLowFilter();
            int syncHolder = syncResult.index;

            for (int yLine = 0; yLine < fullImage.Height; yLine++)
            {
                if (syncHolder + Samplerate / 2 + 1 >= samples.Length)
                    break;
                Array.Copy(samples, syncHolder, lineData, 0, Samplerate / 2 + 1); 
                var downsampledData = downsampler.Treat(ref lineData, new dynamic[] { Samplerate / (float)4160 });
                for (int x = 0; x < fullImage.Width; x++)
                {
                    grayscaleValue = (byte)(downsampledData[x] * 255.0f);
                    fullImage.SetPixel(x, yLine, Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue));
                }
                syncResult = Syncer.GetNextSync(ref samples, syncHolder + (Samplerate / 2) - 20, 40);
                if (syncResult.index < 0)
                    syncHolder += Samplerate / 2;
                else
                    syncHolder = syncResult.index;
            }
            ret.FullImage = fullImage;
            ret.ImageA = new Bitmap(fullImage.Width / 2, fullImage.Height);
            ret.ImageB = new Bitmap(fullImage.Width / 2, fullImage.Height);
            for (int y = 0; y < fullImage.Height; y++)
            {
                for (int x = 0; x < 1040; x++)
                {
                    ret.ImageA.SetPixel(x, y, fullImage.GetPixel(x, y));
                    ret.ImageB.SetPixel(x, y, fullImage.GetPixel(x + 1040, y));
                }
            }
            // Reading telemetry will enhance the image quality, this is because it needs to get precise values to get temperature data
            new TelemetryReader().ReadTelemetry(ref ret.ImageA);
            return ret;
        }
    }
}
