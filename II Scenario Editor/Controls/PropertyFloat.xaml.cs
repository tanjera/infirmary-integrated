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

    public partial class PropertyFloat : UserControl {
        public Keys Key;

        public enum Keys {
            RRInspiratoryRatio, RRExpiratoryRatio
        }

        public event EventHandler<PropertyFloatEventArgs> PropertyChanged;

        public class PropertyFloatEventArgs : EventArgs {
            public Keys Key;
            public float Value;
        }

        public PropertyFloat () {
            InitializeComponent ();
        }

        public void Init (Keys key, double increment, double minvalue, double maxvalue) {
            Key = key;
            switch (Key) {
                default: break;
                case Keys.RRInspiratoryRatio: lblKey.Content = "Inspiratory Ratio: "; break;
                case Keys.RRExpiratoryRatio: lblKey.Content = "Expiratory Ratio: "; break;
            }

            numValue.Increment = (decimal)increment;
            numValue.Minimum = (decimal)minvalue;
            numValue.Maximum = (decimal)maxvalue;
            numValue.ValueChanged += sendPropertyChange;
            numValue.LostFocus += sendPropertyChange;
        }

        public void Set (float value) {
            numValue.ValueChanged -= sendPropertyChange;
            numValue.Value = (decimal)value;
            numValue.ValueChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            PropertyFloatEventArgs ea = new PropertyFloatEventArgs ();
            ea.Key = Key;
            decimal d = numValue.Value ?? 0;
            ea.Value = (float)d;
            PropertyChanged (this, ea);
        }
    }
}