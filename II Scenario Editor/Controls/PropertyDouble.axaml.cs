using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyDouble : UserControl {
        public Keys Key;

        public enum Keys {
            T,
            CO,
            RRInspiratoryRatio, RRExpiratoryRatio
        }

        public new event EventHandler<PropertyDoubleEventArgs>? PropertyChanged;

        public class PropertyDoubleEventArgs : EventArgs {
            public Keys Key;
            public double? Value;
        }

        public PropertyDouble () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (Keys key, double increment, double minvalue, double maxvalue) {
            Label lblKey = this.FindControl<Label> ("lblKey");
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.T: lblKey.Content = "Temperature: "; break;
            }

            numValue.Increment = increment;
            numValue.Minimum = minvalue;
            numValue.Maximum = maxvalue;
            numValue.ValueChanged += sendPropertyChange;
            numValue.LostFocus += sendPropertyChange;
        }

        public void Set (double value) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            numValue.ValueChanged -= sendPropertyChange;
            numValue.Value = value;
            numValue.ValueChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            PropertyDoubleEventArgs ea = new PropertyDoubleEventArgs ();
            ea.Key = Key;
            ea.Value = numValue.Value;
            PropertyChanged?.Invoke (this, ea);
        }
    }
}