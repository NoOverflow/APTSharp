using System;
using static APTSharp.Sound.WavParser;

namespace APTSharpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            WavData soundData = APTSharp.Sound.WavParser.ParseWav("Example/noaa.wav");
            float[] rawFloatData = soundData.GetAsFloat32();

            Console.WriteLine("Wav Channels : {0}", soundData.ChannelsCount);
            Console.WriteLine("Wav Frequency : {0}", soundData.Frequency);
            Console.ReadLine();
        }
    }
}
