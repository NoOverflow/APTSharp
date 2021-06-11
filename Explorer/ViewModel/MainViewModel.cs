using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Explorer.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Bitmap ImageA { get; set; }
        public Bitmap ImageB { get; set; }
        public Bitmap ImageAB { get; set; }

        public double[] Temperatures { get; set; }
        public bool IsCurrentColorCorrected { get; set; }

        public bool IsCurrentThermalCorrected { get; set; }
        public bool IsCurrentFire { get; set; }

        /// <summary>
        /// Frame A Color Corrected (False)
        /// </summary>
        public Bitmap ImageACC { get; set; }

        /// <summary>
        /// Frame A Thermal Corrected Image
        /// </summary>
        public Bitmap ImageATC { get; set; } 
        public Bitmap ImageFire { get; set; } 

        public Bitmap CurrentImage { get; set; }

        ImageSource _CurrentImageSource;
        public ImageSource CurrentImageSource
        {
            get { return _CurrentImageSource; }
            set
            {
                _CurrentImageSource = value;
                OnPropertyChanged("CurrentImageSource");
            }   
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
