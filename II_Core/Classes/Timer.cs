using System;

namespace II {
    public class Timer {
        public int Interval = 0;
        public bool Locked = false;

        DateTime Last;
        bool Running = false;

        public bool IsRunning { get { return Running; } }

        public event EventHandler<EventArgs> Tick;

        public void Start () {
            Running = true;
        }

        public void Continue (int interval) {
            Interval = interval;
            Running = true;
        }

        public void Stop () {
            Running = false;
        }

        public void Set (int interval) {
            Interval = interval;
            Last = DateTime.Now;
        }

        public void Reset ()
            => Last = DateTime.Now;

        public void Reset (int interval)
            => Set (interval);

        public void ResetAuto () {
            Reset ();
            Start ();
        }

        public void ResetAuto (int interval) {
            Reset (interval);
            Start ();
        }

        public void Process () {
            if (!Running)
                return;

            if ((DateTime.Now - Last).TotalSeconds * 1000 > Interval) {
                Last = DateTime.Now;
                Tick?.Invoke (this, new EventArgs ());
            }
        }

        public void Process (object sender, EventArgs e)
            => Process ();
    }
}