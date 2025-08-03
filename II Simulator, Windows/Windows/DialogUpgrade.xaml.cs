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
    public partial class DialogUpgrade : Window {
        public App? Instance;

        public enum UpgradeOptions {
            None,
            Website,
            Delay,
            Mute
        }

        public DialogUpgrade () {
            InitializeComponent ();
        }

        public DialogUpgrade (App? app) {
            InitializeComponent ();

            DataContext = this;
            Instance = app;

            Init ();
        }

        public event EventHandler<UpgradeEventArgs>? OnUpgradeRoute;

        public class UpgradeEventArgs : EventArgs {
            public UpgradeOptions Route;

            public UpgradeEventArgs (UpgradeOptions d)
                => Route = d;
        }

        public void Init () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection

            wdwDialogUpgrade.Title = Instance.Language.Localize ("UPGRADE:Upgrade");
            lblUpdateAvailable.Content = Instance.Language.Localize ("UPGRADE:UpdateAvailable");
            lblWebsite.Content = Instance.Language.Localize ("UPGRADE:OpenDownloadPage");
            lblDelay.Content = Instance.Language.Localize ("UPGRADE:Later");
            lblMute.Content = Instance.Language.Localize ("UPGRADE:Mute");
        }

        private void btnWebsite_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Website));
            Close ();
        }

        private void btnDelay_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Delay));
            Close ();
        }

        private void btnMute_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Mute));
            Close ();
        }
    }
}