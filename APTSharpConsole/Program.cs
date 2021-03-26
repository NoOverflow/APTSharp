using APTSharp;
using NAudio.Wave;
using System;

namespace APTSharpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ret = new APTSharp.APT().Parse("Example/noaa15.wav");

            ret.FullImage.Save("testAB.bmp");
            ret.ImageA.Save("testA.bmp");
            ret.ImageB.Save("testB.bmp");
        }
    }
}
