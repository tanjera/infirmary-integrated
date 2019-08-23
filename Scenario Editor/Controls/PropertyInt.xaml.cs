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

    public partial class PropertyInt : UserControl {
        public Keys Key;

        public enum Keys {
            HR, RR, ETCO2, SPO2,            // Heart rate, respiratory rate, end-tidal capnography, pulse oximetry
            CVP,                            // Central venous pressure,
            ICP, IAP                        // Intracranial pressure, intra-abdominal pressure
        }

        public event EventHandler<PropertyIntEventArgs> PropertyChanged;

        public class PropertyIntEventArgs : EventArgs {
            public Keys Key;
            public int Value;
        }

        public PropertyInt (int row, Keys key, int value, int increment, int minvalue, int maxvalue) {
            InitializeComponent ();

            this.SetValue (Grid.RowProperty, row);

            Key = key;
            switch (Key) {
                default: break;
                case Keys.HR: lblKey.Content = "Heart Rate: "; break;
                case Keys.RR: lblKey.Content = "Respiratory Rate: "; break;
                case Keys.ETCO2: lblKey.Content = "End-tidal CO2: "; break;
                case Keys.SPO2: lblKey.Content = "Pulse Oximetry: "; break;
                case Keys.CVP: lblKey.Content = "Central Venous Pressure: "; break;
                case Keys.ICP: lblKey.Content = "Intra-cranial Pressure: "; break;
                case Keys.IAP: lblKey.Content = "Intra-abdominal Pressure: "; break;
            }

            numValue.Value = value;
            numValue.Increment = increment;
            numValue.Minimum = minvalue;
            numValue.Maximum = maxvalue;
            numValue.ValueChanged += onValueChanged; ;
        }

        private void onValueChanged (object sender, RoutedPropertyChangedEventArgs<object> e) {
            PropertyIntEventArgs ea = new PropertyIntEventArgs ();
            ea.Key = Key;
            ea.Value = numValue.Value ?? 0;
            PropertyChanged (this, ea);
        }
    }
}