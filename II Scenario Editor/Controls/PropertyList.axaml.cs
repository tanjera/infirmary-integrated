using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyList : UserControl {
        private bool isInitiated = false;

        public Keys Key;
        public List<string>? Values;

        public enum Keys {
        }

        public new event EventHandler<PropertyListEventArgs>? PropertyChanged;

        public class PropertyListEventArgs : EventArgs {
            public Keys Key;
            public List<string> Values = new ();
        }

        public PropertyList () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key, string [] values, List<string> readable, bool multiselect = true) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyListEventArgs>)d;
            }

            if (values.Length != readable.Count)
                return Task.CompletedTask;

            Label lblKey = this.GetControl<Label> ("lblKey");
            ListBox listBox = this.GetControl<ListBox> ("listBox");

            Key = key;
            Values = new List<string> (values);

            switch (Key) {
                default: break;
            }

            for (int i = 0; i < values.Length; i++)
                listBox.Items.Add (new ComboBoxItem () {
                    Tag = values [i],
                    Content = readable [i]
                });

            listBox.SelectedItems.Clear ();

            listBox.SelectionMode = multiselect ? SelectionMode.Multiple : SelectionMode.Single;

            if (!isInitiated) {
                listBox.SelectionChanged += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public Task Update (List<string> values, List<string> readable, List<string> selected) {
            if (values.Count != readable.Count)
                return Task.CompletedTask;

            ListBox listBox = this.GetControl<ListBox> ("listBox");

            listBox.SelectionChanged -= SendPropertyChange;

            Values = new List<string> (values);

            for (int i = 0; i < values.Count; i++)
                listBox.Items.Add (new ComboBoxItem () {
                    Tag = values [i],
                    Content = readable [i]
                });

            listBox.SelectedItems.Clear ();

            foreach (ListBoxItem i in listBox.Items)
                if (selected.Contains (i.Tag?.ToString () ?? ""))
                    listBox.SelectedItems.Add (i);

            listBox.SelectionChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        public Task Set (List<string> selected) {
            ListBox listBox = this.GetControl<ListBox> ("listBox");

            listBox.SelectionChanged -= SendPropertyChange;

            listBox.SelectedItems.Clear ();
            foreach (ListBoxItem i in listBox.Items) {
                if (selected.Contains (i.Tag?.ToString () ?? ""))
                    listBox.SelectedItems.Add (i);
            }

            listBox.SelectionChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            ListBox listBox = this.GetControl<ListBox> ("listBox");

            PropertyListEventArgs ea = new PropertyListEventArgs ();
            ea.Key = Key;
            if (Values != null && Values.Count == listBox.ItemCount) {
                foreach (ListBoxItem i in listBox.SelectedItems) {
                    string? t = i.Tag?.ToString ();
                    if (!String.IsNullOrEmpty (t))
                        ea.Values.Add (t);
                }
            }

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{String.Join (", ", ea.Values)}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}