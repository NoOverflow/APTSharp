using APTSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Explorer
{
    /// <summary>
    /// Logique d'interaction pour LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private static Dictionary<Status, string> TranslationDict = new Dictionary<Status, string>()
        {
            { Status.COLOR_CORRECTING, "Correcting colors ..." },
            { Status.COMPUTING_BBT, "Computing black body temperature ..." },
            { Status.COMPUTING_TEMP, "Computing temperature ..." },
            { Status.GETTING_NORMAL_COEFFS, "Getting normal coefficients ..." },
            { Status.READING_RAW_WEDGES, "Reading wedge values ..." },
            { Status.SOUND_CLEANING, "Normalizing sound file ..." },
            { Status.SOUND_PARSING, "Parsing sound file ..." },
            { Status.SYNCING, "Syncing line ..." },
            { Status.CREATING_FALSE_COLORS, "Making up false colors ..." },
            { Status.CREATING_THERMAL_COLORS, "Getting thermal image ..." },
            { Status.DONE, "Done." },
        };

        public LoadingWindow(ref StatusContext ctx)
        {
            InitializeComponent();
            ctx.OnStatusChange += Ctx_OnStatusChange;
        }

        private void Ctx_OnStatusChange(object sender, Status newStatus)
        {
            StatusLabel.Dispatcher.Invoke(() => { StatusLabel.Content = TranslationDict[newStatus]; });
            if (newStatus == Status.DONE)
            {
                this.Dispatcher.Invoke(() => { this.Close(); });
            }
        }
    }
}
