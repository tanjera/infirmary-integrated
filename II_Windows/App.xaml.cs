using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static string [] Start_Args;

        public static II.Localization.Languages Language = new II.Localization.Languages();

        public static PatientEditor Patient_Editor;
        public static DeviceMonitor Device_Monitor;
        public static DialogAbout Dialog_About;
        public static DialogLanguage Dialog_Language;

        private void App_Startup (object sender, StartupEventArgs e) {
            Start_Args = e.Args;
        }
    }
}
