﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

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

            dblI.ValueChanged += sendPropertyChange;
            dblII.ValueChanged += sendPropertyChange;
            dblIII.ValueChanged += sendPropertyChange;
            dblaVR.ValueChanged += sendPropertyChange;
            dblaVL.ValueChanged += sendPropertyChange;
            dblaVF.ValueChanged += sendPropertyChange;
            dblV1.ValueChanged += sendPropertyChange;
            dblV2.ValueChanged += sendPropertyChange;
            dblV3.ValueChanged += sendPropertyChange;
            dblV4.ValueChanged += sendPropertyChange;
            dblV5.ValueChanged += sendPropertyChange;
            dblV6.ValueChanged += sendPropertyChange;

            dblI.LostFocus += sendPropertyChange;
            dblII.LostFocus += sendPropertyChange;
            dblIII.LostFocus += sendPropertyChange;
            dblaVR.LostFocus += sendPropertyChange;
            dblaVL.LostFocus += sendPropertyChange;
            dblaVF.LostFocus += sendPropertyChange;
            dblV1.LostFocus += sendPropertyChange;
            dblV2.LostFocus += sendPropertyChange;
            dblV3.LostFocus += sendPropertyChange;
            dblV4.LostFocus += sendPropertyChange;
            dblV5.LostFocus += sendPropertyChange;
            dblV6.LostFocus += sendPropertyChange;
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

            dblI.ValueChanged -= sendPropertyChange;
            dblII.ValueChanged -= sendPropertyChange;
            dblIII.ValueChanged -= sendPropertyChange;
            dblaVR.ValueChanged -= sendPropertyChange;
            dblaVL.ValueChanged -= sendPropertyChange;
            dblaVF.ValueChanged -= sendPropertyChange;
            dblV1.ValueChanged -= sendPropertyChange;
            dblV2.ValueChanged -= sendPropertyChange;
            dblV3.ValueChanged -= sendPropertyChange;
            dblV4.ValueChanged -= sendPropertyChange;
            dblV5.ValueChanged -= sendPropertyChange;
            dblV6.ValueChanged -= sendPropertyChange;

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

            dblI.ValueChanged += sendPropertyChange;
            dblII.ValueChanged += sendPropertyChange;
            dblIII.ValueChanged += sendPropertyChange;
            dblaVR.ValueChanged += sendPropertyChange;
            dblaVL.ValueChanged += sendPropertyChange;
            dblaVF.ValueChanged += sendPropertyChange;
            dblV1.ValueChanged += sendPropertyChange;
            dblV2.ValueChanged += sendPropertyChange;
            dblV3.ValueChanged += sendPropertyChange;
            dblV4.ValueChanged += sendPropertyChange;
            dblV5.ValueChanged += sendPropertyChange;
            dblV6.ValueChanged += sendPropertyChange;
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
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
            PropertyChanged?.Invoke (this, ea);
        }
    }
}