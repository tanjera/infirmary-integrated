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

    public class ChartWindow : Window {
        public App? Instance { get; set; }

        public States State;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public ChartWindow () {
        }

        public ChartWindow (App? app) {
            Instance = app;

            Closed += this.OnClosed;

            State = States.Running;
        }

        public virtual void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;
        }
    }
}