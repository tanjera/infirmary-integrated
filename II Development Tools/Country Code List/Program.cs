using System.Globalization;

namespace Country_Code_List {

    internal class Program {

        private static void Main (string [] args) {
            List<string> codes = new List<string> (new string [] {
                "PL",
                "KE",
                "US",
                "DE",
                "CN",
                "BG",
                "GB",
                "CA",
                "IN",
                "BR",
                "AU",
                "PH",
                "HR",
                "PK",
                "MY",
                "JP",
                "TR",
                "CL",
                "GR",
                "MX",
                "IL",
                "PT",
                "IT",
                "NO",
                "IR",
                "TW",
                "FR",
                "NA",
                "ZA",
                "HK",
                "SG",
                "BE",
                "KR",
                "ES",
                "MA",
                "ID",
                "AR",
                "NP",
                "AT",
                "UA",
                "SK",
                "RU",
                "CY",
                "LT",
                "IE",
                "NL",
                "SE",
                "PE",
                "VE",
                "RO",
                "SI",
                "KZ",
                "LY",
                "LV" }
                );

            foreach (string code in codes) {
                RegionInfo ri = new RegionInfo (code);
                Console.WriteLine (ri.EnglishName);
            }

            Console.WriteLine ("\n\nPress enter key to proceed.");

            var key = Console.ReadKey ();
            while (key.Key != ConsoleKey.Enter)
                key = Console.ReadKey ();
        }
    }
}