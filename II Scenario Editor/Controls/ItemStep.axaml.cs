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

    public partial class ItemStep : UserControl {
        public List<Line> IProgressions = new ();

        /* Data structures */
        public string UUID;
        public ItemStepEnd IStepEnd;
        public II.Scenario.Step Step = new II.Scenario.Step ();

        /* Exposed properties */
        public int Index;

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

        public II.Patient Patient {
            get { return Step.Patient; }
            set { Step.Patient = value; }
        }

        public string? Label {
            get {
                Label lblName = this.FindControl<Label> ("lblName");
                return lblName.Content.ToString ();
            }
        }

        public ItemStep () {
            InitializeComponent ();

            IStepEnd = this.FindControl<ItemStepEnd> ("iseStepEnd");
            IStepEnd.SetStep (this);

            UUID = Guid.NewGuid ().ToString ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetNumber (int index) {
            Label lblNumber = this.FindControl<Label> ("lblNumber");

            Index = index;
            lblNumber.Content = Index.ToString ();
        }

        public void SetName (string? name) {
            if (String.IsNullOrEmpty (name))
                name = "";

            Label lblName = this.FindControl<Label> ("lblName");

            Step.Name = name;
            lblName.Content = name;
        }

        public void SetBorder_Step (bool isSelected) {
            Border step = this.FindControl<Border> ("brdStep");
            step.BorderThickness = (isSelected) ? StrokeThickness_Selected : StrokeThickness_Default;
        }

        public void SetBorder_End (bool isSelected)
            => IStepEnd.SetBorder_End (isSelected);

        public class StepEnd : Border {
            public ItemStep Step;

            public StepEnd (ItemStep step) {
                Step = step;
            }
        }
    }
}