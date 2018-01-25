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


namespace DictionaryBuilder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DictionaryBuild : Window {
        public DictionaryBuild () {
            InitializeComponent ();
        }

        private void OnClick_ProcessText (object sender, RoutedEventArgs e) {
            StringReader srI1 = new StringReader (txtInput1.Text);
            StringReader srI2 = new StringReader (txtInput2.Text);
            StringBuilder sbOut = new StringBuilder ();
            txtOutput.Clear ();

            string eachI1, eachI2;
            while ((eachI1 = srI1.ReadLine ()) != null && (eachI2 = srI2.ReadLine ()) != null) {
                if (eachI1 == "" && eachI2 == "") {
                    //sbOut.AppendLine ("");
                    continue;
                } else if (eachI1.ToUpper () == "KEY") {
                    sbOut.AppendLine (String.Format ("\t\tstatic Dictionary<string, string> {0} = new Dictionary<string, string> () {{",
                        eachI2.ToUpper ().Substring (0, 3)));
                } else
                    sbOut.AppendLine (String.Format ("\t\t\t{{{0,-60} {1}}},",
                        String.Format("\"{0}\",", eachI1),
                        String.Format ("\"{0}\"", eachI2)));
            }

            sbOut.AppendLine ("\t\t};\n");

            txtOutput.Text = sbOut.ToString ();
            Clipboard.SetText (txtOutput.Text);
        }
    }
}
