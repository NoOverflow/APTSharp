using APTSharp.Enhance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

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
        public ChannelId ChId;

        /// <summary>
        /// The patch temperature measured in Kelvin
        /// </summary>
        public double PatchTemperature;

        /// <summary>
        /// The temperature for each pixel measured in Kelvin
        /// </summary>
        public double[] Temperatures;

        /// <summary>
        /// The predicted or given satellite
        /// </summary>
        public SatelliteId Satellite;
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

        private StatusContext StatusContext;

        public static readonly Dictionary<ChannelId, byte> CHANNEL_ID_TO_DIGV = new Dictionary<ChannelId, byte>()
        {
            { ChannelId.ID1, 31 },
            { ChannelId.ID2, 63 },
            { ChannelId.ID3A, 95 },
            { ChannelId.ID3B, 191 },
            { ChannelId.ID4, 127 },
            { ChannelId.ID5, 159 }
        };

        public static readonly Dictionary<WedgeId, byte> WID_TO_DIGV = new Dictionary<WedgeId, byte>()
        {
            { WedgeId.WMI1, 31 },
            { WedgeId.WMI2, 63 },
            { WedgeId.WMI3, 95 },
            { WedgeId.WMI4, 127 },
            { WedgeId.WMI5, 159 },
            { WedgeId.WMI6, 191 },
            { WedgeId.WMI7, 223 },
            { WedgeId.WMIW, 255 },
            { WedgeId.WMIZ, 0 }
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

        private (double A, double B) GetNormalisationCoeffs(Dictionary<WedgeId, (int sum, int count)> WedgeValues)
        {
            this.StatusContext.UpdateCurrentState(Status.GETTING_NORMAL_COEFFS);
            double wm1 = GetWedgeValue(WedgeValues, WedgeId.WMI1);
            double wm2 = GetWedgeValue(WedgeValues, WedgeId.WMI2);
            double wm3 = GetWedgeValue(WedgeValues, WedgeId.WMI3);
            double wm4 = GetWedgeValue(WedgeValues, WedgeId.WMI4);
            double wm5 = GetWedgeValue(WedgeValues, WedgeId.WMI5);
            double wm6 = GetWedgeValue(WedgeValues, WedgeId.WMI6);
            double wm7 = GetWedgeValue(WedgeValues, WedgeId.WMI7);
            double wmw = GetWedgeValue(WedgeValues, WedgeId.WMIW);

            double ewm1 = WID_TO_DIGV[WedgeId.WMI1];
            double ewm2 = WID_TO_DIGV[WedgeId.WMI2];
            double ewm3 = WID_TO_DIGV[WedgeId.WMI3];
            double ewm4 = WID_TO_DIGV[WedgeId.WMI4];
            double ewm5 = WID_TO_DIGV[WedgeId.WMI5];
            double ewm6 = WID_TO_DIGV[WedgeId.WMI6];
            double ewm7 = WID_TO_DIGV[WedgeId.WMI7];
            double ewmw = WID_TO_DIGV[WedgeId.WMIW];

            double SXY = (ewm1 * wm1 + ewm2 * wm2 + ewm3 * wm3 + ewm4 * wm4 + ewm5 * wm5 + ewm6 * wm6 + ewm7 * wm7 + ewmw * wmw);
            double SX = WID_TO_DIGV.Sum(x => x.Value);
            double SXSQ = (Math.Pow(ewm1, 2) + Math.Pow(ewm2, 2) + Math.Pow(ewm3, 2) + Math.Pow(ewm4, 2) + Math.Pow(ewm5, 2) + Math.Pow(ewm6, 2) + Math.Pow(ewm7, 2) + Math.Pow(ewmw, 2));
            double SY = (wm1 + wm2 + wm3 + wm4 + wm5 + wm6 + wm7 + wmw);

            double B = (8 * SXY - (SX * SY)) / (8 * SXSQ - Math.Pow(SX, 2));
            double A = (SY - B * SX) / 8;

            return (A, B);
        }

        private double ComputeBlackBodyTemperature(byte sensorValue, float[] coeffs)
        {
            this.StatusContext.UpdateCurrentState(Status.COMPUTING_BBT);
            // The value is shifted by two as coefficients are given for 10 bits HRPT telemetry
            int corrected10bSV = sensorValue << 2;

            // Equation given in section 7.1.2.4 of the NOAA KLM User guide
            return coeffs[0] +
                (coeffs[1] * corrected10bSV) +
                (coeffs[2] * Math.Pow(corrected10bSV, 2)) +
                (coeffs[3] * Math.Pow(corrected10bSV, 3)) +
                (coeffs[4] * Math.Pow(corrected10bSV, 4));
        }


        // Plank consts
        private const decimal C1 = 1.1910427E-5M; // mW/(m²-sr-cme-4)
        private const decimal C2 = 1.4387752M; // cm-K

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The satellite from which the count is coming from</param>
        /// <param name="cid">The channel ID of the instrument used</param>
        /// <param name="sensorValue">The raw byte value</param>
        /// <param name="bbt">The black body temperature coming from the PRTs</param>
        /// <param name="cs">The average space count</param>
        /// <param name="cbb">The average black body count</param>
        /// <returns></returns>
        private double ComputeTemperature(SatelliteId id, ChannelId cid, byte sensorValue, double bbt, double cs, double cbb, double cra, double crb)
        {
            StatusContext.UpdateCurrentState(Status.COMPUTING_TEMP);
            double ce = sensorValue * 4;

            cs = cs * 4;
            cbb = cbb * 4;

            double nuc = SatelliteConfigs.Configs[id].EFBBT_COEFFS[cid][0];
            double nue = nuc;// 927.0246f; // Absolutely not fucking safe
            double a = SatelliteConfigs.Configs[id].EFBBT_COEFFS[cid][1]; 
            double b = SatelliteConfigs.Configs[id].EFBBT_COEFFS[cid][2]; 
            double ns = SatelliteConfigs.Configs[id].EFBBT_COEFFS[cid][3];  

            double efbbt = a + b * bbt;
            double nbb = ((double)C1 * Math.Pow(nue, 3)) / (Math.Exp((double)C2 * nuc / efbbt) - 1);
            double nlin = ns + (nbb - ns) * ((cs - ce) / (cs - cbb));
            double ncor = SatelliteConfigs.Configs[id].NL_RAD_CORR_COEFFS[cid][0] + 
                          SatelliteConfigs.Configs[id].NL_RAD_CORR_COEFFS[cid][1] * nlin + 
                          SatelliteConfigs.Configs[id].NL_RAD_CORR_COEFFS[cid][2] * Math.Pow(nlin, 2);
            double ne = nlin + ncor;
            double emte = ((double)C2 * nuc) / (Math.Log(1 + ((double)C1 * Math.Pow(nuc, 3) / ne)));
            double te = (emte - a) / b;

            return te + 10.0f;
        }

        /// <summary>
        /// Find a reference point to locate wedge starts
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
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

                // TODO REPLACE THIS ARBITRARY VALUE
                if (oldAverage - average > 60)
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

        /// <summary>
        /// Fill the temperature buffer to prevent recalculation
        /// </summary>
        /// <param name="WedgeValues">The wedge values</param>
        /// <param name="frame">The current frame</param>
        /// <param name="tel">The telemetry structure to fill</param>
        /// <returns></returns>
        private double[] GetTemperatures(ref Dictionary<WedgeId, (int sum, int count)> WedgeValues, ref Bitmap frame, ref TelemetryFrame tel, double cra, double crb)
        {
            double tempT1 = ComputeBlackBodyTemperature(GetWedgeValue(WedgeValues, WedgeId.TT1), SatelliteConfigs.Configs[tel.Satellite].PTR_COEFFS[0]);
            double tempT2 = ComputeBlackBodyTemperature(GetWedgeValue(WedgeValues, WedgeId.TT2), SatelliteConfigs.Configs[tel.Satellite].PTR_COEFFS[1]);
            double tempT3 = ComputeBlackBodyTemperature(GetWedgeValue(WedgeValues, WedgeId.TT3), SatelliteConfigs.Configs[tel.Satellite].PTR_COEFFS[2]);
            double tempT4 = ComputeBlackBodyTemperature(GetWedgeValue(WedgeValues, WedgeId.TT4), SatelliteConfigs.Configs[tel.Satellite].PTR_COEFFS[3]);
            double cbb = ((int)GetWedgeValue(WedgeValues, WedgeId.TT1) + GetWedgeValue(WedgeValues, WedgeId.TT2) + GetWedgeValue(WedgeValues, WedgeId.TT3) + GetWedgeValue(WedgeValues, WedgeId.TT4)) / 4;
            double avBlackBodyTemperature = (tempT1 + tempT2 + tempT3 + tempT4) / 4;
            double[] ret = new double[frame.Width * frame.Height];

            for (int y = 0; y < frame.Height; y++)
            {
                for (int x = 0; x < frame.Width; x++)
                {
                    ret[y * frame.Width + x] = ComputeTemperature(tel.Satellite, tel.ChId, frame.GetPixel(x, y).R, avBlackBodyTemperature, SpaceCount, cbb, cra, crb); 
                }
            }
            return ret;
        }

        private static byte SpaceCount = 0;

        private Dictionary<WedgeId, (int sum, int count)> ReadWedges(ref Bitmap frame)
        {
            StatusContext.UpdateCurrentState(Status.READING_RAW_WEDGES);
            var reference = FindReferencePoint(ref frame);
            int upwedges = reference.y / TELEMETRY_HEIGHT;
            int yOffset = reference.y % TELEMETRY_HEIGHT;
            int currentWedgeLine = 0;
            WedgeId currentWedge = (WedgeId)((int)WedgeId.WMIZ - (upwedges % 16));
            Dictionary<WedgeId, (int sum, int count)> WedgeValues = new Dictionary<WedgeId, (int sum, int count)>();

            long spSum = 0;
            long spDiv = 0;

            for (int i = 0; i < 16; i++)
            {
                WedgeValues[(WedgeId)i] = (0, 0);
            }
            for (int y = yOffset; y < frame.Height - 1; y++)
            {
                int sum = 0;
                byte average = 0;
                
                for (int x = 40; x < 45; x++)
                {
                    byte spValue = frame.GetPixel(x, y).R;

                    // We substract minute values, as this will only be really relevant for B frames
                    // TODO Get rid of this arbitrary value
                    if (spValue > 50)
                    {
                        spSum += spValue;
                        spDiv++;
                    }
                }
                for (int x = frame.Width - 46; x < frame.Width - 1; x++)
                {
                    sum += frame.GetPixel(x, y).R;
                }
                average = (byte)(sum / TELEMETRY_WIDTH);
                AddTelemetryValue(ref WedgeValues, currentWedge, average);
                currentWedgeLine++;
                if (currentWedgeLine != 0 && currentWedgeLine % (TELEMETRY_HEIGHT) == 0)
                {
                    currentWedge = (WedgeId)(((int)currentWedge + 1) % 16);
                    //Console.WriteLine("New wedge starts at {0}, it is a {1}", y + 1, Enum.GetName(typeof(WedgeId), currentWedge));
                    currentWedgeLine = 0;
                }
            }
            SpaceCount = (byte)(spSum / spDiv);
            return WedgeValues;
        }


        /// <summary>
        /// This telemetry reader assumes that the satellite didn't change settings over all recorded frames
        /// </summary>
        /// <param name="frame"></param>
        public TelemetryFrame ReadTelemetry(ref StatusContext statusContext, SatelliteId id, ref Bitmap frame, bool getTemperature)
        {
            this.StatusContext = statusContext;
            TelemetryFrame ret = new TelemetryFrame();
            var WedgeValues = ReadWedges(ref frame);
            var NormalisationCoeffs = GetNormalisationCoeffs(WedgeValues);

            ret.Satellite = id;
            ColorCorrection.ColorCorrectBitmap(ref frame, NormalisationCoeffs.A, NormalisationCoeffs.B);
            WedgeValues = ReadWedges(ref frame);
            ret.PatchTemperature = GetPatchTemperature(GetWedgeValue(WedgeValues, WedgeId.PT));
            ret.ChId = GetChannelID(GetWedgeValue(WedgeValues, WedgeId.CID));
            if (getTemperature)
                ret.Temperatures = GetTemperatures(ref WedgeValues, ref frame, ref ret, NormalisationCoeffs.A, NormalisationCoeffs.B);
            return ret;
        }
    }
}
