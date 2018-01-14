using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

namespace StringPairing {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StringPair : Window {
        public StringPair () {
            InitializeComponent ();

            txtFormat1.Text = "\t\t\tnew Pair(\"{0}\",";
            txtFormat2.Text = "\"{0}\"),";
        }

        private void OnClick_ProcessText (object sender, RoutedEventArgs e) {
            StringReader srI1 = new StringReader (txtInput1.Text);
            StringReader srI2 = new StringReader (txtInput2.Text);
            StringBuilder sbOut = new StringBuilder ();
            string formI1 = txtFormat1.Text,
                    formI2 = txtFormat2.Text;
            txtOutput.Clear ();

            string eachI1, eachI2;
            while ((eachI1 = srI1.ReadLine ()) != null && (eachI2 = srI2.ReadLine ()) != null) {
                if ((eachI1 == "" && eachI2 == "")
                    || eachI1.ToUpper() == "KEY") {
                    //sbOut.AppendLine ("");
                } else
                    sbOut.AppendLine (String.Format ("{0,-55}{1}",
                        String.Format(formI1, eachI1),
                        String.Format(formI2, eachI2)
                        ));
            }

            txtOutput.Text = sbOut.ToString ();
        }
    }
}
