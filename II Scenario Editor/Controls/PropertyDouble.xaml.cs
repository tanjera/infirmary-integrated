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

    public partial class PropertyDouble : UserControl {
        public Keys Key;

        public enum Keys {
            T,
            RRInspiratoryRatio, RRExpiratoryRatio
        }

        public event EventHandler<PropertyDoubleEventArgs> PropertyChanged;

        public class PropertyDoubleEventArgs : EventArgs {
            public Keys Key;
            public double Value;
        }

        public PropertyDouble () {
            InitializeComponent ();
        }

        public void Init (Keys key, double increment, double minvalue, double maxvalue) {
            Key = key;
            switch (Key) {
                default: break;
                case Keys.T: lblKey.Content = "Temperature: "; break;
            }

            numValue.Increment = increment;
            numValue.Minimum = minvalue;
            numValue.Maximum = maxvalue;
            numValue.ValueChanged += sendPropertyChange;
            numValue.LostFocus += sendPropertyChange;
        }

        public void Set (double value) {
            numValue.ValueChanged -= sendPropertyChange;
            numValue.Value = value;
            numValue.ValueChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            PropertyDoubleEventArgs ea = new PropertyDoubleEventArgs ();
            ea.Key = Key;
            ea.Value = numValue.Value ?? 0;
            PropertyChanged (this, ea);
        }
    }
}