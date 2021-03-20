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

            ret.ImageRes.Save("test.bmp");
        }
    }
}
