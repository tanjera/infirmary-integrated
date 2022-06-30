using System;
using System.Collections.Generic;
using System.Globalization;

namespace II.Localization {
    public partial class Language {
        public Values Value;

        public Language (string s) {
            object? lang = new();
            if (Enum.TryParse (typeof (Values), s, out lang))
                Value = lang is not null ? (Values)lang : Values.ENG;
            else
                Value = Values.ENG;
        }

        public Language (Values v) {
            Value = v;
        }

        public Language () {
            if (Enum.TryParse<Values> (CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName.ToUpper (), out Values tryParse))
                Value = tryParse;
            else
                Value = Values.ENG;
        }

        public enum Values {
            AMH,    // Amharic
            ARA,    // Arabic
            DEU,    // German
            ENG,    // English
            SPA,    // Spanish
            FAS,    // Farsi
            FRA,    // French
            HEB,    // Hebrew
            HIN,    // Hindi
            ITA,    // Italian
            KOR,    // Korean
            POR,    // Portuguese
            RUS,    // Russian
            SWA,    // Swahili
            ZHO     // Chinese
        }

        public string Description { get { return Descriptions [(int)Value]; } }

        public static List<string> MenuItem_Formats {
            get { return Descriptions; }
        }

        public static Values Parse_MenuItem (string inc) {
            try {
                int i = Descriptions.FindIndex (o => { return o == inc; });
                if (i >= 0)
                    return (Values)Enum.GetValues (typeof (Values)).GetValue (i);
                else
                    return Values.ENG;
            } catch {
                return Values.ENG;
            }
        }

        public static List<string> Descriptions = new () {
            "አማርኛ (Amharic)",
            "عربى (Arabic)",
            "Deutsche (German)",
            "English",
            "Español (Spanish)",
            "فارسی (Farsi)",
            "Français (French)",
            "עברית (Hebrew)",
            "हिंदी (Hindi)",
            "Italiano (Italian)",
            "한국어 (Korean)",
            "Português (Portuguese)",
            "русский (Russian)",
            "Swahili (Kiswahili)",
            "中文 (Chinese)"
        };

        public Dictionary<string, string> Dictionary {
            get {
                switch (Value) {
                    default: return new Dictionary<string, string> ();
                    case Values.AMH: return AMH;
                    case Values.ARA: return ARA;
                    case Values.DEU: return DEU;
                    case Values.ENG: return ENG;
                    case Values.SPA: return SPA;
                    case Values.FAS: return FAS;
                    case Values.FRA: return FRA;
                    case Values.HEB: return HEB;
                    case Values.HIN: return HIN;
                    case Values.ITA: return ITA;
                    case Values.KOR: return KOR;
                    case Values.POR: return POR;
                    case Values.RUS: return RUS;
                    case Values.SWA: return SWK;
                    case Values.ZHO: return ZHO;
                }
            }
        }

        public string Localize (string key) {
            return Dictionary.ContainsKey (key) ? Dictionary [key] : "";
        }
    }
}