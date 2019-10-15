using System;
using System.Collections.Generic;
using System.IO;
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

        /* Waveform settings */
        private int DrawResolution;
        private int DrawLength;
        private int IndexOffset;

        /* Settings for display */
        private double DisplayYOffset = 0;

        /* Settings for editing vertices */
        private double EditYAmplitude = 0.1;

        /* Drawing variables, offsets and multipliers */
        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        // Define WPF UI commands for binding
        private ICommand icNewFile, icLoadFile, icSaveFile;

        public ICommand IC_NewFile { get { return icNewFile; } }
        public ICommand IC_LoadFile { get { return icLoadFile; } }
        public ICommand IC_SaveFile { get { return icSaveFile; } }

        public Editor () {
            InitializeComponent ();
            DataContext = this;

            Vertices = new List<Vertex> ();

            // Initiate ICommands for KeyBindings
            icNewFile = new ActionCommand (() => NewFile ());
            icLoadFile = new ActionCommand (() => LoadFile ());
            icSaveFile = new ActionCommand (() => SaveFile ());
        }

        private void NewFile () {
            if (MessageBox.Show (
                    "Are you sure you want to create a new scenario? All unsaved work will be lost!",
                    "Create New Scenario?",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;

            DrawResolution = 100;
            intDrawResolution.Value = 100; ;

            DrawLength = 1;
            intDrawLength.Value = 1; ;

            IndexOffset = 0;
            intIndexOffset.Value = 0; ;

            Vertices = new List<Vertex> ();
            for (int i = 0; i < (DrawResolution * DrawLength); i++)
                Vertices.Add (new Vertex () { Y = 0 });

            UpdateWave ();
        }

        private void SaveFile () {
            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated waveform files (*.iiwf)|*.iiwf|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    StringBuilder sb = new StringBuilder ();

                    sb.AppendLine (String.Format ("{0}:{1}", "DrawResolution", DrawResolution));
                    sb.AppendLine (String.Format ("{0}:{1}", "DrawLength", DrawLength));
                    sb.AppendLine (String.Format ("{0}:{1}", "IndexOffset", IndexOffset));

                    StringBuilder sbVert = new StringBuilder ();
                    for (int i = 0; i < Vertices.Count; i++)
                        sbVert.Append (String.Format ("({0} {1}) ", i, System.Math.Round (Vertices [i].Y, 2)));

                    sb.AppendLine (String.Format ("{0}:{1}", "Vertices", sbVert.ToString ().Trim ()));

                    StreamWriter sw = new StreamWriter (s);
                    sw.Write (sb.ToString ().Trim ());

                    sw.Close ();
                    s.Close ();
                }
            }
        }

        private void LoadFile () {
            Stream s;
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated waveform files (*.iiwf)|*.iiwf|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true) {
                if ((s = dlgLoad.OpenFile ()) != null) {
                    StreamReader sr = new StreamReader (s);
                    string file = sr.ReadToEnd ().Trim ();
                    sr.Close ();
                    s.Close ();

                    StringReader sRead = new StringReader (file);

                    try {
                        string line;
                        while ((line = sRead.ReadLine ()) != null) {
                            if (line.Contains (":")) {
                                string pName = line.Substring (0, line.IndexOf (':')),
                                        pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                                switch (pName) {
                                    default: break;
                                    case "DrawResolution": DrawResolution = int.Parse (pValue); break;
                                    case "DrawLength": DrawLength = int.Parse (pValue); break;
                                    case "IndexOffset": IndexOffset = int.Parse (pValue); break;

                                    case "Vertices":
                                        Vertices = new List<Vertex> ();
                                        while (pValue.Length > 0) {
                                            if (!pValue.Trim ().StartsWith ("(") || !pValue.Contains (")"))
                                                break;

                                            /* Pull current coordinate set from string of coordinates */
                                            string coord = pValue.Trim ().Substring (0, pValue.IndexOf (")") + 1);
                                            pValue = pValue.Substring (coord.Length).Trim ();   // And remove the current coordinate from pValue

                                            /* Process the current coordinate and add to Vertices */
                                            string [] coords = coord.Trim ('(', ')').Split (' ');
                                            int x = int.Parse (coords [0]);
                                            double y = double.Parse (coords [1]);

                                            if (Vertices.Count == x)
                                                Vertices.Add (new Vertex (y));
                                            else {
                                                LoadFail ();
                                                return;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    } catch {
                        LoadFail ();
                    } finally {
                        sRead.Close ();
                    }
                }
            }

            UpdateWave ();
        }

        private void LoadFail () {
            MessageBox.Show (
                    "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                    "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateWave () {
            CalculateDrawOffsets ();

            if (Vertices == null || Vertices.Count < 2)
                return;

            TranslatePointsToPixels ();
            DrawWave ();

            DrawReferences ();
            DrawOffsetReference ();
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

        private void DrawReference (System.Windows.Shapes.Path pathElement, int yOffset) {
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new Point (0, Math.Clamp (yOffset, 0, cnvDrawing.ActualHeight)), true, false);
                drawContext.LineTo (new Point (cnvDrawing.ActualWidth, Math.Clamp (yOffset, 0, cnvDrawing.ActualHeight)), true, true);
            }

            drawGeometry.Freeze ();
            pathElement.Data = drawGeometry;
        }

        private void DrawOffsetReference () {
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new Point (Math.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth), 0), true, false);
                drawContext.LineTo (new Point (Math.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth), cnvDrawing.ActualHeight), true, true);
            }

            drawGeometry.Freeze ();
            pathIndexOffset.Data = drawGeometry;
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
            if (index < 0 || index > Vertices.Count)
                return;

            Vertices [index].Y = Math.Clamp (amount, -1.0, 1.0);
            TranslatePointToPixel (index);
            DrawWave ();
        }

        private void SetVertexToPixel (int index, Point position) {
            if (index < 0 || index > Vertices.Count)
                return;

            Vertices [index].Pixel.Y = position.Y;
            TranslatePixelToPoint (index);
        }

        private void MoveVertex (int index, double amount) {
            if (index < 0 || index > Vertices.Count)
                return;

            Vertices [index].Y = Math.Clamp ((Vertices [index].Y + (EditYAmplitude * amount)), -1.0, 1.0);
            TranslatePointToPixel (index);
            DrawWave ();
        }

        private void MoveReference (double amount) {
            DisplayYOffset = Math.Clamp ((DisplayYOffset + (amount * 0.1)), -1.0, 1.0);
            UpdateWave ();      // Calculates draw offsets as well
            DrawReferences ();
        }

        private void MoveOffset (int index) {
            intIndexOffset.Value = index;
            IndexOffset = index;

            DrawOffsetReference ();
        }

        private int NearestVertexByDistance (Point refPoint) {
            if (Vertices == null || Vertices.Count == 0)
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
            if (Vertices == null || Vertices.Count == 0)
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

        private void MenuItemNew_Click (object sender, RoutedEventArgs e)
            => NewFile ();

        private void MenuItemLoad_Click (object sender, RoutedEventArgs e)
            => LoadFile ();

        private void MenuItemSave_Click (object sender, RoutedEventArgs e)
            => SaveFile ();

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void btnApplyResolutions_Click (object sender, RoutedEventArgs e) {
            DrawResolution = intDrawResolution.Value ?? 0;
            DrawLength = intDrawLength.Value ?? 0;
            IndexOffset = intIndexOffset.Value ?? 0;

            Vertices = new List<Vertex> ();
            for (int i = 0; i < (DrawResolution * DrawLength); i++)
                Vertices.Add (new Vertex () { Y = 0 });

            UpdateWave ();
        }

        private void cnvDrawing_MouseDown (object sender, MouseButtonEventArgs e) {
            Point pos = e.GetPosition (sender as IInputElement);
            int index = NearestVertexByXAxis (pos);

            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl)) {
                    MoveOffset (index);
                } else {
                    SetVertexToPixel (index, pos);
                }
            }
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