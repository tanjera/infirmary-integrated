using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyInt : UserControl {
        public Keys Key;

        public enum Keys {
            HR, RR, ETCO2, SPO2,            // Heart rate, respiratory rate, end-tidal capnography, pulse oximetry
            CVP,                            // Central venous pressure,
            ICP, IAP,                       // Intracranial pressure, intra-abdominal pressure
            PacemakerThreshold,
            ProgressTimer
        }

        public new event EventHandler<PropertyIntEventArgs>? PropertyChanged;

        public class PropertyIntEventArgs : EventArgs {
            public Keys Key;
            public int Value;
        }

        public PropertyInt () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (Keys key, int increment, int minvalue, int maxvalue) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyIntEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.HR: lblKey.Content = "Heart Rate: "; break;
                case Keys.RR: lblKey.Content = "Respiratory Rate: "; break;
                case Keys.ETCO2: lblKey.Content = "End-tidal CO2: "; break;
                case Keys.SPO2: lblKey.Content = "Pulse Oximetry: "; break;
                case Keys.CVP: lblKey.Content = "Central Venous Pressure: "; break;
                case Keys.ICP: lblKey.Content = "Intra-cranial Pressure: "; break;
                case Keys.IAP: lblKey.Content = "Intra-abdominal Pressure: "; break;
                case Keys.PacemakerThreshold: lblKey.Content = "Pacemaker Capture Threshold: "; break;
                case Keys.ProgressTimer: lblKey.Content = "Time (in seconds) until next step: "; break;
            }

            numValue.Increment = increment;
            numValue.Minimum = minvalue;
            numValue.Maximum = maxvalue;
            numValue.ValueChanged += SendPropertyChange;
            numValue.LostFocus += SendPropertyChange;
        }

        public void Set (int value) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            numValue.ValueChanged -= SendPropertyChange;
            numValue.Value = value;
            numValue.ValueChanged += SendPropertyChange;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            PropertyIntEventArgs ea = new PropertyIntEventArgs ();
            ea.Key = Key;
            ea.Value = (int)numValue.Value;

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}