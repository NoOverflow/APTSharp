using APTSharp.Enhance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace APTSharp
{
    public enum WedgeId
    {
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
        TT2 = 10,  // Thermistor 2 Temperature
        TT3 = 11,  // Thermistor 3 Temperature
        TT4 = 12,  // Thermistor 4 Temperature
        PT = 13,   // Patch temp 
        BS = 14,   // Back scan
        CID = 15  // Channel Identifier
    }

    public enum ChannelId
    {
        ID1,
        ID2,
        ID3A,
        ID3B,
        ID4,
        ID5
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

    public struct TelemetryFrame
    {
        /// <summary>
        /// The channel Id, used to determine the instrument used
        /// </summary>
        ChannelId ChId;

        /// <summary>
        /// The patch temperature measured in Kelvin
        /// </summary>
        double PatchTemperature;
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

        public static readonly Dictionary<ChannelId, byte> CHANNEL_ID_TO_DIGV = new Dictionary<ChannelId, byte>()
        {
            { ChannelId.ID1, 31 },
            { ChannelId.ID2, 63 },
            { ChannelId.ID3A, 95 },
            { ChannelId.ID3B, 191 },
            { ChannelId.ID4, 127 },
            { ChannelId.ID5, 159 }
        };

        /// <summary>
        /// Get the patch temperature from the AVHRR 8-bit value
        /// </summary>
        /// <returns>The temperature in Kelvin</returns>
        private double GetPatchTemperature(byte sensorValue)
        {
            return 0.124f * (float)sensorValue + 90.113f;
        }

        /// <summary>
        /// Get the channel ID from the AVHRR 8-bit value
        /// </summary>
        /// <returns>The determined channel ID</returns>
        private ChannelId GetChannelID(byte sensorValue)
        {
            ChannelId ret = ChannelId.ID1;
            byte offset = 255;

            foreach (var corress in CHANNEL_ID_TO_DIGV)
            {
                if (Math.Abs(corress.Value - sensorValue) < offset)
                {
                    ret = corress.Key;
                    offset = (byte)Math.Abs(corress.Value - sensorValue);
                }
            }
            return ret;
        }

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
                Console.WriteLine("Average line {0}: {1}", y, average);
                if (oldAverage - average > 120)
                    return (WedgeId.WMIZ, y);
                oldAverage = average;
            }
            throw new Exception("No reference point could be found. Can't get telemetry");
        }

        private byte GetWedgeValue(Dictionary<WedgeId, (int sum, int count)> WedgeValues, WedgeId wedgeId)
        {
            return ((byte)(WedgeValues[wedgeId].sum / WedgeValues[wedgeId].count));
        }

        private void AddTelemetryValue(ref Dictionary<WedgeId, (int sum, int count)> WedgeValues, WedgeId wedgeId, byte average)
        {
            // TODO add incorrect value detection
            if (wedgeId == WedgeId.WMIW)
            {
                Console.WriteLine("Got white line average {0}", average);
            }
            if (wedgeId == WedgeId.WMIZ)
            {
                Console.WriteLine("Got black line average {0}", average);
            }
            WedgeValues[wedgeId] = (WedgeValues[wedgeId].sum + average, WedgeValues[wedgeId].count + 1);
        }

        private Dictionary<WedgeId, (int sum, int count)> ReadWedges(ref Bitmap frame)
        {
            var reference = FindReferencePoint(ref frame);

            // How many wedges up
            int upwedges = reference.y / TELEMETRY_HEIGHT;
            int yOffset = reference.y % TELEMETRY_HEIGHT;
            int currentWedgeLine = 0;
            WedgeId currentWedge = (WedgeId)((int)WedgeId.WMIZ - (upwedges % 16));
            Dictionary<WedgeId, (int sum, int count)> WedgeValues = new Dictionary<WedgeId, (int sum, int count)>();

            for (int i = 0; i < 16; i++)
            {
                WedgeValues[(WedgeId)i] = (0, 0);
            }
            for (int y = yOffset; y < frame.Height - 1; y++)
            {
                int sum = 0;
                byte average = 0;

                for (int x = frame.Width - 46; x < frame.Width - 1; x++)
                {
                    sum += frame.GetPixel(x, y).R;
                }
                average = (byte)(sum / TELEMETRY_WIDTH);
                //Console.WriteLine("Current Y = {0}", y);
                AddTelemetryValue(ref WedgeValues, currentWedge, average);
                currentWedgeLine++;
                if (currentWedgeLine != 0 && currentWedgeLine % (TELEMETRY_HEIGHT) == 0)
                {
                    currentWedge = (WedgeId)(((int)currentWedge + 1) % 16);
                    //Console.WriteLine("New wedge starts at {0}, it is a {1}", y + 1, Enum.GetName(typeof(WedgeId), currentWedge));
                    currentWedgeLine = 0;
                }
            }
            return WedgeValues;
        }

        /// <summary>
        /// This telemetry reader assumes that the satellite didn't change settings over all recorded frames
        /// </summary>
        /// <param name="frame"></param>
        public void ReadTelemetry(ref Bitmap frame)
        {
            byte supposedWhite = 0;
            byte supposedBlack = 255;
            byte supposedPt = 255;
            byte supposedID = 255;
            var WedgeValues = ReadWedges(ref frame);

            supposedWhite = GetWedgeValue(WedgeValues, WedgeId.WMIW);
            supposedBlack = GetWedgeValue(WedgeValues, WedgeId.WMIZ);
            supposedPt = GetWedgeValue(WedgeValues, WedgeId.PT);
            Console.WriteLine("Patch temperature before correction: " + GetPatchTemperature(supposedPt) + "K");
            ColorCorrection.ColorCorrectBitmap(ref frame, supposedWhite, supposedBlack);
            WedgeValues = ReadWedges(ref frame);
            supposedPt = GetWedgeValue(WedgeValues, WedgeId.PT);
            supposedID = GetWedgeValue(WedgeValues, WedgeId.CID);
            Console.WriteLine("Patch temperature after correction: " + GetPatchTemperature(supposedPt) + "K");
            Console.WriteLine("------------");
            Console.WriteLine("Supposed white {0}", supposedWhite);
            Console.WriteLine("Supposed black {0}", supposedBlack);
            Console.WriteLine("Supposed ChannelID For this image {0}", GetChannelID(supposedID));
        }
    }
}
