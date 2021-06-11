using APTSharp;
using NAudio.Wave;
using System;

namespace APTSharpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            StatusContext ctx = new StatusContext();
            var ret = new APTSharp.APT().Parse("Example/noaa15.wav", SatelliteId.NOAA_15, ref ctx);

            ret.FullImage.Save("testAB.bmp");
            ret.FrameB.frame.Save("testA.bmp");
            ret.FrameB.frame.Save("testB.bmp");
        }
    }
}
