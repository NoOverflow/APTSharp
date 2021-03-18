using APTSharp.Demodulation;
using APTSharp.Sound;
using APTSharp.Units.Demodulation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

    }

    public class APT
    {
        public static (IDataTreatmentUnit treatUnit, object[] args)[] TreatmentUnits =
        {
            (new SquareRootDemodulator(), new object[] { }),
            (new Upsampler(), new object[] { 13}),
            (new Downsampler(), new object[] { 150}),
        };

        public static APTData ParseAPTFile(string wavPath)
        {
            APTData ret = new APTData();
            AudioFileReader reader = new AudioFileReader(wavPath);
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 2];

            isp.Read(buffer, 0, buffer.Length);
            ret.TotalSeconds = (long)Math.Ceiling(reader.TotalTime.TotalSeconds);
            ret.TotalLines = ret.TotalSeconds * 2;
            foreach (var pair in TreatmentUnits) {
                buffer = pair.treatUnit.Treat(buffer, pair.args);
            }
            var syncer = new Syncer(buffer);

            syncer.OnSyncA += Syncer_OnSyncA;
            syncer.OnSyncB += Syncer_OnSyncB;
            syncer.Synchronize();
            return (ret);
        }

        private static void Syncer_OnSyncB(float sample)
        {
        }

        private static void Syncer_OnSyncA(float sample)
        {
        }
    }
}
