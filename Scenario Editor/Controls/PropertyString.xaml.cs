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

    public partial class PropertyString : UserControl {
        public Keys Key;

        public enum Keys {
            Name,
            Description
        }

        public event EventHandler<PropertyStringEventArgs> PropertyChanged;

        public class PropertyStringEventArgs : EventArgs {
            public Keys Key;
            public string Value;
        }

        public PropertyString (int row, Keys key, string value) {
            InitializeComponent ();

            this.SetValue (Grid.RowProperty, row);

            Key = key;
            switch (Key) {
                default: break;
                case Keys.Name: lblKey.Content = "Name: "; break;
                case Keys.Description: lblKey.Content = "Description: "; break;
            }

            txtValue.Text = value;
            txtValue.TextChanged += onTextChanged;
        }

        private void onTextChanged (object sender, TextChangedEventArgs e) {
            PropertyStringEventArgs ea = new PropertyStringEventArgs ();
            ea.Key = Key;
            ea.Value = txtValue.Text ?? "";
            PropertyChanged (this, ea);
        }
    }
}