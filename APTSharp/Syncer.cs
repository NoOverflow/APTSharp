using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace APTSharp
{
    public enum SyncType
    {
        SYNC_A,
        SYNC_B
    }
    public static class Syncer
    {
        private static readonly int[] SYNC_A = { -1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1 };

        private const float MIN_FIDELITY = 6.0f;

        private static bool SampleToBool(float sample)
        {
            return sample > 0.5f;
        }

        public static (SyncType type, int index) GetNextSync(ref float[] samples, int startPos, int range)
        {
            float fid = 0;
            (int index, float value) bestShot = (-1, -1.0f);
            float average = samples.Average();

            for (int i = startPos; i < startPos + range && i < samples.Length; i++)
            { 
                for (int j = 0; j < SYNC_A.Length && (i +j) < samples.Length; j++)
                {
                    var correctedSample = samples[i + j] - average;

                    fid += correctedSample * SYNC_A[j];
                }
                if (fid > MIN_FIDELITY && fid > bestShot.value)
                {
                    bestShot.value = (float)fid;
                    bestShot.index = i;
                }
                fid = 0.0f;
            }
            return (SyncType.SYNC_A, bestShot.index);
        }
    }
}
