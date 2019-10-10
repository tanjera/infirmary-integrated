using II.Localization;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DialogAbout.xaml
    /// </summary>
    public partial class DialogAbout : Window {

        public DialogAbout () {
            InitializeComponent ();

            // Populate UI strings per language selection
            Language.Values l = App.Language.Value;

            dlgAbout.Title = App.Language.Dictionary ["ABOUT:AboutProgram"];
            lblInfirmaryIntegrated.Content = App.Language.Dictionary ["II:InfirmaryIntegrated"];
            lblVersion.Content = String.Format (App.Language.Dictionary ["ABOUT:Version"], II.Utility.Version);
            tblDescription.Text = App.Language.Dictionary ["ABOUT:Description"];
        }

        private void Hyperlink_RequestNavigate (object sender, RequestNavigateEventArgs e) {
            Process.Start (new ProcessStartInfo (e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}