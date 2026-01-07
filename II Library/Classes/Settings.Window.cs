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
        public int? X { get; set; }
        public int? Y { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public Avalonia.Controls.WindowState? WindowState { get; set; }

        public static Window Load (string raw) {
            Window w = new();
            string [] split = raw.Split (",");

            if (int.TryParse (split [0], out int x))
                w.X = x;
            if (int.TryParse (split [1], out int y))
                w.Y = y;
            if (double.TryParse (split [2], out double width))
                w.Width = width;
            if (double.TryParse (split [3], out double height))
                w.Height = height;
            if (Enum.TryParse<Avalonia.Controls.WindowState> (split [4], out var state))
                w.WindowState = state;
            
            return w;
        }

        public string Save () {
            return String.Join (",",
                X.ToString () ?? "",
                Y.ToString () ?? "",
                Width.ToString () ?? "",
                Height.ToString () ?? "",
                WindowState.ToString () ?? "");
        }
    }
}