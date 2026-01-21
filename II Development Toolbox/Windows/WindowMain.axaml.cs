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

public partial class WindowMain : Window {
    private string solutionDir = "";

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
    
    public WindowMain () {
        InitializeComponent ();
        
        // Find the Infirmary Integrated solution directory by iterating upwards in the directory tree
        string currentDir = Directory.GetCurrentDirectory();
        while (!Path.GetDirectoryName (currentDir).EndsWith("II Development Toolbox")) {
            currentDir = Directory.GetParent (currentDir)?.FullName;
        }
        string tbDir = Directory.GetParent (currentDir)?.FullName;
         solutionDir = Directory.GetParent (tbDir)?.FullName;

        // Set default file paths for Dictionary Builder
        this.GetControl<TextBox> ("db_tbInputFilepath").Text = Path.Combine(solutionDir, "II Library/", "Localization Strings.xlsx");
        this.GetControl<TextBox> ("db_tbOutputFilepath").Text = Path.Combine(solutionDir, "II Library/Classes/", "Localization.Dictionary.cs");
        
        // Set default file paths for Tone Generator
        this.GetControl<TextBox> ("tg_tbOutputFilepath").Text = solutionDir;
        
        // Set default file paths and contents for Waveform Dictionary Builder
        this.GetControl<TextBox> ("wfdb_tbInputFilepath").Text = Path.Combine(solutionDir, "II Library/Waveforms/");
        this.GetControl<TextBox> ("wfdb_tbOutputFilepath").Text = Path.Combine(solutionDir, "II Library/Classes/", "Waveform.Dictionary.Plots.cs");
        this.GetControl<TextBlock> ("wfdb_tbiiwfSpecifications").Text = iiwfSpecification;

        // Set default file paths for Waveform Generator
        this.GetControl<TextBox> ("wfg_tbOutputFilepath").Text = Path.Combine(solutionDir, "II Library/Waveforms/");
    }

    public async Task DictionaryBuilder_Process () {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string FilepathIn = this.GetControl<TextBox> ("db_tbInputFilepath").Text;
        string FilepathOut = this.GetControl<TextBox> ("db_tbOutputFilepath").Text;

        if (!File.Exists (FilepathIn)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The input file was not found!",
                    Title = "File Not Found",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
            });

            return;
        }
        
        List<string> Languages = new List<string>();
        List<Dictionary<string, string>> Dictionaries = new List<Dictionary<string, string>>();

        /* Read the .xlsx into a Language and Dictionary lists */
        using (var stream = File.Open(FilepathIn, FileMode.Open, FileAccess.Read)) {
            using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                int colCount = reader.FieldCount;
                int rowCurrent = 0;

                while (reader.Read())
                {
                    if (rowCurrent == 0)
                    {
                        // $B1 -> $...1: language codes
                        for (int j = 1; j < colCount; j++)
                        {
                            Languages.Add(reader.GetString(j).ToUpper().Substring(0, 3));
                            Dictionaries.Add(new Dictionary<string, string>());
                        }
                    }
                    else if (rowCurrent > 0)
                    {
                        string key = reader.GetString(0);
                        if (String.IsNullOrEmpty(key))
                            continue;

                        Console.WriteLine($"Processing row {rowCurrent:000}: {key}");

                        for (int i = 1; i < colCount; i++)
                        {
                            Dictionaries[i - 1].Add(key, reader.GetString(i));
                        }
                    }

                    rowCurrent += 1;
                }
            }
        }
        
        /* Compile dictionaries into C# localization code */
        
        StringBuilder dictOut = new StringBuilder ();
        dictOut.Append (
            "using System;\n"
            + "using System.Collections.Generic;\n"
            + "using System.Globalization;\n"
            + "using System.Text;\n\n"
            + "namespace II.Localization {\n\n"
            + "\tpublic partial class Language {\n\n");
        
        for (int i = 0; i < Languages.Count; i++) {
            dictOut.AppendLine (String.Format ("\t\tstatic Dictionary<string, string> {0} = new Dictionary<string, string> () {{", Languages [i]));

            foreach (KeyValuePair<string, string> pair in Dictionaries [i])
                dictOut.AppendLine (String.Format ("\t\t\t{{{0,-60} {1}}},",
                    String.Format ("\"{0}\",", pair.Key),
                    String.Format ("\"{0}\"", pair.Value)));

            dictOut.AppendLine ("\t\t};\n");
        }

        dictOut.AppendLine ("\t}\n}");
        
        StreamWriter outFile = new StreamWriter (FilepathOut, false);
        
        outFile.Write (dictOut.ToString ());
        outFile.Close ();
        
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Success: The dictionary has been built!",
                Title = "Task Completed",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.OK,
            };

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            await dlg.AsyncShow (this);
        });
    }


    public async Task ToneGenerator_Process () {
        string FilepathOut = this.GetControl<TextBox> ("tg_tbOutputFilepath").Text;
        double Length = (double)this.GetControl<NumericUpDown> ("tg_numLength").Value;
        
        FileStream stream = new FileStream (FilepathOut, FileMode.Create);
        BinaryWriter writer = new BinaryWriter (stream);

        int RIFF = 0x46464952;
        int WAVE = 0x45564157;
        int formatChunkSize = 16;
        int headerSize = 8;
        int format = 0x20746D66;
        short formatType = 1;
        short tracks = 1;
        int samplesPerSecond = 44100;
        short bitsPerSample = 16;
        short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
        int bytesPerSecond = samplesPerSecond * frameSize;
        int waveSize = 4;
        int data = 0x61746164;
        int samplesTotal = (int)(samplesPerSecond * Length);
        int dataChunkSize = samplesTotal * frameSize;
        int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

        writer.Write (RIFF);
        writer.Write (fileSize);
        writer.Write (WAVE);
        writer.Write (format);
        writer.Write (formatChunkSize);
        writer.Write (formatType);
        writer.Write (tracks);
        writer.Write (samplesPerSecond);
        writer.Write (bytesPerSecond);
        writer.Write (frameSize);
        writer.Write (bitsPerSample);
        writer.Write (data);
        writer.Write (dataChunkSize);

        double ampl = 10000;

        for (int k = 0; k < 2; k++) {
            for (int i = 0; i < (samplesTotal / Length) * .75; i++) {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (System.Math.Sin (t * 330 * 2.0 * System.Math.PI)));
                writer.Write (s);
            }

            for (int i = 0; i < (samplesTotal / Length) * .25; i++) {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(0 * (System.Math.Sin (t * 220 * 2.0 * System.Math.PI)));
                writer.Write (s);
            }

            for (int i = 0; i < (samplesTotal / Length) * 1; i++) {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (System.Math.Sin (t * 220 * 2.0 * System.Math.PI)));
                writer.Write (s);
            }

            for (int i = 0; i < (samplesTotal / Length) * 3; i++) {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(0 * (System.Math.Sin (t * 220 * 2.0 * System.Math.PI)));
                writer.Write (s);
            }
        }

        writer.Close ();
        stream.Close ();
        
        await Dispatcher.UIThread.InvokeAsync (async () => {
            DialogMessage dlg = new () {
                Message = "Success: The tone has been generated to a file!",
                Title = "Task Completed",
                Indicator = DialogMessage.Indicators.InfirmaryIntegrated,
                Option = DialogMessage.Options.OK,
            };

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            await dlg.AsyncShow (this);
        });
    }
    
    public async Task WaveformDictionaryBuilder_Process () {
        string FilepathIn = this.GetControl<TextBox> ("wfdb_tbInputFilepath").Text;
        string FilepathOut = this.GetControl<TextBox> ("wfdb_tbOutputFilepath").Text;

        if (!Directory.Exists (FilepathIn)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The input directory was not found!",
                    Title = "Directory Not Found",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
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

        string [] files = Directory.GetFiles (FilepathIn, "*.iiwf");

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

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            await dlg.AsyncShow (this);
        });
    }


    public async Task WaveformGenerator_Process () {
        string WaveName = this.GetControl<TextBox> ("wfg_tbWaveformName").Text;
        string FilepathOut = this.GetControl<TextBox> ("wfg_tbOutputFilepath").Text;

        if (String.IsNullOrEmpty (WaveName)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The waveform name was not entered! Cannot proceed without this input.",
                    Title = "Invalid Input",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    this.Show ();

                await dlg.AsyncShow (this);
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

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            await dlg.AsyncShow (this);
        });
    }

    private async Task db_SelectInputFile () {
        if (!this.StorageProvider.CanOpen) {
            return;
        }
            
        var fpoo = new FilePickerOpenOptions ()
        {
            SuggestedFileName = "Localization Strings",
            FileTypeFilter = new[] {
                new FilePickerFileType("Excel Spreadsheet")
                {
                    Patterns = new[] { "*.xlsx" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            AllowMultiple = false,
            SuggestedStartLocation = 
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir, "II Library/"))
        };

        var files = await this.StorageProvider.OpenFilePickerAsync(fpoo);

        if (files.First()?.TryGetLocalPath() is not null) {
            this.GetControl<TextBox> ("db_tbInputFilepath").Text = files?.First()?.TryGetLocalPath();
        }
    }

    private async Task db_SelectOutputFile () {
        if (!this.StorageProvider.CanSave) {
            return;
        }
        
        var fpso = new FilePickerSaveOptions
        {
            SuggestedFileName = "Localization.Dictionary.cs",
            DefaultExtension = ".cs",
            FileTypeChoices = new[] {
                new FilePickerFileType("C# Class")
                {
                    Patterns = new[] { "*.cs" },
                    MimeTypes = new[] { "text/plain" }
                }
            },
            SuggestedStartLocation =
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir, "II Library/Classes/")),
            ShowOverwritePrompt = false
        };
        
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("db_tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private async Task tg_SelectOutputFile () {
        if (!this.StorageProvider.CanSave) {
            return;
        }
        
        var fpso = new FilePickerSaveOptions
        {
            SuggestedFileName = "Tone",
            DefaultExtension = ".wav",
            FileTypeChoices = new[] {
                new FilePickerFileType("Audio File")
                {
                    Patterns = new[] { "*.wav" }
                }
            },
            SuggestedStartLocation =
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir)),
            ShowOverwritePrompt = false
        };
        
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("tg_tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private async Task wfdb_SelectInputFile () {
        if (!this.StorageProvider.CanOpen) {
            return;
        }
            
        var fpoo = new FolderPickerOpenOptions ()
        {
            AllowMultiple = false,
            SuggestedStartLocation = 
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir, "II Library/Waveforms"))
        };

        var dirs = await this.StorageProvider.OpenFolderPickerAsync(fpoo);

        if (dirs.First()?.TryGetLocalPath() is not null) {
            this.GetControl<TextBox> ("wfdb_tbInputFilepath").Text = dirs?.First()?.TryGetLocalPath();
        }
    }

    private async Task wfdb_SelectOutputFile () {
        if (!this.StorageProvider.CanSave) {
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
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir, "II Library/Classes/")),
            ShowOverwritePrompt = false,
        };
        
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("wfdb_tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private async Task wfg_SelectOutputFile () {
        if (!this.StorageProvider.CanSave) {
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
                await this.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(solutionDir, "II Library/Waveforms/")),
            ShowOverwritePrompt = false
        };
        
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(fpso);

        if (file is not null && file.TryGetLocalPath () is not null) {
            this.GetControl<TextBox> ("wfg_tbOutputFilepath").Text = file.TryGetLocalPath (); 
        }
    }
    
    private void db_btnInputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = db_SelectInputFile ();
    
    private void db_btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = db_SelectOutputFile ();

    private void db_btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = DictionaryBuilder_Process ();
    
    private void tg_btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = tg_SelectOutputFile ();

    private void tg_btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = ToneGenerator_Process ();
    
    private void wfdb_btnInputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = wfdb_SelectInputFile ();
    
    private void wfdb_btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = wfdb_SelectOutputFile ();

    private void wfdb_btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = WaveformDictionaryBuilder_Process ();
    
    private void wfg_btnOutputFilepath_OnClick (object? sender, RoutedEventArgs e) 
        => _ = wfg_SelectOutputFile ();

    private void wfg_btnProcess_OnClick (object? sender, RoutedEventArgs e)
        => _ = WaveformGenerator_Process ();

}