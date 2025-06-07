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

namespace IISE.Controls {

    public partial class ItemStepEnd : UserControl {
        public ItemStep Step;

        /* Permanents for reference/interface styling */

        
        public static Color
            Fill_Default = Color.FromRgb(221, 221, 221),
            Fill_NoProgressions = Color.FromRgb(255, 78, 54),
            Fill_NoDefaultProgression = Color.FromRgb(212, 162, 155),
            Fill_NoOptionalProgression = Color.FromRgb(222, 230, 133),
            Fill_MultipleProgressions = Color.FromRgb(185, 207, 163);

        public static double
            Thickness_Default = .5d,
            Thickness_Selected = 2.0d;


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
            Border end = this.GetControl<Border> ("brdStepEnd");
            end.BorderThickness = new Thickness(isSelected ? Thickness_Selected : Thickness_Default);

            return Task.CompletedTask;
        }

        public Task SetEndStep_Fill (Color color) {
            Border end = this.GetControl<Border> ("brdStepEnd");
            end.Background = new SolidColorBrush(color);

            return Task.CompletedTask;
        }
    }
}