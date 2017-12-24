using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace II.Controls {
    public partial class Numeric : UserControl {

        Patient lPatient;
        ContextMenu contextMenu = new ContextMenu();

        _.ColorScheme tColorScheme = _.ColorScheme.Normal;

        public ControlType controlType;

        Font fontLarge = new Font("Arial", 50f, FontStyle.Bold),
             fontMedium = new Font("Arial", 25f, FontStyle.Bold),
             fontSmall = new Font ("Arial", 15f, FontStyle.Bold);

        public class ControlType {
            public Values Value;
            public ControlType (Values v) { Value = v; }

            public enum Values {
                ECG, T, RR, ETCO2,
                SPO2, NIBP, ABP, CVP,
                PA
            }

            public string Description { get { return Descriptions[(int)Value]; } }
            public Color Color { get { return Colors[(int)Value]; } }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string>();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), Descriptions[(int)v]));
                    return o;
                }
            }
            public static Values Parse_MenuItem (string inc) {
                string portion = inc.Substring (0, inc.IndexOf (':'));
                try {
                    return (Values)Enum.Parse (typeof (Values), portion);
                } catch {
                    return Values.ECG;
                }
            }

            public static string[] Descriptions = new string[] {
                "Electrocardiograph",
                "Temperature",
                "Respiratory Rate",
                "End-tidal Capnography",
                "Pulse Oximetry",
                "Non-invasive Blood Pressure",
                "Arterial Blood Pressure",
                "Central Venous Pressure",
                "Pulmonary Artery Pressure"
            };

            public static Color[] Colors = new Color[] {
                Color.Green,
                Color.LightGray,
                Color.Salmon,
                Color.Aqua,
                Color.Orange,
                Color.White,
                Color.Red,
                Color.Blue,
                Color.Yellow
            };
        }


        public Numeric (ControlType.Values v, _.ColorScheme cs) {
            InitializeComponent ();

            controlType = new ControlType(v);
            tColorScheme = cs;

            ApplyColorScheme ();
            labelType.Text = "";
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";

            this.DoubleBuffered = true;
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            this.Dock = DockStyle.Fill;

            contextMenu.MenuItems.Add ("Select Value to Display:");
            contextMenu.MenuItems.Add ("-");
            foreach (string mif in ControlType.MenuItem_Formats)
                contextMenu.MenuItems.Add(new MenuItem(mif, MenuValueSelect_Click));
        }

        public void Update (Patient p) {
            lPatient = (p == null ? lPatient : p);
            if (lPatient == null)
                return;

            ApplyColorScheme ();
            label1.Show ();
            label2.Show ();
            label3.Show ();

            switch (controlType.Value) {
                default:
                case ControlType.Values.ECG:
                    labelType.Text = "ECG";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.HR, 0.02f));
                    label2.Hide ();
                    label3.Hide ();
                    break;

                case ControlType.Values.T:
                    labelType.Text = "T";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0.0} C", p.T);
                    label2.Hide ();
                    label3.Hide ();
                    break;

                case ControlType.Values.SPO2:
                    labelType.Text = "SpO2";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.SPO2, 0.01f));
                    label2.Font = fontSmall;
                    label2.Text = String.Format ("HR: {0:0}", _.RandomPercentRange (p.HR, 0.01f));
                    label3.Hide ();
                    break;

                case ControlType.Values.RR:
                    labelType.Text = "RR";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.RR, 0.02f));
                    label2.Hide ();
                    label3.Hide ();
                    break;

                case ControlType.Values.ETCO2:
                    labelType.Text = "ETCO2";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.ETCO2, 0.02f));
                    label2.Font = fontSmall;
                    label2.Text = String.Format ("RR: {0:0}", _.RandomPercentRange (p.RR, 0.02f));
                    label3.Hide ();
                    break;

                case ControlType.Values.NIBP:
                    labelType.Text = "NIBP";
                    label1.Font = fontMedium;
                    label2.Font = fontMedium;
                    label3.Font = fontMedium;
                    label1.Text = String.Format("{0:0}", p.NSBP);
                    label2.Text = String.Format("/ {0:0}", p.NDBP);
                    label3.Text = String.Format("({0:0})", p.NMAP);
                    break;

                case ControlType.Values.ABP:
                    labelType.Text = "ABP";
                    label1.Font = fontMedium;
                    label2.Font = fontMedium;
                    label3.Font = fontMedium;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.ASBP, 0.02f));
                    label2.Text = String.Format ("/ {0:0}", _.RandomPercentRange(p.ADBP, 0.02f));
                    label3.Text = String.Format ("({0:0})", _.RandomPercentRange(p.AMAP, 0.02f));
                    break;

                case ControlType.Values.CVP:
                    labelType.Text = "CVP";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.CVP, 0.02f));
                    label2.Hide ();
                    label3.Hide ();
                    break;

                case ControlType.Values.PA:
                    labelType.Text = "PA";
                    label1.Font = fontMedium;
                    label2.Font = fontMedium;
                    label3.Font = fontMedium;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.PSP, 0.02f));
                    label2.Text = String.Format ("/ {0:0}", _.RandomPercentRange(p.PDP, 0.02f));
                    label3.Text = String.Format ("({0:0})", _.RandomPercentRange(p.PMP, 0.02f));
                    break;
            }
        }

        public void SetColorScheme (_.ColorScheme cs) {
            tColorScheme = cs;
            ApplyColorScheme ();
        }

        public void SetFontSize (float mod)
        {
            fontLarge = new Font ("Arial", 50 * mod, FontStyle.Bold);
            fontMedium = new Font ("Arial", 25 * mod, FontStyle.Bold);
            fontSmall = new Font ("Arial", 15 * mod, FontStyle.Bold);

            Update (lPatient);
        }

        private void ApplyColorScheme () {
            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    this.BackColor = Color.Black;

                    labelType.ForeColor = controlType.Color;
                    label1.ForeColor = controlType.Color;
                    label2.ForeColor = controlType.Color;
                    label3.ForeColor = controlType.Color;
                    break;

                case _.ColorScheme.Monochrome:
                    this.BackColor = Color.White;

                    labelType.ForeColor = Color.Black;
                    label1.ForeColor = Color.Black;
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;
                    break;
            }
        }

        private void MenuValueSelect_Click (object sender, EventArgs e) {
            controlType.Value = ControlType.Parse_MenuItem (((MenuItem)sender).Text);
            Update (lPatient);
        }

        private void OnClick(object sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                contextMenu.Show(this, new Point(me.X, me.Y));
            }
        }
    }
}
