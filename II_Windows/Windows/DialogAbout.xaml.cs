using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using II.Localization;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for DialogAbout.xaml
    /// </summary>
    public partial class DialogAbout : Window {
        public DialogAbout () {
            InitializeComponent ();

            // Populate UI strings per language selection
            Languages.Values l = App.Language.Value;

            dlgAbout.Title = App.Language.Dictionary["ABOUT:AboutProgram"];
            lblInfirmaryIntegrated.Content = App.Language.Dictionary["II:InfirmaryIntegrated"];
            lblVersion.Content = String.Format(App.Language.Dictionary["ABOUT:Version"], II.Utility.Version);
            tblDescription.Text = App.Language.Dictionary["ABOUT:Description"];
        }

        private void Hyperlink_RequestNavigate (object sender, RequestNavigateEventArgs e) {
            Process.Start (new ProcessStartInfo (e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
