using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;

using II;
using II.Settings;

namespace IISE.Controls {

    public partial class PropertyTracing : UserControl {
        private bool isInitiated = false;

        public int Index;
        public Device.Devices Device;
        public Device.Tracing Tracing;

        private Label? plblIndex;
        private ComboBox? pcmbTracing;

        public new event EventHandler<PropertyTracingEventArgs>? PropertyChanged;

        public class PropertyTracingEventArgs : EventArgs {
            public int Index;
            public Device.Tracing Tracing;
            public Device.Devices Device;
            
            public bool toAdd = false;
            public bool toRemove = false;
            public bool toMove = false;
            public int toMove_Delta = 0;
        }
        
        public PropertyTracing () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private Task ReferenceView () {
            plblIndex = this.GetControl<Label> ("lblIndex");
            pcmbTracing = this.GetControl<ComboBox> ("cmbTracing");


            return Task.CompletedTask;
        }

        public async Task Init (Device.Devices device, int index, Device.Tracing tracing) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyTracingEventArgs>)d;
            }

            await ReferenceView ();

            Device = device;
            Index = index;
            Tracing = tracing;

            if (plblIndex is not null && pcmbTracing is not null) {
                plblIndex.Content = $"{Index + 1}:";
                
                foreach (string s in Enum.GetNames (typeof (Device.Tracing))) {
                    if (s == "IABP" || s == "FHR" || s == "TOCO")
                        continue;
                    
                    pcmbTracing.Items.Add (new ComboBoxItem () {
                        Content = s switch {
                            "ECG_I" => "Electrocardiograph (ECG): Lead I",
                            "ECG_II" => "Electrocardiograph (ECG): Lead II",
                            "ECG_III" => "Electrocardiograph (ECG): Lead III",
                            "ECG_AVR" => "Electrocardiograph (ECG): Lead aVR",
                            "ECG_AVL" => "Electrocardiograph (ECG): Lead aVL",
                            "ECG_AVF" => "Electrocardiograph (ECG): Lead aVF",
                            "ECG_V1" => "Electrocardiograph (ECG): Lead V1",
                            "ECG_V2" => "Electrocardiograph (ECG): Lead V2",
                            "ECG_V3" => "Electrocardiograph (ECG): Lead V3",
                            "ECG_V4" => "Electrocardiograph (ECG): Lead V4",
                            "ECG_V5" => "Electrocardiograph (ECG): Lead V5",
                            "ECG_V6" => "Electrocardiograph (ECG): Lead V6",
                            
                            "SPO2" => "Pulse Oximetry (SpO2)",
                            "RR" => "Respiratory Rate (RR)",
                            "ETCO2" => "End-tidal Carbon Dioxide (ETCO2)",

                            "CVP" => "Central Venous Pressure (CVP)",
                            "ABP" => "Arterial Blood Pressure (ABP)",
                            "PA" => "Pulmonary Arterial Pressures (PA)",
                            "ICP" => "Intra-cranial Pressure (ICP)",
                            "IAP" => "Intra-abdominal Pressure (IAP)",
                            _ => ""
                        }
                    });
                }
                
                pcmbTracing.SelectedIndex = new List<Device.Tracing>(Enum.GetValues<Device.Tracing>())
                    .IndexOf(Tracing);
                
                if (!isInitiated) {
                    pcmbTracing.SelectionChanged += SendPropertyChange;
                    pcmbTracing.LostFocus += SendPropertyChange;
                }

                isInitiated = true;
            }
        }

        public void Set (PropertyTracingEventArgs? ptea) {
            if (ptea is null)
                return;

            Task.WaitAll(ReferenceView());

            if (plblIndex is not null && pcmbTracing is not null) {
                pcmbTracing.SelectionChanged -= SendPropertyChange;
                pcmbTracing.LostFocus -= SendPropertyChange;
                
                Index = ptea.Index;
                Tracing = ptea.Tracing;
                Device = ptea.Device;

                plblIndex.Content = $"{Index + 1}:";
                pcmbTracing.SelectedIndex = new List<Device.Tracing>(Enum.GetValues<Device.Tracing>())
                    .IndexOf(ptea.Tracing);
                
                pcmbTracing.SelectionChanged += SendPropertyChange;
                pcmbTracing.LostFocus += SendPropertyChange;
            }
        }

        
        private void SendPropertyChange (object? sender, EventArgs e) {
            pcmbTracing = this.GetControl<ComboBox> ("cmbTracing");
            Tracing = (Device.Tracing) pcmbTracing.SelectedIndex;

            PropertyTracingEventArgs ptea = new() {
                Index = Index,
                Device = Device,
                Tracing = Tracing,
            };
            
            Debug.WriteLine ($"PropertyChanged: Tracing {ptea.Device}:{ptea.Index} '{ptea.Tracing.ToString()}'");
            PropertyChanged?.Invoke (this, ptea);
        }

        private void BtnMoveUp_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Tracing {Device}:{Index} '{Tracing.ToString()}' toMove -1");
            
            PropertyChanged?.Invoke(this, 
                new PropertyTracingEventArgs () {
                    Index = Index,
                    Device = Device,
                    Tracing = Tracing,
                    toMove = true,
                    toMove_Delta = -1
                });
        }
        
        private void BtnMoveDown_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Tracing {Device}:{Index} '{Tracing.ToString()}' toMove +1");
            
            PropertyChanged?.Invoke(this, new PropertyTracingEventArgs () {
                Index = Index,
                Device = Device,
                Tracing = Tracing,
                toMove = true,
                toMove_Delta = 1,
            });
        }
        
        private void BtnAdd_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Tracing {Device}:{Index} '{Tracing.ToString()}' toMove +1");
            
            PropertyChanged?.Invoke(this, new PropertyTracingEventArgs () {
                Index = Index,
                Device = Device,
                Tracing = Tracing,
                toAdd = true,
            });
        }
        
        private void BtnRemove_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Tracing {Device}:{Index} '{Tracing.ToString()}' toMove +1");
            
            PropertyChanged?.Invoke(this, new PropertyTracingEventArgs () {
                Index = Index,
                Device = Device,
                Tracing = Tracing,
                toRemove = true,
            });
        }
    }
}