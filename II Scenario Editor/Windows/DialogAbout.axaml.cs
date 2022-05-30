using System;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;

namespace II_Scenario_Editor {

    public partial class DialogAbout : Window {

        public DialogAbout () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init () {
            DataContext = this;

            this.FindControl<Label> ("lblVersion").Content = String.Format ("Version {0}",
                Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0");
        }

        private void Hyperlink_Website (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("http://www.infirmary-integrated.com/");

        private void Hyperlink_GitRepo (object sender, RoutedEventArgs e)
            => II.InterOp.OpenBrowser ("https://github.com/tanjera/infirmary-integrated");
    }
}