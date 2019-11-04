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

namespace II.Scenario_Editor.Controls {

    public partial class PropertyECGSegment : UserControl {
        public Keys Key;

        public enum Keys {
            STElevation,
            TWave
        }

        public event EventHandler<PropertyECGEventArgs> PropertyChanged;

        public class PropertyECGEventArgs : EventArgs {
            public Keys Key;
            public float [] Values;
        }

        public PropertyECGSegment () {
            InitializeComponent ();
        }

        public void Init (Keys key) {
            Key = key;
            switch (Key) {
                default: break;
                case Keys.STElevation: lblKey.Content = "ST Segment Elevation: "; break;
                case Keys.TWave: lblKey.Content = "T Wave Elevation: "; break;
            }

            dblI.ValueChanged += sendPropertyChange;
            dblII.ValueChanged += sendPropertyChange;
            dblIII.ValueChanged += sendPropertyChange;
            dblaVR.ValueChanged += sendPropertyChange;
            dblaVL.ValueChanged += sendPropertyChange;
            dblaVF.ValueChanged += sendPropertyChange;
            dblV1.ValueChanged += sendPropertyChange;
            dblV2.ValueChanged += sendPropertyChange;
            dblV3.ValueChanged += sendPropertyChange;
            dblV4.ValueChanged += sendPropertyChange;
            dblV5.ValueChanged += sendPropertyChange;
            dblV6.ValueChanged += sendPropertyChange;

            dblI.LostFocus += sendPropertyChange;
            dblII.LostFocus += sendPropertyChange;
            dblIII.LostFocus += sendPropertyChange;
            dblaVR.LostFocus += sendPropertyChange;
            dblaVL.LostFocus += sendPropertyChange;
            dblaVF.LostFocus += sendPropertyChange;
            dblV1.LostFocus += sendPropertyChange;
            dblV2.LostFocus += sendPropertyChange;
            dblV3.LostFocus += sendPropertyChange;
            dblV4.LostFocus += sendPropertyChange;
            dblV5.LostFocus += sendPropertyChange;
            dblV6.LostFocus += sendPropertyChange;
        }

        public void Set (float [] values) {
            dblI.ValueChanged -= sendPropertyChange;
            dblII.ValueChanged -= sendPropertyChange;
            dblIII.ValueChanged -= sendPropertyChange;
            dblaVR.ValueChanged -= sendPropertyChange;
            dblaVL.ValueChanged -= sendPropertyChange;
            dblaVF.ValueChanged -= sendPropertyChange;
            dblV1.ValueChanged -= sendPropertyChange;
            dblV2.ValueChanged -= sendPropertyChange;
            dblV3.ValueChanged -= sendPropertyChange;
            dblV4.ValueChanged -= sendPropertyChange;
            dblV5.ValueChanged -= sendPropertyChange;
            dblV6.ValueChanged -= sendPropertyChange;

            dblI.Value = values [0];
            dblII.Value = values [1];
            dblIII.Value = values [2];
            dblaVR.Value = values [3];
            dblaVL.Value = values [4];
            dblaVF.Value = values [5];
            dblV1.Value = values [6];
            dblV2.Value = values [7];
            dblV3.Value = values [8];
            dblV4.Value = values [9];
            dblV5.Value = values [10];
            dblV6.Value = values [11];

            dblI.ValueChanged += sendPropertyChange;
            dblII.ValueChanged += sendPropertyChange;
            dblIII.ValueChanged += sendPropertyChange;
            dblaVR.ValueChanged += sendPropertyChange;
            dblaVL.ValueChanged += sendPropertyChange;
            dblaVF.ValueChanged += sendPropertyChange;
            dblV1.ValueChanged += sendPropertyChange;
            dblV2.ValueChanged += sendPropertyChange;
            dblV3.ValueChanged += sendPropertyChange;
            dblV4.ValueChanged += sendPropertyChange;
            dblV5.ValueChanged += sendPropertyChange;
            dblV6.ValueChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            PropertyECGEventArgs ea = new PropertyECGEventArgs ();
            ea.Key = Key;
            ea.Values = new float [] {
                (float)dblI.Value, (float)dblII.Value, (float)dblIII.Value,
                (float)dblaVR.Value, (float)dblaVL.Value, (float)dblaVF.Value,
                (float)dblV1.Value, (float)dblV2.Value, (float)dblV3.Value,
                (float)dblV4.Value, (float)dblV5.Value, (float)dblV6.Value
                };
            PropertyChanged (this, ea);
        }
    }
}