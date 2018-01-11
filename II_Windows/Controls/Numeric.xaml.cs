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

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Numeric.xaml
    /// </summary>
    public partial class Numeric : UserControl {

        Patient lPatient;
        public ControlType controlType;

        double fontLarge = 50d, fontMedium = 25d, fontSmall = 15d,
            _fontLarge = 50d, _fontMedium = 25d, _fontSmall = 15d;

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


        public Numeric (ControlType.Values v) {
            InitializeComponent ();

            controlType = new ControlType (v);

            lblNumType.Content = "";
            lblLine1.Content = "";
            lblLine2.Content = "";
            lblLine3.Content = "";
        }

        public void Update (Patient p) {
            lPatient = (p == null ? lPatient : p);
            if (lPatient == null)
                return;

            lblNumType.Foreground = controlType.Color;
            lblLine1.Foreground = controlType.Color;
            lblLine2.Foreground = controlType.Color;
            lblLine3.Foreground = controlType.Color;

            lblLine1.Visibility = Visibility.Visible;
            lblLine2.Visibility = Visibility.Visible;
            lblLine3.Visibility = Visibility.Visible;

            switch (controlType.Value) {
                default:
                case ControlType.Values.ECG:
                    lblNumType.Content = "ECG";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.HR, 0.02f));
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.T:
                    lblNumType.Content = "T";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0.0} C", p.T);
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.SPO2:
                    lblNumType.Content = "SpO2";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.SPO2, 0.01f));
                    lblLine2.FontSize = fontSmall;
                    lblLine2.Content = String.Format ("HR: {0:0}", Utility.RandomPercentRange (p.HR, 0.01f));
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.RR:
                    lblNumType.Content = "RR";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.RR, 0.02f));
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.ETCO2:
                    lblNumType.Content = "ETCO2";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.ETCO2, 0.02f));
                    lblLine2.FontSize = fontSmall;
                    lblLine2.Content = String.Format ("RR: {0:0}", Utility.RandomPercentRange (p.RR, 0.02f));
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.NIBP:
                    lblNumType.Content = "NIBP";
                    lblLine1.FontSize = fontMedium;
                    lblLine2.FontSize = fontMedium;
                    lblLine3.FontSize = fontMedium;
                    lblLine1.Content = String.Format ("{0:0}", p.NSBP);
                    lblLine2.Content = String.Format ("/ {0:0}", p.NDBP);
                    lblLine3.Content = String.Format ("({0:0})", p.NMAP);
                    break;

                case ControlType.Values.ABP:
                    lblNumType.Content = "ABP";
                    lblLine1.FontSize = fontMedium;
                    lblLine2.FontSize = fontMedium;
                    lblLine3.FontSize = fontMedium;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.ASBP, 0.02f));
                    lblLine2.Content = String.Format ("/ {0:0}", Utility.RandomPercentRange (p.ADBP, 0.02f));
                    lblLine3.Content = String.Format ("({0:0})", Utility.RandomPercentRange (p.AMAP, 0.02f));
                    break;

                case ControlType.Values.CVP:
                    lblNumType.Content = "CVP";
                    lblLine1.FontSize = fontLarge;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.CVP, 0.02f));
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.PA:
                    lblNumType.Content = "PA";
                    lblLine1.FontSize = fontMedium;
                    lblLine2.FontSize = fontMedium;
                    lblLine3.FontSize = fontMedium;
                    lblLine1.Content = String.Format ("{0:0}", Utility.RandomPercentRange (p.PSP, 0.02f));
                    lblLine2.Content = String.Format ("/ {0:0}", Utility.RandomPercentRange (p.PDP, 0.02f));
                    lblLine3.Content = String.Format ("({0:0})", Utility.RandomPercentRange (p.PMP, 0.02f));
                    break;
            }
        }

        public void SetFontSize (float coeff) {
            fontLarge = _fontLarge * coeff;
            fontMedium = _fontMedium * coeff;
            fontSmall = _fontSmall * coeff;

            Update (lPatient);
        }

    }
}