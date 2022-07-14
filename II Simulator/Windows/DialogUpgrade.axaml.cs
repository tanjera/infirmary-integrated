using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;

namespace IISIM {

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
#if DEBUG
            this.AttachDevTools ();
#endif

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

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init () {
            // Populate UI strings per language selection
            if (Instance is not null) {
                this.FindControl<Window> ("dlgUpgrade").Title = Instance.Language.Localize ("UPGRADE:Upgrade");
                this.FindControl<Label> ("lblUpdateAvailable").Content = Instance.Language.Localize ("UPGRADE:UpdateAvailable");
                this.FindControl<Label> ("lblWebsite").Content = Instance.Language.Localize ("UPGRADE:OpenDownloadPage");
                this.FindControl<Label> ("lblDelay").Content = Instance.Language.Localize ("UPGRADE:Later");
                this.FindControl<Label> ("lblMute").Content = Instance.Language.Localize ("UPGRADE:Mute");
            }
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