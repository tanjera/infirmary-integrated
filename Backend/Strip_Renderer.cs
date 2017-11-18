using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Infirmary_Integrated.Rhythms {
    public class Strip_Renderer {

        Graphics g;
        Pen p;
        Strip s;

        public Strip_Renderer (Graphics _Graphics, ref Strip _Strip, Pen _Pen) {
            g = _Graphics;
            p = _Pen;
            s = _Strip;
        }

        public void Draw() {

        }
    }
}
