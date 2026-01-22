using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ExcelDataReader;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
// ReSharper disable LocalVariableHidesMember
// ReSharper disable InconsistentNaming


namespace IIDT;

public partial class PanelWaveformEditor : UserControl {
    private Window Control;
    private string SolutionDir = "";
    
    private List<Vertex>? Vertices;

    /* Waveform settings */
    private int DrawResolution;             // In milliseconds
    private int DrawLength;                 // In milliseconds
    private int SystoleLength;              // In milliseconds
    private int IndexOffset;
    private string WaveName;

    /* Settings for display */
    private double DisplayYOffset = 0;

    /* Settings for editing vertices */
    private double EditYAmplitude = 0.1;
    private Bitmap? referenceImage;

    /* Drawing variables, offsets and multipliers */
    private StreamGeometry drawGeometry;
    private StreamGeometryContext drawContext;
    private int drawXOffset, drawYOffset;
    private double drawXMultiplier, drawYMultiplier;

    /* For Save vs. Save As*/
    private string FilePath;
    
    public PanelWaveformEditor (Window control, string solutionDir) {
        Control = control;
        SolutionDir = solutionDir;
        
        InitializeComponent ();
    }
    
    protected override void OnLoaded (RoutedEventArgs e) {
        base.OnLoaded(e);

        Init ();
    }

    private void Init () {
        NumericUpDown numDrawResolution = this.GetControl<NumericUpDown>("numDrawResolution");
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        NumericUpDown numSystoleLength = this.GetControl<NumericUpDown>("numSystoleLength");
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        
        FilePath = "";

        DrawResolution = 10;
        numDrawResolution.Value = 10;

        DrawLength = 1000;
        numDrawLength.Value = 1000;

        SystoleLength = 330;
        numSystoleLength.Value = 330;

        IndexOffset = 0;
        numIndexOffset.Value = 0;

        Vertices = new List<Vertex> ();
        for (int i = 0; i < (DrawLength / DrawResolution); i++)
            Vertices.Add (new Vertex () { Y = 0 });
        
        UpdateWave ();
    }
    
    private async Task NewFile () {
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Are you sure you want to create a new waveform? All unsaved work will be lost!",
                Title = "Create New Waveform?",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.YesNo,
            };

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            if (await dlg.AsyncShow (Control) == DialogMessage.Responses.No)
                return;
        });
        
        Init ();
    }

    private async Task SaveFile () {
        WaveName = this.GetControl<TextBox> ("tbWaveName").Text.Trim ().Replace (' ', '_');

        if (String.IsNullOrEmpty (WaveName)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "You must enter a name for the Waveform in order to save.",
                    Title =  "Waveform Name Required",
                    Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                    Option = DialogMessage.Options.OK,
                };

                if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    Control.Show ();

                await dlg.AsyncShow (Control);
                    
            });
            
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
            sb.AppendLine (String.Format ("{0}:{1}", "SystoleLength", SystoleLength));

            StringBuilder sbVert = new StringBuilder ();
            for (int i = 0; i < Vertices.Count; i++)
                sbVert.Append (String.Format ("({0} {1}) ", i, Math.Round (Vertices [i].Y, 2)));

            sb.AppendLine (String.Format ("{0}:{1}", "Vertices", sbVert.ToString ().Trim ()));

            sw.Write (sb.ToString ().Trim ());

            sw.Close ();
        }

        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Success: The waveform was saved successfully!",
                Title = "Task Completed",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.OK,
            };

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            await dlg.AsyncShow (Control);
        });
    }

    private async Task SaveAsFile () {
        if (!Control.StorageProvider.CanOpen) {
            return;
        }
            
        var fpso = new FilePickerSaveOptions ()
        {
            SuggestedFileName = String.IsNullOrEmpty(WaveName) ? "Waveform" : WaveName,
            DefaultExtension = ".iiwf",
            FileTypeChoices = new[] {
                new FilePickerFileType("Infirmary Integrated waveform files")
                {
                    Patterns = new[] { "*.iiwf" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            SuggestedStartLocation = 
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/"))
        };

        var file = await Control.StorageProvider.SaveFilePickerAsync(fpso);

        if (file?.TryGetLocalPath() is not null) {
            FilePath = file.TryGetLocalPath();
        }

        SaveFile ();
    }

    private async Task LoadFile () {
        if (!Control.StorageProvider.CanOpen) {
            return;
        }
            
        var fpoo = new FilePickerOpenOptions ()
        {
            FileTypeFilter = new[] {
                new FilePickerFileType("Infirmary Integrated waveform files")
                {
                    Patterns = new[] { "*.iiwf" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            AllowMultiple = false,
            SuggestedStartLocation = 
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/"))
        };

        var files = await Control.StorageProvider.OpenFilePickerAsync(fpoo);

        if (files.First()?.TryGetLocalPath() is null) {
            return;
        }
        
        
        StreamReader sr = new StreamReader (files.First()?.TryGetLocalPath());
        string file = sr.ReadToEnd ().Trim ();
        sr.Close ();

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
                        case "SystoleLength": SystoleLength = int.Parse (pValue); break;

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
            DrawLength = Vertices.Count * DrawResolution;
            FilePath = files.First()?.TryGetLocalPath();
        } catch {
            LoadFail ();
        } finally {
            sRead.Close ();
        }
            
        UpdateUI ();
        UpdateWave ();
    }

    private async Task LoadFail () {
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                Title = "Unable to Load File",
                Indicator = DialogMessage.Indicators.Error,
                Option = DialogMessage.Options.OK,
            };

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            await dlg.AsyncShow (Control);
        });
    }

    private async Task SetBackground () {
        Image imgReference = this.GetControl<Image> ("imgReference");
        
        imgReference.Source = null;
        
        if (!Control.StorageProvider.CanOpen) {
            return;
        }
            
        var fpoo = new FilePickerOpenOptions ()
        {
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll },
            AllowMultiple = false,
            SuggestedStartLocation = 
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/"))
        };

        var files = await Control.StorageProvider.OpenFilePickerAsync(fpoo);

        if (files.First()?.TryGetLocalPath() is null) {
            return;
        }
        
        referenceImage = new Bitmap (new FileStream (files.First()?.TryGetLocalPath (), FileMode.Open));

        imgReference.Source = referenceImage;
        
        SetZAxes ();
    }

    private void RemoveBackground () {
        Image imgReference = this.GetControl<Image> ("imgReference");
        
        imgReference.Source = null;
    }

    private void UpdateUI () {
        TextBox tbWaveName = this.GetControl<TextBox> ("tbWaveName");
        NumericUpDown numDrawResolution = this.GetControl<NumericUpDown>("numDrawResolution");
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        NumericUpDown numSystoleLength = this.GetControl<NumericUpDown>("numSystoleLength");
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        
        tbWaveName.Text = WaveName;
        numDrawResolution.Value = DrawResolution;
        numDrawLength.Value = DrawLength;
        numSystoleLength.Value = SystoleLength;
        numIndexOffset.Value = IndexOffset;
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
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");
        
        /* +2 accounts for beginning and end margin */
        drawXMultiplier = cnvDrawing.Bounds.Width / ((DrawLength / DrawResolution) + 2);
        drawYMultiplier = -cnvDrawing.Bounds.Height / 2;

        drawXOffset = (int)drawXMultiplier;
        drawYOffset = (int)cnvDrawing.Bounds.Height - (int)(cnvDrawing.Bounds.Height * ((DisplayYOffset + 1) / 2));
    }

    private void TranslatePixelToPoint (int vertexIndex) {
        SetVertex (vertexIndex, (Vertices [vertexIndex].Pixel.Y - drawYOffset) / drawYMultiplier);
    }

    private void TranslatePointsToPixels () {
        if (Vertices is null || Vertices.Count < 2)
            return;
        
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");
        
        for (int i = 0; i < Vertices.Count; i++) {
            Vertices [i].Pixel = new Point (
                Utility.Clamp ((int)((i * drawXMultiplier) + drawXOffset), 0, cnvDrawing.Bounds.Width),
                Utility.Clamp ((int)((Vertices [i].Y * drawYMultiplier) + drawYOffset), 0, cnvDrawing.Bounds.Height));    
        }
    }

    private void DrawReferences () {
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");
        
        int offsetHigh = (int)cnvDrawing.Bounds.Height - (int)(cnvDrawing.Bounds.Height * ((DisplayYOffset + 2) / 2));
        int offsetLow = (int)cnvDrawing.Bounds.Height - (int)(cnvDrawing.Bounds.Height * ((DisplayYOffset) / 2));

        DrawReference (pathReferenceHigh, offsetHigh);
        DrawReference (pathReferenceLow, offsetLow);
        DrawReference (pathReferenceMid, drawYOffset);
    }

    private void DrawReference (Avalonia.Controls.Shapes.Path pathElement, int yOffset) {
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");
        
        drawGeometry = new StreamGeometry ();

        using (drawContext = drawGeometry.Open ()) {
            drawContext.BeginFigure (new Avalonia.Point (0, Utility.Clamp (yOffset, 0, cnvDrawing.Bounds.Height)), true);
            drawContext.LineTo (new Avalonia.Point (cnvDrawing.Bounds.Width, Utility.Clamp (yOffset, 0, cnvDrawing.Bounds.Height)), true);
            drawContext.EndFigure(false);
        }

        pathElement.Data = drawGeometry;
    }

    private void DrawOffsetReference () {
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");
        
        drawGeometry = new StreamGeometry ( );

        using (drawContext = drawGeometry.Open ()) {
            drawContext.BeginFigure (new Avalonia.Point (Utility.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.Bounds.Width), 0), true);
            drawContext.LineTo (new Avalonia.Point (Utility.Clamp ((int)((IndexOffset * drawXMultiplier) + drawXOffset), 0, cnvDrawing.Bounds.Width), cnvDrawing.Bounds.Height), true);
            drawContext.EndFigure (false);
        }

        pathIndexOffset.Data = drawGeometry;
    }

    private void DrawWave () {
        drawGeometry = new StreamGeometry ();

        using (drawContext = drawGeometry.Open ()) {
            drawContext.BeginFigure (new Avalonia.Point(Vertices [0].Pixel.X, Vertices[0].Pixel.Y), true);

            for (int i = 1; i < Vertices.Count; i++)
                drawContext.LineTo (new Avalonia.Point(Vertices [i].Pixel.X, Vertices[i].Pixel.Y), true);
            
            drawContext.EndFigure (false);
        }

        pathWave.Data = drawGeometry;
    }

    private void TrimWave () {
        TrimWaveStart ();
        TrimWaveEnd ();
    }

    private void TrimWaveStart () {
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        
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
        numIndexOffset.Value = IndexOffset;

        DrawLength = Vertices.Count * DrawResolution;
        numDrawLength.Value = DrawLength;

        UpdateWave ();
    }

    private void TrimWaveEnd () {
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        
        for (int i = (Vertices?.Count ?? 0) - 2; i > 2 && i > IndexOffset; i--) {
            if (Vertices? [i].Y == 0 && Vertices [i + 1].Y == 0) {
                Vertices.RemoveAt (i + 1);
            } else {
                break;
            }
        }

        DrawLength = Vertices.Count * DrawResolution;
        numDrawLength.Value = DrawLength;
        UpdateWave ();
    }

    private void TrimWaveOffset () {
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        
        for (int i = 0; i < IndexOffset;) {
            Vertices?.RemoveAt (i);
            IndexOffset--;
        }

        numIndexOffset.Value = IndexOffset;
        DrawLength = Vertices.Count * DrawResolution;
        numDrawLength.Value = DrawLength;
        UpdateWave ();
    }

    private void SetVertex (int index, double amount) {
        if (index < 0 || index > Vertices?.Count)
            return;
        
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");

        Vertices? [index].Y = Utility.Clamp (amount, -1.0, 1.0);
        
        Vertices? [index].Pixel = new Point (
            Utility.Clamp ((int)((index * drawXMultiplier) + drawXOffset), 0, cnvDrawing.Bounds.Width),
            Utility.Clamp ((int)((Vertices [index].Y * drawYMultiplier) + drawYOffset), 0, cnvDrawing.Bounds.Height));  
        
        DrawWave ();
    }

    private void SetVertexToPixel (int index, Point position) {
        if (index < 0 || index > Vertices?.Count)
            return;

        Vertices? [index].Pixel.Y = position.Y;
        TranslatePixelToPoint (index);
    }

    private void MoveVertex (int index, double amount) {
        if (index < 0 || index > Vertices?.Count)
            return;
        
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");

        Vertices? [index].Y = Utility.Clamp ((Vertices [index].Y + (EditYAmplitude * amount)), -1.0, 1.0);
        
        Vertices? [index].Pixel = new Point (
            Utility.Clamp ((int)((index * drawXMultiplier) + drawXOffset), 0, cnvDrawing.Bounds.Width),
            Utility.Clamp ((int)((Vertices [index].Y * drawYMultiplier) + drawYOffset), 0, cnvDrawing.Bounds.Height));
        
        DrawWave ();
    }

    private void MoveReference (double amount) {
        DisplayYOffset = Utility.Clamp ((DisplayYOffset + (amount * 0.1)), -1.0, 1.0);
        UpdateWave ();      // Calculates draw offsets as well
        DrawReferences ();
    }

    private void MoveOffset (int index) {
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        
        numIndexOffset.Value = index;
        IndexOffset = index;

        DrawOffsetReference ();
    }

    private int NearestVertexByDistance (Point refPoint) {
        if (Vertices == null || Vertices.Count == 0)
            return -1;

        int nearestIndex = 0;
        double nearestDistance = Point.Distance (Vertices [0].Pixel, refPoint);

        for (int i = 1; i < Vertices.Count; i++) {
            double nd = Point.Distance(Vertices [i].Pixel, refPoint);

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
        double nearestDistance = Math.Abs (Vertices [0].Pixel.X - refPoint.X);

        for (int i = 1; i < Vertices.Count; i++) {
            double nd = Math.Abs (Vertices [i].Pixel.X - refPoint.X);

            if (nd < nearestDistance) {
                nearestDistance = nd;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private void SetZAxes () {
        Canvas cnvDrawing = this.GetControl<Canvas>("cnvDrawing");
        Image imgReference = this.GetControl<Image>("imgReference");
        Avalonia.Controls.Shapes.Path pathWave = this.GetControl<Avalonia.Controls.Shapes.Path>("pathWave");
        Avalonia.Controls.Shapes.Path pathIndexOffset = this.GetControl<Avalonia.Controls.Shapes.Path>("pathIndexOffset");
        Avalonia.Controls.Shapes.Path pathReferenceHigh = this.GetControl<Avalonia.Controls.Shapes.Path>("pathReferenceHigh");
        Avalonia.Controls.Shapes.Path pathReferenceMid = this.GetControl<Avalonia.Controls.Shapes.Path>("pathReferenceMid");
        Avalonia.Controls.Shapes.Path pathReferenceLow = this.GetControl<Avalonia.Controls.Shapes.Path>("pathReferenceLow");

        /* Ensures all cnvDrawing.Children aren't overlapping improperly */

        cnvDrawing.ZIndex = 0;
        imgReference.ZIndex = 1;
        pathReferenceHigh.ZIndex = 2;
        pathReferenceMid.ZIndex = 2;
        pathReferenceLow.ZIndex = 2;
        pathIndexOffset.ZIndex = 3;
        pathWave.ZIndex = 4;
    }

    private void Filter_Normalize (double newMin = -1, double newMax = 1) {
        if (Vertices.Count == 0)
            return;

        double oldMin = Vertices [0].Y,
            oldMax = Vertices [0].Y;

        // Obtain existing minimum and maximum
        for (int i = 0; i < Vertices.Count; i++) {
            oldMin = (Vertices [i].Y < oldMin) ? Vertices [i].Y : oldMin;
            oldMax = (Vertices [i].Y > oldMax) ? Vertices [i].Y : oldMax;
        }

        // Rescale (min-max normalization) of vertex set
        for (int i = 0; i < Vertices.Count; i++)
            Vertices [i].Y = (newMin + (((Vertices [i].Y - oldMin) * (newMax - newMin)) / (oldMax - oldMin)));

        UpdateWave ();
    }

    private void MenuItemNew_Click (object sender, RoutedEventArgs e)
        => _ = NewFile ();

    private void MenuItemLoad_Click (object sender, RoutedEventArgs e)
        => _ = LoadFile ();

    private void MenuItemSave_Click (object sender, RoutedEventArgs e)
        => _ = SaveFile ();

    private void MenuItemSaveAs_Click (object sender, RoutedEventArgs e)
        => _ = SaveAsFile ();

    private void MenuItemSetBackground_Click (object sender, RoutedEventArgs e)
        => _ = SetBackground ();

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
        TextBox tbWaveName = this.GetControl<TextBox>("tbWaveName");
        NumericUpDown numDrawResolution = this.GetControl<NumericUpDown>("numDrawResolution");
        NumericUpDown numDrawLength = this.GetControl<NumericUpDown>("numDrawLength");
        NumericUpDown numIndexOffset = this.GetControl<NumericUpDown>("numIndexOffset");
        
        tbWaveName.Text = tbWaveName.Text?.Trim ().Replace (' ', '_');
        WaveName = tbWaveName.Text ?? "";
        DrawResolution = (int?)numDrawResolution.Value ?? 10;
        DrawLength = (int?)numDrawLength.Value ?? 1000;
        IndexOffset = (int?)numIndexOffset.Value ?? 0;

        Vertices = new List<Vertex> ();
        for (int i = 0; i < (DrawLength / DrawResolution); i++)
            Vertices.Add (new Vertex () { Y = 0 });

        UpdateWave ();
    }

    private void menuFilterNormalize_Click (object sender, RoutedEventArgs e)
        => Filter_Normalize ();

    private void menuFilterNormalizePositive_Click (object sender, RoutedEventArgs e)
        => Filter_Normalize (0, 1);

    private void numSystoleLength_ValueChanged (object sender, NumericUpDownValueChangedEventArgs e) {
        NumericUpDown numSystoleLength = this.GetControl<NumericUpDown>("numSystoleLength");
        
        SystoleLength = (int?)numSystoleLength.Value ?? 330;
    }

    private void cnvDrawing_KeyDown (object sender, KeyEventArgs e) {
        Image imgReference = this.GetControl<Image> ("imgReference");
        
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return; // Prevent keyboard shortcuts from being "handled" and not actually running!

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
            switch (e.Key) {
                default: break;

                case Key.Up:
                    imgReference.Height = imgReference.Bounds.Height - 10;
                    break;

                case Key.Down:
                    imgReference.Height = imgReference.Bounds.Height + 10;
                    break;

                case Key.Left:
                    imgReference.Width = imgReference.Bounds.Width - 10;
                    break;

                case Key.Right:
                    imgReference.Width = imgReference.Bounds.Width + 10;
                    break;
            }
        } else {
            switch (e.Key) {
                default: break;

                case Key.Up:
                    Canvas.SetTop (imgReference,
                        Utility.Clamp (imgReference.Bounds.Top - 10, 0,
                        cnvDrawing.Bounds.Height - imgReference.Bounds.Height));
                    break;

                case Key.Down:
                    Canvas.SetTop (imgReference,
                        Utility.Clamp (imgReference.Bounds.Top + 10, 0,
                        cnvDrawing.Bounds.Height - imgReference.Bounds.Height));
                    break;

                case Key.Left:
                    Canvas.SetLeft (imgReference,
                        Utility.Clamp (imgReference.Bounds.Left - 10, 0,
                        cnvDrawing.Bounds.Width - imgReference.Bounds.Width));
                    break;

                case Key.Right:

                    Canvas.SetLeft (imgReference,
                        Utility.Clamp (imgReference.Bounds.Left + 10, 0,
                            cnvDrawing.Bounds.Width - imgReference.Bounds.Width));
                    break;
            }
        }

        e.Handled = true;
    }

    private void cnvDrawing_MouseDown (object sender, PointerPressedEventArgs e) {
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");

        Point pos = new(e.GetCurrentPoint (cnvDrawing).Position);
        int index = NearestVertexByXAxis (pos);

        if (e.Properties.IsLeftButtonPressed) {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                MoveOffset (index);
            } else {
                SetVertexToPixel (index, pos);
            }
        }

        cnvDrawing.Focus ();
    }

    private void cnvDrawing_MouseMove (object sender, PointerEventArgs e) {
        Canvas cnvDrawing = this.GetControl<Canvas> ("cnvDrawing");

        Point pos = new(e.GetCurrentPoint (cnvDrawing).Position);
        int index = NearestVertexByXAxis (pos);
        
        if (e.Properties.IsLeftButtonPressed) {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                MoveOffset (index);
            } else {
                SetVertexToPixel (index, pos);
            }
        }
    }

    private void cnvDrawing_MouseWheel (object sender, PointerWheelEventArgs e) {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
            MoveReference (e.Delta.Y / 5);
        }
    }
}