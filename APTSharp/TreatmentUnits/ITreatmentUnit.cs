using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp.TreatmentUnits
{
    interface ITreatmentUnit
    {
        float[] Treat(ref float[] data, params dynamic[] args);
    }
}
