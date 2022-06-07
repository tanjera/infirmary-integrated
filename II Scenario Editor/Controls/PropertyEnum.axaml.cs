using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyEnum : UserControl {
        public Keys Key;
        public List<string>? Values;

        public enum Keys {
            Cardiac_Axis,
            Cardiac_Rhythms,
            Respiratory_Rhythms,
            PACatheter_Rhythms
        }

        public new event EventHandler<PropertyEnumEventArgs>? PropertyChanged;

        public class PropertyEnumEventArgs : EventArgs {
            public Keys Key;
            public string? Value;
        }

        public PropertyEnum () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (Keys key, string [] values, List<string> readable) {
            Label lblKey = this.FindControl<Label> ("lblKey");
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            Key = key;
            Values = new List<string> (values);

            switch (Key) {
                default: break;
                case Keys.Cardiac_Axis: lblKey.Content = "Cardiac Axis: "; break;
                case Keys.Cardiac_Rhythms: lblKey.Content = "Cardiac Rhythm: "; break;
                case Keys.Respiratory_Rhythms: lblKey.Content = "Respiratory Rhythm: "; break;
                case Keys.PACatheter_Rhythms: lblKey.Content = "Pulmonary Artery Catheter Placement: "; break;
            }

            List<ComboBoxItem> listItems = new List<ComboBoxItem> ();

            foreach (string s in readable)
                listItems.Add (new ComboBoxItem () { Content = s });

            cmbEnumeration.Items = listItems;
            cmbEnumeration.SelectionChanged += sendPropertyChange;
            cmbEnumeration.LostFocus += sendPropertyChange;
        }

        public void Set (int index) {
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            cmbEnumeration.SelectionChanged -= sendPropertyChange;
            cmbEnumeration.SelectedIndex = index;
            cmbEnumeration.SelectionChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            ComboBox cmbEnumeration = this.FindControl<ComboBox> ("cmbEnumeration");

            if (cmbEnumeration.SelectedIndex < 0)
                return;

            PropertyEnumEventArgs ea = new PropertyEnumEventArgs ();
            ea.Key = Key;
            if (Values != null && Values.Count > 0)
                ea.Value = Values [cmbEnumeration.SelectedIndex];

            PropertyChanged?.Invoke (this, ea);
        }
    }
}