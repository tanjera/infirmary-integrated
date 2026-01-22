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

public partial class PanelToneGenerator : UserControl {
    private Window Control;
    private string SolutionDir = "";
    
    public PanelToneGenerator (Window control, string solutionDir) {
        Control = control;
        SolutionDir = solutionDir;
        
        InitializeComponent ();
        
        // Set default file paths for Tone Generator
        this.GetControl<TextBox> ("tbOutputFilepath").Text = SolutionDir;
    }

     public async Task ToneGenerator_Process () {
        string FilepathOut = this.GetControl<TextBox> ("tbOutputFilepath").Text;
        double Length = (double)this.GetControl<NumericUpDown> ("numLength").Value;
        
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
            SuggestedFileName = "Tone",
            DefaultExtension = ".wav",
            FileTypeChoices = new[] {
                new FilePickerFileType("Audio File")
                {
                    Patterns = new[] { "*.wav" }
                }
            },
            SuggestedStartLocation =
                await Control.StorageProvider.TryGetFolderFromPathAsync (Path.Combine(SolutionDir)),
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
        => _ = ToneGenerator_Process ();
}