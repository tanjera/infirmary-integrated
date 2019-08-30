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

    public sealed class ItemStep : Shape {
        public Scenario.Step Step = new Scenario.Step ();
        public Label Label = new Label ();
        public ItemProgression Progression;

        public ItemStep () {
            Label.IsHitTestVisible = false;
        }

        protected override Geometry DefiningGeometry {
            get { return new RectangleGeometry (new Rect (0, 0, this.Width, this.Height)); }
        }

        public Patient Patient {
            get { return Step.Patient; }
        }

        public void SetName (string name) {
            Step.Name = name;
            Label.Content = name;

            Width = Math.Max (50, Label.ActualWidth + 8);
            if (Progression != null)
                Canvas.SetLeft (Progression, Canvas.GetLeft (this) + this.Width);
        }

        public ItemStep Duplicate () {
            ItemStep dup = new ItemStep ();
            dup.Width = this.Width;
            dup.Height = this.Height;

            ItemProgression prog = new ItemProgression ();
            prog.Width = Progression != null ? Progression.Width : 30;
            prog.Height = Progression != null ? Progression.Height : 30;
            prog.Stroke = Brushes.Black;
            prog.Fill = Brushes.LightSkyBlue;
            dup.Progression = prog;

            Canvas.SetLeft (dup, Canvas.GetLeft (this) + 10);
            Canvas.SetTop (dup, Canvas.GetTop (this) + 10);

            Canvas.SetLeft (prog, Canvas.GetLeft (this) + 10 + this.ActualWidth);
            Canvas.SetTop (prog, Canvas.GetTop (this) + 10);

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