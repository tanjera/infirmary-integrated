using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IIDT {

    internal class Vertex {
        public double Y;
        public Point Pixel;

        public Vertex () {
            Pixel  = new  Point (0, 0);
        }

        public Vertex (double y) {
            Y = y;
            
            Pixel  = new  Point (0, 0);
        }
    }
}