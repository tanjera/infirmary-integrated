/* Infirmary Integrated Scenario Editor
 * By Ibi Keller (Tanjera), (c) 2023
 */

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
using II.Settings;
using IISE.Controls;

namespace IISE.Windows {

    public partial class PanelSimulation : UserControl {
        public App? Instance;
        
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        private Scenario? Scenario = new (new Timer());

        /* View Controls for referencing by ViewModel */
        private PropertyCheck vpchkMonitorEnabled;
        private PropertyCheck vpchkDefibEnabled;
        private PropertyCheck vpchkECGEnabled;
        private PropertyCheck vpchkIABPEnabled;
        private PropertyString vpstrScenarioAuthor;
        private PropertyString vpstrScenarioName;
        private PropertyString vpstrScenarioDescription;
        
        private StackPanel vspMonitorNumerics;
        private StackPanel vspMonitorTracings;
        private StackPanel vspMonitorAlarms;
        private List<PropertyNumeric> listMonitorNumerics;
        private List<PropertyTracing> listMonitorTracings;
        private List<PropertyAlarm> listMonitorAlarms;

        private StackPanel vspDefibNumerics;
        private StackPanel vspDefibTracings;
        private List<PropertyNumeric> listDefibNumerics;
        private List<PropertyTracing> listDefibTracings;
        
        public PanelSimulation (App? app) {
            Instance = app;
            
            InitializeComponent ();

            DataContext = this;

            _ = InitView ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task SetScenario (Scenario s) {
            Scenario = s;

            await UpdateViewModel ();
        }

        private Task ReferenceView () {
            vpstrScenarioAuthor = this.GetControl<PropertyString> ("pstrScenarioAuthor");
            vpstrScenarioName = this.GetControl<PropertyString> ("pstrScenarioName");
            vpstrScenarioDescription = this.GetControl<PropertyString> ("pstrScenarioDescription");

            vpchkMonitorEnabled = this.GetControl<PropertyCheck> ("pchkMonitorEnabled");
            vpchkDefibEnabled = this.GetControl<PropertyCheck> ("pchkDefibEnabled");
            vpchkECGEnabled = this.GetControl<PropertyCheck> ("pchkECGEnabled");
            vpchkIABPEnabled = this.GetControl<PropertyCheck> ("pchkIABPEnabled");

            vspMonitorNumerics = this.GetControl<StackPanel> ("spMonitorNumerics");
            vspMonitorTracings = this.GetControl<StackPanel> ("spMonitorTracings");
            vspMonitorAlarms = this.GetControl<StackPanel> ("spMonitorAlarms");

            vspDefibNumerics = this.GetControl<StackPanel> ("spDefibNumerics");
            vspDefibTracings = this.GetControl<StackPanel> ("spDefibTracings");
            
            return Task.CompletedTask;
        }

        private async Task InitView () {
            await ReferenceView ();

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
            
            // Populate Monitor:PropertyNumeric into StackPanel and initiate
            listMonitorNumerics = new ();

            for (int i = 0; i < Scenario?.DeviceMonitor.Numerics.Count; i++) {
                PropertyNumeric pn = new ();
                await pn.Init(Device.Devices.Monitor, 
                    i, 
                    Scenario.DeviceMonitor.Numerics[i],
                    Scenario.DeviceMonitor.Transducers_Zeroed.Contains(Scenario.DeviceMonitor.Numerics [i]));

                pn.PropertyChanged += UpdateScenario;
                
                vspMonitorNumerics.Children.Add (pn);
                listMonitorNumerics.Add (pn);
            }

            // Populate Monitor:PropertyTracing into StackPanel and initiate
            listMonitorTracings = new ();

            for (int i = 0; i < Scenario?.DeviceMonitor?.Tracings?.Count; i++) {
                PropertyTracing pt = new ();
                await pt.Init(Device.Devices.Monitor, i, Scenario.DeviceMonitor.Tracings[i]);

                pt.PropertyChanged += UpdateScenario;
                
                vspMonitorTracings.Children.Add (pt);
                listMonitorTracings.Add (pt);
            }
            
            // Populate Monitor:PropertyAlarms into StackPanel and initiate
            listMonitorAlarms = new ();
            foreach (Alarm.Parameters param in Enum.GetValues (typeof (Alarm.Parameters))) {
                PropertyAlarm pa = new ();
                await pa.Init (Device.Devices.Monitor, param);

                pa.PropertyChanged += UpdateScenario;

                vspMonitorAlarms.Children.Add (pa);
                listMonitorAlarms.Add (pa);
            }
            
            
            // Populate Defib:PropertyNumeric into StackPanel and initiate
            listDefibNumerics = new ();

            for (int i = 0; i < Scenario?.DeviceDefib.Numerics.Count; i++) {
                PropertyNumeric pn = new ();
                await pn.Init(Device.Devices.Defib, 
                    i, 
                    Scenario.DeviceDefib.Numerics[i],
                    Scenario.DeviceDefib.Transducers_Zeroed.Contains(Scenario.DeviceDefib.Numerics [i]));

                pn.PropertyChanged += UpdateScenario;
                
                vspDefibNumerics.Children.Add (pn);
                listDefibNumerics.Add (pn);
            }

            // Populate Defib:PropertyTracing into StackPanel and initiate
            listDefibTracings = new ();

            for (int i = 0; i < Scenario?.DeviceDefib?.Tracings?.Count; i++) {
                PropertyTracing pt = new ();
                await pt.Init(Device.Devices.Defib, i, Scenario.DeviceDefib.Tracings[i]);

                pt.PropertyChanged += UpdateScenario;
                
                vspDefibTracings.Children.Add (pt);
                listDefibTracings.Add (pt);
            }
        }

        private void UpdateScenario (object? sender, PropertyNumeric.PropertyNumericEventArgs e) {
            if (sender is PropertyNumeric) {
                switch (e.Device) {
                    default: break;
                    
                    case Device.Devices.Monitor:
                        if (Scenario is not null) {
                            if (e.toMove)
                                MoveNumeric (sender, e);
                            else if (e.toAdd)
                                AddNumeric(sender, e);
                            else if (e.toRemove)
                                RemoveNumeric(sender, e);

                            // If needed, toggle if the transducer is zeroed
                            if (e.Numeric_Zeroed && !Scenario.DeviceMonitor.Transducers_Zeroed.Contains(e.Numeric)) {
                                Scenario.DeviceMonitor.Transducers_Zeroed.Add(e.Numeric);
                            } else if (!e.Numeric_Zeroed &&
                                       Scenario.DeviceMonitor.Transducers_Zeroed.Contains (e.Numeric)) {
                                Scenario.DeviceMonitor.Transducers_Zeroed.Remove(e.Numeric);
                            }
                            
                            Scenario.DeviceMonitor.Numerics = new ();
                            
                            // Iterate all PropertyNumerics and make sure they are valid and correlate
                            foreach (PropertyNumeric pn in listMonitorNumerics) {
                                Scenario.DeviceMonitor.Numerics.Add (pn.Numeric);
                                
                                // Ensure duplicate PropertyNumerics have matching chkTransducers
                                pn.SetTransducer (Scenario.DeviceMonitor.Transducers_Zeroed.Contains (pn.Numeric));
                            }
                        }
                        break;
                    
                    
                    case Device.Devices.Defib:
                        if (Scenario is not null) {
                            if (e.toMove)
                                MoveNumeric (sender, e);
                            else if (e.toAdd)
                                AddNumeric(sender, e);
                            else if (e.toRemove)
                                RemoveNumeric(sender, e);

                            // If needed, toggle if the transducer is zeroed
                            if (e.Numeric_Zeroed && !Scenario.DeviceDefib.Transducers_Zeroed.Contains(e.Numeric)) {
                                Scenario.DeviceDefib.Transducers_Zeroed.Add(e.Numeric);
                            } else if (!e.Numeric_Zeroed &&
                                       Scenario.DeviceDefib.Transducers_Zeroed.Contains (e.Numeric)) {
                                Scenario.DeviceDefib.Transducers_Zeroed.Remove(e.Numeric);
                            }
                            
                            Scenario.DeviceDefib.Numerics = new ();
                            
                            // Iterate all PropertyNumerics and make sure they are valid and correlate
                            foreach (PropertyNumeric pn in listDefibNumerics) {
                                Scenario.DeviceDefib.Numerics.Add (pn.Numeric);
                                
                                // Ensure duplicate PropertyNumerics have matching chkTransducers
                                pn.SetTransducer (Scenario.DeviceDefib.Transducers_Zeroed.Contains (pn.Numeric));
                            }
                        }
                        break;
                }
            }
        }
        
        private void UpdateScenario (object? sender, PropertyTracing.PropertyTracingEventArgs e) {
            if (sender is PropertyTracing) {
                switch (e.Device) {
                    default: break;
                    
                    case Device.Devices.Monitor:
                        if (Scenario is not null) {
                            if (e.toMove)
                                MoveTracing (sender, e);
                            else if (e.toAdd)
                                AddTracing(sender, e);
                            else if (e.toRemove)
                                RemoveTracing(sender, e);
                            
                            Scenario.DeviceMonitor.Tracings = new ();
                            foreach (PropertyTracing pt in listMonitorTracings)
                                Scenario.DeviceMonitor.Tracings.Add(pt.Tracing);
                        }
                        break;
                    
                    case Device.Devices.Defib:
                        if (Scenario is not null) {
                            if (e.toMove)
                                MoveTracing (sender, e);
                            else if (e.toAdd)
                                AddTracing(sender, e);
                            else if (e.toRemove)
                                RemoveTracing(sender, e);
                            
                            Scenario.DeviceDefib.Tracings = new ();
                            foreach (PropertyTracing pt in listDefibTracings)
                                Scenario.DeviceDefib.Tracings.Add(pt.Tracing);
                        }
                        break;
                }
            }
        }
        
        private void UpdateScenario (object? sender, PropertyAlarm.PropertyAlarmEventArgs e) {
            if (sender is PropertyAlarm) {
                switch (e.Device) {
                    default: break;
                    case Device.Devices.Monitor:
                        Alarm? alarm;
                        if ((alarm = Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == e.Key)) is not null)
                            alarm.Set (e.Key, e.Value?.Enabled, e.Value?.Low, e.Value?.High, e.Value?.Priority);
                        break;
                }
            }
        }

        private void UpdateScenario (object? sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (Scenario is null)
                return;
            
            switch (e.Key) {
                default: break;
                case PropertyCheck.Keys.MonitorIsEnabled: Scenario.DeviceMonitor.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.DefibIsEnabled: Scenario.DeviceDefib.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.ECGIsEnabled: Scenario.DeviceECG.IsEnabled = e.Value; break;
                case PropertyCheck.Keys.IABPIsEnabled: Scenario.DeviceIABP.IsEnabled = e.Value; break;
            }
        }

        private void UpdateScenario (object? sender, PropertyString.PropertyStringEventArgs e) {
            if (Scenario is null)
                return;
            
            switch (e.Key) {
                default: break;
                case PropertyString.Keys.ScenarioAuthor: Scenario.Author = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioName: Scenario.Name = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioDescription: Scenario.Description = e.Value ?? ""; break;
            }
        }

        private async Task UpdateViewModel () {
            if (Scenario is null)
                return;
            
            await ReferenceView ();

            await vpchkMonitorEnabled.Set (Scenario.DeviceMonitor.IsEnabled);
            await vpchkDefibEnabled.Set (Scenario.DeviceDefib.IsEnabled);
            await vpchkECGEnabled.Set (Scenario.DeviceECG.IsEnabled);
            await vpchkIABPEnabled.Set (Scenario.DeviceIABP.IsEnabled);

            await vpstrScenarioAuthor.Set (Scenario.Author ?? "");
            await vpstrScenarioName.Set (Scenario.Name ?? "");
            await vpstrScenarioDescription.Set (Scenario.Description ?? "");
            
            // Update Monitor:PropertyNumerics
            for (int i = 0; i < Scenario?.DeviceMonitor.Numerics.Count; i++) {
                if (i < listMonitorNumerics.Count) {        // Set existing PropertyNumerics
                    listMonitorNumerics [i].Set (new PropertyNumeric.PropertyNumericEventArgs () {
                        Index = i,
                        Device = Device.Devices.Monitor,
                        Numeric = Scenario.DeviceMonitor.Numerics [i],
                        Numeric_Zeroed = Scenario.DeviceMonitor.Transducers_Zeroed.Contains(Scenario.DeviceMonitor.Numerics [i])
                    });
                } else {                                   // Add new as needed
                    PropertyNumeric pn = new ();
                    
                    await pn.Init(Device.Devices.Monitor, 
                        i, 
                        Scenario.DeviceMonitor.Numerics[i],
                        Scenario.DeviceMonitor.Transducers_Zeroed.Contains(Scenario.DeviceMonitor.Numerics [i]));

                    pn.PropertyChanged += UpdateScenario;
                
                    vspMonitorNumerics.Children.Add (pn);
                    listMonitorNumerics.Add (pn);
                }
            }
            
            // If there were more Monitor:PropertyNumerics than there should be, trim the excess
            for (int i = listMonitorNumerics.Count - 1; i >= Scenario?.DeviceMonitor?.Numerics?.Count; i--) {
                listMonitorNumerics[i].PropertyChanged -= UpdateScenario;
                
                vspMonitorNumerics.Children.RemoveAt (i);
                listMonitorNumerics.RemoveAt (i);
            }
            
            // Update Monitor:PropertyTracings
            for (int i = 0; i < Scenario?.DeviceMonitor?.Tracings.Count; i++) {
                if (i < listMonitorTracings.Count) {        // Set existing PropertyTracings
                    listMonitorTracings [i].Set (new PropertyTracing.PropertyTracingEventArgs () {
                        Index = i,
                        Device = Device.Devices.Monitor,
                        Tracing = Scenario.DeviceMonitor.Tracings [i]
                    });
                } else {                                   // Add new as needed
                    PropertyTracing pt = new ();
                    
                    await pt.Init(Device.Devices.Monitor, i, Scenario.DeviceMonitor.Tracings[i]);

                    pt.PropertyChanged += UpdateScenario;
                
                    vspMonitorTracings.Children.Add (pt);
                    listMonitorTracings.Add (pt);
                }
            }
            
            // If there were more Monitor:PropertyTracings than there should be, trim the excess
            for (int i = listMonitorTracings.Count - 1; i >= Scenario?.DeviceMonitor?.Tracings?.Count; i--) {
                listMonitorTracings[i].PropertyChanged -= UpdateScenario;
                
                vspMonitorTracings.Children.RemoveAt (i);
                listMonitorTracings.RemoveAt (i);
            }
            
            // Update Monitor:Alarms
            foreach (PropertyAlarm pa in listMonitorAlarms) {
                Alarm? alarm;
                if ((alarm = Scenario?.DeviceMonitor?.Alarms?.Find (a => a.Parameter == pa.Key)) is not null)
                    await pa.Set (alarm);
            }
            
            
            // Update Defib:PropertyNumerics
            for (int i = 0; i < Scenario?.DeviceDefib.Numerics.Count; i++) {
                if (i < listDefibNumerics.Count) {        // Set existing PropertyNumerics
                    listDefibNumerics [i].Set (new PropertyNumeric.PropertyNumericEventArgs () {
                        Index = i,
                        Device = Device.Devices.Defib,
                        Numeric = Scenario.DeviceDefib.Numerics [i],
                        Numeric_Zeroed = Scenario.DeviceDefib.Transducers_Zeroed.Contains(Scenario.DeviceDefib.Numerics [i])
                    });
                } else {                                   // Add new as needed
                    PropertyNumeric pn = new ();
                    
                    await pn.Init(Device.Devices.Defib, 
                        i, 
                        Scenario.DeviceDefib.Numerics[i],
                        Scenario.DeviceDefib.Transducers_Zeroed.Contains(Scenario.DeviceDefib.Numerics [i]));

                    pn.PropertyChanged += UpdateScenario;
                
                    vspDefibNumerics.Children.Add (pn);
                    listDefibNumerics.Add (pn);
                }
            }
            
            // If there were more Defib:PropertyNumerics than there should be, trim the excess
            for (int i = listDefibNumerics.Count - 1; i >= Scenario?.DeviceDefib?.Numerics?.Count; i--) {
                listDefibNumerics[i].PropertyChanged -= UpdateScenario;
                
                vspDefibNumerics.Children.RemoveAt (i);
                listDefibNumerics.RemoveAt (i);
            }
            
            // Update Defib:PropertyTracings
            for (int i = 0; i < Scenario?.DeviceDefib?.Tracings.Count; i++) {
                if (i < listDefibTracings.Count) {        // Set existing PropertyTracings
                    listDefibTracings [i].Set (new PropertyTracing.PropertyTracingEventArgs () {
                        Index = i,
                        Device = Device.Devices.Defib,
                        Tracing = Scenario.DeviceDefib.Tracings [i]
                    });
                } else {                                   // Add new as needed
                    PropertyTracing pt = new ();
                    
                    await pt.Init(Device.Devices.Defib, i, Scenario.DeviceDefib.Tracings[i]);

                    pt.PropertyChanged += UpdateScenario;
                
                    vspDefibTracings.Children.Add (pt);
                    listDefibTracings.Add (pt);
                }
            }
            
            // If there were more Defib:PropertyTracings than there should be, trim the excess
            for (int i = listDefibTracings.Count - 1; i >= Scenario?.DeviceDefib?.Tracings?.Count; i--) {
                listDefibTracings[i].PropertyChanged -= UpdateScenario;
                
                vspDefibTracings.Children.RemoveAt (i);
                listDefibTracings.RemoveAt (i);
            }
        }

        private void AddNumeric (object sender, PropertyNumeric.PropertyNumericEventArgs e) {
            if (Scenario is null)
                return;
            
            List<Device.Numeric>? ldn = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Numerics,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Numerics,
                _ => null
            };
            
            List<Device.Numeric>? ldnz = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Transducers_Zeroed,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Transducers_Zeroed,
                _ => null
            };
            
            if (e.Index >= 0 && e.Index < ldn?.Count) {
                var n =  ldn [e.Index];
                ldn?.Insert (e.Index, n);

                if (e.Numeric_Zeroed && !(ldnz?.Contains (n) ?? true)) {
                    ldnz?.Add (n);
                }
            }

            // In UpdateViewModel, listMonitorNumerics will be rebuilt based on the newly modified Scenario.DeviceMonitor.lNumerics
            Task.WaitAll(UpdateViewModel());
        }
        
        private void AddTracing (object sender, PropertyTracing.PropertyTracingEventArgs e) {
            if (Scenario is null)
                return;

            List<Device.Tracing>? ldt = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Tracings,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Tracings,
                _ => null
            };
            
            if (e.Index >= 0 && e.Index < ldt?.Count) {
                ldt?.Insert (e.Index, ldt? [e.Index] ?? new Device.Tracing());
            }
            
            // In UpdateViewModel, listMonitorTracings will be rebuilt based on the newly modified Scenario.DeviceMonitor.Tracings
            Task.WaitAll(UpdateViewModel());
        }
        
        private void RemoveNumeric (object sender, PropertyNumeric.PropertyNumericEventArgs e) {
            if (Scenario is null)
                return;
            
            List<Device.Numeric>? ldn = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Numerics,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Numerics,
                _ => null
            };
            
            if (ldn?.Count <= 1)
                return; // Don't remove the last Numeric...
            if (e.Index >= 0 && e.Index < ldn?.Count) {
                ldn?.RemoveAt (e.Index);
            }

            // In UpdateViewModel, listMonitorNumerics will be rebuilt based on the newly modified Scenario.DeviceMonitor.lNumerics
            Task.WaitAll(UpdateViewModel());
        }
        
        private void RemoveTracing (object sender, PropertyTracing.PropertyTracingEventArgs e) {
            if (Scenario is null)
                return;

            List<Device.Tracing>? ldt = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Tracings,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Tracings,
                _ => null
            };
            
            if (ldt?.Count <= 1)
                return; // Don't remove the last Numeric...
            if (e.Index >= 0 && e.Index < ldt?.Count) {
                ldt?.RemoveAt (e.Index);
            }

            // In UpdateViewModel, listMonitorTracings will be rebuilt based on the newly modified Scenario.DeviceMonitor.Tracings
            Task.WaitAll(UpdateViewModel());
        }
        
        private void MoveNumeric (object sender, PropertyNumeric.PropertyNumericEventArgs e) {
            if (Scenario is null)
                return;
            
            List<Device.Numeric>? ldn = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Numerics,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Numerics,
                _ => null
            };
            
            if (e.Index + e.toMove_Delta >= 0 && e.Index + e.toMove_Delta < ldn?.Count) {
                var n =  ldn [e.Index];
                ldn.RemoveAt (e.Index);
                ldn.Insert (e.Index + e.toMove_Delta, n);
            }

            // In UpdateViewModel, listMonitorNumerics will be rebuilt based on the newly modified Scenario.DeviceMonitor.lNumerics
            Task.WaitAll(UpdateViewModel());
        }
        
        private void MoveTracing (object sender, PropertyTracing.PropertyTracingEventArgs e) {
            if (Scenario is null)
                return;
            
            List<Device.Tracing>? ldt = e.Device switch {
                Device.Devices.Monitor => Scenario?.DeviceMonitor?.Tracings,
                Device.Devices.Defib => Scenario?.DeviceDefib?.Tracings,
                _ => null
            };
            
            if (e.Index + e.toMove_Delta >= 0 && e.Index + e.toMove_Delta < ldt?.Count) {
                var n =  ldt [e.Index];
                ldt.RemoveAt (e.Index);
                ldt.Insert (e.Index + e.toMove_Delta, n);
            }

            // In UpdateViewModel, listMonitorTracingss will be rebuilt based on the newly modified Scenario.DeviceMonitor.Tracings
            Task.WaitAll(UpdateViewModel());
        }
        
        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => Instance?.WindowMain?.MenuHelpAbout_Click (sender, e);

        /* Menu Items specific to this Panel */

        /* Any other Routed events for this Panel */
    }
}