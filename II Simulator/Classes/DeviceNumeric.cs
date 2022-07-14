using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

namespace IISIM {

    public class DeviceNumeric : UserControl {
        public App? Instance;

        public object? DeviceParent;
        public Color.Schemes? ColorScheme;

        public DeviceNumeric () {
        }

        public DeviceNumeric (App? app) {
            Instance = app;
        }

        ~DeviceNumeric () {
        }
    }
}