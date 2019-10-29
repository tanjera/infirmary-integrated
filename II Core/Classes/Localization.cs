using System;
using System.Collections.Generic;
using System.Globalization;

namespace II.Localization {
    public partial class Language {
        public Values Value;
        public Language (Values v) { Value = v; }
        public Language () {
            if (Enum.TryParse<Values> (CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName.ToUpper (), out Values tryParse))
                Value = tryParse;
            else
                Value = Values.ENU;
        }

        public enum Values {
            AMH,    // Amharic              AMH     amh
            ARA,    // Arabic               ARA     ar
            CHS,    // Chinese (Simp.)      CHS     zh-Hans
            DEU,    // German               DEU     de
            ENU,    // English              ENU     en
            ESP,    // Spanish              ESP     es
            FAR,    // Farsi                FAR     fa
            FRA,    // French               FRA     fr
            HEB,    // Hebrew               HEB     he
            HIN,    // Hindi                HIN     hi
            ITA,    // Italian              ITA     it
            KOR,    // Korean               KOR     ko
            PTB,    // Portuguese           PTB     pt
            RUS,    // Russian              RUS     ru
            SWK     // Swahili              SWK     sw
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
                    return Values.ENU;
            } catch {
                return Values.ENU;
            }
        }

        public static List<string> Descriptions = new List<string> {
            "አማርኛ (Amharic)",
            "عربى (Arabic)",
            "中文 (Chinese)",
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
            "Swahili (Kiswahili)"
        };

        public Dictionary<string, string> Dictionary {
            get {
                switch (Value) {
                    default: return new Dictionary<string, string> ();
                    case Values.AMH: return AMH;
                    case Values.ARA: return ARA;
                    case Values.CHS: return CHS;
                    case Values.DEU: return DEU;
                    case Values.ENU: return ENU;
                    case Values.ESP: return ESP;
                    case Values.FAR: return FAR;
                    case Values.FRA: return FRA;
                    case Values.HEB: return HEB;
                    case Values.HIN: return HIN;
                    case Values.ITA: return ITA;
                    case Values.KOR: return KOR;
                    case Values.PTB: return PTB;
                    case Values.RUS: return RUS;
                    case Values.SWK: return SWK;
                }
            }
        }

        public string Localize (string key) {
            try {
                return Dictionary [key];
            } catch {
                return "";
            }
        }
    }
}