using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyCheck : UserControl {
        public Keys Key;

        public enum Keys {
            PulsusParadoxus,
            PulsusAlternans,
            MechanicallyVentilated
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

        public void Init (Keys key) {
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
                case Keys.MechanicallyVentilated: chkValue.Content = "Mechanically ventilated?"; break;
            }

            chkValue.Checked += sendPropertyChange;
            chkValue.LostFocus += sendPropertyChange;
        }

        public void Set (bool value) {
            CheckBox chkValue = this.FindControl<CheckBox> ("chkValue");

            chkValue.Checked -= sendPropertyChange;
            chkValue.IsChecked = value;
            chkValue.Checked += sendPropertyChange;
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            CheckBox chkValue = this.FindControl<CheckBox> ("chkValue");

            PropertyCheckEventArgs ea = new PropertyCheckEventArgs ();
            ea.Key = Key;
            ea.Value = chkValue.IsChecked ?? false;

            PropertyChanged?.Invoke (this, ea);
        }
    }
}