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

    public partial class PropertyCheck : UserControl {
        public Keys Key;

        public enum Keys {
            PulsusParadoxus,
            PulsusAlternans,
            MechanicallyVentilated
        }

        public event EventHandler<PropertyCheckEventArgs> PropertyChanged;

        public class PropertyCheckEventArgs : EventArgs {
            public Keys Key;
            public bool Value;
        }

        public PropertyCheck () {
            InitializeComponent ();
        }

        public void Init (Keys key) {
            Key = key;
            switch (Key) {
                default: break;
                case Keys.PulsusParadoxus: chkValue.Content = "Pulsus Paradoxus"; break;
                case Keys.PulsusAlternans: chkValue.Content = "Pulsus Alternans"; break;
                case Keys.MechanicallyVentilated: chkValue.Content = "Mechanically ventilated?"; break;
            }

            chkValue.Checked += sendPropertyChange;
            chkValue.LostFocus += sendPropertyChange;
        }

        public void Set (bool value) {
            chkValue.Checked -= sendPropertyChange;
            chkValue.IsChecked = value;
            chkValue.Checked += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            PropertyCheckEventArgs ea = new PropertyCheckEventArgs ();
            ea.Key = Key;
            ea.Value = chkValue.IsChecked ?? false;
            PropertyChanged (this, ea);
        }
    }
}