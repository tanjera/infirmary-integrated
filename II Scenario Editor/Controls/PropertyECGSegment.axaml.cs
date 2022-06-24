using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyECGSegment : UserControl {
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

        public void Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyECGEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            NumericUpDown dblI = this.FindControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.FindControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.FindControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.FindControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.FindControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.FindControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.FindControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.FindControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.FindControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.FindControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.FindControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.FindControl<NumericUpDown> ("dblV6");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.STElevation: lblKey.Content = "ST Segment Elevation: "; break;
                case Keys.TWave: lblKey.Content = "T Wave Elevation: "; break;
            }

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

        public void Set (double [] values) {
            NumericUpDown dblI = this.FindControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.FindControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.FindControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.FindControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.FindControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.FindControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.FindControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.FindControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.FindControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.FindControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.FindControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.FindControl<NumericUpDown> ("dblV6");

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

            dblI.Value = values [0];
            dblII.Value = values [1];
            dblIII.Value = values [2];
            dblaVR.Value = values [3];
            dblaVL.Value = values [4];
            dblaVF.Value = values [5];
            dblV1.Value = values [6];
            dblV2.Value = values [7];
            dblV3.Value = values [8];
            dblV4.Value = values [9];
            dblV5.Value = values [10];
            dblV6.Value = values [11];

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
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown dblI = this.FindControl<NumericUpDown> ("dblI");
            NumericUpDown dblII = this.FindControl<NumericUpDown> ("dblII");
            NumericUpDown dblIII = this.FindControl<NumericUpDown> ("dblIII");
            NumericUpDown dblaVR = this.FindControl<NumericUpDown> ("dblaVR");
            NumericUpDown dblaVL = this.FindControl<NumericUpDown> ("dblaVL");
            NumericUpDown dblaVF = this.FindControl<NumericUpDown> ("dblaVF");
            NumericUpDown dblV1 = this.FindControl<NumericUpDown> ("dblV1");
            NumericUpDown dblV2 = this.FindControl<NumericUpDown> ("dblV2");
            NumericUpDown dblV3 = this.FindControl<NumericUpDown> ("dblV3");
            NumericUpDown dblV4 = this.FindControl<NumericUpDown> ("dblV4");
            NumericUpDown dblV5 = this.FindControl<NumericUpDown> ("dblV5");
            NumericUpDown dblV6 = this.FindControl<NumericUpDown> ("dblV6");

            PropertyECGEventArgs ea = new PropertyECGEventArgs ();
            ea.Key = Key;
            ea.Values = new double [] {
                dblI.Value, dblII.Value, dblIII.Value,
                dblaVR.Value, dblaVL.Value, dblaVF.Value,
                dblV1.Value, dblV2.Value, dblV3.Value,
                dblV4.Value, dblV5.Value, dblV6.Value
                };

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Values}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}