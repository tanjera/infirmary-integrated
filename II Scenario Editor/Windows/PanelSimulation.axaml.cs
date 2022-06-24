using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II_Scenario_Editor.Controls;

namespace II_Scenario_Editor.Windows {

    public partial class PanelSimulation : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        private Scenario Scenario;
        private WindowMain IMain;

        public PanelSimulation () {
            InitializeComponent ();

            DataContext = this;

            _ = InitViewModel ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task InitReferences (WindowMain main) {
            IMain = main;
        }

        public async Task SetScenario (Scenario s) {
            Scenario = s;

            await UpdateViewModel ();
        }

        private Task InitViewModel () {
            PropertyString pstrScenarioAuthor = this.FindControl<PropertyString> ("pstrScenarioAuthor");
            PropertyString pstrScenarioName = this.FindControl<PropertyString> ("pstrScenarioName");
            PropertyString pstrScenarioDescription = this.FindControl<PropertyString> ("pstrScenarioDescription");

            // Initiate controls for editing Scenario properties
            pstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            pstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            pstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);

            pstrScenarioAuthor.PropertyChanged += UpdateScenario;
            pstrScenarioName.PropertyChanged += UpdateScenario;
            pstrScenarioDescription.PropertyChanged += UpdateScenario;

            return Task.CompletedTask;
        }

        private void UpdateScenario (object? sender, PropertyString.PropertyStringEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyString.Keys.ScenarioAuthor: Scenario.Author = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioName: Scenario.Name = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioDescription: Scenario.Description = e.Value ?? ""; break;
            }
        }

        private Task UpdateViewModel () {
            this.FindControl<PropertyString> ("pstrScenarioAuthor").Set (Scenario.Author ?? "");
            this.FindControl<PropertyString> ("pstrScenarioName").Set (Scenario.Name ?? "");
            this.FindControl<PropertyString> ("pstrScenarioDescription").Set (Scenario.Description ?? "");

            return Task.CompletedTask;
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => IMain.MenuHelpAbout_Click (sender, e);

        /* Menu Commands: For HotKey support!! */

        private void MenuFileNew_Command ()
            => IMain.MenuFileNew_Click (this, new RoutedEventArgs ());

        private void MenuFileLoad_Command ()
            => IMain.MenuFileLoad_Click (this, new RoutedEventArgs ());

        private void MenuFileSave_Command ()
            => IMain.MenuFileSave_Click (this, new RoutedEventArgs ());

        /* Menu Items specific to this Panel */

        /* Any other Routed events for this Panel */
    }
}