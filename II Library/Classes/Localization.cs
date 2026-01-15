/* Localization.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.Globalization;

namespace II.Localization {
    public partial class Language {
        public Languages Selection;

        public Language (string s) {
            object? lang = new ();
            if (Enum.TryParse (typeof (Languages), s, out lang))
                Selection = lang is not null ? (Languages)lang : Languages.ENG;
            else
                Selection = Languages.ENG;
        }

        public Language (Languages v) {
            Selection = v;
        }

        public Language () {
            if (Enum.TryParse<Languages> (CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName.ToUpper (), out Languages tryParse))
                Selection = tryParse;
            else
                Selection = Languages.ENG;
        }

        public enum Languages {
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

        public string Description { get { return Descriptions [(int)Selection]; } }

        public static List<string> MenuItem_Formats {
            get { return Descriptions; }
        }

        public static Languages Parse_MenuItem (string inc) {
            try {
                int i = Descriptions.FindIndex (o => { return o == inc; });
                if (i >= 0)
                    return (Languages)Enum.GetValues (typeof (Languages)).GetValue (i);
                else
                    return Languages.ENG;
            } catch {
                return Languages.ENG;
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

        public Dictionary<string, string> Dictionary { get => GetDictionary(Selection); }

        public static Dictionary<string, string> GetDictionary (Languages lang) {
            switch (lang) {
                default: return new Dictionary<string, string> ();
                case Languages.AMH: return AMH;
                case Languages.ARA: return ARA;
                case Languages.DEU: return DEU;
                case Languages.ENG: return ENG;
                case Languages.SPA: return SPA;
                case Languages.FAS: return FAS;
                case Languages.FRA: return FRA;
                case Languages.HEB: return HEB;
                case Languages.HIN: return HIN;
                case Languages.ITA: return ITA;
                case Languages.KOR: return KOR;
                case Languages.POR: return POR;
                case Languages.RUS: return RUS;
                case Languages.SWA: return SWA;
                case Languages.ZHO: return ZHO;
            }
        }
        
        public static string Localize (Languages language, string key) 
            => GetDictionary (language).GetValueOrDefault (key, "");
        
        
        public string Localize (string key) 
            => Dictionary.GetValueOrDefault(key, "");
    }
}