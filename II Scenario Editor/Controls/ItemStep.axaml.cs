using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace II_Scenario_Editor.Controls {

    public partial class ItemStep : UserControl {

        public ItemStep () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}