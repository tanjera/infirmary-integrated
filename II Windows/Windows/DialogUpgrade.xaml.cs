using II.Localization;
using System;
using System.Windows;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DialogLanguage.xaml
    /// </summary>
    public partial class DialogUpgrade : Window {

        public event EventHandler<DecisionEventArgs> OnDecision;

        public enum Decisions {
            NULL,
            INSTALL,
            WEBSITE,
            DELAY,
            MUTE
        }

        public class DecisionEventArgs : EventArgs {
            public Decisions Decision;

            public DecisionEventArgs (Decisions d)
                => Decision = d;
        }

        public DialogUpgrade () {
            InitializeComponent ();

            // Populate UI strings per language selection
            dlgUpgrade.Title = App.Language.Localize ("UPGRADE:Upgrade");
            lblUpdateAvailable.Content = App.Language.Localize ("UPGRADE:UpdateAvailable");
            btnInstall.Content = App.Language.Localize ("UPGRADE:DownloadInstall");
            btnWebsite.Content = App.Language.Localize ("UPGRADE:OpenDownloadPage");
            btnDelay.Content = App.Language.Localize ("UPGRADE:Later");
            btnMute.Content = App.Language.Localize ("UPGRADE:Mute");

            this.Focus ();
        }

        private void btnInstall_Click (object sender, RoutedEventArgs e) {
            OnDecision (this, new DecisionEventArgs (Decisions.INSTALL));
            Close ();
        }

        private void btnWebsite_Click (object sender, RoutedEventArgs e) {
            OnDecision (this, new DecisionEventArgs (Decisions.WEBSITE));
            Close ();
        }

        private void btnDelay_Click (object sender, RoutedEventArgs e) {
            OnDecision (this, new DecisionEventArgs (Decisions.DELAY));
            Close ();
        }

        private void btnMute_Click (object sender, RoutedEventArgs e) {
            OnDecision (this, new DecisionEventArgs (Decisions.MUTE));
            Close ();
        }
    }
}