using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using II;

namespace IISE.Controls {

    public partial class PropertyAlarm : UserControl {
        public Devices Device;
        public Alarms.Alarm.Parameters Key;
        public List<string>? Values;

        private Label? plblKey;
        private NumericUpDown? pnumHigh;
        private NumericUpDown? pnumLow;
        private CheckBox? pchkEnabled;
        private ComboBox? pcmbSeverity;

        public new event EventHandler<PropertyAlarmEventArgs>? PropertyChanged;

        public class PropertyAlarmEventArgs : EventArgs {
            public Alarms.Alarm.Parameters Key;
            public Alarms.Alarm? Value;
            public Devices Device;
        }

        public enum Devices {
            Monitor,
            Defib,
            ECG,
            IABP
        }

        public PropertyAlarm () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task Init (Devices device, Alarms.Alarm.Parameters key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyAlarmEventArgs>)d;
            }

            await ReferenceViewModel ();

            Device = device;
            Key = key;

            if (plblKey is not null) {
                switch (Key) {
                    default: break;
                    case Alarms.Alarm.Parameters.HR: plblKey.Content = "Heart Rate (HR):"; break;
                    case Alarms.Alarm.Parameters.RR: plblKey.Content = "Respiratory Rate (RR):"; break;
                    case Alarms.Alarm.Parameters.SPO2: plblKey.Content = "Pulse Oximetry (SpO2):"; break;
                    case Alarms.Alarm.Parameters.ETCO2: plblKey.Content = "End-Tidal CO2 (ETCO2):"; break;
                    case Alarms.Alarm.Parameters.CVP: plblKey.Content = "Central Venous Pressure (CVP):"; break;
                    case Alarms.Alarm.Parameters.ICP: plblKey.Content = "Intracranial Pressure (ICP):"; break;
                    case Alarms.Alarm.Parameters.IAP: plblKey.Content = "Intraabdominal Pressure (IAP):"; break;
                    case Alarms.Alarm.Parameters.NSBP: plblKey.Content = "Non-invasive Blood Pressure (NiBP) Systolic:"; break;
                    case Alarms.Alarm.Parameters.NDBP: plblKey.Content = "Non-invasive Blood Pressure (NiBP) Diastolic:"; break;
                    case Alarms.Alarm.Parameters.NMAP: plblKey.Content = "Non-invasive Blood Pressure (NiBP) Mean (MAP):"; break;
                    case Alarms.Alarm.Parameters.ASBP: plblKey.Content = "Arterial Blood Pressure (ABP) Systolic:"; break;
                    case Alarms.Alarm.Parameters.ADBP: plblKey.Content = "Arterial Blood Pressure (ABP) Diastolic:"; break;
                    case Alarms.Alarm.Parameters.AMAP: plblKey.Content = "Arterial Blood Pressure (ABP) Mean (MAP):"; break;
                    case Alarms.Alarm.Parameters.PSP: plblKey.Content = "Pulmonary Arterial Pressure (PAP) Systolic:"; break;
                    case Alarms.Alarm.Parameters.PDP: plblKey.Content = "Pulmonary Arterial Pressure (PAP) Diastolic:"; break;
                    case Alarms.Alarm.Parameters.PMP: plblKey.Content = "Pulmonary Arterial Pressure (PAP) Mean (mPAP):"; break;
                }
            }

            if (pnumHigh is not null && pnumLow is not null && pchkEnabled is not null && pcmbSeverity is not null) {
                pnumHigh.ValueChanged += SendPropertyChange;
                pnumHigh.LostFocus += SendPropertyChange;
                pnumLow.ValueChanged += SendPropertyChange;
                pnumLow.LostFocus += SendPropertyChange;

                pchkEnabled.Checked += SendPropertyChange;
                pchkEnabled.Unchecked += SendPropertyChange;
                pchkEnabled.LostFocus += SendPropertyChange;

                Values = new ();
                List<ComboBoxItem> listItems = new ();
                foreach (string s in Enum.GetNames (typeof (Alarms.Alarm.Severities))) {
                    listItems.Add (new ComboBoxItem () { Content = s });
                    Values.Add (s);
                }

                pcmbSeverity.Items = listItems;
                pcmbSeverity.SelectionChanged += SendPropertyChange;
                pcmbSeverity.LostFocus += SendPropertyChange;
            }
        }

        private Task ReferenceViewModel () {
            plblKey = this.FindControl<Label> ("lblKey");
            pnumHigh = this.FindControl<NumericUpDown> ("numHigh");
            pnumLow = this.FindControl<NumericUpDown> ("numLow");
            pchkEnabled = this.FindControl<CheckBox> ("chkEnabled");
            pcmbSeverity = this.FindControl<ComboBox> ("cmbSeverity");

            return Task.CompletedTask;
        }

        public async Task Set (Alarms.Alarm? alarm) {
            if (alarm is null)
                return;

            await ReferenceViewModel ();

            pchkEnabled.Checked -= SendPropertyChange;
            pchkEnabled.Unchecked -= SendPropertyChange;
            pnumHigh.ValueChanged -= SendPropertyChange;
            pnumLow.ValueChanged -= SendPropertyChange;
            pcmbSeverity.SelectionChanged -= SendPropertyChange;

            pchkEnabled.IsChecked = alarm.Enabled ?? false;
            pnumHigh.Value = alarm.High ?? 0;
            pnumLow.Value = alarm.Low ?? 0;
            pcmbSeverity.SelectedIndex = alarm.Severity.GetHashCode ();

            pchkEnabled.Checked += SendPropertyChange;
            pchkEnabled.Unchecked += SendPropertyChange;
            pnumHigh.ValueChanged += SendPropertyChange;
            pnumLow.ValueChanged += SendPropertyChange;
            pcmbSeverity.SelectionChanged += SendPropertyChange;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            pnumHigh = this.FindControl<NumericUpDown> ("numHigh");
            pnumLow = this.FindControl<NumericUpDown> ("numLow");
            pchkEnabled = this.FindControl<CheckBox> ("chkEnabled");
            pcmbSeverity = this.FindControl<ComboBox> ("cmbSeverity");

            PropertyAlarmEventArgs ea = new ();
            ea.Device = Device;
            ea.Key = Key;
            if (Values != null && Values.Count > 0)
                ea.Value = new Alarms.Alarm (Key, pchkEnabled.IsChecked, (int)pnumHigh.Value, (int)pnumLow.Value, (Alarms.Alarm.Severities)pcmbSeverity.SelectedIndex);

            Debug.WriteLine ($"PropertyChanged: Alarm {ea.Device}:{ea.Key} '{ea.Value?.Parameter} {ea.Value?.Enabled} {ea.Value?.Low} {ea.Value?.High}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}