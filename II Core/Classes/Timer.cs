using System;

namespace II {
    public class Timer {
        private int _Interval = 0;
        private bool _Locked = false;

        DateTime Last;
        bool Running = false;

        public bool IsRunning { get { return Running; } }

        public event EventHandler<EventArgs> Tick;

        ~Timer () => Dispose ();

        public void Dispose () {
            if (Tick == null)
                return;

            foreach (Delegate d in Tick?.GetInvocationList ())
                Tick -= (EventHandler<EventArgs>)d;
        }

        public bool IsLocked { get => _Locked; }
        public int Interval { get => _Interval; }
        public int Elapsed { get => (int)((DateTime.Now - Last).TotalSeconds * 1000); }

        public void Lock () => _Locked = true;
        public void Unlock () => _Locked = false;

        public void Start () {
            Running = true;
        }

        public void Continue (int interval) {
            _Interval = interval;
            Running = true;
        }

        public void Stop () {
            Running = false;
        }

        public void Set (int interval) {
            _Interval = interval;
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

            if ((DateTime.Now - Last).TotalSeconds * 1000 > _Interval) {
                Last = DateTime.Now;
                Tick?.Invoke (this, new EventArgs ());
            }
        }

        public void Process (object sender, EventArgs e)
            => Process ();
    }
}