using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyDouble : UserControl {
        public Keys Key;

        public enum Keys {
            T,
            CO,
            QRSInterval,
            QTcInterval,
            RRInspiratoryRatio,
            RRExpiratoryRatio
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

        public Task Init (Keys key, double increment, double minvalue, double maxvalue) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyDoubleEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.T: lblKey.Content = "Temperature: "; break;
                case Keys.CO: lblKey.Content = "Cardiac Output: "; break;

                case Keys.QRSInterval:
                    lblKey.Content = "QRS Interval: ";
                    numValue.FormatString = "0.00";
                    break;

                case Keys.QTcInterval:
                    lblKey.Content = "QTc Interval: ";
                    numValue.FormatString = "0.00"; break;

                case Keys.RRExpiratoryRatio: lblKey.Content = "Expiratory Ratio: "; break;
                case Keys.RRInspiratoryRatio: lblKey.Content = "Inspiratory Ratio: "; break;
            }

            numValue.Increment = increment;
            numValue.Minimum = minvalue;
            numValue.Maximum = maxvalue;
            numValue.ValueChanged += SendPropertyChange;
            numValue.LostFocus += SendPropertyChange;

            return Task.CompletedTask;
        }

        public Task Set (double value) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            numValue.ValueChanged -= SendPropertyChange;
            numValue.Value = value;
            numValue.ValueChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown numValue = this.FindControl<NumericUpDown> ("numValue");

            PropertyDoubleEventArgs ea = new PropertyDoubleEventArgs ();
            ea.Key = Key;
            ea.Value = numValue.Value;

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}