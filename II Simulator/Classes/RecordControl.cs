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

namespace IISIM {

    public class RecordControl : UserControl {
        public App? Instance { get; set; }

        public RecordControl () {
        }

        public RecordControl (App? app) {
            Instance = app;
        }
    }
}