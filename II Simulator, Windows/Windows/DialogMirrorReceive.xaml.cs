using II;
using II.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for DialogMirrorReceive.xaml
    /// </summary>
    public partial class DialogMirrorReceive : Window {
        public App? Instance;

        public DialogMirrorReceive () {
            InitializeComponent ();
        }

        public DialogMirrorReceive (App? app) {
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
            wdwDialogMirrorReceive.Title = Instance.Language.Localize ("MIRROR:ReceiveTitle");
            txtMessage.Text = Instance.Language.Localize ("MIRROR:EnterSettings");

            txtAccessionKey.Text = Instance.Language.Localize ("MIRROR:AccessionKey");
            txtAccessPassword.Text = Instance.Language.Localize ("MIRROR:AccessPassword");

            btnContinue.Content = Instance.Language.Localize ("BUTTON:Continue");
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Regex regex = new ("^[a-zA-Z0-9]*$");
            if ((tbAccessionKey.Text ?? "").Length > 0
                    && regex.IsMatch (tbAccessionKey.Text ?? "")) {
                Instance.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                Instance.Mirror.Accession = tbAccessionKey.Text ?? "";
                Instance.Mirror.PasswordAccess = tbAccessPassword.Text ?? "";

                this.Close ();
            } else {
                System.Windows.MessageBox.Show (
                    Instance.Language.Localize ("MIRROR:SettingsInvalid"),
                    Instance.Language.Localize ("MIRROR:ReceiveTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}