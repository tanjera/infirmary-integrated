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
        public UEIStepEnd IStepEnd;
        public List<UIEProgression> IProgressions = new List<UIEProgression> ();
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
            IStepEnd = new UEIStepEnd (this);

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

            IStepEnd.Width = MinSize_IProgression.X;
            IStepEnd.Height = MinSize_IProgression.Y;
            IStepEnd.Fill = Fill;
            IStepEnd.Stroke = Stroke;
            IStepEnd.StrokeThickness = 0.75;
            IStepEnd.VerticalAlignment = VerticalAlignment.Center;
            IStepEnd.Margin = new Thickness (3);
            gridMain.Children.Add (IStepEnd);
            Grid.SetColumn (IStepEnd, 1);

            Grid.SetZIndex (IStep, 1);
            Grid.SetZIndex (IStepEnd, 1);
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
                dup.Step.Progressions.Add (new Scenario.Step.Progression (p.DestinationIndex, p.Description));

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

            public Point GetPosition (UIElement relativeTo) {
                return this.TranslatePoint (new Point (0, 0), relativeTo);
            }

            public Point GetCenter (UIElement relativeTo) {
                return this.TranslatePoint (new Point (this.ActualWidth / 2, this.ActualHeight / 2), relativeTo);
            }
        }

        public sealed class UEIStepEnd : Shape {
            public ItemStep ItemStep;

            public UEIStepEnd (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            protected override Geometry DefiningGeometry {
                get { return new EllipseGeometry (new Rect (0, 0, Math.Max (this.Width, this.MinWidth), Math.Max (this.Height, this.MinHeight))); }
            }

            public Point GetPosition (UIElement relativeTo) {
                return this.TranslatePoint (new Point (0, 0), relativeTo);
            }

            public Point GetCenter (UIElement relativeTo) {
                return this.TranslatePoint (new Point (this.ActualWidth / 2, this.ActualHeight / 2), relativeTo);
            }
        }

        public class UIEProgression : Shape {
            public ItemStep From, To;
            public UIElement Canvas;

            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }

            protected override Geometry DefiningGeometry {
                get { return new LineGeometry (new Point (X1, Y1), new Point (X2, Y2)); }
            }

            public UIEProgression (ItemStep from, ItemStep to, UIElement canvas) {
                From = from;
                To = to;
                Canvas = canvas;

                Point pFrom = From.IStepEnd.GetCenter (Canvas);
                X1 = pFrom.X;
                Y1 = pFrom.Y;

                Point pTo = To.IStep.GetCenter (Canvas);
                X2 = pTo.X;
                Y2 = pTo.Y;

                Stroke = Brushes.Black;
                StrokeThickness = 1.0;
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Center;
            }

            public void UpdatePositions () {
                Point pFrom = From.IStepEnd.GetCenter (Canvas);
                X1 = pFrom.X;
                Y1 = pFrom.Y;

                Point pTo = To.IStep.GetCenter (Canvas);
                X2 = pTo.X;
                Y2 = pTo.Y;

                InvalidateVisual ();
            }
        }
    }
}