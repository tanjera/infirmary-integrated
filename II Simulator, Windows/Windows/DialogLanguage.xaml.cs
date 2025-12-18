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
    /// Interaction logic for DialogLanguage.xaml
    /// </summary>
    public partial class DialogLanguage : Window {
        public App? Instance;

        public DialogLanguage () {
            InitializeComponent ();
        }

        public DialogLanguage (App? app) {
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

            wdwDialogLanguage.Title = Instance.Language.Localize ("LANGUAGE:Title");
            lblChooseLanguage.Content = Instance.Language.Localize ("LANGUAGE:Select");
            btnContinue.Content = Instance.Language.Localize ("BUTTON:Continue");

            foreach (string each in II.Localization.Language.Descriptions)
                cmbLanguages.Items.Add (new MenuItem () {
                    Header = each,
                });
            cmbLanguages.SelectedIndex = Enum.Parse (typeof (II.Localization.Language.Values), Instance.Settings.Language).GetHashCode();
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnClick_Continue)}");
                return;
            }

            Instance.Language.Value = Enum.GetValues<Language.Values> () [cmbLanguages.SelectedIndex];

            Instance.Settings.Language = Instance.Language.Value.ToString ();
            Instance.Settings.Save ();

            // Show messagebox prompting user to restart the program for changes to take effect
            System.Windows.MessageBox.Show (
                    Instance.Language.Localize ("MESSAGE:RestartForChanges"),
                    Instance.Language.Localize ("MESSAGE:Restart"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

            this.Close ();
        }
    }
}