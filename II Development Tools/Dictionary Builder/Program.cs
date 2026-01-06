using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;

namespace Dictionary_Builder;

class Program
{
    static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (args.Length < 1)
        {
            Console.WriteLine("Error: Not enough arguments");
            Console.WriteLine("Expected usage: dictbuild ii_library_dir");
            return;
        }

        string FilepathIn = Path.Combine(args[0], "Localization Strings.xlsx");
        string FilepathOut = Path.Combine(args[0], "Classes", "Localization.Dictionary.cs");

        if (!Directory.Exists(args[0]))
        {
            Console.WriteLine($"Error: directory not found: {args[0]}");
            return;
        }

        if (!Directory.Exists(Path.Combine(args[0], "Classes")))
        {
            Console.WriteLine($"Error: directory not found: {Path.Combine(args[0], "Classes")}");
            return;
        }

        if (!File.Exists(FilepathIn))
        {
            Console.WriteLine($"Error: file not found: {FilepathIn}");
            return;
        }

        if (!File.Exists(FilepathOut))
        {
            Console.WriteLine($"Error: file not found: {FilepathOut}");
            return;
        }


        List<string> Languages = new List<string>();
        List<Dictionary<string, string>> Dictionaries = new List<Dictionary<string, string>>();

        /* Read the .xlsx into a Language and Dictionary lists */
        using (var stream = File.Open(FilepathIn, FileMode.Open, FileAccess.Read)) {
            using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                int colCount = reader.FieldCount;
                int rowCurrent = 0;

                while (reader.Read())
                {
                    if (rowCurrent == 0)
                    {
                        // $B1 -> $...1: language codes
                        for (int j = 1; j < colCount; j++)
                        {
                            Languages.Add(reader.GetString(j).ToUpper().Substring(0, 3));
                            Dictionaries.Add(new Dictionary<string, string>());
                        }
                    }
                    else if (rowCurrent > 0)
                    {
                        string key = reader.GetString(0);
                        if (String.IsNullOrEmpty(key))
                            continue;

                        Console.WriteLine($"Processing row {rowCurrent:000}: {key}");

                        for (int i = 1; i < colCount; i++)
                        {
                            Dictionaries[i - 1].Add(key, reader.GetString(i));
                        }
                    }

                    rowCurrent += 1;
                }
            }
        }






        /* Compile dictionaries into C# localization code */
        
        StringBuilder dictOut = new StringBuilder ();
        dictOut.Append (
            "using System;\n"
            + "using System.Collections.Generic;\n"
            + "using System.Globalization;\n"
            + "using System.Text;\n\n"
            + "namespace II.Localization {\n\n"
            + "\tpublic partial class Language {\n\n");
        
        for (int i = 0; i < Languages.Count; i++) {
            dictOut.AppendLine (String.Format ("\t\tstatic Dictionary<string, string> {0} = new Dictionary<string, string> () {{", Languages [i]));

            foreach (KeyValuePair<string, string> pair in Dictionaries [i])
                dictOut.AppendLine (String.Format ("\t\t\t{{{0,-60} {1}}},",
                    String.Format ("\"{0}\",", pair.Key),
                    String.Format ("\"{0}\"", pair.Value)));

            dictOut.AppendLine ("\t\t};\n");
        }

        dictOut.AppendLine ("\t}\n}");
        
        StreamWriter outFile = new StreamWriter (FilepathOut, false);
        
        outFile.Write (dictOut.ToString ());
        outFile.Close ();
        
        Console.Write (Environment.NewLine);
        Console.WriteLine($"Output written to {FilepathOut}");
        Console.WriteLine($"You may now close this program.");

    }
}