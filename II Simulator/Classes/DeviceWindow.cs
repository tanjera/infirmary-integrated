using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

using LibVLCSharp.Shared;

namespace IISIM {

    public class DeviceWindow : Window {
        public App? Instance { get; set; }

        public Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables controlling for audio alarms */
        public MediaPlayer? AudioPlayer;
        
        public DeviceWindow () {
        }

        public DeviceWindow (App? app) {
            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitAudio ();
            InitTimers ();
        }

        ~DeviceWindow () {
            Dispose ();
        }

        public virtual void Dispose () {
            /* Clean subscriptions from the Main Timer */
            if (Instance is not null) {
                Instance.Timer_Main.Elapsed -= TimerTracing.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Cardiac.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Respiratory.Process;
                Instance.Timer_Main.Elapsed -= TimerAncillary_Delay.Process;
            }

            /* Dispose of local Timers */
            TimerTracing.Dispose ();
            TimerNumerics_Cardiac.Dispose ();
            TimerNumerics_Respiratory.Dispose ();
            TimerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (Instance?.Physiology != null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public virtual void InitAudio () {
            if (Instance?.AudioLib is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitAudio)}");
                return;
            }

            AudioPlayer = new MediaPlayer (Instance.AudioLib);
        }

        public virtual void DisposeAudio () {
            /* Note: It's important to nullify objects after Disposing them because this function may
             * be triggered multiple times (e.g. on Window.Close() and on Application.Exit()).
             * Since LibVLC wraps a C++ library, nullifying and null checking prevents accessing
             * released/reassigned memory blocks (a Memory Exception)
             */

            if (AudioPlayer is not null) {
                if (AudioPlayer.IsPlaying)
                    AudioPlayer.Stop ();
                AudioPlayer.Dispose ();
            }
            AudioPlayer = null;
        }

        public virtual void InitTimers () {
            if (Instance is null)
                return;

            /* TimerAncillary_Delay is attached/detached to events in the Devices for their
             * specific uses (e.g. IABP priming, Defib charging, etc.) ... only want to link it
             * to Timer_Main, otherwise do not set, start, or link to any events here!
             */
            Instance.Timer_Main.Elapsed += TimerAncillary_Delay.Process;

            Instance.Timer_Main.Elapsed += TimerAlarm.Process;
            Instance.Timer_Main.Elapsed += TimerTracing.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Cardiac.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Respiratory.Process;

            TimerAlarm.Tick += OnTick_Alarm;
            TimerTracing.Tick += OnTick_Tracing;
            TimerNumerics_Cardiac.Tick += OnTick_Vitals_Cardiac;
            TimerNumerics_Respiratory.Tick += OnTick_Vitals_Respiratory;

            TimerAlarm.Set (2500);
            TimerTracing.Set (Draw.RefreshTime);
            TimerNumerics_Cardiac.Set (3000);
            TimerNumerics_Respiratory.Set (5000);

            TimerAlarm.Start ();
            TimerTracing.Start ();
            TimerNumerics_Cardiac.Start ();
            TimerNumerics_Respiratory.Start ();
        }

        public virtual void OnClosed (object? sender, EventArgs e) {
            Dispose ();
        }

        public virtual void OnClosing (object? sender, CancelEventArgs e) {
            TimerAlarm?.Dispose ();
            DisposeAudio ();
        }

        public virtual void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
        }

        public virtual void OnTick_Alarm (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Tracing (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
        }
    }
}