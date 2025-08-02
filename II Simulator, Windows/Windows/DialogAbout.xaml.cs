using II;
using II.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DialogAbout.xaml
    /// </summary>
    public partial class DialogAbout : Window {
        public App? Instance;

        public DialogAbout () {
            InitializeComponent ();
        }

        public DialogAbout (App? app) {
            InitializeComponent ();

            DataContext = this;
            Instance = app;

            Init ();
        }

        public void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            wdwDialogAbout.Title = Instance.Language.Localize ("ABOUT:AboutProgram");
            lblInfirmaryIntegrated.Content = Instance.Language.Localize ("II:InfirmaryIntegrated");
            lblVersion.Content = String.Format (Instance.Language.Localize ("ABOUT:Version"),
                Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0");
            tblDescription.Text = Instance.Language.Localize ("ABOUT:Description");
        }

        private void Hyperlink_Website (object sender, RoutedEventArgs e) {
            string url = "http://www.infirmary-integrated.com/".Replace ("&", "^&");
            Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void Hyperlink_GitRepo (object sender, RoutedEventArgs e) {
            string url = "https://github.com/tanjera/infirmary-integrated".Replace ("&", "^&");
            Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}