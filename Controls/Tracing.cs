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
        public Tracing () {
            InitializeComponent ();
            
            this.DoubleBuffered = true;
        }
    }
}
