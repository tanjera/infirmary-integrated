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
    /// Interaction logic for DialogEULA.xaml
    /// </summary>
    public partial class DialogEULA : Window {
        public App? Instance;

        public DialogEULA () {
            InitializeComponent ();
        }

        public DialogEULA (App? app) {
            InitializeComponent ();

            DataContext = this;
            Instance = app;

            Init ();
        }

        private void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection

            wdwDialogEULA.Title = Instance.Language.Localize ("EULA:Title");
            txtAgreeTerms.Text = Instance.Language.Localize ("EULA:AgreeToTerms");
            btnContinue.Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            Instance?.Settings.Save ();

            this.Close ();
        }

        private void Hyperlink_Terms (object sender, RoutedEventArgs e) {
            string url = "https://github.com/tanjera/infirmary-integrated".Replace ("&", "^&");
            Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}