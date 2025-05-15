using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyECGSegment : UserControl {
        private bool isInitiated = false;

        public Keys Key;

        public enum Keys {
            STElevation,
            TWave
        }

        public new event EventHandler<PropertyECGEventArgs>? PropertyChanged;

        public class PropertyECGEventArgs : EventArgs {
            public Keys Key;
            public double []? Values;
        }

        public PropertyECGSegment () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyECGEventArgs>)d;
            }

            Label lblKey = this.GetControl<Label> ("lblKey");
            NumericUpDown dblI = this.GetControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.GetControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.GetControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.GetControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.GetControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.GetControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.GetControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.GetControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.GetControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.GetControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.GetControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.GetControl<NumericUpDown> ("dblV6");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.STElevation: lblKey.Content = "ST Segment Elevation: "; break;
                case Keys.TWave: lblKey.Content = "T Wave Elevation: "; break;
            }

            if (!isInitiated) {
                dblI.ValueChanged += SendPropertyChange;
                dblII.ValueChanged += SendPropertyChange;
                dblIII.ValueChanged += SendPropertyChange;
                dblaVR.ValueChanged += SendPropertyChange;
                dblaVL.ValueChanged += SendPropertyChange;
                dblaVF.ValueChanged += SendPropertyChange;
                dblV1.ValueChanged += SendPropertyChange;
                dblV2.ValueChanged += SendPropertyChange;
                dblV3.ValueChanged += SendPropertyChange;
                dblV4.ValueChanged += SendPropertyChange;
                dblV5.ValueChanged += SendPropertyChange;
                dblV6.ValueChanged += SendPropertyChange;

                dblI.LostFocus += SendPropertyChange;
                dblII.LostFocus += SendPropertyChange;
                dblIII.LostFocus += SendPropertyChange;
                dblaVR.LostFocus += SendPropertyChange;
                dblaVL.LostFocus += SendPropertyChange;
                dblaVF.LostFocus += SendPropertyChange;
                dblV1.LostFocus += SendPropertyChange;
                dblV2.LostFocus += SendPropertyChange;
                dblV3.LostFocus += SendPropertyChange;
                dblV4.LostFocus += SendPropertyChange;
                dblV5.LostFocus += SendPropertyChange;
                dblV6.LostFocus += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public Task Set (double []? values) {
            if (values is null || values.Length != 12)
                return Task.CompletedTask;

            NumericUpDown dblI = this.GetControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.GetControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.GetControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.GetControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.GetControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.GetControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.GetControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.GetControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.GetControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.GetControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.GetControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.GetControl<NumericUpDown> ("dblV6");

            dblI.ValueChanged -= SendPropertyChange;
            dblII.ValueChanged -= SendPropertyChange;
            dblIII.ValueChanged -= SendPropertyChange;
            dblaVR.ValueChanged -= SendPropertyChange;
            dblaVL.ValueChanged -= SendPropertyChange;
            dblaVF.ValueChanged -= SendPropertyChange;
            dblV1.ValueChanged -= SendPropertyChange;
            dblV2.ValueChanged -= SendPropertyChange;
            dblV3.ValueChanged -= SendPropertyChange;
            dblV4.ValueChanged -= SendPropertyChange;
            dblV5.ValueChanged -= SendPropertyChange;
            dblV6.ValueChanged -= SendPropertyChange;

            dblI.Value = (decimal?)values [0];
            dblII.Value = (decimal?)values [1];
            dblIII.Value = (decimal?)values [2];
            dblaVR.Value = (decimal?)values [3];
            dblaVL.Value = (decimal?)values [4];
            dblaVF.Value = (decimal?)values [5];
            dblV1.Value = (decimal?)values [6];
            dblV2.Value = (decimal?)values [7];
            dblV3.Value = (decimal?)values [8];
            dblV4.Value = (decimal?)values [9];
            dblV5.Value = (decimal?)values [10];
            dblV6.Value = (decimal?)values [11];

            dblI.ValueChanged += SendPropertyChange;
            dblII.ValueChanged += SendPropertyChange;
            dblIII.ValueChanged += SendPropertyChange;
            dblaVR.ValueChanged += SendPropertyChange;
            dblaVL.ValueChanged += SendPropertyChange;
            dblaVF.ValueChanged += SendPropertyChange;
            dblV1.ValueChanged += SendPropertyChange;
            dblV2.ValueChanged += SendPropertyChange;
            dblV3.ValueChanged += SendPropertyChange;
            dblV4.ValueChanged += SendPropertyChange;
            dblV5.ValueChanged += SendPropertyChange;
            dblV6.ValueChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown dblI = this.GetControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.GetControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.GetControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.GetControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.GetControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.GetControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.GetControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.GetControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.GetControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.GetControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.GetControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.GetControl<NumericUpDown> ("dblV6");

            PropertyECGEventArgs ea = new PropertyECGEventArgs ();
            ea.Key = Key;
            ea.Values = new double [] {
                (double)(dblI.Value ?? 0), 
                (double)(dblII.Value ?? 0),
                (double)(dblIII.Value ?? 0),
                (double)(dblaVR.Value ?? 0),
                (double)(dblaVL.Value ?? 0), 
                (double)(dblaVF.Value ?? 0),
                (double)(dblV1.Value ?? 0), 
                (double)(dblV2.Value ?? 0),
                (double)(dblV3.Value ?? 0),
                (double)(dblV4.Value ?? 0), 
                (double)(dblV5.Value ?? 0),
                (double)(dblV6.Value ?? 0)
                };

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Values}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}