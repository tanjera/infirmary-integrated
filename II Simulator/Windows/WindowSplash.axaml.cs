/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IISIM {

    public partial class WindowSplash : Window {

        public WindowSplash () {
            InitializeComponent ();

            DataContext = this;
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }
    }
}