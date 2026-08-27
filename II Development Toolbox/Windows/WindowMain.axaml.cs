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
    private string SolutionDir = "";


    
    public WindowMain () {
        InitializeComponent ();
        
        // Find the Infirmary Integrated solution directory by iterating upwards in the directory tree
        string currentDir = Directory.GetCurrentDirectory();
        try {
            while (!currentDir?.EndsWith ("II Development Toolbox") ?? true) {
                currentDir = Directory.GetParent (currentDir ?? "")?.FullName ?? "";
            }
        } catch (Exception e) {
            currentDir = Directory.GetCurrentDirectory ();
        }

        // After iterating up the filetree, currentDir should be the Toolbox directory
        SolutionDir = Directory.GetParent (currentDir)?.FullName ?? "";

        this.GetControl<TabItem>("tiDictionaryBuilder").Content = new PanelDictionaryBuilder(this, SolutionDir);
        this.GetControl<TabItem>("tiToneGenerator").Content = new PanelToneGenerator(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformDictionaryBuilder").Content = new PanelWaveformDictionaryBuilder(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformEditor").Content = new PanelWaveformEditor(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformGenerator").Content = new PanelWaveformGenerator(this, SolutionDir);
    }
}