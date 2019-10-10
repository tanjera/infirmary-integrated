using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Language_List {
    class Program {
        [STAThread]
        static void Main (string [] args) {
            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("WIN                 ENGLISHNAME");
            foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.NeutralCultures)) {
                sb.Append (String.Format(" {0,-3}", ci.ThreeLetterWindowsLanguageName));
                sb.AppendLine (String.Format(" {0,-40}", ci.EnglishName));
            }

            Clipboard.SetText (sb.ToString ());
            Console.WriteLine ("Language list copied to clipboard.");
            Console.WriteLine ("Press any key to exit.");
            Console.ReadKey ();
        }
    }
}

