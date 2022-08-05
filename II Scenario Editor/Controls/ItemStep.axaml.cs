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

        public static Brush
            Fill_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
            Fill_FirstStep = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#38761d")),
            Fill_Linked = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b0d68b")),
            Fill_Unlinked = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#ea9999"));

        public static Thickness
            StrokeThickness_Default = new Thickness (.5d),
            StrokeThickness_Selected = new Thickness (2.0d);

        public II.Record Records {
            get { return Step.Records; }
            set { Step.Records = value; }
        }

        public II.Physiology Physiology {
            get { return Step.Physiology; }
            set { Step.Physiology = value; }
        }

        public new string Name {
            get { return Step.Name ?? ""; }
            set {
                Step.Name = value;
                this.FindControl<Label> ("lblName").Content = value;
            }
        }

        public string Description {
            get { return Step.Description ?? ""; }
            set {
                Step.Description = value;
                this.FindControl<Label> ("lblDescription").Content = value;
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

            IStepEnd = this.FindControl<ItemStepEnd> ("iseStepEnd");
            _ = IStepEnd.SetStep (this);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task UpdateViewModel () {
            this.FindControl<Label> ("lblName").Content = Name;
            this.FindControl<Label> ("lblDescription").Content = Description;

            return Task.CompletedTask;
        }

        public Task SetStep_Border (bool isSelected) {
            Border step = this.FindControl<Border> ("brdStep");
            step.BorderThickness = (isSelected) ? StrokeThickness_Selected : StrokeThickness_Default;

            return Task.CompletedTask;
        }

        public Task SetStep_Fill (Brush brush) {
            Border step = this.FindControl<Border> ("brdStep");
            step.Background = brush;

            return Task.CompletedTask;
        }

        public async Task SetEndStep_Border (bool isSelected)
            => await IStepEnd.SetEndStep_Border (isSelected);

        public async Task SetEndStep_Fill (Brush brush)
            => await IStepEnd.SetEndStep_Fill (brush);
    }
}