using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp
{
    public interface IDataTreatmentUnit
    {
        float[] Treat(float[] data, params object[] parameters);
    }
}
