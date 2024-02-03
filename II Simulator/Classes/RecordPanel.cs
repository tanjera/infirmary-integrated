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

    public class RecordPanel : UserControl {
        public App? Instance { get; set; }

        public RecordPanel () {
        }

        public RecordPanel (App? app) {
            Instance = app;
        }
    }
}