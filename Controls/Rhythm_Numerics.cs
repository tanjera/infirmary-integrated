using System;
using System.Drawing;
using System.Windows.Forms;

namespace II.Controls {
    public partial class Rhythm_Numerics : UserControl {

        Patient lPatient;
        ContextMenu contextMenu = new ContextMenu();

        _.ColorScheme tColorScheme = _.ColorScheme.Normal;

        Font fontLarge = new Font("Microsoft Sans Serif", 20f, FontStyle.Bold),
             fontSmall = new Font("Microsoft Sans Serif", 15f, FontStyle.Bold);

        public ControlType cType;

        public enum ControlType {
            ECG,
            TEMP,
            RR,
            ETCO2,
            SPO2,
            NIBP,
            ABP,
            CVP,
            PA
        };

        static public Color valueColors(ControlType c) {
            switch (c) {
                default:
                case ControlType.ECG: return Color.Green;
                case ControlType.TEMP: return Color.LightGray;
                case ControlType.RR: return Color.Salmon;
                case ControlType.ETCO2: return Color.Aqua;
                case ControlType.SPO2: return Color.Orange;
                case ControlType.NIBP: return Color.White;
                case ControlType.ABP: return Color.Red;
                case ControlType.CVP: return Color.Blue;
                case ControlType.PA: return Color.Yellow;
            }
        }

        public Rhythm_Numerics (ControlType t) {
            InitializeComponent ();

            cType = t;

            this.DoubleBuffered = true;
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            this.Dock = DockStyle.Fill;

            contextMenu.MenuItems.Add("Select Output Display:");
            contextMenu.MenuItems.Add("-");
            foreach (ControlType ct in Enum.GetValues(typeof(ControlType)))
                contextMenu.MenuItems.Add(new MenuItem(_.UnderscoreToSpace(ct.ToString()), contextMenu_Click));
        }

        public void Update (Patient p) {
            lPatient = p;

            Color tColorBack;
            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    tColorBack = Color.Black;
                    this.BackColor = Color.Black;

                    labelType.ForeColor = valueColors (cType);
                    label1.ForeColor = valueColors (cType);
                    label2.ForeColor = valueColors (cType);
                    label3.ForeColor = valueColors (cType);
                    break;

                case _.ColorScheme.Monochrome:
                    tColorBack = Color.White;
                    this.BackColor = Color.White;

                    labelType.ForeColor = Color.Black;
                    label1.ForeColor = Color.Black;
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;
                    break;
            }

            switch (cType) {
                default:
                case ControlType.ECG:
                    labelType.Text = "ECG";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.HR, 0.02f));
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.TEMP:
                    labelType.Text = "T";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0.0} ºC", p.T);
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.SPO2:
                    labelType.Text = "SpO2";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0} %", _.RandomPercentRange(p.SpO2, 0.01f));
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.RR:
                    labelType.Text = "RR";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.RR, 0.02f));
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.ETCO2:
                    labelType.Text = "ETCO2";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.ETCO2, 0.02f));
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.NIBP:
                    labelType.Text = "NiBP";
                    label1.Font = fontSmall;
                    label2.Font = fontSmall;
                    label3.Font = fontSmall;
                    label1.Text = String.Format("{0:0}", p.NSBP);
                    label2.Text = String.Format("/ {0:0}", p.NDBP);
                    label3.Text = String.Format("({0:0})", p.NMAP);
                    break;

                case ControlType.ABP:
                    labelType.Text = "ABP";
                    label1.Font = fontSmall;
                    label2.Font = fontSmall;
                    label3.Font = fontSmall;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.ASBP, 0.02f));
                    label2.Text = String.Format ("/ {0:0}", _.RandomPercentRange(p.ADBP, 0.02f));
                    label3.Text = String.Format ("({0:0})", _.RandomPercentRange(p.AMAP, 0.02f));
                    break;

                case ControlType.CVP:
                    labelType.Text = "CVP";
                    label1.Font = fontLarge;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.CVP, 0.02f));
                    label2.ForeColor = tColorBack;
                    label3.ForeColor = tColorBack;
                    break;

                case ControlType.PA:
                    labelType.Text = "PA";
                    label1.Font = fontSmall;
                    label2.Font = fontSmall;
                    label3.Font = fontSmall;
                    label1.Text = String.Format("{0:0}", _.RandomPercentRange(p.PSP, 0.02f));
                    label2.Text = String.Format ("/ {0:0}", _.RandomPercentRange(p.PDP, 0.02f));
                    label3.Text = String.Format ("({0:0})", _.RandomPercentRange(p.PMP, 0.02f));
                    break;
            }
        }

        public void setColorScheme (_.ColorScheme cs) {
            tColorScheme = cs;
            Update (lPatient);
        }

        private void contextMenu_Click(object sender, EventArgs e) {
            cType = (ControlType)Enum.Parse(typeof(ControlType), _.SpaceToUnderscore(((MenuItem)sender).Text));
            Update (lPatient);
        }

        private void onClick(object sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                contextMenu.Show(this, new Point(me.X, me.Y));
            }
        }
    }
}
