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
                
            setLead (l);
        }

        public void setLead(Rhythms.Leads l) {
            labelType.Text = _.UnderscoreToSpace(l.ToString ());
            labelType.ForeColor = Rhythms.Strip.stripColors (l);
        }

        private void contextMenu_Click (object sender, EventArgs e) {
            Rhythms.Leads l = (Rhythms.Leads)Enum.Parse (typeof(Rhythms.Leads), _.SpaceToUnderscore(((MenuItem)sender).Text));
            TracingEdited (this, new TracingEdited_EventArgs (l));
        }
        
        private void onClick(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                contextMenu.Show(this, new Point(me.X, me.Y));
            }
        }
    }
}
