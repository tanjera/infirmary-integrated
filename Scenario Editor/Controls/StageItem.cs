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

    public sealed class StageItem : Shape {
        public Scenario.Stage Stage = new Scenario.Stage ();
        public Label Label = new Label ();

        public StageItem () {
            Label.IsHitTestVisible = false;
        }

        protected override Geometry DefiningGeometry {
            get { return new RectangleGeometry (new Rect (0, 0, this.Width, this.Height)); }
        }

        public Patient Patient {
            get { return Stage.Patient; }
        }

        public StageItem Duplicate () {
            StageItem dup = new StageItem ();

            dup.Width = this.Width;
            dup.Height = this.Height;
            Canvas.SetLeft (dup, Canvas.GetLeft (this) + 10);
            Canvas.SetTop (dup, Canvas.GetTop (this) + 10);

            dup.Label.Content = this.Label.Content?.ToString ();

            dup.Stage.Name = this.Stage.Name;
            dup.Stage.Description = this.Stage.Description;
            dup.Stage.Patient.Load_Process (this.Stage.Patient.Save ());
            dup.Stage.ProgressionTime = this.Stage.ProgressionTime;
            foreach (Scenario.Stage.Progression p in this.Stage.Progressions)
                dup.Stage.Progressions.Add (new Scenario.Stage.Progression (p.Description, p.DestinationIndex));

            return dup;
        }
    }
}