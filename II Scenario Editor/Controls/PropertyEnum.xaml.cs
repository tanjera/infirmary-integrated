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

    public partial class PropertyEnum : UserControl {
        public Keys Key;
        public List<string> Values;

        public enum Keys {
            Cardiac_Axis,
            Cardiac_Rhythms,
            Respiratory_Rhythms,
            PACatheter_Rhythms
        }

        public event EventHandler<PropertyEnumEventArgs> PropertyChanged;

        public class PropertyEnumEventArgs : EventArgs {
            public Keys Key;
            public string Value;
        }

        public PropertyEnum () {
            InitializeComponent ();
        }

        public void Init (Keys key, string [] values, List<string> readable) {
            Key = key;
            Values = new List<string> (values);

            switch (Key) {
                default: break;
                case Keys.Cardiac_Axis: lblKey.Content = "Cardiac Axis: "; break;
                case Keys.Cardiac_Rhythms: lblKey.Content = "Cardiac Rhythm: "; break;
                case Keys.Respiratory_Rhythms: lblKey.Content = "Respiratory Rhythm: "; break;
                case Keys.PACatheter_Rhythms: lblKey.Content = "Pulmonary Artery Catheter Placement: "; break;
            }

            cmbEnumeration.Items.Clear ();
            foreach (string s in readable) {
                ComboBoxItem cbi = new ComboBoxItem ();
                cbi.Content = s;
                cmbEnumeration.Items.Add (cbi);
            }

            cmbEnumeration.SelectionChanged += sendPropertyChange;
            cmbEnumeration.LostFocus += sendPropertyChange;
        }

        public void Set (int index) {
            cmbEnumeration.SelectionChanged -= sendPropertyChange;
            cmbEnumeration.SelectedIndex = index;
            cmbEnumeration.SelectionChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            if (cmbEnumeration.SelectedIndex < 0)
                return;

            PropertyEnumEventArgs ea = new PropertyEnumEventArgs ();
            ea.Key = Key;
            ea.Value = Values [cmbEnumeration.SelectedIndex];
            PropertyChanged (this, ea);
        }
    }
}