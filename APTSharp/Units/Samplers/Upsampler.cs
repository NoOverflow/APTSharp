using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.Units.Demodulation
{
    public class Upsampler : IDataTreatmentUnit
    {
        public float[] Treat(float[] data, params object[] parameters)
        {
            int holder = 0;
            int factor = (int)parameters[0];
            List<float> ret = new List<float>();

            for (int i = 0; i < data.Length;) {
                if (holder == 0) {
                    ret.Add(data[i]);
                    i++;
                } else {
                    ret.Add(0f);
                }
                holder = (holder + 1) % factor;
            }
            return (ret.ToArray());
        }
    }
}
