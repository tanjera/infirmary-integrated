/* Settings.Simulator.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera) (c) 2023
 *
 * Stores settings for persistence between sessions and for loading/saving simulations
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II.Settings {
    public class Window {
        public int? Left { get; set; }
        public int? Top { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? WindowState { get; set; }
    }
}