using System;
using System.Collections.Generic;
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

namespace Waveform_Editor {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Editor : Window {
        private int DrawResolution;
        private int DrawLength;
        private List<decimal> Points;

        public Editor () {
            InitializeComponent ();
        }

        private void btnApplyResolutions_Click (object sender, RoutedEventArgs e) {
            DrawResolution = intDrawResolution.Value ?? 0;
            DrawLength = intDrawLength.Value ?? 0;

            Points = new List<decimal> ();
            for (int i = 0; i < (DrawResolution * DrawLength); i++)
                Points.Add (0);
        }
    }
}