using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using II;
using II.Localization;
using II.Rhythm;

namespace II_Avalonia.Controls {

    public partial class IABPNumeric : UserControl {
        public DeviceDefib deviceParent;
        public ControlType controlType;
        public Color.Schemes colorScheme;

        public class ControlType {
            public Values Value;

            public ControlType (Values v) {
                Value = v;
            }

            public enum Values {
                ECG, ABP, IABP_AP
            }

            public Color.Leads GetLead_Color {
                get { return SwitchLead_Color (this.Value); }
            }

            private static Color.Leads SwitchLead_Color (Values value) => value switch {
                ControlType.Values.ECG => Color.Leads.ECG,
                ControlType.Values.ABP => Color.Leads.ABP,
                ControlType.Values.IABP_AP => Color.Leads.IABP,
                _ => Color.Leads.ECG
            };

            public static string LookupString (Values value) {
                return String.Format ("NUMERIC:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
            }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string> ();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString (v)));
                    return o;
                }
            }
        }

        public IABPNumeric () {
            InitializeComponent ();
        }

        public IABPNumeric (ControlType.Values v, Color.Schemes cs) {
            InitializeComponent ();

            controlType = new ControlType (v);
            colorScheme = cs;

            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface () {
            Border borderNumeric = this.FindControl<Border> ("borderNumeric");
            TextBlock lblNumType = this.FindControl<TextBlock> ("lblNumType");
            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            borderNumeric.BorderBrush = Color.GetLead (controlType.GetLead_Color, colorScheme);

            lblNumType.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
            lblLine1.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
            lblLine2.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
            lblLine3.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);

            lblLine1.IsVisible = true;
            lblLine2.IsVisible = true;
            lblLine3.IsVisible = true;

            lblNumType.Text = App.Language.Localize (ControlType.LookupString (controlType.Value));

            /* Set lines to be visible/hidden as appropriate */
            switch (controlType.Value) {
                default:
                case ControlType.Values.ABP:
                case ControlType.Values.IABP_AP:
                    break;

                case ControlType.Values.ECG:
                    lblLine2.IsVisible = false;
                    lblLine3.IsVisible = false;
                    break;
            }
        }

        public void UpdateVitals () {
            if (App.Patient == null)
                return;

            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            switch (controlType.Value) {
                default:
                case ControlType.Values.ECG:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.MeasureHR_ECG (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlType.Values.ABP:
                    if (App.Patient.TransducerZeroed_ABP) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.ASBP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (
                            (!App.Device_IABP.Running ? App.Patient.ADBP : App.Patient.IABP_DBP), 0.02f));

                        // IABP shows MAP calculated by IABP!! Different from how monitors calculate MAP...
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (App.Patient.IABP_MAP, 0.02f));
                    } else {
                        lblLine1.Text = Utility.WrapString (App.Language.Localize ("NUMERIC:ZeroTransducer"));
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlType.Values.IABP_AP:

                    // Flash augmentation pressure reading if below alarm limit
                    lblLine1.Foreground = App.Patient.IABP_AP < App.Device_IABP.AugmentationAlarm
                        ? (lblLine1.Foreground == Brushes.Red ? Brushes.SkyBlue : Brushes.Red)
                        : Brushes.SkyBlue;

                    lblLine1.Text = App.Device_IABP.Running ? String.Format ("{0:0}", App.Patient.IABP_AP) : "";

                    lblLine2.Text = String.Format ("{0:0}%", App.Device_IABP.Augmentation);
                    lblLine3.Text = String.Format ("{0}: {1:0}", App.Language.Localize ("IABP:Alarm"),
                        App.Device_IABP.AugmentationAlarm);
                    break;
            }
        }
    }
}