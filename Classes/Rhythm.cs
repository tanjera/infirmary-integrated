using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Infirmary_Integrated.Rhythms {

    public static class Rhythm {
        public static List<Vector2> SpO2_Rhythm__Absent (float _Length) {
            /* SpO2 waveform non-existant
		     */

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat.Add (new Vector2 (0, 0));
            thisBeat = Concatenate (thisBeat, Line (_Length, 0.0f, new Vector2 (0, 0)));
            return thisBeat;
        }
        public static List<Vector2> SpO2_Rhythm__Normal_Sinus(float _Rate) {
            /* SpO2 during normal sinus perfusion is similar to a sine wave leaning right
		     */
            float _Length = 60 / _Rate;

            List<Vector2> thisBeat = new List<Vector2>();
            thisBeat = Concatenate(thisBeat, Curve(_Length / 2, 0.5f, 0.0f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, Line(_Length / 2, 0.0f, Last(thisBeat)));
            return thisBeat;
        }        
        public static List<Vector2> EKG_Rhythm__Asystole (float _Length, float _Isoelectric) {
            /* Asystole is the absence of electrical activity.
		     */

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat.Add (new Vector2 (0, _Isoelectric));
            thisBeat = Concatenate (thisBeat, Line (_Length, _Isoelectric, new Vector2 (0, _Isoelectric)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Atrial_Fibrillation (float _Rate, float _Isoelectric, float _Prematurity, float _Variance) {
            /* Atrial fibrillation is marked by fibrillation in place of any P wave,
             * irregular pattern of ventricular firing.
             */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff);

            float _Wave = 0f,
                    // Atrial fibrillations are typically +/- 0.1mV around isoelectric.
                    _Amplitude = _.RandomFloat(-0.1f, 0.1f);
            /* Vary TQ interval since AFIB intermittently leads to shortened RR intervals
             * To maintain the stated rate (that is printed on the cardiac monitor...) the
             * model needs to account for % of time rate is increased with premature firing,
             * and when rate is *not* increased, we need to lengthen the RR interval to compensate.
             */
            float TQ = (60 / (_.RandomFloat(0.0f, 1.0f) <= _Prematurity
                        ? _Rate + _.RandomFloat(0, _Variance)
                        : _Rate - ((1 - _Prematurity) * (_Variance / 2))))
                    - QT;

            List<Vector2> thisBeat = new List<Vector2> ();

            while (TQ > 0f) {
                // Fibrillate!
                // Each atrial fibrillation lasts from 0.04 to 0.08 seconds
                _Wave = _.RandomFloat(0.04f, 0.08f);

                thisBeat = Concatenate (thisBeat, Curve (_Wave, _Isoelectric + _Amplitude, _Isoelectric, Last (thisBeat)));
                // Flip the amplitude's sign *without* crawling
                _Amplitude = (_Amplitude <= 0 ? 1 : -1) * _.RandomFloat(0.02f, 0.10f);
                TQ -= _Wave;
            }

            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Atrial_Flutter(float _Rate, float _Isoelectric) {
            return EKG_Rhythm__Atrial_Flutter (_Rate, 4, _Isoelectric);
        }
        public static List<Vector2> EKG_Rhythm__Atrial_Flutter(float _Rate, int _Flutters, float _Isoelectric) {
            /* Atrial flutter is normal sinus rhythm with repeated P waves throughout
		     * TP interval. Clamped from 1-160.
		     */

            _Rate = _.Clamp(_Rate, 1, 160);
            _Flutters = _.Clamp(_Flutters, 2, 5);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(160, 60, _Rate)),
                    QRS = _.Lerp(0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp(0.10f, 0.16f, lerpCoeff),
                    TP = ((60 / _Rate) - QT);

            List<Vector2> thisBeat = new List<Vector2>();
            thisBeat = Concatenate(thisBeat, L2_P(TP / _Flutters, _Isoelectric + .1f, _Isoelectric, new Vector2(0, _Isoelectric)));
            for (int i = 1; i < _Flutters; i++)
                thisBeat = Concatenate(thisBeat, L2_P(TP / _Flutters, _Isoelectric + .1f, _Isoelectric, Last(thisBeat)));

            thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_ST_Segment(QT - QRS, _Isoelectric, Last(thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__AV_Block__1st_Degree (float _Rate, float _Isoelectric) {
            /* 1st degree AV block consists of normal sinus rhythm with a PR interval > .20 seconds
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.26f, 0.36f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, L2_P (PR / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, L2_PR_Segment ((PR * 2) / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__AV_Block__3rd_Degree (float _Rate, float _Isoelectric) {
            /* 3rd degree AV block consists of a regular P wave and a regular QRS complex, but they
             * both proceed at their own rates- a ratio of 3:2 atrial : ventricular beats.
             */

            _Rate = _.Clamp (_Rate, 1, 130);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP / 2, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP / 2, _Isoelectric, Last (thisBeat)));

            return thisBeat;
        }        
        public static List<Vector2> EKG_Rhythm__AV_Block__Mobitz_II (float _Rate, float _Isoelectric, float _Occurrance) {
            /* AV Block 2nd degree Mobitz Type II is a normal sinus rhythm with occasional
		     * dropped QRS complexes.
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                        PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                        QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));

            if (_.RandomFloat (0.0f, 1.0f) <= _Occurrance)
                thisBeat = Concatenate (thisBeat, Line ((PR / 3) + QT + TP, _Isoelectric, Last (thisBeat)));
            else {
                thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR / 3, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
            }

            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__AV_Block__Wenckebach (float _Rate, float _Isoelectric, int _Drops_On = 4) {
            /* AV Block 2nd degree Wenckebach is a normal sinus rhythm marked by lengthening
		     * PQ intervals for 2-4 beats then a dropped QRS complex.
		     * 
		     * Renders amount of beats _Drops_On, PQ interval lengthens and QRS drops on _Drops_On
		     */

            _Rate = _.Clamp (_Rate, 40, 120);
            _Drops_On = _.Clamp (_Drops_On, 2, 4);

            for (int currBeat = 1; currBeat <= _Drops_On; currBeat++) {
                // Determine speed of some waves and segments based on rate using lerp
                float lerpCoeff = _.Clamp (_.InverseLerp (120, 40, _Rate)),
                        PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                        // PR segment varies due to Wenckebach
                        PR_Segment = (PR / 3) * currBeat,
                        QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT + PR_Segment));

                List<Vector2> thisBeat = new List<Vector2> ();
                thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));
                thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR_Segment, _Isoelectric, Last (thisBeat)));

                if (currBeat != _Drops_On) {
                    // Render QRS complex on the beats it's not dropped...
                    thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
                    thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
                } else {
                    // If QRS is dropped, add its time to the TP segment to keep the rhythm at a normal rate
                    TP += QRS;
                    TP += QT - QRS;
                }

                thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
                return thisBeat;
            }

            return new List<Vector2> ();
        }
        public static List<Vector2> EKG_Rhythm__Block__Bundle_Branch (float _Rate, float _Isoelectric) {
            /* Bundle branch blocks are marked by QRS complexes that are widened or split
             * as one ventricle fires slightly after the other.
             */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.12f, 0.22f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 6, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 6, _Isoelectric + 0.9f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 6, _Isoelectric + 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 6, _Isoelectric + 0.7f, Last (thisBeat)));

            thisBeat = Concatenate (thisBeat, L2_S (QRS / 6, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 6, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Idioventricular (float _Rate, float _Isoelectric) {
            /* Idioventricular rhythms originate in the ventricules (fascicles, bundle branches,
		     * or Bundle of His) and can have different and erratic shapes varying by origin.
		     * Marked by absent P waves, wide and distorted QRS complexes. Regular idioventricular
		     * rhythms run from 15-45 bpm, accelerated idioventricular rhythms are 45-100 bpm.
		     */

            _Rate = _.Clamp (_Rate, 1, 100);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (75, 25, _Rate)),
                    QRS = _.Lerp (0.3f, 0.4f, lerpCoeff),
                    SQ = ((60 / _Rate) - QRS);

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2, _Isoelectric + 1.0f, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (QRS / 2, _Isoelectric - 0.3f, _Isoelectric - 0.4f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Curve (SQ / 3, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, Line ((SQ * 2) / 3, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Junctional(float _Rate, float _Isoelectric) {
            return EKG_Rhythm__Junctional (_Rate, _Isoelectric, false);
        }
        public static List<Vector2> EKG_Rhythm__Junctional(float _Rate, float _Isoelectric, bool _P_Inverted) {
            /* Junctional rhythm is normal sinus with either an absent or inverted P wave,
		     * regularly between 40-60 bpm, with accelerated junctional tachycardia from
		     * 60-115 bpm. Function clamped at 1-130.
		     */

            _Rate = _.Clamp(_Rate, 1, 130);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(130, 60, _Rate)),
                        PR = _.Lerp(0.16f, 0.2f, lerpCoeff),
                        QRS = _.Lerp(0.08f, 0.12f, lerpCoeff),
                        QT = _.Lerp(0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2>();
            thisBeat = Concatenate(thisBeat, L2_P((PR * 2) / 3,
                (_P_Inverted ? _Isoelectric - .05f : _Isoelectric),
                _Isoelectric, new Vector2(0, _Isoelectric)));
            thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR / 3, _Isoelectric, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
            thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Normal_Sinus (float _Rate, float _Isoelectric) {
            /* Normal sinus rhythm (NSR) includes bradycardia (1-60), normocardia (60-100), 
		     * and sinus tachycardia (100 - 160)
		     */

            _Rate = _.Clamp (_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                    PR = _.Lerp (0.16f, 0.2f, lerpCoeff),
                    QRS = _.Lerp (0.08f, 0.12f, lerpCoeff),
                    QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                    TP = ((60 / _Rate) - (PR + QT));

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat = Concatenate (thisBeat, L2_P ((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2 (0, _Isoelectric)));
            thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR / 3, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Premature_Atrial_Contractions(float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Quick overload for single beat, does not prevent 2 consecutive PACs or lengthen following beat... */
            return EKG_Rhythm__Premature_Atrial_Contractions(_Rate, 1, _Isoelectric, _Occurrance, _Variance);
        }
        public static List<Vector2> EKG_Rhythm__Premature_Atrial_Contractions(float _Rate, int _Beats, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature atrial contractions (PAC) are normal sinus rhythm with occasionally shortening 
		     * TP segments, so will just run normal sinus with a random range of heart rate.
		     * Occurrance is percentage chance that a PAC will occur rather than an NSR.
		     */

            bool wasPAC = false;

            List<Vector2> theseBeats = new List<Vector2>();
            theseBeats.Add(new Vector2(0, _Isoelectric));
            for (int i = 0; i < _Beats; i++) {
                // Prevent 2 PAC's from happening consecutively by checking wasPAC
                if ((_.RandomFloat(0.0f, 1.0f) <= _Occurrance) && !wasPAC) {
                    wasPAC = true;
                    return EKG_Rhythm__Normal_Sinus(_Rate + _Variance, _Isoelectric);
                } else {
                    // If there was a PAC last beat...
                    if (wasPAC) {
                        wasPAC = false;
                        return EKG_Rhythm__Normal_Sinus(_Rate - (_Variance / 4), _Isoelectric);
                    }
                    // If there was no PAC last beat and no occurrance this beat
                    else if (!wasPAC)
                        return EKG_Rhythm__Normal_Sinus(_Rate, _Isoelectric);
                }
            }

            return new List<Vector2>();
        }
        public static List<Vector2> EKG_Rhythm__Premature_Junctional_Contractions (float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature junctional contractions are the occurrance of premature regular QRS complexes
             * with the absence of a P wave- much like a PAC but started in the AV node.
             */

            if (_.RandomFloat(0.0f, 1.0f) <= _Occurrance) {
                return EKG_Rhythm__Normal_Sinus (_Rate + _Variance, _Isoelectric);
                return EKG_Rhythm__Junctional (_Rate - (_Variance / 4), _Isoelectric);
            } else
                return EKG_Rhythm__Normal_Sinus (_Rate, _Isoelectric);
        }
        public static List<Vector2> EKG_Rhythm__Premature_Ventricular_Contractions (float _Rate, float _Isoelectric, float _Occurrance, float _Variance) {
            /* Premature ventricular contractions are the occurrance of premature abnormal QRS complexes
             * with the absence of a P wave- much like a PJC but with an abnormally shaped QRS complex.
             */

            if ( _.RandomFloat(0.0f, 1.0f) <= _Occurrance) {
                EKG_Rhythm__Normal_Sinus (_Rate + _Variance, _Isoelectric);

                _Rate = _Rate - (_Variance / 4);
                // Determine speed of some waves and segments based on rate using lerp
                float lerpCoeff = _.Clamp (_.InverseLerp (160, 60, _Rate)),
                        QRS = _.Lerp (0.14f, 0.22f, lerpCoeff),
                        QT = _.Lerp (0.235f, 0.4f, lerpCoeff),
                        TP = ((60 / _Rate) - QT);

                List<Vector2> thisBeat = new List<Vector2> ();

                thisBeat = Concatenate (thisBeat, L2_ST_Segment (QT - QRS, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.8f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric - 0.6f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric + 0.4f, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric, Last (thisBeat)));
                thisBeat = Concatenate (thisBeat, L2_TP_Segment (TP, _Isoelectric, Last (thisBeat)));
                return thisBeat;

            } else
                return EKG_Rhythm__Normal_Sinus (_Rate, _Isoelectric);
        }
        public static List<Vector2> EKG_Rhythm__Supraventricular_Tachycardia (float _Rate, float _Isoelectric) {
            /* Supraventricular tachycardia (SVT) includes heart rates between 160
		     * to 240 beats per minute. Essentially it is NSR without a PR interval
		     * or a P wave (P is mixed with T).
		     */

            _Rate = _.Clamp (_Rate, 160, 240);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp (_.InverseLerp (240, 160, _Rate)),
                        PR = _.Lerp (0.03f, 0.05f, lerpCoeff),
                        QRS = _.Lerp (0.05f, 0.08f, lerpCoeff),
                        QT = _.Lerp (0.17f, 0.235f, lerpCoeff);

            List<Vector2> thisBeat = new List<Vector2> ();
            thisBeat.Add (new Vector2 (0, _Isoelectric));
            thisBeat = Concatenate (thisBeat, L2_PR_Segment (PR, _Isoelectric, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_Q (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_R (QRS / 4, _Isoelectric + 0.9f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_S (QRS / 4, _Isoelectric - 0.3f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_J (QRS / 4, _Isoelectric - 0.1f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_ST_Segment (((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last (thisBeat)));
            thisBeat = Concatenate (thisBeat, L2_T (((QT - QRS) * 3) / 5, _Isoelectric + 0.15f, _Isoelectric, Last (thisBeat)));
            return thisBeat;
        }        
        public static List<Vector2> EKG_Rhythm__Ventricular_Fibrillation(float _Length, float _Isoelectric) {
            /* Ventricular fibrillation is random peaks/curves with no recognizable waves, no regularity.
		     */

            float _Wave = 0f,
                    _Amplitude = _.RandomFloat(-0.6f, 0.6f);
            List<Vector2> thisBeat = new List<Vector2>();

            while (_Length > 0f) {
                _Wave = _.RandomFloat(0.1f, 0.2f);

                thisBeat = Concatenate(thisBeat, Curve(_Wave, _Isoelectric + _Amplitude, _Isoelectric, Last(thisBeat)));
                // Flip the sign of amplitude and randomly crawl larger/smaller, models the
                // flippant waves in v-fib.
                _Amplitude = 0 - _.Clamp(_.RandomFloat(_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
                _Length -= _Wave;
            }

            return thisBeat;
        }
        public static List<Vector2> EKG_Rhythm__Ventricular_Standstill(float _Rate, float _Isoelectric) {
            /* Ventricular standstill is the absence of ventricular activity- only P waves exist
		     */

            _Rate = _.Clamp(_Rate, 1, 160);
            // Determine speed of some waves and segments based on rate using lerp
            float lerpCoeff = _.Clamp(_.InverseLerp(160f, 60f, _Rate)),
                        P = _.Lerp(0.10f, 0.14f, lerpCoeff),
                        TP = ((60 / _Rate) - P);

            List<Vector2> thisBeat = new List<Vector2>();
            thisBeat = Concatenate(thisBeat, L2_P(P, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
            thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
            return thisBeat;
        }

        
        
        /* 
	     * Shaping and point plotting functions 
	     */

        static Vector2 Last (List<Vector2> _Original) {
            if (_Original.Count < 1)
                return new Vector2(0, 0);
            else
                return _Original[_Original.Count - 1];
        }
        static List<Vector2> Concatenate(List<Vector2> _Original, List<Vector2> _Addition) {
            // Offsets the X value of a Vector2[] so that it can be placed at the end
            // of an existing Vector2[] and continue from that point on. 

            // Nothing to add? Return something.
            if (_Original.Count == 0 && _Addition.Count == 0)
                return new List<Vector2>();
            else if (_Addition.Count == 0)
                return _Original;

            float _Offset = 0f;
            if (_Original.Count == 0)
                _Offset = 0;
            else if (_Original.Count > 0)
                _Offset = _Original[_Original.Count - 1].X;

            foreach (Vector2 eachVector in _Addition)
                _Original.Add(new Vector2(eachVector.X + _Offset, eachVector.Y));

            return _Original;
        }

        static float Slope (Vector2 _P1, Vector2 _P2) {
            return ((_P2.Y - _P1.Y) / (_P2.X - _P1.X));
        }
        static Vector2 Bezier (Vector2 _Start, Vector2 _Control, Vector2 _End, float _Percent) {
            return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * _End);
        }

        static List<Vector2> Curve(float _Length, float _mV, float _mV_End, Vector2 _Start) {
            int i;
            float x;
            List<Vector2> _Out = new List<Vector2>();

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Vector2(0, _Start.Y), new Vector2(_Length / 4, _mV), new Vector2(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Vector2(_Length / 2, _mV), new Vector2(_Length / 4 * 3, _mV), new Vector2(_Length, _mV_End), x));
            }

            _Out.Add(new Vector2(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Vector2> Peak(float _Length, float _mV, float _mV_End, Vector2 _Start) {
            int i;
            float x;
            List<Vector2> _Out = new List<Vector2>();

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Vector2(0, _Start.Y), new Vector2(_Length / 3, _mV / 1), new Vector2(_Length / 2, _mV), x));
            }

            for (i = 1; i * ((2 * _.Draw_Resolve) / _Length) <= 1; i++) {
                x = i * ((2 * _.Draw_Resolve) / _Length);
                _Out.Add(Bezier(new Vector2(_Length / 2, _mV), new Vector2(_Length / 5 * 3, _mV / 1), new Vector2(_Length, _mV_End), x));
            }

            _Out.Add(new Vector2(_Length, _mV_End));        // Finish the curve

            return _Out;
        }
        static List<Vector2> Line(float _Length, float _mV, Vector2 _Start) {
            List<Vector2> _Out = new List<Vector2>();
            _Out.Add(new Vector2(_Length, _mV));

            return _Out;
        }


        /* 
	     * Individual cardiac wave functions-
	     * Note: The most simplified overload is for normal sinus rhythm @ 60 bpm,
	     * 		but modifications can be made with the more complex overloads to simulate
	     *		dysrhythmias.
	     */

        static List<Vector2> L2_P(Vector2 _Start) { return L2_P(.08f, .1f, 0f, _Start); }
        static List<Vector2> L2_P(float _Length, float _mV, float _mV_End, Vector2 _Start) { return Peak(_Length, _mV, _mV_End, _Start); }
        static List<Vector2> L2_Q(Vector2 _Start) { return L2_Q(1f, -.1f, _Start); }
        static List<Vector2> L2_Q(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_R(Vector2 _Start) { return L2_R(1f, .9f, _Start); }
        static List<Vector2> L2_R(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_S(Vector2 _Start) { return L2_S(1f, -.3f, _Start); }
        static List<Vector2> L2_S(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_J(Vector2 _Start) { return L2_J(1f, -.1f, _Start); }
        static List<Vector2> L2_J(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_T(Vector2 _Start) { return L2_T(.16f, .25f, 0f, _Start); }
        static List<Vector2> L2_T(float _Length, float _mV, float _mV_End, Vector2 _Start) { return Peak(_Length, _mV, _mV_End, _Start); }
        static List<Vector2> L2_PR_Segment(Vector2 _Start) { return L2_PR_Segment(.08f, 0f, _Start); }
        static List<Vector2> L2_PR_Segment(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_ST_Segment(Vector2 _Start) { return L2_ST_Segment(.1f, 0f, _Start); }
        static List<Vector2> L2_ST_Segment(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }
        static List<Vector2> L2_TP_Segment(Vector2 _Start) { return L2_TP_Segment(.48f, .0f, _Start); }
        static List<Vector2> L2_TP_Segment(float _Length, float _mV, Vector2 _Start) { return Line(_Length, _mV, _Start); }

    }
}