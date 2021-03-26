using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.TreatmentUnits.Sampling
{
    class Downsampler : ITreatmentUnit
    {
        public float[] Treat(ref float[] data, params dynamic[] args)
        {
            float factor = (float)args[0];
            float currentIndex = 0.0f;
            float[] ret = new float[(int)Math.Floor(data.Length / factor)];

            for (int i = 0; i < ret.Length; i++, currentIndex += factor)
            {
                ret[i] = data[(int)Math.Floor(currentIndex)];
            }
            return ret;
        }
    }
}
