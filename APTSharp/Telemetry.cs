using APTSharp.Enhance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace APTSharp
{
    public enum WedgeId
    {
        UNKNOWN = -1,
        WMI1 = 0,
        WMI2 = 1,
        WMI3 = 2,
        WMI4 = 3,
        WMI5 = 4,
        WMI6 = 5,
        WMI7 = 6,
        WMIW = 7, // Wedge Modulation Index White (Full white) 
        WMIZ = 8, // Wedge Modulation Index Zero (Full black) 
        TT1 = 9,  // Thermistor 1 Temperature
        TT2 = 10,  // Thermistor 1 Temperature
        TT3 = 11,  // Thermistor 1 Temperature
        TT4 = 12,  // Thermistor 1 Temperature
        PT = 13,   // Patch temp 
        BS = 14,   // Back scan
        CID = 15,  // Channel Identifier
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

        /// <summary>
        /// A telemetry wedge height
        /// </summary>
        public const int TELEMETRY_HEIGHT = 8;

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

        private WedgeId CurrentWedge = WedgeId.UNKNOWN;
        private double GetBlackBodyTemperature(byte sensorValue, float[] coeffs)
        {
            int corrected10bSV = sensorValue << 2;

            return coeffs[0] +
                (coeffs[1] * corrected10bSV) +
                (coeffs[2] * Math.Pow(corrected10bSV, 2)) +
                (coeffs[3] * Math.Pow(corrected10bSV, 3)) +
                (coeffs[4] * Math.Pow(corrected10bSV, 4));
        }

        private (WedgeId wai, int y) FindReferencePoint(ref Bitmap frame)
        {
            bool averageSet = false;
            byte oldAverage = 0;

            for (int y = 0; y < frame.Height; y++)
            {
                int sum = 0;
                byte average = 0;

                for (int x = frame.Width - 46; x < frame.Width - 1; x++)
                {
                    sum += frame.GetPixel(x, y).R;
                }
                average = (byte)(sum / TELEMETRY_WIDTH);
                if (!averageSet)
                {
                    oldAverage = average;
                    averageSet = true;
                }
                // Console.WriteLine("Average line {0}: {1}", y, average);
                if (oldAverage - average > 120)
                    return (WedgeId.WMIZ, y);
                oldAverage = average;
            }
            throw new Exception("No reference point could be found. Can't get telemetry");
        }
        
        /// <summary>
        /// This telemetry reader assumes that the satellite didn't change settings over all recorded frames
        /// </summary>
        /// <param name="frame"></param>
        public void ReadTelemetry(ref Bitmap frame)
        {
            byte supposedWhite = 0;
            byte supposedBlack = 255;
            var reference = FindReferencePoint(ref frame);

            // How many wedges up
            int upwedges = (reference.y - 1) / TELEMETRY_HEIGHT;
            int yOffset = (reference.y - 1) % TELEMETRY_HEIGHT;
            int currentWedgeLine = 0;

            for (int y = yOffset; y < frame.Height - 1; y++)
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
                if (++currentWedgeLine % TELEMETRY_HEIGHT == 0)
                {
                    Console.WriteLine("New wedge starts at {0}", y + 1);
                    currentWedgeLine = 0;
                }
            }
            Console.WriteLine("------------");
            Console.WriteLine("Supposed white {0}", supposedWhite);
            Console.WriteLine("Supposed black {0}", supposedBlack);
            ColorCorrection.ColorCorrectBitmap(ref frame, supposedWhite, supposedBlack);
        }
    }
}
