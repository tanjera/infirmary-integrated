using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;
using II.Server;

using Xceed.Wpf.Toolkit;

namespace II_Avalonia {

    public partial class PatientEditor : Window {

        public PatientEditor () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif
            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            DataContext = this;
            App.Patient_Editor = this;

            App.Settings.Load ();
            InitInitialRun ();
            //TODO InitUsageStatistics ();
            InitInterface ();
            //TODO InitUpgrade ();
            //TODO InitMirroring ();
            //TODO InitScenario (true);

            /* TODO
            if (App.Start_Args.Length > 0)
                LoadOpen (App.Start_Args [0]);
            */

            //TODO SetParameterStatus (App.Settings.AutoApplyChanges);

            /* Debugging and testing code below */
        }

        private void InitInitialRun () {
            string setLang = App.Settings.Language;
            if (setLang == null || setLang == ""
                || !Enum.TryParse<Language.Values> (setLang, out App.Language.Value)) {
                App.Language = new Language ();
                //TODO DialogInitial ();
            }
        }

        private void InitInterface () {
            /* Populate UI strings per language selection */
            this.FindControl<Window> ("wdwPatientEditor").Title = App.Language.Localize ("PE:WindowTitle");
            this.FindControl<MenuItem> ("menuNew").Header = App.Language.Localize ("PE:MenuNewFile");
            this.FindControl<MenuItem> ("menuFile").Header = App.Language.Localize ("PE:MenuFile");
            this.FindControl<MenuItem> ("menuLoad").Header = App.Language.Localize ("PE:MenuLoadSimulation");
            this.FindControl<MenuItem> ("menuSave").Header = App.Language.Localize ("PE:MenuSaveSimulation");
            this.FindControl<MenuItem> ("menuExit").Header = App.Language.Localize ("PE:MenuExitProgram");

            this.FindControl<MenuItem> ("menuSettings").Header = App.Language.Localize ("PE:MenuSettings");
            this.FindControl<MenuItem> ("menuSetLanguage").Header = App.Language.Localize ("PE:MenuSetLanguage");

            this.FindControl<MenuItem> ("menuHelp").Header = App.Language.Localize ("PE:MenuHelp");
            this.FindControl<MenuItem> ("menuAbout").Header = App.Language.Localize ("PE:MenuAboutProgram");

            this.FindControl<HeaderedContentControl> ("lblGroupDevices").Header = App.Language.Localize ("PE:Devices");
            this.FindControl<Label> ("lblDeviceMonitor").Content = App.Language.Localize ("PE:CardiacMonitor");
            this.FindControl<Label> ("lblDevice12LeadECG").Content = App.Language.Localize ("PE:12LeadECG");
            this.FindControl<Label> ("lblDeviceDefibrillator").Content = App.Language.Localize ("PE:Defibrillator");
            this.FindControl<Label> ("lblDeviceIABP").Content = App.Language.Localize ("PE:IABP");
        }
    }
}