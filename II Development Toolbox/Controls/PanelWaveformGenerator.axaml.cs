using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;


namespace IIDT;

public partial class PanelWaveformGenerator : UserControl {
    private Window Control;
    private string SolutionDir = "";
    
    public PanelWaveformGenerator (Window control, string solutionDir) {
        Control = control;
        SolutionDir = solutionDir;
        
        InitializeComponent ();
        
        // Set default file paths for Waveform Generator
        this.GetControl<TextBox> ("tbOutputFilepath").Text = Path.Combine(SolutionDir, "II Library/Waveforms/");
    }
    
     public async Task WaveformGenerator_Process () {
        string WaveName = this.GetControl<TextBox> ("tbWaveformName").Text;
        string FilepathOut = this.GetControl<TextBox> ("tbOutputFilepath").Text;

        if (String.IsNullOrEmpty (WaveName)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The waveform name was not entered! Cannot proceed without this input.",
                    Title = "Invalid Input",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    Control.Show ();

                await dlg.AsyncShow (Control);
            });

            return;
        }
        
        
        /* Compile waveform into C# code */
        
        int DrawResolution = 10;
        double DrawLength = 0.0;
        int IndexOffset = 0;
        List<Vertex> Vertices = new List<Vertex> ();

        /* Generate waveform into List<Point> via Waveform.cs */
        List<Point> Wave = Waveform.Generate (DrawResolution);
        WaveName = WaveName.Trim ().Replace (' ', '_');

        /* Convert List<Point> to List<Vertex> and calculate associated WaveData parameters */
        DrawLength = Math.Round (Wave.Count > 0 ? Wave.Last ().X : 0 , 1);

        for (int i = 0; i < Wave.Count; i++)         // NOTE: MAY NEED TO SCALE X AXIS TO EACH X POINT @ DRAWRESOLUTION
            Vertices.Add (new Vertex (Wave [i].Y));

        StreamWriter sw = new (FilepathOut, false);

        sw.WriteLine ($"WaveName:{WaveName}");
        sw.WriteLine ($"DrawResolution:{DrawResolution}");
        sw.WriteLine ($"IndexOffset:{IndexOffset}");

        StringBuilder sbVert = new StringBuilder ();
        for (int i = 0; i < Vertices.Count; i++)
            sbVert.Append ($"({i} {Math.Round (Vertices [i].Y, 2)}) ");

        sw.WriteLine ("{0}:{1}", "Vertices", sbVert.ToString ().Trim ());
        sw.Close ();
        
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Success: The waveform has been written!",
                Title = "Task Completed",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.OK,
            };

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            await dlg.AsyncShow (Control);
        });
    }
     
    private async Task SelectOutputFile () {
        if (!Control.StorageProvider.CanSave) {
            return;
        }
        
        var fpso = new FilePickerSaveOptions
        {
            SuggestedFileName = "Waveform.iiwf",
            DefaultExtension = ".iiwf",
            FileTypeChoices = new[] {
                new FilePickerFileType("Waveform Files")
                {
                    Patterns = new[] { "*.iiwf" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            SuggestedStartLocation =
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/Waveforms/")),
            ShowOverwritePrompt = false
        };
        
        IStorageFile? file = await Control.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private void btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = SelectOutputFile ();

    private void btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = WaveformGenerator_Process ();
}