using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using II;
using II.Localization;

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Numeric.xaml
    /// </summary>
    public partial class IABPNumeric : UserControl {

        public ControlType controlType;

        public class ControlType {
            public Values Value;
            public ControlType (Values v) { Value = v; }

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
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString(v)));
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

            lblNumType.Text = App.Language.Dictionary[ControlType.LookupString(controlType.Value)];

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
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.HR, 0.02f));
                    break;

                case ControlType.Values.ABP:
                    if (App.Patient.TransducerZeroed_ABP) {
                        lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.ASBP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", Utility.RandomPercentRange (
                            (!App.Patient.IABPRunning ? App.Patient.ADBP : App.Patient.IABP_DBP), 0.02f));
                        // IABP shows MAP calculated by IABP!! Different from how monitors calculate MAP...
                        lblLine3.Text = String.Format ("({0:0})", Utility.RandomPercentRange (App.Patient.IABP_MAP, 0.02f));
                    } else {
                        lblLine1.Text = Utility.WrapString(App.Language.Dictionary["NUMERIC:ZeroTransducer"]);
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlType.Values.IABP_AP:
                    // Flash augmentation pressure reading if below alarm limit
                    lblLine1.Foreground = App.Patient.IABP_AP < App.Patient.IABPAugmentationAlarm
                        ? (lblLine1.Foreground == Brushes.Red ? Brushes.SkyBlue : Brushes.Red)
                        : Brushes.SkyBlue;

                    lblLine1.Text = App.Patient.IABPRunning ? String.Format ("{0:0}", App.Patient.IABP_AP) : "";

                    lblLine2.Text = String.Format ("{0:0}%", App.Patient.IABPAugmentation);
                    lblLine3.Text = String.Format ("{0}: {1:0}", App.Language.Dictionary ["IABP:Alarm"],
                        App.Patient.IABPAugmentationAlarm);
                    break;
            }
        }
    }
}