using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IISIM.Controls {

    public partial class MARDrug : TextBox {

        public MARDrug () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}