using System;
using System.Collections.Generic;
using System.Numerics;

namespace Infirmary_Integrated.Rhythms {

    public class Strip {

        static float Timing_Resolution = 0.01f;
        static float Drawing_Length = 5.0f;

        // Three arrays for the strip, draw functions use _Current
        // and Scroll() keeps them flowing
        List<Vector2> Strip_Future,
                        Strip_Current,
                        Strip_History;

        public Strip () {
            Strip_Future = new List<Vector2> ();
            Strip_Current = new List<Vector2> ();
            Strip_History = new List<Vector2> ();
        }

        public void Reset () {
            Strip_Future.Clear ();
            Strip_Current.Clear ();
            Strip_History.Clear ();
        }

        public void Stop () {
            Strip_Future.Clear ();
        }

        Vector2 Last (List<Vector2> _In) {
            if (_In.Count < 1)
                return new Vector2 (_.Time, 0);
            else
                return _In[_In.Count - 1];
        }

        public void Concatenate (List<Vector2> _Addition) {
            if (_Addition.Count == 0)
                return;
            else
                _Addition = Timed_Waveform (_Addition);

            float _Offset = 0f;
            if (Strip_Future.Count == 0)
                _Offset = _.Time;
            else if (Strip_Future.Count > 0)
                _Offset = Last (Strip_Future).X;

            foreach (Vector2 eachVector in _Addition)
                Strip_Future.Add (new Vector2 (eachVector.X + _Offset, eachVector.Y));
        }

        List<Vector2> Timed_Waveform (List<Vector2> _Buffer) {
            List<Vector2> _Out = new List<Vector2> ();
            float _Length = Last (_Buffer).X;

            if (_Buffer.Count == 0)
                return new List<Vector2> ();

            _Out.Add (_Buffer[0]);
            float i = _Buffer[0].X + Timing_Resolution;
            int n = 0;

            while (i < _Length) {
                if ((_Buffer[n].X <= i) && (_Buffer[n + 1].X >= i)) {
                    _Out.Add (Vector2.Lerp (_Buffer[n], _Buffer[n + 1],
                        _.InverseLerp (_Buffer[n].X, _Buffer[n + 1].X, i)));
                    i += Timing_Resolution;
                } else if (i < _Buffer[n].X) {
                    i += Timing_Resolution;
                } else if (i > _Buffer[n].X) {
                    if (n < _Buffer.Count - 1)
                        n++;
                    else
                        break;
                }
            }

            return _Out;
        }

        public void Scroll () {
            // Knock the future strip into the current strip and/or clean
            // up leftovers, but don't overshoot into the future
            if (Strip_Future.Count > 0) {
                for (int i = 0; i < Strip_Future.Count; i++) {
                    if (Strip_Future[i].X > _.Time) {
                        continue;
                    } else if (Strip_Future[i].X < _.Time - Drawing_Length) {
                        Strip_History.Add (Strip_Future[i]);
                        Strip_Future.RemoveAt (i);
                        i--;
                    } else if (Strip_Future[i].X >= _.Time - Drawing_Length
                            && Strip_Future[i].X <= _.Time) {
                        Strip_Current.Add (Strip_Future[i]);
                        Strip_Future.RemoveAt (i);
                        i--;
                    }
                }
            }

            if (Strip_Current.Count > 0) {
                for (int i = 0; i < Strip_Current.Count; i++) {
                    if (Strip_Current[i].X < _.Time - Drawing_Length) {
                        Strip_History.Add (Strip_Current[i]);
                        Strip_Current.RemoveAt (i);
                        i--;
                    }
                }
            }
        }
    }
}
