using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Waveform_Editor {

    internal class Vertex {
        public double Y;
        public Point Pixel;

        public Vertex () {
        }

        public Vertex (double y) {
            Y = y;
        }
    }
}