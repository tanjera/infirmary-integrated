using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace IISIM.Classes {

    internal class Media {

        public static string CreateFile_Audio (MemoryStream audio) {
            string tempFilePath = Path.GetTempFileName () + ".wav";
            using (FileStream fs = File.Create (tempFilePath)) {
                audio.CopyTo (fs);
            }

            return tempFilePath;
        }
    }
}