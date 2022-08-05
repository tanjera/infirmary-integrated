using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace IISIM.Controls {

    public partial class MARHeader : UserControl {
        public string? Date { set => this.FindControl<Label> ("lblDate").Content = value; }
        public string? Time { set => this.FindControl<Label> ("lblTime").Content = value; }

        public bool? Bold {
            set {
                this.FindControl<Label> ("lblDate").FontWeight = value ?? false ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
                this.FindControl<Label> ("lblTime").FontWeight = value ?? false ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
            }
        }

        public MARHeader () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}