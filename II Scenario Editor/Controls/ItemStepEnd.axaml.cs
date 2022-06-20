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

        public readonly Brush
            Stroke_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#555555")),
            Fill_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
            Fill_StepZero = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b0d68b")),
            Fill_StepEndNoProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#d4a29b")),
            Fill_StepEndNoOptionalProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dee685")),
            Fill_StepEndMultipleProgressions = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b9cfa3"));

        public readonly Avalonia.Thickness
            StrokeThickness_Default = new Thickness (.5d),
            StrokeThickness_Selected = new Thickness (2.0d);

        public ItemStepEnd () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetStep (ItemStep step) {
            Step = step;
        }

        public void SetBorder_End (bool isSelected) {
            Border end = this.FindControl<Border> ("brdStepEnd");
            end.BorderThickness = (isSelected) ? StrokeThickness_Selected : StrokeThickness_Default;
        }
    }
}