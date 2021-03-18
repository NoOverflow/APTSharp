using APTSharp;
using NAudio.Wave;
using System;
using static APTSharp.Sound.WavParser;

namespace APTSharpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ret = APTSharp.APT.ParseAPTFile("Example/noaa19.wav");

            AudioFileReader reader = new AudioFileReader("Example/noaa.wav");
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 2];
            isp.Read(buffer, 0, buffer.Length);




            Console.ReadLine();
        }
    }
}
