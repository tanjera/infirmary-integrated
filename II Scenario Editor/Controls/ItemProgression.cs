using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace II.Scenario_Editor.Controls {

    public sealed class ItemProgression : Shape {
        public ItemStep Step;

        public ItemProgression () {
        }

        protected override Geometry DefiningGeometry {
            get { return new EllipseGeometry (new Rect (0, 0, this.Width, this.Height)); }
        }
    }
}