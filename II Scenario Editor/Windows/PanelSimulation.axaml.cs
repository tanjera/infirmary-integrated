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

        /* View Controls for referencing by ViewModel */
        private PropertyCheck vpchkMonitorEnabled;
        private PropertyCheck vpchkDefibEnabled;
        private PropertyCheck vpchkECGEnabled;
        private PropertyCheck vpchkIABPEnabled;
        private PropertyString vpstrScenarioAuthor;
        private PropertyString vpstrScenarioName;
        private PropertyString vpstrScenarioDescription;

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
            ReferenceViewModel ();

            // Initiate controls
            vpchkMonitorEnabled.Init (PropertyCheck.Keys.MonitorIsEnabled);
            vpchkDefibEnabled.Init (PropertyCheck.Keys.DefibIsEnabled);
            vpchkECGEnabled.Init (PropertyCheck.Keys.ECGIsEnabled);
            vpchkIABPEnabled.Init (PropertyCheck.Keys.IABPIsEnabled);

            vpstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            vpstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            vpstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);

            vpchkMonitorEnabled.PropertyChanged += UpdateScenario;
            vpchkDefibEnabled.PropertyChanged += UpdateScenario;
            vpchkECGEnabled.PropertyChanged += UpdateScenario;
            vpchkIABPEnabled.PropertyChanged += UpdateScenario;

            vpstrScenarioAuthor.PropertyChanged += UpdateScenario;
            vpstrScenarioName.PropertyChanged += UpdateScenario;
            vpstrScenarioDescription.PropertyChanged += UpdateScenario;

            return Task.CompletedTask;
        }

        private Task ReferenceViewModel () {
            vpstrScenarioAuthor = this.FindControl<PropertyString> ("pstrScenarioAuthor");
            vpstrScenarioName = this.FindControl<PropertyString> ("pstrScenarioName");
            vpstrScenarioDescription = this.FindControl<PropertyString> ("pstrScenarioDescription");

            vpchkMonitorEnabled = this.FindControl<PropertyCheck> ("pchkMonitorEnabled");
            vpchkDefibEnabled = this.FindControl<PropertyCheck> ("pchkDefibEnabled");
            vpchkECGEnabled = this.FindControl<PropertyCheck> ("pchkECGEnabled");
            vpchkIABPEnabled = this.FindControl<PropertyCheck> ("pchkIABPEnabled");

            return Task.CompletedTask;
        }

        private void UpdateScenario (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyCheck.Keys.MonitorIsEnabled: Scenario.DeviceMonitor.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.DefibIsEnabled: Scenario.DeviceDefib.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.ECGIsEnabled: Scenario.DeviceECG.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.IABPIsEnabled: Scenario.DeviceIABP.IsEnabled = e.Value; break;
            }
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
            ReferenceViewModel ();

            vpchkMonitorEnabled.Set (Scenario.DeviceMonitor.IsEnabled);
            vpchkDefibEnabled.Set (Scenario.DeviceDefib.IsEnabled);
            vpchkECGEnabled.Set (Scenario.DeviceECG.IsEnabled);
            vpchkIABPEnabled.Set (Scenario.DeviceIABP.IsEnabled);

            vpstrScenarioAuthor.Set (Scenario.Author ?? "");
            vpstrScenarioName.Set (Scenario.Name ?? "");
            vpstrScenarioDescription.Set (Scenario.Description ?? "");

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