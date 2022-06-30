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
using IISE.Controls;

namespace IISE.Windows {

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
        private StackPanel vspMonitorAlarms;
        private List<PropertyAlarm> listMonitorAlarms;

        public PanelSimulation () {
            InitializeComponent ();

            DataContext = this;

            _ = InitViewModel ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task InitReferences (WindowMain main) {
            IMain = main;

            return Task.CompletedTask;
        }

        public async Task SetScenario (Scenario s) {
            Scenario = s;

            await UpdateViewModel ();
        }

        private async Task InitViewModel () {
            await ReferenceViewModel ();

            // Initiate controls
            await vpchkMonitorEnabled.Init (PropertyCheck.Keys.MonitorIsEnabled);
            await vpchkDefibEnabled.Init (PropertyCheck.Keys.DefibIsEnabled);
            await vpchkECGEnabled.Init (PropertyCheck.Keys.ECGIsEnabled);
            await vpchkIABPEnabled.Init (PropertyCheck.Keys.IABPIsEnabled);

            await vpstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            await vpstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            await vpstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);

            vpchkMonitorEnabled.PropertyChanged += UpdateScenario;
            vpchkDefibEnabled.PropertyChanged += UpdateScenario;
            vpchkECGEnabled.PropertyChanged += UpdateScenario;
            vpchkIABPEnabled.PropertyChanged += UpdateScenario;

            vpstrScenarioAuthor.PropertyChanged += UpdateScenario;
            vpstrScenarioName.PropertyChanged += UpdateScenario;
            vpstrScenarioDescription.PropertyChanged += UpdateScenario;

            // Populate PropertyAlarms into StackPanel and initiate
            listMonitorAlarms = new ();
            foreach (Alarms.Alarm.Parameters param in Enum.GetValues (typeof (Alarms.Alarm.Parameters))) {
                PropertyAlarm pa = new ();
                await pa.Init (PropertyAlarm.Devices.Monitor, param);

                pa.PropertyChanged += UpdateScenario;

                vspMonitorAlarms.Children.Add (pa);
                listMonitorAlarms.Add (pa);
            }
        }

        private Task ReferenceViewModel () {
            vpstrScenarioAuthor = this.FindControl<PropertyString> ("pstrScenarioAuthor");
            vpstrScenarioName = this.FindControl<PropertyString> ("pstrScenarioName");
            vpstrScenarioDescription = this.FindControl<PropertyString> ("pstrScenarioDescription");

            vpchkMonitorEnabled = this.FindControl<PropertyCheck> ("pchkMonitorEnabled");
            vpchkDefibEnabled = this.FindControl<PropertyCheck> ("pchkDefibEnabled");
            vpchkECGEnabled = this.FindControl<PropertyCheck> ("pchkECGEnabled");
            vpchkIABPEnabled = this.FindControl<PropertyCheck> ("pchkIABPEnabled");

            vspMonitorAlarms = this.FindControl<StackPanel> ("spMonitorAlarms");

            return Task.CompletedTask;
        }

        private void UpdateScenario (object? sender, PropertyAlarm.PropertyAlarmEventArgs e) {
            if (sender is PropertyAlarm && listMonitorAlarms.Contains (sender))
                _ = Scenario.DeviceMonitor.Alarms?.Set (e.Key, e.Value);
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

        private async Task UpdateViewModel () {
            await ReferenceViewModel ();

            await vpchkMonitorEnabled.Set (Scenario.DeviceMonitor.IsEnabled);
            await vpchkDefibEnabled.Set (Scenario.DeviceDefib.IsEnabled);
            await vpchkECGEnabled.Set (Scenario.DeviceECG.IsEnabled);
            await vpchkIABPEnabled.Set (Scenario.DeviceIABP.IsEnabled);

            await vpstrScenarioAuthor.Set (Scenario.Author ?? "");
            await vpstrScenarioName.Set (Scenario.Name ?? "");
            await vpstrScenarioDescription.Set (Scenario.Description ?? "");

            foreach (PropertyAlarm pa in listMonitorAlarms) {
                await pa.Set (await Scenario.DeviceMonitor.Alarms?.Get (pa.Key));
            }
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