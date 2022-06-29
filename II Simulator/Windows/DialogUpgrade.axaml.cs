using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace II_Simulator {

    public partial class DialogUpgrade : Window {

        public DialogUpgrade () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        public event EventHandler<UpgradeEventArgs> OnUpgradeRoute;

        public class UpgradeEventArgs : EventArgs {
            public Bootstrap.UpgradeRoute Route;

            public UpgradeEventArgs (Bootstrap.UpgradeRoute d)
                => Route = d;
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init () {
            DataContext = this;

            // Populate UI strings per language selection
            this.FindControl<Window> ("dlgUpgrade").Title = App.Language.Localize ("UPGRADE:Upgrade");
            this.FindControl<Label> ("lblUpdateAvailable").Content = App.Language.Localize ("UPGRADE:UpdateAvailable");
            this.FindControl<Label> ("lblWebsite").Content = App.Language.Localize ("UPGRADE:OpenDownloadPage");
            this.FindControl<Label> ("lblDelay").Content = App.Language.Localize ("UPGRADE:Later");
            this.FindControl<Label> ("lblMute").Content = App.Language.Localize ("UPGRADE:Mute");
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