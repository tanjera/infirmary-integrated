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

        public event EventHandler<TracingEdited_EventArgs> TracingEdited;
        public class TracingEdited_EventArgs : EventArgs {
            public Rhythms.Leads Lead { get; set; }
            public TracingEdited_EventArgs (Rhythms.Leads lead) { Lead = lead; }
        }

        public Tracing (Rhythms.Leads l) {
            InitializeComponent ();
            
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;

            foreach (Rhythms.Leads el in Enum.GetValues (typeof (Rhythms.Leads))) {
                MenuItem mi = new MenuItem (el.ToString (), contextMenu_Click);                
                contextMenu.MenuItems.Add (mi);               
            }

            setLead (l);
        }

        public void setLead(Rhythms.Leads l) {
            labelType.Text = l.ToString ();
            labelType.ForeColor = Rhythms.Strip.stripColors (l);
        }

        private void contextMenu_Click (object sender, EventArgs e) {
            Rhythms.Leads l = (Rhythms.Leads)Enum.Parse (typeof(Rhythms.Leads), ((MenuItem)sender).Text);
            TracingEdited (this, new TracingEdited_EventArgs (l));
            
        }

        private void Tracing_Click (object sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right) {
                contextMenu.Show (this, new Point (me.X, me.Y));
            }
        }
    }
}
