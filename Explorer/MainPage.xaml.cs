using APTSharp;
using Explorer.ViewModel;
using Microsoft.Win32;
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
    /// Logique d'interaction pour MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {

        public MainPage()
        {
            MainViewModel mvm = (MainViewModel)Application.Current.MainWindow.DataContext;

            InitializeComponent();
            ((MainViewModel)this.DataContext).CurrentImage = (Bitmap)mvm.CurrentImage.Clone();
            ((MainViewModel)this.DataContext).ImageA = (Bitmap)mvm.ImageA.Clone();
            ((MainViewModel)this.DataContext).ImageB = (Bitmap)mvm.ImageB.Clone();
            ((MainViewModel)this.DataContext).ImageAB = (Bitmap)mvm.ImageAB.Clone();
            ((MainViewModel)this.DataContext).ImageACC = (Bitmap)mvm.ImageACC.Clone();
            ((MainViewModel)this.DataContext).ImageATC = (Bitmap)mvm.ImageATC.Clone();
            ((MainViewModel)this.DataContext).ImageFire = (Bitmap)mvm.ImageFire.Clone();
            ((MainViewModel)this.DataContext).IsCurrentColorCorrected = mvm.IsCurrentColorCorrected;
            ((MainViewModel)this.DataContext).IsCurrentThermalCorrected = mvm.IsCurrentThermalCorrected;
            ((MainViewModel)this.DataContext).Temperatures = mvm.Temperatures;
            ((MainViewModel)this.DataContext).CurrentImage = ((MainViewModel)this.DataContext).ImageA;
            ((MainViewModel)this.DataContext).CurrentImageSource = BitmapToImageSource(((MainViewModel)this.DataContext).CurrentImage);
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


        private void NoaaImage_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(ScrollViewer);
            var context = (MainViewModel)this.DataContext;

            position.Y += ScrollViewer.VerticalOffset;
            if (context.Temperatures == null || (position.Y * 1040 + position.X >= context.Temperatures.Length))
            {
                return;
            }
            double kelTemp = context.Temperatures[(int)position.Y * (int)1040 + (int)position.X];

            if (kelTemp == Double.NaN || kelTemp.ToString("F2") == "NaN")
                kelTemp = 0.0f;
            TemperatureLabel.Content = String.Format("Temperature:\n{0}K ({1}°C)\n(X={2} Y={3})", kelTemp.ToString("F2"), (kelTemp - 273.15f).ToString("F2"), position.X, position.Y);

        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.CurrentImageSource.Freeze();
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
            context.IsCurrentThermalCorrected = false;
            context.IsCurrentFire = false;
            context.CurrentImage = context.IsCurrentColorCorrected ? context.ImageACC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void ThermalCorrectCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.IsCurrentThermalCorrected = ThermalCorrectCheckbox.IsChecked.Value;
            context.IsCurrentColorCorrected = false;
            context.IsCurrentFire = false;
            context.CurrentImage = context.IsCurrentThermalCorrected ? context.ImageATC : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void IsFireCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;

            context.IsCurrentFire = FireDetectCheckbox.IsChecked.Value;
            context.IsCurrentColorCorrected = false;
            context.IsCurrentThermalCorrected = false;
            context.CurrentImage = context.IsCurrentFire ? context.ImageFire : context.ImageA;
            context.CurrentImageSource = BitmapToImageSource(context.CurrentImage);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (MainViewModel)this.DataContext;
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG File|*.png";
            sfd.FileName = "";
            sfd.Title = "Explorer - Save Image";
            if (sfd.ShowDialog().GetValueOrDefault())
            {
                context.CurrentImage.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Image exported successfully to: \r\n" + sfd.FileName, "Explorer - Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
