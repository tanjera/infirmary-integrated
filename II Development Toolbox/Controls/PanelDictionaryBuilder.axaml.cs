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

public partial class PanelDictionaryBuilder : UserControl {
    private Window Control;
    private string SolutionDir = "";
    
    public PanelDictionaryBuilder (Window control, string solutionDir) {
        Control = control;
        SolutionDir = solutionDir;
        
        InitializeComponent ();
        
        // Set default file paths for Dictionary Builder
        this.GetControl<TextBox> ("tbInputFilepath").Text = Path.Combine(solutionDir, "II Library/", "Localization Strings.xlsx");
        this.GetControl<TextBox> ("tbOutputFilepath").Text = Path.Combine(solutionDir, "II Library/Classes/", "Localization.Dictionary.cs");
    }
    
    public async Task DictionaryBuilder_Process () {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string FilepathIn = this.GetControl<TextBox> ("tbInputFilepath").Text;
        string FilepathOut = this.GetControl<TextBox> ("tbOutputFilepath").Text;

        if (!File.Exists (FilepathIn)) {
            await Dispatcher.UIThread.InvokeAsync (async () => {
                DialogMessage dlg = new () {
                    Message = "Error: The input file was not found!",
                    Title = "File Not Found",
                    Indicator = DialogMessage.Indicators.Error,
                    Option = DialogMessage.Options.OK,
                };

                if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                    Control.Show ();

                await dlg.AsyncShow (Control);
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

            if (!Control.IsVisible)                    // Avalonia's parent must be visible to attach a window
                Control.Show ();

            await dlg.AsyncShow (Control);
        });
    }
    
    private async Task SelectInputFile () {
        if (!Control.StorageProvider.CanOpen) {
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
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/"))
        };

        var files = await Control.StorageProvider.OpenFilePickerAsync(fpoo);

        if (files.First()?.TryGetLocalPath() is not null) {
            this.GetControl<TextBox> ("tbInputFilepath").Text = files?.First()?.TryGetLocalPath();
        }
    }

    private async Task SelectOutputFile () {
        if (!Control.StorageProvider.CanSave) {
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
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir, "II Library/Classes/")),
            ShowOverwritePrompt = false
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
        => _ = DictionaryBuilder_Process ();

}