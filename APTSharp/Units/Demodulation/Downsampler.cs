using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.Units.Demodulation
{
    public class Downsampler : IDataTreatmentUnit
    {
        public float[] Treat(float[] data, params object[] parameters)
        {
            List<float> ret = new List<float>();
            int factor = (int)parameters[0];
            float holder = 0;

            for (int i = 0; i < data.Length; i += factor, holder = 0) {
                for (int j = 0; j < factor && (i + j) < data.Length; j++)
                    holder += data[i + j];
                ret.Add(holder / (float)factor);
            }
            return (ret.ToArray());
        }
    }
}
