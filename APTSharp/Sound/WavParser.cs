using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace APTSharp.Sound
{
    public class WavParser
    {
        #region WAV Offsets
        public const short CHANNEL_COUNT_OFFSET = 22;
        public const short FREQUENCY_OFFSET = 24;
        public const short BITPERSAMPLE_OFFSET = 34;
        public static readonly byte[] BLOCK_HEADER = { 0x64, 0x61, 0x74, 0x61};
        #endregion

        public struct WavData
        {
            public short ChannelsCount;
            public int Frequency;
            public byte[] Mono;
            public short BitsPerSample;

            public float[] GetAsFloat32()
            {
                if (Mono.Length % 4 != 0)
                    throw new Exception("Can't convert to to float32 array");
                float[] retArray = new float[Mono.Length / 4];
                int j = 0;

                for (int i = 0; i < Mono.Length; i += 4)
                    retArray[j++] = ((sbyte)((Mono[i + 3] << 24) | (Mono[i + 2] << 16) | (Mono[i + 1] << 8) | Mono[i])) / 2147483648f;
                return (retArray);
            }
        }

        /// <summary>
        /// Parse a single channel wave file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static WavData ParseWav(string path)
        {
            int i = 0;
            WavData returnData = new WavData();
            byte[] rawWavData = File.ReadAllBytes(path);

            returnData.ChannelsCount = (short)(rawWavData[CHANNEL_COUNT_OFFSET + 1] << 8 | rawWavData[CHANNEL_COUNT_OFFSET]);
            returnData.Frequency = (int)(rawWavData[FREQUENCY_OFFSET + 3] << 24 | rawWavData[FREQUENCY_OFFSET + 2] << 16 | rawWavData[FREQUENCY_OFFSET + 1] << 8 | rawWavData[FREQUENCY_OFFSET]);
            returnData.BitsPerSample = (short)(rawWavData[BITPERSAMPLE_OFFSET + 1] << 8 | rawWavData[BITPERSAMPLE_OFFSET]);

            // Find WAV Data block header
            while (i < rawWavData.Length && !(rawWavData[i] == BLOCK_HEADER[0] && rawWavData[i + 1] == BLOCK_HEADER[1] && rawWavData[i + 2] == BLOCK_HEADER[2] && rawWavData[i + 3] == BLOCK_HEADER[3]))
                i++;
            if (i >= rawWavData.Length)
                throw new Exception("Can't find WAV Data block");
            i += 4; // Skip header

            int blockSize = (int)(rawWavData[i + 3] << 24 | rawWavData[i + 2] << 16 | rawWavData[i + 1] << 8 | rawWavData[i]);

            i += 4;
            if (i + blockSize > rawWavData.Length)
                throw new Exception("Invalid WAV Data block size");
            returnData.Mono = new byte[blockSize];
            for (int j = 0; j < blockSize; j++)
                returnData.Mono[j] = rawWavData[i + j];
            return (returnData); 
        }
    }
}
