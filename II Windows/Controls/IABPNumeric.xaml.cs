using II;
using II.Rhythm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace II_Windows.Controls {

    /// <summary>
    /// Interaction logic for Numeric.xaml
    /// </summary>
    public partial class IABPNumeric : UserControl {
        public ControlType controlType;

        public class ControlType {
            public Values Value;

            public ControlType (Values v) {
                Value = v;
            }

            public enum Values {
                ECG, ABP, IABP_AP
            }

            public static string LookupString (Values value) {
                return String.Format ("NUMERIC:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
            }

            public Brush Color { get { return Coloring [(int)Value]; } }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string> ();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString (v)));
                    return o;
                }
            }

            public static Brush [] Coloring = new Brush [] {
                Brushes.Green,
                Brushes.Red,
                Brushes.SkyBlue
            };
        }

        public IABPNumeric (ControlType.Values v) {
            InitializeComponent ();

            controlType = new ControlType (v);
            UpdateInterface ();
        }

        private void UpdateInterface () {
            borderNumeric.BorderBrush = controlType.Color;

            lblNumType.Foreground = controlType.Color;
            lblLine1.Foreground = controlType.Color;
            lblLine2.Foreground = controlType.Color;
            lblLine3.Foreground = controlType.Color;

            lblLine1.Visibility = Visibility.Visible;
            lblLine2.Visibility = Visibility.Visible;
            lblLine3.Visibility = Visibility.Visible;

            lblNumType.Text = App.Language.Localize (ControlType.LookupString (controlType.Value));

            switch (controlType.Value) {
                default:
                case ControlType.Values.ABP:
                case ControlType.Values.IABP_AP:
                    break;

                case ControlType.Values.ECG:
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;
            }
        }

        public void UpdateVitals () {
            if (App.Patient == null)
                return;

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