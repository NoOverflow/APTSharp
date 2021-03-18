using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace APTSharp.Demodulation
{
    public class SquareRootDemodulator : IDataTreatmentUnit
    {
        public float[] Treat(float[] data, params object[] parameters)
        {
            float[] ret = data;

            for (int i = 0; i < ret.Length; i++)
                ret[i] = (float)Math.Sqrt(ret[i] * ret[i]);
            return (ret);
        }
    }
}
