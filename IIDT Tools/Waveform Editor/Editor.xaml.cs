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

namespace Waveform_Editor {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Editor : Window {
        /* List of vertices for Waveform */
        private List<Vertex> Vertices;

        /* Waveform resolution and length */
        private int DrawResolution;
        private int DrawLength;

        /* Settings for display */
        private double DisplayYOffset = 0;

        /* Settings for editing vertices */
        private double EditYAmplitude = 0.1;

        /* Drawing variables, offsets and multipliers */
        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        public Editor () {
            InitializeComponent ();

            DrawReferences ();
        }

        private void UpdateWave () {
            CalculateDrawOffsets ();

            if (Vertices == null || Vertices.Count < 2)
                return;

            TranslatePointsToPixels ();
            DrawWave ();
        }

        private void CalculateDrawOffsets () {
            /* +2 accounts for beginning and end margin */
            drawXMultiplier = cnvDrawing.ActualWidth / ((DrawLength * DrawResolution) + 2);
            drawYMultiplier = -cnvDrawing.ActualHeight / 2;

            drawXOffset = (int)drawXMultiplier;
            drawYOffset = (int)cnvDrawing.ActualHeight - (int)(cnvDrawing.ActualHeight * ((DisplayYOffset + 1) / 2));
        }

        private void TranslatePixelToPoint (int vertexIndex) {
            SetVertex (vertexIndex, (Vertices [vertexIndex].Pixel.Y - drawYOffset) / drawYMultiplier);
        }

        private void TranslatePointToPixel (int vertexIndex) {
            Vertices [vertexIndex].Pixel = new Point (
                        Math.Clamp ((int)((vertexIndex * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth),
                        Math.Clamp ((int)((Vertices [vertexIndex].Y * drawYMultiplier) + drawYOffset), 0, cnvDrawing.ActualHeight));
        }

        private void TranslatePointsToPixels () {
            if (Vertices == null || Vertices.Count < 2)
                return;

            for (int i = 0; i < Vertices.Count; i++)
                TranslatePointToPixel (i);
        }

        private void DrawReferences () {
            int offsetHigh = (int)cnvDrawing.ActualHeight - (int)(cnvDrawing.ActualHeight * ((DisplayYOffset + 2) / 2));
            int offsetLow = (int)cnvDrawing.ActualHeight - (int)(cnvDrawing.ActualHeight * ((DisplayYOffset) / 2));

            DrawReference (pathReferenceHigh, offsetHigh);
            DrawReference (pathReferenceLow, offsetLow);
            DrawReference (pathReferenceMid, drawYOffset);
        }

        private void DrawReference (Path pathElement, int yOffset) {
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new Point (0, yOffset), true, false);
                drawContext.LineTo (new Point (cnvDrawing.ActualWidth, yOffset), true, true);
            }

            drawGeometry.Freeze ();
            pathElement.Data = drawGeometry;
        }

        private void DrawWave () {
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (Vertices [0].Pixel, true, false);

                for (int i = 1; i < Vertices.Count; i++)
                    drawContext.LineTo (Vertices [i].Pixel, true, true);
            }

            drawGeometry.Freeze ();
            pathWave.Data = drawGeometry;
        }

        private void SetVertex (int index, double amount) {
            Vertices [index].Y = Math.Clamp (amount, -1.0, 1.0);
            TranslatePointToPixel (index);
            DrawWave ();
        }

        private void SetVertexToPixel (int index, Point position) {
            Vertices [index].Pixel.Y = position.Y;
            TranslatePixelToPoint (index);
        }

        private void MoveVertex (int index, double amount) {
            Vertices [index].Y = Math.Clamp ((Vertices [index].Y + (EditYAmplitude * amount)), -1.0, 1.0);
            TranslatePointToPixel (index);
            DrawWave ();
        }

        private void MoveReference (double amount) {
            DisplayYOffset = Math.Clamp ((DisplayYOffset + (amount * 0.1)), -1.0, 1.0);
            UpdateWave ();      // Calculates draw offsets as well
            DrawReferences ();
        }

        private int NearestVertexByDistance (Point refPoint) {
            if (Vertices.Count == 0)
                return -1;

            int nearestIndex = 0;
            double nearestDistance = Point.Subtract (Vertices [0].Pixel, refPoint).Length;

            for (int i = 1; i < Vertices.Count; i++) {
                double nd = Point.Subtract (Vertices [i].Pixel, refPoint).Length;

                if (nd < nearestDistance) {
                    nearestDistance = nd;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private int NearestVertexByXAxis (Point refPoint) {
            if (Vertices.Count == 0)
                return -1;

            int nearestIndex = 0;
            double nearestDistance = System.Math.Abs (Vertices [0].Pixel.X - refPoint.X);

            for (int i = 1; i < Vertices.Count; i++) {
                double nd = System.Math.Abs (Vertices [i].Pixel.X - refPoint.X);

                if (nd < nearestDistance) {
                    nearestDistance = nd;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private void btnApplyResolutions_Click (object sender, RoutedEventArgs e) {
            DrawResolution = intDrawResolution.Value ?? 0;
            DrawLength = intDrawLength.Value ?? 0;

            Vertices = new List<Vertex> ();
            for (int i = 0; i < (DrawResolution * DrawLength); i++)
                Vertices.Add (new Vertex () { Y = 0 });

            UpdateWave ();
        }

        private void cnvDrawing_MouseDown (object sender, MouseButtonEventArgs e) {
            Point pos = e.GetPosition (sender as IInputElement);
            int index = NearestVertexByXAxis (pos);
            SetVertexToPixel (index, pos);
        }

        private void cnvDrawing_MouseMove (object sender, MouseEventArgs e) {
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                Point pos = e.GetPosition (sender as IInputElement);
                int index = NearestVertexByXAxis (pos);
                SetVertexToPixel (index, pos);
            }
        }

        private void cnvDrawing_MouseWheel (object sender, MouseWheelEventArgs e) {
            /* Determine actions based on key modifiers */
            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift)) {
                /* Shift moves the Y reference point! */
                MoveReference (e.Delta / 120);
            } else if (Keyboard.IsKeyDown (Key.LeftAlt) || Keyboard.IsKeyDown (Key.RightAlt)) {
                /* Alt selects nearest vertex by actual distance (across X and Y axis) */
                MoveVertex (NearestVertexByDistance (e.GetPosition (sender as IInputElement)), e.Delta / 120);
            } else if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl)) {
                /* Ctrl selects nearest vertex by distance only on X axis but with higher editing precision*/
                MoveVertex (NearestVertexByXAxis (e.GetPosition (sender as IInputElement)), e.Delta / 480);
            } else {
                /* No modifier selects nearest vertex by distance only on X axis */
                MoveVertex (NearestVertexByXAxis (e.GetPosition (sender as IInputElement)), e.Delta / 120);
            }
        }
    }
}