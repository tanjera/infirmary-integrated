using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyBP : UserControl {
        public Keys Key;

        public enum Keys {
            NSBP, NDBP, NMAP,               // Non-invasive blood pressures
            ASBP, ADBP, AMAP,               // Arterial line blood pressures
            PSP, PDP, PMP,                  // Pulmonary artery pressures
        }

        public new event EventHandler<PropertyIntEventArgs>? PropertyChanged;

        public class PropertyIntEventArgs : EventArgs {
            public Keys Key;
            public int? Value;
        }

        public PropertyBP () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key,
            int sysInc, int sysMin, int sysMax,
            int diasInc, int diasMin, int diasMax) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyIntEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            NumericUpDown numSystolic = this.FindControl<NumericUpDown> ("numSystolic");
            NumericUpDown numDiastolic = this.FindControl<NumericUpDown> ("numDiastolic");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.NSBP: lblKey.Content = "Non-invasive Blood Pressure: "; break;
                case Keys.ASBP: lblKey.Content = "Arterial Blood Pressure: "; break;
                case Keys.PSP: lblKey.Content = "Pulmonary Arterial Pressures: "; break;
            }

            numSystolic.Increment = sysInc;
            numSystolic.Minimum = sysMin;
            numSystolic.Maximum = sysMax;
            numSystolic.ValueChanged += SendPropertyChange;
            numSystolic.LostFocus += SendPropertyChange;

            numDiastolic.Increment = diasInc;
            numDiastolic.Minimum = diasMin;
            numDiastolic.Maximum = diasMax;
            numDiastolic.ValueChanged += SendPropertyChange;
            numDiastolic.LostFocus += SendPropertyChange;

            return Task.CompletedTask;
        }

        public Task Set (int systolic, int diastolic) {
            NumericUpDown numSystolic = this.FindControl<NumericUpDown> ("numSystolic");
            NumericUpDown numDiastolic = this.FindControl<NumericUpDown> ("numDiastolic");

            numSystolic.ValueChanged -= SendPropertyChange;
            numDiastolic.ValueChanged -= SendPropertyChange;

            numSystolic.Value = systolic;
            numDiastolic.Value = diastolic;

            numSystolic.ValueChanged += SendPropertyChange;
            numDiastolic.ValueChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown numSystolic = this.FindControl<NumericUpDown> ("numSystolic");
            NumericUpDown numDiastolic = this.FindControl<NumericUpDown> ("numDiastolic");

            PropertyIntEventArgs ea = new PropertyIntEventArgs ();
            List<Keys> keys = new List<Keys> ();

            switch (Key) {
                default: break;
                case PropertyBP.Keys.NSBP:
                    keys = new List<Keys> () { PropertyBP.Keys.NSBP, PropertyBP.Keys.NDBP, PropertyBP.Keys.NMAP };
                    break;

                case PropertyBP.Keys.ASBP:
                    keys = new List<Keys> () { PropertyBP.Keys.ASBP, PropertyBP.Keys.ADBP, PropertyBP.Keys.AMAP };
                    break;

                case PropertyBP.Keys.PSP:
                    keys = new List<Keys> () { PropertyBP.Keys.PSP, PropertyBP.Keys.PDP, PropertyBP.Keys.PMP };
                    break;
            }

            for (int i = 0; i < keys.Count; i++) {
                ea = new PropertyIntEventArgs ();

                ea.Key = keys [i];

                switch (i) {
                    default: break;
                    case 0: ea.Value = (int)numSystolic.Value; break;
                    case 1: ea.Value = (int)numDiastolic.Value; break;
                    case 2: ea.Value = II.Patient.CalculateMAP ((int)numSystolic.Value, (int)numDiastolic.Value); break;
                }

                Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
                PropertyChanged?.Invoke (this, ea);
            }
        }
    }
}