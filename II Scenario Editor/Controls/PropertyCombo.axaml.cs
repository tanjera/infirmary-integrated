using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyCombo : UserControl {
        private bool isInitiated = false;

        public Keys Key;
        public List<string>? Values;

        public enum Keys {
            DefaultSource,
            DefaultProgression
        }

        public new event EventHandler<PropertyComboEventArgs>? PropertyChanged;

        public class PropertyComboEventArgs : EventArgs {
            public Keys Key;
            public string? Value;
        }

        public PropertyCombo () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key, string [] values, List<string> readable) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyComboEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            Key = key;
            Values = new List<string> (values);

            switch (Key) {
                default: break;
                case Keys.DefaultSource: lblKey.Content = "Default Step to Progress From: "; break;
                case Keys.DefaultProgression: lblKey.Content = "Default Step to Progress To: "; break;
            }

            List<ComboBoxItem> listItems = new List<ComboBoxItem> ();

            foreach (string s in readable)
                listItems.Add (new ComboBoxItem () { Content = s });

            cmbEnumeration.Items = listItems;

            if (!isInitiated) {
                cmbEnumeration.SelectionChanged += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public Task Update (List<string> values, List<string> readable, int index = 0) {
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            cmbEnumeration.SelectionChanged -= SendPropertyChange;

            Values = new List<string> (values);

            List<ComboBoxItem> listItems = new List<ComboBoxItem> ();
            foreach (string s in readable)
                listItems.Add (new ComboBoxItem () { Content = s });

            cmbEnumeration.Items = listItems;
            cmbEnumeration.SelectedIndex = index;

            cmbEnumeration.SelectionChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        public Task Set (int index) {
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            cmbEnumeration.SelectionChanged -= SendPropertyChange;
            cmbEnumeration.SelectedIndex = index;
            cmbEnumeration.SelectionChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            if (cmbEnumeration.SelectedIndex < 0)
                return;

            PropertyComboEventArgs ea = new PropertyComboEventArgs ();
            ea.Key = Key;
            if (Values != null && Values.Count > 0)
                ea.Value = Values [cmbEnumeration.SelectedIndex];

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}