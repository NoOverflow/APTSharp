using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.TreatmentUnits.Filter
{
    class FIRLowFilter : ITreatmentUnit
    {
        private float[] GetFIRCoeffs(int inFreq, int halfAmplFreq, int length)
        {
            if (length % 2 == 0)
                length++;
            float freq = (float)halfAmplFreq / inFreq;
            var coefs = new float[length];
            var center = Math.Floor((double)length / 2);
            double sum = 0.0f;
            double val = 0.0f;

            for (var i = 0; i < length; ++i)
            {
                
                if (i == center)
                {
                    val = 2 * Math.PI * freq;
                }
                else
                {
                    var angle = 2 * Math.PI * (i + 1) / (length + 1);
                    val = Math.Sin(2 * Math.PI * freq * (i - center)) / (i - center);
                    val *= 0.42 - 0.5 * Math.Cos(angle) + 0.08 * Math.Cos(2 * angle);
                }
                sum += val;
                coefs[i] = (float)val;
            }
            for (var i = 0; i < length; ++i)
            {
                coefs[i] /= (float)sum;
            }
            return coefs;
        }

        private float[] coeffs = null;

        public float[] Treat(ref float[] data, params dynamic[] parameters)
        {
            int inputFreq = parameters[0];
            int halfAmplFreq = parameters[1];
            int kerlen = parameters[2];
            coeffs = (coeffs == null ? GetFIRCoeffs(inputFreq, halfAmplFreq, kerlen) : coeffs);
            List<float> ret = new List<float>();
            float holder = 0.0f;

            for (int i = 0; i < data.Length; i++)
            {
                holder = 0.0f;
                for (int j = 0; j < coeffs.Length && (j + i) < data.Length; j++)
                {
                    holder += coeffs[j] * data[j + i];
                }
                ret.Add(holder);
            }
            return ret.ToArray();
        }
    }
}
