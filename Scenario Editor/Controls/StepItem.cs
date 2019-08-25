using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace II.Scenario_Editor.Controls {

    public sealed class StepItem : Shape {
        public Scenario.Step Step = new Scenario.Step ();
        public Label Label = new Label ();

        public StepItem () {
            Label.IsHitTestVisible = false;
        }

        protected override Geometry DefiningGeometry {
            get { return new RectangleGeometry (new Rect (0, 0, this.Width, this.Height)); }
        }

        public Patient Patient {
            get { return Step.Patient; }
        }

        public StepItem Duplicate () {
            StepItem dup = new StepItem ();

            dup.Width = this.Width;
            dup.Height = this.Height;
            Canvas.SetLeft (dup, Canvas.GetLeft (this) + 10);
            Canvas.SetTop (dup, Canvas.GetTop (this) + 10);

            dup.Label.Content = this.Label.Content?.ToString ();

            dup.Step.Name = this.Step.Name;
            dup.Step.Description = this.Step.Description;
            dup.Step.Patient.Load_Process (this.Step.Patient.Save ());
            dup.Step.ProgressTime = this.Step.ProgressTime;
            foreach (Scenario.Step.Progression p in this.Step.Progressions)
                dup.Step.Progressions.Add (new Scenario.Step.Progression (p.Description, p.DestinationIndex));

            return dup;
        }
    }
}