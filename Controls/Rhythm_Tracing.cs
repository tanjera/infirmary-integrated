using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace II.Controls {
    public partial class Rhythm_Tracing : UserControl {

        ContextMenu contextMenu = new ContextMenu ();
        _.ColorScheme tColorScheme = _.ColorScheme.Normal;
        Rhythms.Leads tLead;

        public event EventHandler<TracingEdited_EventArgs> TracingEdited;
        public class TracingEdited_EventArgs : EventArgs {
            public Rhythms.Leads Lead { get; set; }
            public TracingEdited_EventArgs (Rhythms.Leads lead) { Lead = lead; }
        }

        public Rhythm_Tracing (Rhythms.Leads l) {
            InitializeComponent ();
            
            this.DoubleBuffered = true;         
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
            this.Dock = DockStyle.Fill;

            tLead = l;

            contextMenu.MenuItems.Add("Select Input Source:");
            contextMenu.MenuItems.Add("-");
            MenuItem ecgList = new MenuItem("ECG Leads");
            contextMenu.MenuItems.Add(ecgList);
            foreach (Rhythms.Leads el in Enum.GetValues (typeof (Rhythms.Leads))) {
                if (el.ToString().StartsWith("ECG"))
                    ecgList.MenuItems.Add(new MenuItem(_.UnderscoreToSpace(el.ToString()), contextMenu_Click));
                else
                    contextMenu.MenuItems.Add(new MenuItem(_.UnderscoreToSpace(el.ToString()), contextMenu_Click));
            }

            setColorScheme (tColorScheme);
            setLead (tLead);
        }

        public void setLead(Rhythms.Leads l) {
            tLead = l;
            labelType.Text = _.UnderscoreToSpace(l.ToString ());            
        }

        public void setColorScheme(_.ColorScheme cs) {
            tColorScheme = cs;

            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    labelType.ForeColor = Rhythms.Strip.stripColors (tLead);
                    labelType.BackColor = Color.Black;
                    break;

                case _.ColorScheme.Monochrome:
                    labelType.ForeColor = Color.Black;
                    labelType.BackColor = Color.White;
                    break;
            }
        }        

        private void contextMenu_Click (object sender, EventArgs e) {
            tLead = (Rhythms.Leads)Enum.Parse (typeof(Rhythms.Leads), _.SpaceToUnderscore(((MenuItem)sender).Text));
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
