using APTSharp.Enhance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace APTSharp
{
    public enum WedgeId
    {
        WMI1, 
        WMI2,
        WMI3,
        WMI4,
        WMI5,
        WMI6,
        WMI7,
        WMIW, // Wedge Modulation Index White (Full white) 
        WMIZ, // Wedge Modulation Index Zero (Full black) 
        TT1,  // Thermistor 1 Temperature
        TT2,  // Thermistor 1 Temperature
        TT3,  // Thermistor 1 Temperature
        TT4,  // Thermistor 1 Temperature
        PT,   // Patch temp 
        BS,   // Back scan
        CID,  // Channel Identifier
    }

    public enum SatelliteId
    {
        NOAA_15,
        NOAA_18,
        NOAA_19
    }

    

    public struct TelemetryWedge
    {
        public WedgeId Id { get; set; }
        public int TheoricalContrast { get; set; }

    }

    public class TelemetryReader
    {
        /// <summary>
        /// A telemetry line width
        /// </summary>
        public const int TELEMETRY_WIDTH = 45;

        public static readonly Dictionary<SatelliteId, float[][]> PTR_COEFFS = new Dictionary<SatelliteId, float[][]>()
        {
            {
                SatelliteId.NOAA_19,
                new float[][] {
                    new float[]
                    {
                        276.6067f,
                        0.051111f,
                        1.405783e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.6119f,
                        0.051090f,
                        1496037e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.6311f,
                        0.051033f,
                        1.496990E-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.6268f,
                        0.051058f,
                        1.493110e-06f,
                        0.0f,
                        0.0f
                    }
                }
            },
            {
                SatelliteId.NOAA_18,
                new float[][]
                {
                    new float[]
                    {
                        276.601f,
                        0.05090f,
                        1.657e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.683f,
                        0.05101f,
                        1.482e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.565f,
                        0.05117f,
                        1.313e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.615f,
                        0.05103f,
                        1.4841e-06f,
                        0.0f,
                        0.0f
                    }
                }
            },
            {
                SatelliteId.NOAA_15,
                new float[][]
                {
                    new float[]
                    {
                        276.60157f,
                        0.051045f,
                        1.36328E-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.62531f,
                        0.050909f,
                        1.47266e-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.67413f,
                        0.050907f,
                        1.47656E-06f,
                        0.0f,
                        0.0f
                    },
                    new float[]
                    {
                        276.59258f,
                        0.050966f,
                        1.47656e-06f,
                        0.0f,
                        0.0f
                    }
                }
            }
        };

        private double GetBlackBodyTemperature(byte sensorValue, float[] coeffs)
        {
            int corrected10bSV = sensorValue << 2;

            return coeffs[0] +
                (coeffs[1] * corrected10bSV) +
                (coeffs[2] * Math.Pow(corrected10bSV, 2)) +
                (coeffs[3] * Math.Pow(corrected10bSV, 3)) +
                (coeffs[4] * Math.Pow(corrected10bSV, 4));
        }

        public void ReadTelemetry(ref Bitmap frame)
        {
            byte supposedWhite = 0;
            byte supposedBlack = 255;

            for (int y = 0; y < frame.Height - 1; y++)
            {
                int sum = 0;
                byte average = 0;

                for (int x = frame.Width - 46; x < frame.Width - 1; x++)
                {
                    sum += frame.GetPixel(x, y).R;
                }
                average = (byte)(sum / TELEMETRY_WIDTH);
                if (average > supposedWhite)
                    supposedWhite = average;
                if (average < supposedBlack)
                    supposedBlack = average;
                Console.WriteLine("Average line {0}: {1}", y, average);
            }
            Console.WriteLine("------------");
            Console.WriteLine("Supposed white {0}", supposedWhite);
            Console.WriteLine("Supposed black {0}", supposedBlack);
            ColorCorrection.ColorCorrectBitmap(ref frame, supposedWhite, supposedBlack);
        }
    }
}
