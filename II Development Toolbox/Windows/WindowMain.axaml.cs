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
        while (!Path.GetDirectoryName (currentDir).EndsWith("II Development Toolbox")) {
            currentDir = Directory.GetParent (currentDir)?.FullName;
        }
        string tbDir = Directory.GetParent (currentDir)?.FullName;
        SolutionDir = Directory.GetParent (tbDir)?.FullName;

        this.GetControl<TabItem>("tiDictionaryBuilder").Content = new PanelDictionaryBuilder(this, SolutionDir);
        this.GetControl<TabItem>("tiToneGenerator").Content = new PanelToneGenerator(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformDictionaryBuilder").Content = new PanelWaveformDictionaryBuilder(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformEditor").Content = new PanelWaveformEditor(this, SolutionDir);
        this.GetControl<TabItem>("tiWaveformGenerator").Content = new PanelWaveformGenerator(this, SolutionDir);
    }
}