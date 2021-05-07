using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace APTSharp.TreatmentUnits
{
    class Normalizer : ITreatmentUnit
    {
        public float[] Treat(ref float[] data, params dynamic[] args)
        {
            float max = data.Max();            
            float min = data.Min();
            float span = max - min;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - min) / span; 
            }
            return (data);
        }
    }
}
