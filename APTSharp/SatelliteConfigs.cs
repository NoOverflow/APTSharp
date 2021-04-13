using System;
using System.Collections.Generic;
using System.Text;

namespace APTSharp
{
    public struct SatelliteConfig
    {
        public float[][] PTR_COEFFS;

        public Dictionary<ChannelId, float[]> EFBBT_COEFFS;

        public Dictionary<ChannelId, float[]> NL_RAD_CORR_COEFFS;
    }

    class SatelliteConfigs
    {
        public static readonly Dictionary<SatelliteId, SatelliteConfig> Configs = new Dictionary<SatelliteId, SatelliteConfig>()
        {
            {
                SatelliteId.NOAA_15,
                new SatelliteConfig()
                {
                    PTR_COEFFS = new float[][]
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
                    },
                    // nuC   A    B   NS
                    EFBBT_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 2695.9743f, 1.621256f, 0.998015f, 0.0f }},
                        {ChannelId.ID4, new float[] { 925.4075f, 0.337810f, 0.998719f, -4.50f }},
                        {ChannelId.ID5, new float[] { 839.8979f, 0.304558f, 0.999024f, -3.61f }}
                    },
                    NL_RAD_CORR_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 0.0f, 0.0f, 0.0f } },
                        {ChannelId.ID4, new float[] { 4.76f, -0.0932f, 0.0004524f } },
                        {ChannelId.ID5, new float[] { 3.83f, -0.0659f, 0.0002811f } }
                    }
                }
            },
            {
                SatelliteId.NOAA_18,
                new SatelliteConfig()
                {
                    PTR_COEFFS = new float[][]
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
                    },
                    EFBBT_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 2659.7952f, 1.698704f, 0.996960f, 0.0f }},
                        {ChannelId.ID4, new float[] { 928.1460f, 0.436645f, 0.998607f, -5.53f }},
                        {ChannelId.ID5, new float[] { 833.2532f, 0.253179f, 0.999057f, -2.22f }}
                    },
                    NL_RAD_CORR_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 0.0f, 0.0f, 0.0f } },
                        {ChannelId.ID4, new float[] { 5.82f, -0.11069f, 0.00052337f } },
                        {ChannelId.ID5, new float[] { 2.67f, -0.04360f, 0.00017715f } }
                    }
                }
            },
            {
                SatelliteId.NOAA_19,
                new SatelliteConfig()
                {
                    PTR_COEFFS = new float[][] {
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
                    },
                    EFBBT_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 2670.0f, 1.67396f, 0.997364f }},
                        {ChannelId.ID4, new float[] { 928.9f, 0.53959f, 0.998534f }},
                        {ChannelId.ID5, new float[] { 831.9f, 0.36064f, 0.998913f }}
                    },
                    NL_RAD_CORR_COEFFS = new Dictionary<ChannelId, float[]>()
                    {
                        {ChannelId.ID3B, new float[] { 0.0f, 0.0f, 0.0f } },
                        {ChannelId.ID4, new float[] { 5.70f, -0.11187f, 0.00054668f } },
                        {ChannelId.ID5, new float[] { 3.58f, -0.05991f, 0.00024985f } }
                    }
                }
            }
        };
    }
}
