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

    public partial class PropertyProgression : UserControl {
        public int Index;
        public int IndexStepTo;
        public string Description;

        public event EventHandler<PropertyProgressionEventArgs> PropertyChanged;

        public class PropertyProgressionEventArgs : EventArgs {
            public int Index;
            public int IndexStepTo;
            public string Description;
        }

        public PropertyProgression () {
            InitializeComponent ();
        }

        public void Init (int index, int stepTo, string desc) {
            Index = index;
            IndexStepTo = stepTo;
            Description = desc;

            numStepTo.Value = IndexStepTo;
            txtDescription.Text = Description;

            lblProgressionProperty.Content = String.Format ("Edit Optional Progression To Step #{0:000}", IndexStepTo);

            numStepTo.ValueChanged += sendPropertyChange;
            numStepTo.LostFocus += sendPropertyChange;

            txtDescription.TextChanged += sendPropertyChange;
            txtDescription.LostFocus += sendPropertyChange;
        }

        private void sendPropertyChange (object sender, EventArgs e) {
            PropertyProgressionEventArgs ea = new PropertyProgressionEventArgs ();
            ea.Index = Index;
            ea.IndexStepTo = IndexStepTo;
            ea.Description = Description;
            PropertyChanged (this, ea);
        }
    }
}