using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace II.Controls {
    public partial class Tracing : UserControl {

        ContextMenu contextMenu = new ContextMenu ();
        _.ColorScheme tColorScheme = _.ColorScheme.Normal;
        Leads tLead;

        public event EventHandler<TracingEdited_EventArgs> TracingEdited;
        public class TracingEdited_EventArgs : EventArgs {
            public Leads Lead { get; set; }
            public TracingEdited_EventArgs (Leads lead) { Lead = lead; }
        }

        public Tracing (Leads l) {
            InitializeComponent ();

            this.DoubleBuffered = true;
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            this.Dock = DockStyle.Fill;

            tLead = l;

            contextMenu.MenuItems.Add("Select Input Source:");
            contextMenu.MenuItems.Add("-");
            MenuItem ecgList = new MenuItem("Electrocardiograph (ECG) Leads");
            contextMenu.MenuItems.Add(ecgList);
            foreach (string mif in Leads.MenuItem_Formats) {
                if (mif.StartsWith("ECG"))
                    ecgList.MenuItems.Add(mif, contextMenu_Click);
                else
                    contextMenu.MenuItems.Add(mif, contextMenu_Click);
            }

            setColorScheme (tColorScheme);
            setLead (tLead);
        }

        public void setLead(Leads l) {
            tLead = l;
            labelType.Text = _.UnderscoreToSpace(l.Value.ToString());
            setColorScheme (tColorScheme);
        }

        public void setColorScheme(_.ColorScheme cs) {
            tColorScheme = cs;

            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    labelType.ForeColor = tLead.Color;
                    labelType.BackColor = Color.Black;
                    break;

                case _.ColorScheme.Monochrome:
                    labelType.ForeColor = Color.Black;
                    labelType.BackColor = Color.White;
                    break;
            }
        }

        private void contextMenu_Click (object sender, EventArgs e) {
            tLead = new Leads (Leads.Parse_MenuItem (((MenuItem)sender).Text));
            TracingEdited (this, new TracingEdited_EventArgs (tLead));
        }

        private void onClick(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right) {
                contextMenu.Show(this, new Point(me.X, me.Y));
            }
        }
    }
}
