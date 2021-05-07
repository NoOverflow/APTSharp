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

        ImageSource _CurrentImage;
        public ImageSource CurrentImage
        {
            get { return _CurrentImage; }
            set
            {
                _CurrentImage = value;
                OnPropertyChanged("CurrentImage");
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
