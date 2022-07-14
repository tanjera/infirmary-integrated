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

    public class DeviceWindow : Window {
        public App? Instance { get; set; }

        public States State;

        public Timer
            TimerTracing = new (),
            timerVitals = new (),
            TimerVitals_Cardiac = new (),
            TimerVitals_Respiratory = new (),
            TimerAncillary_Delay = new ();

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceWindow () {
        }

        public DeviceWindow (App? app) {
            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;
        }

        ~DeviceWindow () {
            Dispose ();
        }

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            if (Instance is not null) {
                Instance.Timer_Main.Elapsed -= TimerTracing.Process;
                Instance.Timer_Main.Elapsed -= timerVitals.Process;
                Instance.Timer_Main.Elapsed -= TimerVitals_Cardiac.Process;
                Instance.Timer_Main.Elapsed -= TimerVitals_Respiratory.Process;
                Instance.Timer_Main.Elapsed -= TimerAncillary_Delay.Process;
            }

            /* Dispose of local Timers */
            TimerTracing.Dispose ();
            timerVitals.Dispose ();
            TimerVitals_Cardiac.Dispose ();
            TimerVitals_Respiratory.Dispose ();
            TimerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (Instance?.Patient != null)
                Instance.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            if (Instance is null)
                return;

            Instance.Timer_Main.Elapsed += TimerTracing.Process;

            Instance.Timer_Main.Elapsed += timerVitals.Process;
            Instance.Timer_Main.Elapsed += TimerVitals_Cardiac.Process;
            Instance.Timer_Main.Elapsed += TimerVitals_Respiratory.Process;

            Instance.Timer_Main.Elapsed += TimerAncillary_Delay.Process;

            TimerTracing.Tick += OnTick_Tracing;
            timerVitals.Tick += OnTick_Vitals;
            TimerVitals_Cardiac.Tick += OnTick_Vitals_Cardiac;
            TimerVitals_Respiratory.Tick += OnTick_Vitals_Respiratory;

            TimerTracing.Set (Draw.RefreshTime);

            timerVitals.Set ((int)((Instance.Patient?.GetHR_Seconds ?? 1) * 1000));
            TimerVitals_Cardiac.Set (II.Math.Clamp ((int)(Instance.Patient.GetHR_Seconds * 1000 / 2), 2000, 6000));
            TimerVitals_Respiratory.Set (II.Math.Clamp ((int)(Instance.Patient.GetRR_Seconds * 1000 / 2), 2000, 8000));

            TimerTracing.Start ();
            timerVitals.Start ();
            TimerVitals_Cardiac.Start ();
            TimerVitals_Respiratory.Start ();
        }

        public virtual void TogglePause () {
            if (State == States.Running)
                State = States.Paused;
            else if (State == States.Paused)
                State = States.Running;
        }

        public void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;

            Dispose ();
        }

        public void OnClosing (object? sender, CancelEventArgs e) {
        }

        public virtual void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
        }

        public virtual void OnTick_Tracing (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
        }
    }
}