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

    public partial class PropertyNumeric : UserControl {
        private bool isInitiated = false;

        public int Index;
        public Device.Devices Device;
        public Device.Numeric Numeric;
        public bool Numeric_Zeroed;

        private Label? plblIndex;
        private ComboBox? pcmbNumeric;
        private CheckBox? pchkTransducer;
        
        public new event EventHandler<PropertyNumericEventArgs>? PropertyChanged;

        public class PropertyNumericEventArgs : EventArgs {
            public int Index;
            public Device.Numeric Numeric;
            public Device.Devices Device;
            public bool Numeric_Zeroed = false;
            
            public bool toAdd = false;
            public bool toRemove = false;
            public bool toMove = false;
            public int toMove_Delta = 0;
        }

        public PropertyNumeric () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private Task ReferenceView () {
            
            plblIndex = this.GetControl<Label> ("lblIndex");
            pcmbNumeric = this.GetControl<ComboBox> ("cmbNumeric");
            pchkTransducer = this.GetControl<CheckBox> ("chkTransducer");

            return Task.CompletedTask;
        }

        public async Task Init (Device.Devices device, int index, Device.Numeric numeric, bool zeroed) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyNumericEventArgs>)d;
            }

            await ReferenceView ();

            Device = device;
            Index = index;
            Numeric = numeric;
            Numeric_Zeroed = zeroed;
            
            if (plblIndex is not null && pcmbNumeric is not null && pchkTransducer is not null) {
                plblIndex.Content = $"{Index + 1}:";
                
                foreach (string s in Enum.GetNames (typeof (Device.Numeric))) {
                    pcmbNumeric.Items.Add (new ComboBoxItem () {
                        Content = s switch {
                            "ECG" => "Electrocardiograph (ECG)",
                            "T" => "Temperature (T)",
                            "RR" => "Respiratory Rate (RR)",
                            "ETCO2" => "End-tidal Carbon Dioxide (ETCO2)",
                            "SPO2" => "Pulse Oximetry (SpO2)",
                            "NIBP" => "Non-invasive Blood Pressure (NiBP)",
                            "ABP" => "Arterial Blood Pressure (ABP)",
                            "CVP" => "Central Venous Pressure (CVP)",
                            "CO" => "Cardiac Output (CO)",
                            "PA" => "Pulmonary Arterial Pressures (PA)",
                            "ICP" => "Intra-cranial Pressure (ICP)",
                            "IAP" => "Intra-abdominal Pressure (IAP)",
                            _ => ""
                        }
                    });
                }
                
                pcmbNumeric.SelectedIndex = new List<Device.Numeric>(Enum.GetValues<Device.Numeric>())
                    .IndexOf(Numeric);
                
                pchkTransducer.IsChecked = Numeric_Zeroed;
                
                switch ((Device.Numeric)pcmbNumeric.SelectedIndex) {
                    default:
                        pchkTransducer.IsVisible = false;
                        break;
                    
                    case II.Settings.Device.Numeric.ABP:
                    case II.Settings.Device.Numeric.CVP:
                    case II.Settings.Device.Numeric.IAP:
                    case II.Settings.Device.Numeric.ICP:
                    case II.Settings.Device.Numeric.PA:
                        pchkTransducer.IsVisible = true;
                        break;
                }
                    
                if (!isInitiated) {
                    pcmbNumeric.SelectionChanged += SendPropertyChange;
                    pcmbNumeric.SelectionChanged += UpdateTransducer;
                    pcmbNumeric.LostFocus += SendPropertyChange;
                    pchkTransducer.IsCheckedChanged += SendPropertyChange;
                }

                isInitiated = true;
            }
        }

        public void Set (PropertyNumericEventArgs? pnea) {
            if (pnea is null)
                return;

            Task.WaitAll(ReferenceView());

            if (plblIndex is not null && pcmbNumeric is not null && pchkTransducer is not null) {
                pcmbNumeric.SelectionChanged -= SendPropertyChange;
                pcmbNumeric.SelectionChanged -= UpdateTransducer;
                pcmbNumeric.LostFocus -= SendPropertyChange;
                pchkTransducer.IsCheckedChanged -= SendPropertyChange;
                
                Index = pnea.Index;
                Numeric = pnea.Numeric;
                Device = pnea.Device;
                Numeric_Zeroed = pnea.Numeric_Zeroed;

                plblIndex.Content = $"{Index + 1}:";
                pcmbNumeric.SelectedIndex = new List<Device.Numeric>(Enum.GetValues<Device.Numeric>())
                    .IndexOf(pnea.Numeric);

                switch (pnea.Numeric) {
                    default:
                        pchkTransducer.IsVisible = false;
                        break;
                    
                    case II.Settings.Device.Numeric.ABP:
                    case II.Settings.Device.Numeric.CVP:
                    case II.Settings.Device.Numeric.IAP:
                    case II.Settings.Device.Numeric.ICP:
                    case II.Settings.Device.Numeric.PA:
                        pchkTransducer.IsVisible = true;
                        break;
                }
                
                pchkTransducer.IsChecked = Numeric_Zeroed;
                
                pcmbNumeric.SelectionChanged += SendPropertyChange;
                pcmbNumeric.SelectionChanged += UpdateTransducer;
                pcmbNumeric.LostFocus += SendPropertyChange;
                pchkTransducer.IsCheckedChanged += SendPropertyChange;
            }
        }

        public void SetTransducer (bool isZeroed) {
            Task.WaitAll(ReferenceView());

            if (pchkTransducer is not null) {

                pchkTransducer.IsCheckedChanged -= SendPropertyChange;

                pchkTransducer.IsChecked = isZeroed;

                pchkTransducer.IsCheckedChanged += SendPropertyChange;
            }
        }

        private void UpdateTransducer (object? sender, EventArgs e) {
            pcmbNumeric = this.GetControl<ComboBox> ("cmbNumeric");
            pchkTransducer = this.GetControl<CheckBox> ("chkTransducer");

            switch ((Device.Numeric)pcmbNumeric.SelectedIndex) {
                default:
                    pchkTransducer.IsVisible = false;
                    break;
                    
                case II.Settings.Device.Numeric.ABP:
                case II.Settings.Device.Numeric.CVP:
                case II.Settings.Device.Numeric.IAP:
                case II.Settings.Device.Numeric.ICP:
                case II.Settings.Device.Numeric.PA:
                    pchkTransducer.IsVisible = true;
                    break;
            }
        }
        
        private void SendPropertyChange (object? sender, EventArgs e) {
            pcmbNumeric = this.GetControl<ComboBox> ("cmbNumeric");
            pchkTransducer = this.GetControl<CheckBox>("chkTransducer");
            
            Numeric = (Device.Numeric) pcmbNumeric.SelectedIndex;
            Numeric_Zeroed = pchkTransducer.IsChecked ?? false;
            
            PropertyNumericEventArgs pnea = new() {
                Index = Index,
                Device = Device,
                Numeric = Numeric,
                Numeric_Zeroed = Numeric_Zeroed
            };
            
            Debug.WriteLine ($"PropertyChanged: Numeric {pnea.Device}:{pnea.Index} '{pnea.Numeric.ToString()}'");
            PropertyChanged?.Invoke (this, pnea);
        }

        private void BtnMoveUp_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Numeric {Device}:{Index} '{Numeric.ToString()}' toMove -1");
            
            PropertyChanged?.Invoke(this, 
                new PropertyNumericEventArgs () {
                    Index = Index,
                    Device = Device,
                    Numeric = Numeric,
                    toMove = true,
                    toMove_Delta = -1
                });
        }
        
        private void BtnMoveDown_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Numeric {Device}:{Index} '{Numeric.ToString()}' toMove +1");
            
            PropertyChanged?.Invoke(this, new PropertyNumericEventArgs () {
                Index = Index,
                Device = Device,
                Numeric = Numeric,
                toMove = true,
                toMove_Delta = 1,
            });
        }
        
        private void BtnAdd_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Numeric {Device}:{Index} '{Numeric.ToString()}' toMove +1");
            
            pchkTransducer = this.GetControl<CheckBox>("chkTransducer");
            
            PropertyChanged?.Invoke(this, new PropertyNumericEventArgs () {
                Index = Index,
                Device = Device,
                Numeric = Numeric,
                Numeric_Zeroed = Numeric_Zeroed,
                toAdd = true,
            });
        }
        
        private void BtnRemove_Click (object? sender, RoutedEventArgs e) {
            Debug.WriteLine ($"PropertyChanged: Numeric {Device}:{Index} '{Numeric.ToString()}' toMove +1");
            
            PropertyChanged?.Invoke(this, new PropertyNumericEventArgs () {
                Index = Index,
                Device = Device,
                Numeric = Numeric,
                toRemove = true,
            });
        }
    }
}