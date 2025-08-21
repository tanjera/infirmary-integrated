using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IISIM.Controls {

    /// <summary>
    /// Interaction logic for DefibNumeric.xaml
    /// </summary>
    public partial class DefibNumeric : UserControl {
        public App? Instance { get; set; }

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Brush? TracingBrush = Brushes.Black;

        public DefibNumeric () {
            InitializeComponent ();
        }

        public DefibNumeric (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Instance = app;
            ColorScheme = cs;
            UpdateInterface ();
        }

        ~DefibNumeric () {
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            // TODO: Implement
        }
    }
}