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
            var ret = APTSharp.APT.ParseAPTFile("Example/noaa15.wav");

            ret.ImageRes.ImageA.Save("testA.bmp");
            ret.ImageRes.ImageB.Save("testB.bmp");
        }
    }
}
