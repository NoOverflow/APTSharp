using Explorer.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

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

        public static System.Drawing.Bitmap ImageSourceToBitmap(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;
            var ret = new APTSharp.APT().Parse("Example/noaa18.wav");
            Bitmap acc;
            Bitmap atc;

            Temperatures = ret.FrameB.telemetry.Temperatures;
            context.ImageA = ret.FrameA.frame;
            context.ImageB = ret.FrameB.frame;
            context.ImageAB = ret.FullImage;
            acc = (Bitmap)context.ImageA.Clone();
            atc = (Bitmap)context.ImageA.Clone();
            APTSharp.Enhance.ColorCorrection.FalseColors(ref acc, ref ret.FrameB.frame);
            APTSharp.Enhance.ColorCorrection.ThermalColors(ref atc, ref ret.FrameB.frame);
            context.ImageACC = acc;
            context.ImageATC = atc;
            context.CurrentImage = ret.FrameB.frame;
            context.CurrentImageSource = BitmapToImageSource(ret.FrameB.frame);
        }

        private void NoaaImage_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(ScrollViewer);

            position.Y += ScrollViewer.VerticalOffset;
            if (Temperatures == null || (position.Y * 1040 + position.X >= Temperatures.Length))
            {
                return;
            }
            double kelTemp = Temperatures[(int)position.Y * (int)1040 + (int)position.X];

            if (kelTemp == Double.NaN || kelTemp.ToString("F2") == "NaN")
                kelTemp = 0.0f;
            TemperatureLabel.Content = String.Format("Temperature:\n{0}K ({1}°C)\n(X={2} Y={3})", kelTemp.ToString("F2"), (kelTemp - 273.15f).ToString("F2"), position.X, position.Y);
                
        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = context.IsCurrentColorCorrected ? context.ImageACC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void BButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = context.ImageB;
            context.CurrentImageSource = BitmapToImageSource(context.ImageB);
        }

        private void ABButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImage = context.ImageAB;
            context.CurrentImageSource = BitmapToImageSource(context.ImageAB);
        }

        private void ColorCorrectButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.IsCurrentColorCorrected = !context.IsCurrentColorCorrected;
            context.CurrentImage = context.IsCurrentColorCorrected ? context.ImageACC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.IsCurrentColorCorrected = ColorCorrectCheckbox.IsChecked.Value;
            context.CurrentImage = context.IsCurrentColorCorrected ? context.ImageACC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void ThermalCorrectCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.IsCurrentThermalCorrected = ThermalCorrectCheckbox.IsChecked.Value;
            context.IsCurrentColorCorrected = false;
            context.CurrentImage = context.IsCurrentThermalCorrected ? context.ImageATC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG File|*.png";
            sfd.FileName = "";
            sfd.Title = "Explorer - Save Image";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                context.CurrentImage.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Image exported successfully to: \r\n" + sfd.FileName, "Explorer - Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
