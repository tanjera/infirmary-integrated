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
        public States State;

        public Timer
            timerTracing = new (),
            timerVitals = new (),
            timerVitals_Cardiac = new (),
            timerVitals_Respiratory = new (),
            timerAncillary_Delay = new ();

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceWindow () {
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
            App.Timer_Main.Elapsed -= timerTracing.Process;
            App.Timer_Main.Elapsed -= timerVitals.Process;
            App.Timer_Main.Elapsed -= timerVitals_Cardiac.Process;
            App.Timer_Main.Elapsed -= timerVitals_Respiratory.Process;
            App.Timer_Main.Elapsed -= timerAncillary_Delay.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();
            timerVitals.Dispose ();
            timerVitals_Cardiac.Dispose ();
            timerVitals_Respiratory.Dispose ();
            timerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (App.Patient != null)
                App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            App.Timer_Main.Elapsed += timerTracing.Process;

            App.Timer_Main.Elapsed += timerVitals.Process;
            App.Timer_Main.Elapsed += timerVitals_Cardiac.Process;
            App.Timer_Main.Elapsed += timerVitals_Respiratory.Process;

            App.Timer_Main.Elapsed += timerAncillary_Delay.Process;

            timerTracing.Tick += OnTick_Tracing;
            timerVitals.Tick += OnTick_Vitals;
            timerVitals_Cardiac.Tick += OnTick_Vitals_Cardiac;
            timerVitals_Respiratory.Tick += OnTick_Vitals_Respiratory;

            timerTracing.Set (Draw.RefreshTime);

            timerVitals.Set ((int)((App.Patient?.GetHR_Seconds ?? 1) * 1000));
            timerVitals_Cardiac.Set (II.Math.Clamp ((int)(App.Patient.GetHR_Seconds * 1000 / 2), 2000, 6000));
            timerVitals_Respiratory.Set (II.Math.Clamp ((int)(App.Patient.GetRR_Seconds * 1000 / 2), 2000, 8000));

            timerTracing.Start ();
            timerVitals.Start ();
            timerVitals_Cardiac.Start ();
            timerVitals_Respiratory.Start ();
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