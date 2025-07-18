/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Diagnostics;

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
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Init)}");
                return;
            }

            // Populate UI strings per language selection

            this.GetControl<Window> ("dlgUpgrade").Title = Instance.Language.Localize ("UPGRADE:Upgrade");
            this.GetControl<Label> ("lblUpdateAvailable").Content = Instance.Language.Localize ("UPGRADE:UpdateAvailable");
            this.GetControl<Label> ("lblWebsite").Content = Instance.Language.Localize ("UPGRADE:OpenDownloadPage");
            this.GetControl<Label> ("lblDelay").Content = Instance.Language.Localize ("UPGRADE:Later");
            this.GetControl<Label> ("lblMute").Content = Instance.Language.Localize ("UPGRADE:Mute");
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