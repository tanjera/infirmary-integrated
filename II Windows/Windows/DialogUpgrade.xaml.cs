using System;
using System.Windows;

using II;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DialogLanguage.xaml
    /// </summary>
    public partial class DialogUpgrade : Window {

        public event EventHandler<UpgradeEventArgs> OnUpgradeRoute;

        public class UpgradeEventArgs : EventArgs {
            public Bootstrap.UpgradeRoute Route;

            public UpgradeEventArgs (Bootstrap.UpgradeRoute d)
                => Route = d;
        }

        public DialogUpgrade () {
            InitializeComponent ();

            // Populate UI strings per language selection
            dlgUpgrade.Title = App.Language.Localize ("UPGRADE:Upgrade");
            lblUpdateAvailable.Content = App.Language.Localize ("UPGRADE:UpdateAvailable");
            lblInstall.Content = App.Language.Localize ("UPGRADE:DownloadInstall");
            lblWebsite.Content = App.Language.Localize ("UPGRADE:OpenDownloadPage");
            lblDelay.Content = App.Language.Localize ("UPGRADE:Later");
            lblMute.Content = App.Language.Localize ("UPGRADE:Mute");

            this.Focus ();
        }

        private void btnInstall_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute (this, new UpgradeEventArgs (Bootstrap.UpgradeRoute.INSTALL));
            Close ();
        }

        private void btnWebsite_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute (this, new UpgradeEventArgs (Bootstrap.UpgradeRoute.WEBSITE));
            Close ();
        }

        private void btnDelay_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute (this, new UpgradeEventArgs (Bootstrap.UpgradeRoute.DELAY));
            Close ();
        }

        private void btnMute_Click (object sender, RoutedEventArgs e) {
            OnUpgradeRoute (this, new UpgradeEventArgs (Bootstrap.UpgradeRoute.MUTE));
            Close ();
        }
    }
}