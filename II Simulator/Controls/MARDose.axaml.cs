using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IISIM.Controls {

    public partial class MARDose : TextBox {

        public MARDose () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}