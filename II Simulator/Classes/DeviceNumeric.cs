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

        /* Variables controlling for visual alarms */
        public Timer? AlarmTimer = new ();
        public bool? AlarmIterator = false;

        public bool? AlarmLine1;
        public bool? AlarmLine2;
        public bool? AlarmLine3;

        public DeviceNumeric () {
        }

        public DeviceNumeric (App? app) {
            Instance = app;

            InitTimers ();
            InitAlarm ();
        }

        ~DeviceNumeric () {
            AlarmTimer?.Dispose ();
        }

        public virtual void InitTimers () {
            if (Instance is null || AlarmTimer is null)
                return;

            Instance.Timer_Main.Elapsed += AlarmTimer.Process;

            AlarmTimer.Tick += (s, e) => { Dispatcher.UIThread.InvokeAsync (() => { OnTick_Alarm (s, e); }); };

            AlarmTimer.Set (1000);
            AlarmTimer.Start ();
        }

        public virtual void InitAlarm () {
            AlarmLine1 = false;
            AlarmLine2 = false;
            AlarmLine3 = false;
        }

        public virtual void OnTick_Alarm (object? sender, EventArgs e) {
        }
    }
}