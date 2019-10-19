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
        private double DrawLength;
        private int IndexOffset;
        private string WaveName;

        /* Settings for display */
        private double DisplayYOffset = 0;

        /* Settings for editing vertices */
        private double EditYAmplitude = 0.1;
        private Image referenceImage = new Image ();

        /* Drawing variables, offsets and multipliers */
        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        /* For Save vs. Save As*/
        private string FilePath;

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

            FilePath = "";

            DrawResolution = 10;
            intDrawResolution.Value = 10;

            DrawLength = 1;
            dblDrawLength.Value = 1;

            IndexOffset = 0;
            intIndexOffset.Value = 0;

            Vertices = new List<Vertex> ();
            for (int i = 0; i < ((DrawLength * 1000) / DrawResolution); i++)
                Vertices.Add (new Vertex () { Y = 0 });

            UpdateWave ();
        }

        private void SaveFile () {
            txtWaveName.Text = txtWaveName.Text.Trim ().Replace (' ', '_');
            WaveName = txtWaveName.Text;

            if (String.IsNullOrEmpty (WaveName)) {
                MessageBox.Show (
                        "You must enter a name for the Waveform in order to save.",
                        "Waveform Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (String.IsNullOrEmpty (FilePath)) {
                SaveAsFile ();
                return;
            }

            using (StreamWriter sw = new StreamWriter (FilePath)) {
                StringBuilder sb = new StringBuilder ();

                sb.AppendLine (String.Format ("{0}:{1}", "WaveName", WaveName));
                sb.AppendLine (String.Format ("{0}:{1}", "DrawResolution", DrawResolution));
                sb.AppendLine (String.Format ("{0}:{1}", "IndexOffset", IndexOffset));

                StringBuilder sbVert = new StringBuilder ();
                for (int i = 0; i < Vertices.Count; i++)
                    sbVert.Append (String.Format ("({0} {1}) ", i, Math.Round (Vertices [i].Y, 2)));

                sb.AppendLine (String.Format ("{0}:{1}", "Vertices", sbVert.ToString ().Trim ()));

                sw.Write (sb.ToString ().Trim ());

                sw.Close ();
            }

            MessageBox.Show (
                        "File saved successfully.",
                        "File Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        private void SaveAsFile () {
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();
            dlgSave.FileName = String.Format ("{0}.iiwf", WaveName);
            dlgSave.Filter = "Infirmary Integrated waveform files (*.iiwf)|*.iiwf|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true)
                FilePath = dlgSave.FileName;

            SaveFile ();
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

                                    case "WaveName": WaveName = pValue; break;
                                    case "DrawResolution": DrawResolution = int.Parse (pValue); break;
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
                        DrawLength = (Vertices.Count * (double)DrawResolution) / 1000;
                        FilePath = dlgLoad.FileName;
                    } catch {
                        LoadFail ();
                    } finally {
                        sRead.Close ();
                    }
                }
            }

            UpdateUI ();
            UpdateWave ();
        }

        private void LoadFail () {
            MessageBox.Show (
                    "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                    "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SetBackground () {
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Image Files(*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true) {
                cnvDrawing.Children.Remove (referenceImage);

                referenceImage = new Image ();
                BitmapImage bi = new BitmapImage (new Uri (dlgLoad.FileName, UriKind.Absolute));

                referenceImage = new Image {
                    Width = bi.Width,
                    Height = bi.Height,
                    Source = bi,
                    Stretch = Stretch.Fill
                };

                cnvDrawing.Children.Add (referenceImage);
                Canvas.SetTop (referenceImage, 0);
                Canvas.SetLeft (referenceImage, 0);

                SetZAxes ();
            }
        }

        private void RemoveBackground () {
            cnvDrawing.Children.Remove (referenceImage);
        }

        private void UpdateUI () {
            txtWaveName.Text = WaveName;
            intDrawResolution.Value = DrawResolution;
            dblDrawLength.Value = Math.Round ((decimal)DrawLength, 1);
            intIndexOffset.Value = IndexOffset;
        }

        private void UpdateWave () {
            CalculateDrawOffsets ();

            if (Vertices == null || Vertices.Count < 2)
                return;

            TranslatePointsToPixels ();
            DrawWave ();

            DrawReferences ();
            DrawOffsetReference ();
            SetZAxes ();
        }

        private void CalculateDrawOffsets () {
            /* +2 accounts for beginning and end margin */
            drawXMultiplier = cnvDrawing.ActualWidth / (((DrawLength * 1000) / DrawResolution) + 2);
            drawYMultiplier = -cnvDrawing.ActualHeight / 2;

            drawXOffset = (int)drawXMultiplier;
            drawYOffset = (int)cnvDrawing.ActualHeight - (int)(cnvDrawing.ActualHeight * ((DisplayYOffset + 1) / 2));
        }

        private void TranslatePixelToPoint (int vertexIndex) {
            SetVertex (vertexIndex, (Vertices [vertexIndex].Pixel.Y - drawYOffset) / drawYMultiplier);
        }

        private void TranslatePointToPixel (int vertexIndex) {
            Vertices [vertexIndex].Pixel = new Point (
                        Utility.Clamp ((int)((vertexIndex * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth),
                        Utility.Clamp ((int)((Vertices [vertexIndex].Y * drawYMultiplier) + drawYOffset), 0, cnvDrawing.ActualHeight));
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
                drawContext.BeginFigure (new Point (0, Utility.Clamp (yOffset, 0, cnvDrawing.ActualHeight)), true, false);
                drawContext.LineTo (new Point (cnvDrawing.ActualWidth, Utility.Clamp (yOffset, 0, cnvDrawing.ActualHeight)), true, true);
            }

            drawGeometry.Freeze ();
            pathElement.Data = drawGeometry;
        }

        private void DrawOffsetReference () {
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new Point (Utility.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth), 0), true, false);
                drawContext.LineTo (new Point (Utility.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.ActualWidth), cnvDrawing.ActualHeight), true, true);
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

        private void TrimWave () {
            TrimWaveStart ();
            TrimWaveEnd ();
        }

        private void TrimWaveStart () {
            int removed = 0;

            for (int i = 0; i < Vertices.Count - 2 && i < IndexOffset; i++) {
                if (Vertices [i].Y == 0 && Vertices [i + 1].Y == 0) {
                    Vertices.RemoveAt (i);
                    removed++;
                    i--;
                } else {
                    break;
                }
            }

            IndexOffset -= removed;
            intIndexOffset.Value = IndexOffset;

            DrawLength = ((double)Vertices.Count * (double)DrawResolution) / 1000;
            dblDrawLength.Value = (decimal)DrawLength;

            UpdateWave ();
        }

        private void TrimWaveEnd () {
            for (int i = Vertices.Count - 2; i > 2 && i > IndexOffset; i--) {
                if (Vertices [i].Y == 0 && Vertices [i + 1].Y == 0) {
                    Vertices.RemoveAt (i + 1);
                } else {
                    break;
                }
            }

            DrawLength = ((double)Vertices.Count * (double)DrawResolution) / 1000;
            dblDrawLength.Value = (decimal)DrawLength;
            UpdateWave ();
        }

        private void TrimWaveOffset () {
            for (int i = 0; i < IndexOffset;) {
                Vertices.RemoveAt (i);
                IndexOffset--;
            }

            intIndexOffset.Value = IndexOffset;
            DrawLength = ((double)Vertices.Count * (double)DrawResolution) / 1000;
            dblDrawLength.Value = (decimal)DrawLength;
            UpdateWave ();
        }

        private void SetVertex (int index, double amount) {
            if (index < 0 || index > Vertices.Count)
                return;

            Vertices [index].Y = Utility.Clamp (amount, -1.0, 1.0);
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

            Vertices [index].Y = Utility.Clamp ((Vertices [index].Y + (EditYAmplitude * amount)), -1.0, 1.0);
            TranslatePointToPixel (index);
            DrawWave ();
        }

        private void MoveReference (double amount) {
            DisplayYOffset = Utility.Clamp ((DisplayYOffset + (amount * 0.1)), -1.0, 1.0);
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

        private void SetZAxes () {
            /* Ensures all cnvDrawing.Children aren't overlapping improperly */
            Canvas.SetZIndex (referenceImage, 1);
            Canvas.SetZIndex (pathReferenceHigh, 2);
            Canvas.SetZIndex (pathReferenceMid, 2);
            Canvas.SetZIndex (pathReferenceLow, 2);
            Canvas.SetZIndex (pathIndexOffset, 3);
            Canvas.SetZIndex (pathWave, 4);
        }

        private void MenuItemNew_Click (object sender, RoutedEventArgs e)
            => NewFile ();

        private void MenuItemLoad_Click (object sender, RoutedEventArgs e)
            => LoadFile ();

        private void MenuItemSave_Click (object sender, RoutedEventArgs e)
            => SaveFile ();

        private void MenuItemSaveAs_Click (object sender, RoutedEventArgs e)
            => SaveAsFile ();

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void MenuItemSetBackground_Click (object sender, RoutedEventArgs e)
            => SetBackground ();

        private void MenuItemRemoveBackground_Click (object sender, RoutedEventArgs e)
            => RemoveBackground ();

        private void MenuItemTrimWave_Click (object sender, RoutedEventArgs e)
            => TrimWave ();

        private void MenuItemTrimWaveStart_Click (object sender, RoutedEventArgs e)
            => TrimWaveStart ();

        private void MenuItemTrimWaveEnd_Click (object sender, RoutedEventArgs e)
            => TrimWaveEnd ();

        private void MenuItemTrimOffset_Click (object sender, RoutedEventArgs e)
            => TrimWaveOffset ();

        private void btnApplyResolutions_Click (object sender, RoutedEventArgs e) {
            txtWaveName.Text = txtWaveName.Text.Trim ().Replace (' ', '_');
            WaveName = txtWaveName.Text;
            DrawResolution = intDrawResolution.Value ?? 10;
            DrawLength = (double)(dblDrawLength.Value ?? 1);
            IndexOffset = intIndexOffset.Value ?? 0;

            Vertices = new List<Vertex> ();
            for (int i = 0; i < ((DrawLength * 1000) / DrawResolution); i++)
                Vertices.Add (new Vertex () { Y = 0 });

            UpdateWave ();
        }

        private void cnvDrawing_KeyDown (object sender, KeyEventArgs e) {
            if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl))
                return; // Prevent keyboard shortcuts from being "handled" and not actually running!

            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift)) {
                switch (e.Key) {
                    default: break;

                    case Key.Up:
                        referenceImage.Height -= 10;
                        break;

                    case Key.Down:
                        referenceImage.Height += 10;
                        break;

                    case Key.Left:
                        referenceImage.Width -= 10;
                        break;

                    case Key.Right:
                        referenceImage.Width += 10;
                        break;
                }
            } else {
                switch (e.Key) {
                    default: break;

                    case Key.Up:
                        Canvas.SetTop (referenceImage,
                            Utility.Clamp (Canvas.GetTop (referenceImage) - 10, 0,
                            cnvDrawing.ActualHeight - referenceImage.ActualHeight));
                        break;

                    case Key.Down:
                        Canvas.SetTop (referenceImage,
                            Utility.Clamp (Canvas.GetTop (referenceImage) + 10, 0,
                            cnvDrawing.ActualHeight - referenceImage.ActualHeight));
                        break;

                    case Key.Left:
                        Canvas.SetLeft (referenceImage,
                            Utility.Clamp (Canvas.GetLeft (referenceImage) - 10, 0,
                            cnvDrawing.ActualWidth - referenceImage.ActualWidth));
                        break;

                    case Key.Right:
                        Canvas.SetLeft (referenceImage,
                            Utility.Clamp (Canvas.GetLeft (referenceImage) + 10, 0,
                            cnvDrawing.ActualWidth - referenceImage.ActualWidth));
                        break;
                }
            }

            e.Handled = true;
        }

        private void cnvDrawing_MouseDown (object sender, MouseButtonEventArgs e) {
            Point pos = e.GetPosition (sender as IInputElement);
            int index = NearestVertexByXAxis (pos);

            if (e.ChangedButton == MouseButton.Left) {
                cnvDrawing.Focus ();

                if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl)) {
                    MoveOffset (index);
                } else {
                    SetVertexToPixel (index, pos);
                }
            }
        }

        private void cnvDrawing_MouseMove (object sender, MouseEventArgs e) {
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl))
                    return;                                 // Moving index offset

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
            }
        }
    }
}