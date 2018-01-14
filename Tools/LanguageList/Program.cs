using System;
using System.Globalization;

namespace LanguageList {
    class Program {
        static void Main (string [] args) {
            Console.WriteLine ("WIN                 ENGLISHNAME");
            foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.NeutralCultures)) {
                Console.Write (" {0,-3}", ci.ThreeLetterWindowsLanguageName);
                Console.WriteLine (" {0,-40}", ci.EnglishName);
            }

            Console.ReadKey ();
        }
    }
}

