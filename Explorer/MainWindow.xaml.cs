using Explorer.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static double[] Temperatures = null;

        public MainWindow()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;
            var ret = new APTSharp.APT().Parse("Example/noaa18.wav");

            Temperatures = ret.FrameB.telemetry.Temperatures;
            context.ImageA = ret.FrameA.frame;
            context.ImageB = ret.FrameB.frame;
            context.ImageAB = ret.FullImage;
            context.CurrentImage = BitmapToImageSource(ret.FrameB.frame);
        }

        private void NoaaImage_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(NoaaImage);

            if (Temperatures == null || (position.Y * NoaaImage.Width + position.X >= Temperatures.Length))
            {
                return;
            }
            double kelTemp = Temperatures[(int)position.Y * (int)NoaaImage.Width + (int)position.X];

            if (kelTemp == Double.NaN || kelTemp.ToString("F2") == "NaN")
                kelTemp = 0.0f;
            TemperatureLabel.Content = String.Format("Temperature:\n{0}K ({1}°C)", kelTemp.ToString("F2"), (kelTemp - 273.15f).ToString("F2"));
                
        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = BitmapToImageSource(context.ImageA);
            // NoaaImage.Source = BitmapToImageSource(context.ImageA);
        }

        private void BButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = BitmapToImageSource(context.ImageB);
        }

        private void ABButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = BitmapToImageSource(context.ImageAB);
        }

        private void ColorCorrectButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;
            var imageA = context.ImageA;
            var imageB = context.ImageB;

            APTSharp.Enhance.ColorCorrection.FalseColors(ref imageA, ref imageB);
            imageA.Save("False Colors Image A.bmp");
            context.ImageA = imageA;
            context.CurrentImage = BitmapToImageSource(context.ImageA);
        }
    }
}
