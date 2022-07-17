using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyCheck : UserControl {
        public Keys Key;

        public enum Keys {
            PulsusParadoxus,
            PulsusAlternans,
            ElectricalAlternans,
            MechanicallyVentilated,

            MonitorIsEnabled,
            DefibIsEnabled,
            ECGIsEnabled,
            IABPIsEnabled
        }

        public new event EventHandler<PropertyCheckEventArgs>? PropertyChanged;

        public class PropertyCheckEventArgs : EventArgs {
            public Keys Key;
            public bool Value;
        }

        public PropertyCheck () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyCheckEventArgs>)d;
            }

            CheckBox chkValue = this.FindControl<CheckBox> ("chkValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.PulsusParadoxus: chkValue.Content = "Pulsus Paradoxus"; break;
                case Keys.PulsusAlternans: chkValue.Content = "Pulsus Alternans"; break;
                case Keys.ElectricalAlternans: chkValue.Content = "Electrical Alternans"; break;
                case Keys.MechanicallyVentilated: chkValue.Content = "Mechanically ventilated?"; break;

                case Keys.MonitorIsEnabled: chkValue.Content = "Enable Cardiac Monitor?"; break;
                case Keys.DefibIsEnabled: chkValue.Content = "Enable Defibrillator?"; break;
                case Keys.ECGIsEnabled: chkValue.Content = "Enable 12 Lead ECG?"; break;
                case Keys.IABPIsEnabled: chkValue.Content = "Enable Intra-Aortic Balloon Pump?"; break;
            }

            chkValue.Checked += SendPropertyChange;
            chkValue.Unchecked += SendPropertyChange;
            chkValue.LostFocus += SendPropertyChange;

            return Task.CompletedTask;
        }

        public Task Set (bool value) {
            CheckBox chkValue = this.FindControl<CheckBox> ("chkValue");

            chkValue.Checked -= SendPropertyChange;
            chkValue.Unchecked -= SendPropertyChange;

            chkValue.IsChecked = value;

            chkValue.Checked += SendPropertyChange;
            chkValue.Unchecked += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            CheckBox chkValue = this.FindControl<CheckBox> ("chkValue");

            PropertyCheckEventArgs ea = new PropertyCheckEventArgs ();
            ea.Key = Key;
            ea.Value = chkValue.IsChecked ?? false;

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}