using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using II;

namespace IISE.Controls {

    public partial class ItemStep : UserControl {
        /* Data structures */
        public Scenario.Step Step = new Scenario.Step ();

        public ItemStepEnd IStepEnd;
        public List<Progression> Progressions = new List<Progression> ();

        /* Exposed properties */
        public string? UUID { get { return Step.UUID; } }

        /* Permanents for reference/interface styling */
        
        public static Color
            Fill_Default = Color.FromRgb(221, 221, 221),
            Fill_FirstStep = Color.FromRgb(56,118,29),
            Fill_Linked = Color.FromRgb(176,214,139),
            Fill_Unlinked = Color.FromRgb(234,153,153);
            

        public static double
            StrokeThickness_Default = .5d,
            StrokeThickness_Selected = 2.0d;
      
        public II.Physiology Physiology {
            get { return Step.Physiology; }
            set { Step.Physiology = value; }
        }

        public new string Name {
            get { return Step.Name ?? ""; }
            set {
                Step.Name = value;
                this.GetControl<Label> ("lblName").Content = value;
            }
        }

        public string Description {
            get { return Step.Description ?? ""; }
            set {
                Step.Description = value;
                this.GetControl<Label> ("lblDescription").Content = value;
            }
        }

        public class StepEnd : Border {
            public ItemStep Step;

            public StepEnd (ItemStep step) {
                Step = step;
            }
        }

        public class Progression : Line {
            public ItemStep Step;
            public ItemStep StepTo;
            public ItemStepEnd StepEnd;
            public Scenario.Step.Progression StepProgression;

            public Progression (ItemStep step, ItemStep stepTo, ItemStepEnd stepEnd, Scenario.Step.Progression stepProgression) {
                Stroke = Brushes.Black;
                StrokeThickness = 1d;

                Step = step;
                StepTo = stepTo;
                StepEnd = stepEnd;
                StepProgression = stepProgression;
            }
        }

        public ItemStep () {
            InitializeComponent ();

            IStepEnd = this.GetControl<ItemStepEnd> ("iseStepEnd");
            _ = IStepEnd.SetStep (this);

            this.PointerReleased += SnapToGrid;
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task UpdateViewModel () {
            this.GetControl<Label> ("lblName").Content = Name;
            this.GetControl<Label> ("lblDescription").Content = Description;

            return Task.CompletedTask;
        }

        public Task SetStep_Border (bool isSelected) {
            Border step = this.GetControl<Border> ("brdStep");
            step.BorderThickness = new Thickness(isSelected ? StrokeThickness_Selected : StrokeThickness_Default);

            return Task.CompletedTask;
        }

        public Task SetStep_Fill (Color color) {
            Border step = this.GetControl<Border> ("brdStep");
            step.Background = new SolidColorBrush(color);

            return Task.CompletedTask;
        }

        public async Task SetEndStep_Border (bool isSelected)
            => await IStepEnd.SetEndStep_Border (isSelected);

        public async Task SetEndStep_Fill (Color color)
            => await IStepEnd.SetEndStep_Fill (color);

        public void SnapToGrid (object? o, PointerReleasedEventArgs e) {
            int interval = 10;
            
            Border step = this.GetControl<Border> ("brdStep");
            
            Canvas.SetLeft (this, II.Math.RoundOff (this.Bounds.Position.X, interval) - step.BorderThickness.Left);
            Canvas.SetTop (this, II.Math.RoundOff (this.Bounds.Position.Y, interval) - step.BorderThickness.Top);
        }
    }
}