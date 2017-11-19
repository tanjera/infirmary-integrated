using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Infirmary_Integrated.Rhythms {

    public class Strip {

        static float Drawing_Length = 5.0f;

        // Three arrays for the strip, draw functions use _Current
        // and Scroll() keeps them flowing
        public List<Vector2> Buffer, Current;

        public Strip () {
            Buffer = new List<Vector2> ();
            Current = new List<Vector2> ();
        }

        public void Reset () {
            Buffer.Clear ();
            Current.Clear ();
        }

        public void Stop () {
            Buffer.Clear ();
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
            if (Buffer.Count == 0)
                _Offset = _.Time;
            else if (Buffer.Count > 0)
                _Offset = Last (Buffer).X;

            foreach (Vector2 eachVector in _Addition)
                Buffer.Add (new Vector2 (eachVector.X + _Offset, eachVector.Y));
        }

        List<Vector2> Timed_Waveform (List<Vector2> _Buffer) {
            List<Vector2> _Out = new List<Vector2> ();
            float _Length = Last (_Buffer).X;

            if (_Buffer.Count == 0)
                return new List<Vector2> ();

            _Out.Add (_Buffer[0]);
            float i = _Buffer[0].X + _.Draw_Resolve;
            int n = 0;

            while (i < _Length) {
                if ((_Buffer[n].X <= i) && (_Buffer[n + 1].X >= i)) {
                    _Out.Add (Vector2.Lerp (_Buffer[n], _Buffer[n + 1],
                        _.InverseLerp (_Buffer[n].X, _Buffer[n + 1].X, i)));
                    i += _.Draw_Resolve;
                } else if (i < _Buffer[n].X) {
                    i += _.Draw_Resolve;
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
            if (Buffer.Count > 0) {
                for (int i = 0; i < Buffer.Count; i++) {
                    if (Buffer[i].X > _.Time) {
                        continue;
                    } else if (Buffer[i].X < _.Time - Drawing_Length) {
                        Buffer.RemoveAt (i);
                        i--;
                    } else if (Buffer[i].X >= _.Time - Drawing_Length
                            && Buffer[i].X <= _.Time) {
                        Current.Add (Buffer[i]);
                        Buffer.RemoveAt (i);
                        i--;
                    }
                }
            }

            if (Current.Count > 0) {
                for (int i = 0; i < Current.Count; i++) {
                    if (Current[i].X < _.Time - Drawing_Length) {
                        Current.RemoveAt (i);
                        i--;
                    }
                }
            }
        }

        
        public class Renderer {

            Panel panel;            
            Pen pen;
            Strip strip;

            int offX, offY;
            float multX, multY;


            public Renderer (Panel _Panel, ref Strip _Strip, Pen _Pen) {
                panel = _Panel;                
                pen = _Pen;
                strip = _Strip;
            }

            public void Draw (PaintEventArgs e) {
                e.Graphics.Clear (Color.Black);

                offX = 0;
                offY = panel.Height / 2;
                multX = panel.Width / (Drawing_Length / _.Draw_Resolve);
                multY = -panel.Height / 2;

                Point lastPoint = new Point (0, offY);
                for (int i = 0; i < strip.Current.Count; i++) {
                    Point thisPoint = new Point (
                        (int)(i * multX) + offX,
                        (int)(strip.Current[i].Y * multY) + offY);
                    e.Graphics.DrawLine (pen, lastPoint, thisPoint);
                    lastPoint = thisPoint;
                }
            }
        }
    }
}
