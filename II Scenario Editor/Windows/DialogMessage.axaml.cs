/* Infirmary Integrated Scenario Editor
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;

namespace IISE {

    public partial class DialogMessage : Window {
        public string? Message { get; set; }
        public Indicators Indicator { get; set; }
        public Options Option { get; set; }
        private Responses? Response;

        public enum Indicators {
            None,
            InfirmaryIntegratedScenarioEditor
        }

        public enum Options {
            OK,
            YesNo
        }

        public enum Responses {
            OK,
            Yes,
            No
        }

        public string [] IconSources = {
            "",
            "avares://Infirmary Integrated Scenario Editor/Resources/Icon_IISE.ico"
        };

        public DialogMessage () {
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
        }

        public void UpdateViewModel () {
            TextBlock lblMessage = this.GetControl<TextBlock> ("lblMessage");
            Image imgIcon = this.GetControl<Image> ("imgIcon");
            Button btnLeft = this.GetControl<Button> ("btnLeft");
            Button btnRight = this.GetControl<Button> ("btnRight");

            lblMessage.Text = string.IsNullOrEmpty (Message) ? "" : Message;

            if (!string.IsNullOrEmpty (IconSources [Indicator.GetHashCode ()])) {
                var asset = AssetLoader.Open (new Uri (IconSources [Indicator.GetHashCode ()]));
                if (asset != null)
                    imgIcon.Source = new Bitmap (asset);
            }

            switch (Option) {
                default:
                case Options.OK:
                    btnRight.IsVisible = true;
                    btnRight.Content = "OK";

                    Response = Responses.OK;            // Default response
                    break;

                case Options.YesNo:
                    btnLeft.IsVisible = true;
                    btnLeft.Content = "No";
                    btnRight.IsVisible = true;
                    btnRight.Content = "Yes";

                    Response = Responses.No;            // Default response
                    break;
            }
        }

        public async Task<Responses?> AsyncShow (Window parent) {
            if (!parent.IsVisible)                    // Avalonia's parent must be visible to attach a window
                parent.Show ();

            UpdateViewModel ();

            this.Activate ();
            await this.ShowDialog (parent);

            return Response;
        }

        public void btnLeft_Click (object sender, RoutedEventArgs e) {
            switch (Option) {
                case Options.YesNo: Response = Responses.No; break;
            }

            this.Close ();
        }

        public void btnRight_Click (object sender, RoutedEventArgs e) {
            switch (Option) {
                case Options.OK: Response = Responses.OK; break;
                case Options.YesNo: Response = Responses.Yes; break;
            }

            this.Close ();
        }
    }
}