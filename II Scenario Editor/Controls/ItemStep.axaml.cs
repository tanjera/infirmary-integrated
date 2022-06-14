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
        /* Interface items */
        public Rectangle IStep;
        public Circle IStepEnd;
        public List<Line> IProgressions = new List<Line> ();

        public Label ILabel {
            get {
                return this.FindControl<Label> ("lblName");
            }
        }

        /* Default values */
        private readonly Point MinSize_IStep = new Point (50, 50);
        private readonly Point MinSize_IProgression = new Point (30, 30);

        /* Data structures */
        public II.Scenario.Step Step = new II.Scenario.Step ();

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

        public II.Patient Patient {
            get { return Step.Patient; }
            set { Step.Patient = value; }
        }

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

            IStep = new Rectangle (this);
            IStepEnd = new Circle (this);

            lblNumber.IsHitTestVisible = false;
            ILabel.IsHitTestVisible = false;
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetNumber (int index) {
            Label lblNumber = this.FindControl<Label> ("lblNumber");

            Index = index;
            lblNumber.Content = Index.ToString ();

            IStep.Fill = (index == 0) ? Fill_StepZero : IStep.Fill = Fill_Default;
        }

        public void SetName (string name) {
            Step.Name = name;
            ILabel.Content = name;
        }

        public void Init () {
            Label lblName = this.FindControl<Label> ("lblName");
            Label lblNumber = this.FindControl<Label> ("lblNumber");

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
            IStepEnd.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            IStepEnd.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            IStepEnd.Margin = new Thickness (3);

            Grid.SetColumn (IStepEnd, 1);
            Grid.SetRow (IStepEnd, 0);
            Grid.SetRowSpan (IStepEnd, 2);

            IStep.ZIndex = 1;
            IStepEnd.ZIndex = 1;
            lblName.ZIndex = 2;
            lblNumber.ZIndex = 2;

            gridMain.Children.Add (IStep);
            gridMain.Children.Add (IStepEnd);
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

        public sealed class Rectangle : Avalonia.Controls.Shapes.Rectangle {
            public ItemStep ItemStep;

            public int RadiusX = 0,
                        RadiusY = 0;

            public Rectangle (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            public Point GetPosition (Control relativeTo) {
                return this.TranslatePoint (new Point (0, 0), relativeTo) ?? new Point (-1, -1);
            }

            public Point GetCenter (Control relativeTo) {
                return this.TranslatePoint (new Point (this.Width / 2, this.Height / 2), relativeTo) ?? new Point (-1, -1);
            }
        }

        public sealed class Circle : Avalonia.Controls.Shapes.Ellipse {
            public ItemStep ItemStep;

            public Circle (ItemStep itemstep) {
                ItemStep = itemstep;
            }

            public Point GetPosition (Control relativeTo) {
                return this.TranslatePoint (new Point (0, 0), relativeTo) ?? new Point (-1, -1); ;
            }

            public Point GetCenter (Control relativeTo) {
                return this.TranslatePoint (new Point (this.Width / 2, this.Height / 2), relativeTo) ?? new Point (-1, -1); ;
            }
        }

        public class Line : Avalonia.Controls.Shapes.Line {
            public ItemStep From, To;
            public Canvas Canvas;

            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }

            public Line (ItemStep from, ItemStep to, Canvas canvas) {
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
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            }

            public void UpdatePositions () {
                Point pFrom = From.IStepEnd.GetCenter (Canvas);
                X1 = pFrom.X;
                Y1 = pFrom.Y;

                Point pTo = To.IStep.GetCenter (Canvas);
                X2 = pTo.X;
                Y2 = pTo.Y;

                //InvalidateVisual ();
            }
        }
    }
}