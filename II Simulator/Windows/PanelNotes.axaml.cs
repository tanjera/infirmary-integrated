/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2024
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

using II;

namespace IISIM {

    public partial class PanelNotes : RecordPanel {
        /* References for UI elements */

        public PanelNotes () {
            InitializeComponent ();
        }

        public PanelNotes (App? app) : base (app) {
            InitializeComponent ();

            DataContext = this;

            /* Establish reference variables */

            InitInterface ();
            PopulateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (InitInterface)}");
                return;
            }

            _ = PopulateInterface ();
        }

        public async Task RefreshInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (RefreshInterface)}");
                return;
            }

            await PopulateInterface ();
        }

        public Task PopulateInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {Name}.{nameof (RefreshInterface)}");
                return Task.CompletedTask;
            }

            ListBox lbNotes = this.FindControl<ListBox> ("lbNotes");

            List<string> listNotes = new List<string> ();
            foreach (var note in Instance?.Records?.Notes ?? new List<Record.Note> ()) {
                listNotes.Add ($"{note.Timestamp}: {note.Title}");
            }
            lbNotes.Items = listNotes;

            return Task.CompletedTask;
        }

        private void SelectNote () {
            ListBox lbNotes = this.FindControl<ListBox> ("lbNotes");
            TextBlock tbNote = this.FindControl<TextBlock> ("tbNote");

            if (lbNotes.SelectedIndex > -1 && Instance?.Records?.Notes.Count >= lbNotes.SelectedIndex)
                tbNote.Text = Instance?.Records?.Notes [lbNotes.SelectedIndex].Content;
        }

        public void Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                        }
                    }
                }
            } catch {
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new ();

            return sWrite.ToString ();
        }

        private void LbNotes_SelectionChanged (object? sender, SelectionChangedEventArgs e)
            => SelectNote ();
    }
}