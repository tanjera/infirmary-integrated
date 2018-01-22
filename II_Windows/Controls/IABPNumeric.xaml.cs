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
                ECG, T, RR, ETCO2,
                SPO2, NIBP, ABP, CVP,
                PA
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
                Brushes.LightGray,
                Brushes.Salmon,
                Brushes.Aqua,
                Brushes.Orange,
                Brushes.White,
                Brushes.Red,
                Brushes.Blue,
                Brushes.Yellow
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

            lblNumType.Text = Strings.Lookup(App.Language.Value, ControlType.LookupString(controlType.Value));

            switch (controlType.Value) {
                default:
                case ControlType.Values.NIBP:
                case ControlType.Values.ABP:
                case ControlType.Values.PA:
                    break;

                case ControlType.Values.ECG:
                case ControlType.Values.T:
                case ControlType.Values.RR:
                case ControlType.Values.CVP:
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.SPO2:
                case ControlType.Values.ETCO2:
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

                case ControlType.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", App.Patient.T);
                    break;

                case ControlType.Values.SPO2:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.SPO2, 0.01f));
                    lblLine2.Text = String.Format ("@ {0:0}", Utility.RandomPercentRange (App.Patient.HR, 0.01f));
                    break;

                case ControlType.Values.RR:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.RR, 0.02f));
                    break;

                case ControlType.Values.ETCO2:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.ETCO2, 0.02f));
                    lblLine2.Text = String.Format ("@ {0:0}", Utility.RandomPercentRange (App.Patient.RR, 0.02f));
                    break;

                case ControlType.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", App.Patient.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", App.Patient.NMAP);
                    break;

                case ControlType.Values.ABP:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.ASBP, 0.02f));
                    lblLine2.Text = String.Format ("/ {0:0}", Utility.RandomPercentRange (App.Patient.ADBP, 0.02f));
                    lblLine3.Text = String.Format ("({0:0})", Utility.RandomPercentRange (App.Patient.AMAP, 0.02f));
                    break;

                case ControlType.Values.CVP:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.CVP, 0.02f));
                    break;

                case ControlType.Values.PA:
                    lblLine1.Text = String.Format ("{0:0}", Utility.RandomPercentRange (App.Patient.PSP, 0.02f));
                    lblLine2.Text = String.Format ("/ {0:0}", Utility.RandomPercentRange (App.Patient.PDP, 0.02f));
                    lblLine3.Text = String.Format ("({0:0})", Utility.RandomPercentRange (App.Patient.PMP, 0.02f));
                    break;
            }
        }
    }
}