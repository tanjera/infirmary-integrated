using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using II;
using II.Rhythm;
using II.Waveform;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceIABP : Window {
        public App? Instance { get; set; }

        public States State;

        public II.Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables controlling for audio alarms */
        public SoundPlayer? AudioPlayer;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceIABP () {
            InitializeComponent ();
        }

        public DeviceIABP (App? app) {
            InitializeComponent ();

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;
        }

        ~DeviceIABP () {
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

        public virtual void DisposeAudio () {
            AudioPlayer?.Dispose ();
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

        public virtual void TogglePause () {
            if (State == States.Running)
                State = States.Paused;
            else if (State == States.Paused)
                State = States.Running;
        }

        public Task Load (string inc) {
            // TODO: Implement
            return Task.CompletedTask;
        }

        public string Save () {
            // TODO: Implement
            return "";
        }

        public virtual void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;

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