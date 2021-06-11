using APTSharp;
using Explorer.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Explorer
{
    /// <summary>
    /// Logique d'interaction pour WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void ProcessSoundThreadLogic(ref StatusContext ctx, ref MainViewModel mvm, string path, SatelliteId id)
        {
            var context = mvm;
            var ret = new APTSharp.APT().Parse(path, id, ref ctx);
            Bitmap acc;
            Bitmap atc;
            Bitmap ftc;

            context.Temperatures = ret.FrameB.telemetry.Temperatures;
            context.ImageA = ret.FrameA.frame;
            context.ImageB = ret.FrameB.frame;
            context.ImageAB = ret.FullImage;
            acc = (Bitmap)context.ImageA.Clone();
            atc = (Bitmap)context.ImageA.Clone();
            ftc = (Bitmap)context.ImageA.Clone();
            APTSharp.Enhance.ColorCorrection.FalseColors(ref ctx, ref acc, ref ret.FrameB.frame);
            APTSharp.Enhance.ColorCorrection.ThermalColors(ref ctx, ref atc, ref ret.FrameB.frame);
            APTSharp.Enhance.ColorCorrection.WildfireColors(ref ctx, ref ftc, ref ret.FrameB.frame, context.Temperatures);
            context.ImageACC = acc;
            context.ImageATC = atc;
            context.ImageFire = ftc;
            context.CurrentImage = ret.FrameB.frame;
            context.CurrentImageSource = BitmapToImageSource(ret.FrameB.frame);
            ctx.UpdateCurrentState(Status.DONE);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            StatusContext ctx = new StatusContext();
            var context = (MainViewModel)Application.Current.MainWindow.DataContext;

            ofd.FileName = "";
            ofd.Filter = "WAV File|*.wav";
            ofd.Title = "Explorer - Load sound file";
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                Application.Current.MainWindow.Visibility = Visibility.Hidden;
                new Thread(() => { ProcessSoundThreadLogic(ref ctx, ref context, ofd.FileName, SatelliteId.NOAA_18); }).Start();
                new LoadingWindow(ref ctx).ShowDialog();
                Application.Current.MainWindow.Visibility = Visibility.Visible;

                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new MainPage());
            }
        }
    }
}
