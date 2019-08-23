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

namespace ScenarioEditor {

    /// <summary>
    /// Interaction logic for TreeItem_Property.xaml
    /// </summary>
    public partial class PropertyItem : UserControl {

        public PropertyItem (int row, string key, string value) {
            InitializeComponent ();

            lblKey.Content = key;
            txtValue.Text = value;
            this.SetValue (Grid.RowProperty, row);
        }
    }
}