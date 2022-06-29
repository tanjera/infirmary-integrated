using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace II_Scenario_Editor.Controls {

    public partial class ItemStepEnd : UserControl {
        public ItemStep Step;

        /* Permanents for reference/interface styling */

        public static Brush
            Fill_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
            Fill_NoProgressions = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#ff4e36")),
            Fill_NoDefaultProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#d4a29b")),
            Fill_NoOptionalProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dee685")),
            Fill_MultipleProgressions = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b9cfa3"));

        public static Thickness
            Thickness_Default = new Thickness (.5d),
            Thickness_Selected = new Thickness (2.0d);

        public ItemStepEnd () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task SetStep (ItemStep step) {
            Step = step;

            return Task.CompletedTask;
        }

        public Task SetEndStep_Border (bool isSelected) {
            Border end = this.FindControl<Border> ("brdStepEnd");
            end.BorderThickness = isSelected ? Thickness_Selected : Thickness_Default;

            return Task.CompletedTask;
        }

        public Task SetEndStep_Fill (Brush brush) {
            Border end = this.FindControl<Border> ("brdStepEnd");
            end.Background = brush;

            return Task.CompletedTask;
        }
    }
}