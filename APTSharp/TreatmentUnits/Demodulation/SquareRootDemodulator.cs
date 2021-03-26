using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.TreatmentUnits.Demodulation
{
    public class SquareRootDemodulator : ITreatmentUnit
    {
        public float[] Treat(ref float[] data, params dynamic[] args)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (float)Math.Sqrt(Math.Pow(data[i], 2));
            return (data);
        }
    }
}
