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
        public int Index;

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

        public readonly Brush
            Stroke_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#555555")),
            Fill_Default = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
            Fill_StepZero = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b0d68b")),
            Fill_StepEndNoProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#d4a29b")),
            Fill_StepEndNoOptionalProgression = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dee685")),
            Fill_StepEndMultipleProgressions = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#b9cfa3"));

        public readonly double
            StrokeThickness_Default = .5,
            StrokeThickness_Selected = 2.0;

        public ItemStep () {
            InitializeComponent ();

            IStep = new UIEStep (this);
            IStepEnd = new UEIStepEnd (this);

            lblNumber.IsHitTestVisible = false;
            ILabel.IsHitTestVisible = false;
        }

        public void Init () {
            IStep.MinWidth = MinSize_IStep.X;
            IStep.MinHeight = MinSize_IStep.Y;
            IStep.Stretch = Stretch.Fill;
            IStep.RadiusX = 5;
            IStep.RadiusY = 5;
            IStep.Fill = Fill_Default;
            IStep.Stroke = Stroke_Default;
            IStep.StrokeThickness = StrokeThickness_Default;
            IStep.Margin = new Thickness (3);

            Grid.SetColumn (IStep, 0);
            Grid.SetRow (IStep, 0);
            Grid.SetRowSpan (IStep, 2);

            IStepEnd.MinWidth = MinSize_IProgression.X;
            IStepEnd.MinHeight = MinSize_IProgression.Y;
            IStepEnd.Fill = Fill_Default;
            IStepEnd.Stroke = Stroke_Default;
            IStepEnd.StrokeThickness = StrokeThickness_Default;
            IStepEnd.HorizontalAlignment = HorizontalAlignment.Center;
            IStepEnd.VerticalAlignment = VerticalAlignment.Center;
            IStepEnd.Margin = new Thickness (3);

            Grid.SetColumn (IStepEnd, 1);
            Grid.SetRow (IStepEnd, 0);
            Grid.SetRowSpan (IStepEnd, 2);

            Grid.SetZIndex (IStep, 1);
            Grid.SetZIndex (IStepEnd, 1);
            Grid.SetZIndex (lblNumber, 2);
            Grid.SetZIndex (ILabel, 2);

            gridMain.Children.Add (IStep);
            gridMain.Children.Add (IStepEnd);
        }

        public void SetNumber (int index) {
            Index = index;
            lblNumber.Content = Index.ToString ();
            IStep.Fill = (index == 0) ? Fill_StepZero : IStep.Fill = Fill_Default;
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

            /* Specifically DON'T duplicate Progressions!!
             * Because that would cause a mess of Progressions that the user likely doesn't want!
             * Let the user manually add progressions. Duplicating an ItemStep is more for maintaining
             * Patient parameters.
             */

            return dup;
        }

        public sealed class UIEStep : Shape {
            public ItemStep ItemStep;

            public int RadiusX = 0,
                        RadiusY = 0;

            public UIEStep (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            protected override Geometry DefiningGeometry {
                get {
                    return new RectangleGeometry (new Rect (0, 0,
                        System.Math.Max ((double.IsNaN (this.Width) ? 0 : this.Width),
                            double.IsNaN (this.MinWidth) ? 0 : this.MinWidth),
                        System.Math.Max ((double.IsNaN (this.Height) ? 0 : this.Height),
                            double.IsNaN (this.MinHeight) ? 0 : this.MinHeight)),
                            RadiusX, RadiusY);
                }
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
                get {
                    return new EllipseGeometry (new Rect (0, 0,
                            System.Math.Max ((double.IsNaN (this.Width) ? 0 : this.Width),
                                double.IsNaN (this.MinWidth) ? 0 : this.MinWidth),
                            System.Math.Max ((double.IsNaN (this.Height) ? 0 : this.Height),
                                double.IsNaN (this.MinHeight) ? 0 : this.MinHeight)));
                }
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