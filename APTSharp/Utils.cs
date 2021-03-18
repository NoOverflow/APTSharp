using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp
{
    public static class Utils
    {
        /// <summary>
        /// We consider a sample with a value higher than 0.5 to be a "1" (Or true)
        /// </summary>
        /// <param name="value">The analog sample from the WAV file</param>
        /// <returns>True if the value > 0.5</returns>
        public static bool AnalogValueToBool(float value)
        {
            return (value > 0.5f);
        }
    }
}
