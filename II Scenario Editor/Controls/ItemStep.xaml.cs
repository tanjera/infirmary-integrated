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

    public partial class ItemStep : UserControl {
        /* Interface items */
        public UIEStep IStep;
        public UIEProgression IProgression;
        public Label ILabel { get { return lblName; } }

        /* Data structures */
        public Scenario.Step Step = new Scenario.Step ();

        /* Exposed properties */

        public double Left {
            get { return double.IsNaN (Canvas.GetLeft (this)) ? (double)0 : Canvas.GetLeft (this); }
            set { Canvas.SetLeft (this, value); }
        }

        public double Top {
            get { return double.IsNaN (Canvas.GetTop (this)) ? (double)0 : Canvas.GetTop (this); }
            set { Canvas.SetTop (this, value); }
        }

        public Patient Patient {
            get { return Step.Patient; }
            set { Step.Patient = value; }
        }

        /* Default values */
        private readonly Point MinSize_IStep = new Point (50, 50);
        private readonly Point MinSize_IProgression = new Point (30, 30);

        private readonly Brush Fill = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
                            Stroke = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#555555"));

        public ItemStep () {
            InitializeComponent ();

            IStep = new UIEStep (this);
            IProgression = new UIEProgression (this);

            ILabel.IsHitTestVisible = false;
        }

        public void Init () {
            IStep.Width = MinSize_IStep.X;                              // Will at least be 50 x 50
            IStep.Height = MinSize_IStep.Y;
            IStep.HorizontalAlignment = HorizontalAlignment.Stretch;    // Will fill the grid space
            IStep.Fill = Fill;
            IStep.Stroke = Stroke;
            IStep.StrokeThickness = 1.0;
            IStep.Margin = new Thickness (3);
            gridMain.Children.Add (IStep);
            Grid.SetColumn (IStep, 0);

            IProgression.Width = MinSize_IProgression.X;
            IProgression.Height = MinSize_IProgression.Y;
            IProgression.Fill = Fill;
            IProgression.Stroke = Stroke;
            IProgression.StrokeThickness = 0.75;
            IProgression.VerticalAlignment = VerticalAlignment.Center;
            IProgression.Margin = new Thickness (3);
            gridMain.Children.Add (IProgression);
            Grid.SetColumn (IProgression, 1);

            Grid.SetZIndex (IStep, 0);
            Grid.SetZIndex (IProgression, 1);
            Grid.SetZIndex (ILabel, 2);
        }

        public void SetName (string name) {
            Step.Name = name;
            ILabel.Content = name;
        }

        public ItemStep Duplicate () {
            ItemStep dup = new ItemStep ();

            // Copy interface properties and interface item properties
            dup.ILabel.Content = ILabel.Content?.ToString ();

            Canvas.SetLeft (dup, Canvas.GetLeft (this) + 10);
            Canvas.SetTop (dup, Canvas.GetTop (this) + 10);

            // Copy data structures
            dup.Step.Name = Step.Name;
            dup.Step.Description = Step.Description;

            dup.Step.Patient.Load_Process (Step.Patient.Save ());

            dup.Step.ProgressFrom = Step.ProgressFrom;
            dup.Step.ProgressTo = Step.ProgressTo;
            dup.Step.ProgressTimer = Step.ProgressTimer;

            foreach (Scenario.Step.Progression p in this.Step.Progressions)
                dup.Step.Progressions.Add (new Scenario.Step.Progression (p.Description, p.DestinationIndex));

            return dup;
        }

        public sealed class UIEStep : Shape {
            public ItemStep ItemStep;

            public UIEStep (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            protected override Geometry DefiningGeometry {
                get { return new RectangleGeometry (new Rect (0, 0, Math.Max (this.Width, this.MinWidth), Math.Max (this.Height, this.MinHeight))); }
            }
        }

        public sealed class UIEProgression : Shape {
            public ItemStep ItemStep;

            public UIEProgression (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            protected override Geometry DefiningGeometry {
                get { return new EllipseGeometry (new Rect (0, 0, Math.Max (this.Width, this.MinWidth), Math.Max (this.Height, this.MinHeight))); }
            }
        }
    }
}