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

public partial class PanelWaveformDictionaryBuilder : UserControl {
    private Window Control;
    private string SolutionDir = "";
    
    private static string iiwfSpecification = 
        @"Infirmary Integrated Waveform File (.iiwf) File Specifications:
                                                                                      
File Format:

WaveName:[WaveName]
DrawResolution:[DrawResolution]
DrawLength:[DrawLength]
IndexOffset:[IndexOffset]
Vertices:[(x1 y1) (x2 y2) ...]


Notes:
WaveName: must be a compatible string for both a function name and a file name!
DrawResolution: drawing resolution *in milliseconds per point*
DrawLength: length of the waveform *in seconds*
IndexOffset: offset for when the waveform's ""starting point"" is; used for back-tracing
Vertices: actual vertices, x is the integer index, y is the double amplitude";
    
    public PanelWaveformDictionaryBuilder (Window control, string solutionDir) {
        Control = control;
        SolutionDir = solutionDir;
        
        InitializeComponent ();
        
        // Set default file paths and contents for Waveform Dictionary Builder
        this.GetControl<TextBox> ("tbInputFilepath").Text = Path.Combine(SolutionDir, "II Library/Waveforms/");
        this.GetControl<TextBox> ("tbOutputFilepath").Text = Path.Combine(SolutionDir, "II Library/Classes/", "Waveform.Dictionary.Plots.cs");
        this.GetControl<TextBlock> ("tbiiwfSpecifications").Text = iiwfSpecification;
    }
    
    public async Task WaveformDictionaryBuilder_Process () {
        string FilepathIn = this.GetControl<TextBox> ("tbInputFilepath").Text;
        string FilepathOut = this.GetControl<TextBox> ("tbOutputFilepath").Text;

        if (!Directory.Exists (FilepathIn)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The input directory was not found!",
                    Title = "Directory Not Found",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    Control.Show ();

                await dlg.AsyncShow (Control);
            });

            return;
        }
        
        
        /* Compile dictionaries into C# code */
        
        StringBuilder dictOut = new StringBuilder ();
        
        dictOut.Append (
              "using System;\n"
            + "using System.Collections.Generic;\n\n"

            + "namespace II.Waveform {\n"
            + "\tpublic static partial class Dictionary {\n");

        Console.WriteLine("Processing waveform files.");

        string [] files = Directory.GetFiles (FilepathIn, "*.iiwf")
            .OrderBy (f => f).ToArray ();

        // Iterate all files, populate respective dictionary
        for (int i = 0; i < files.Length; i++) {
            Console.WriteLine ($"Processing file {i:000}: {files[i]}");

            string WaveName = "";
            int DrawResolution = 0;
            int IndexOffset = 0;
            int SystoleLength = 0;
            List<Vertex> Vertices = new List<Vertex> ();

            /* Load individual .iiwf file */
            StreamReader sr = new StreamReader (files [i]);
            string file = sr.ReadToEnd ().Trim ();
            sr.Close ();

            StringReader sRead = new StringReader (file);

            try {
                string? line;
                while ((line = sRead?.ReadLine ()) != null) {
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
                                    pValue = pValue.Substring (coord.Length)
                                        .Trim (); // And remove the current coordinate from pValue

                                    /* Process the current coordinate and add to Vertices */
                                    string [] coords = coord.Trim ('(', ')').Split (' ');
                                    int x = int.Parse (coords [0]);
                                    double y = double.Parse (coords [1]);

                                    if (Vertices.Count == x)
                                        Vertices.Add (new Vertex (y));
                                }

                                break;
                        }
                    }
                }

                /* Write .iiwf as PlotData to dictOut */

                dictOut.AppendLine (String.Format ("\t\tpublic static Plot {0} = new Plot () {{", WaveName));
                dictOut.AppendLine (String.Format ("\t\t\tDrawResolution = {0},", DrawResolution));
                dictOut.AppendLine (String.Format ("\t\t\tIndexOffset = {0},", IndexOffset));
                dictOut.AppendLine (String.Format ("\t\t\tSystoleLength = {0},", SystoleLength));
                dictOut.AppendLine (String.Format ("\t\t\tVertices = new double[] {{", IndexOffset));

                for (int v = 0; v < Vertices.Count; v++) {
                    dictOut.Append (String.Format ("{0}{1}{2}d{3}",
                        (v > 0 && v % 15 == 0 ? "\n" : ""),
                        (v % 15 == 0 ? "\t\t\t\t" : ""),
                        Vertices [v].Y,
                        (v < Vertices.Count - 1 ? ", " : "")));
                }

                dictOut.AppendLine ("\n\t\t\t}\n\t\t};\n");
            } finally {
                sRead?.Close ();
            }
        }

        dictOut.AppendLine ("\t}\n}");
    
        StreamWriter outFile = new (FilepathOut, false);
        
        outFile.Write (dictOut.ToString ());
        outFile.Close ();
        
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Success: The waveform dictionary has been written!",
                Title = "Task Completed",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.OK,
            };

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            await dlg.AsyncShow (Control);
        });
    }
    
    private async Task SelectInputFile () {
        if (!Control.StorageProvider.CanOpen) {
            return;
        }
            
        var fpoo = new FolderPickerOpenOptions ()
        {
            AllowMultiple = false,
            SuggestedStartLocation = 
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/Waveforms"))
        };

        var dirs = await Control.StorageProvider.OpenFolderPickerAsync(fpoo);

        if (dirs.First()?.TryGetLocalPath() is not null) {
            this.GetControl<TextBox> ("tbInputFilepath").Text = dirs?.First()?.TryGetLocalPath();
        }
    }

    private async Task SelectOutputFile () {
        if (!Control.StorageProvider.CanSave) {
            return;
        }
        
        var fpso = new FilePickerSaveOptions
        {
            SuggestedFileName = "Waveform.Dictionary.Plots.cs",
            DefaultExtension = ".cs",
            FileTypeChoices = new[] {
                new FilePickerFileType("C# Class")
                {
                    Patterns = new[] { "*.cs" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            SuggestedStartLocation =
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/Classes/")),
            ShowOverwritePrompt = false,
        };
        
        IStorageFile? file = await Control.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private void btnInputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = SelectInputFile ();
    
    private void btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = SelectOutputFile ();

    private void btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = WaveformDictionaryBuilder_Process ();
}