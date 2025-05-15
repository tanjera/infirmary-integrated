using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyDate : UserControl {
        private bool isInitiated = false;

        public Keys Key;

        public enum Keys {
            SimulationDate,
            DemographicsDOB
        }

        public new event EventHandler<PropertyDateEventArgs>? PropertyChanged;

        public class PropertyDateEventArgs : EventArgs {
            public Keys Key;
            public DateOnly? Value;
        }

        public PropertyDate () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyDateEventArgs>)d;
            }

            Label lblKey = this.GetControl<Label> ("lblKey");
            DatePicker dpValue = this.GetControl<DatePicker> ("dpValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.SimulationDate: lblKey.Content = "Simulation Date: "; break;
                case Keys.DemographicsDOB: lblKey.Content = "Date of Birth: "; break;
            }

            if (!isInitiated) {
                dpValue.SelectedDateChanged += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public async Task Init (Keys key, DateOnly value) {
            await Init (key);
            await Set (value);
        }

        public Task Set (DateTime? value)
            => Set (DateOnly.FromDateTime (value ?? new DateTime ()));

        public Task Set (DateOnly? value) {
            if (value is null)
                return Task.CompletedTask;

            DatePicker dpValue = this.GetControl<DatePicker> ("dpValue");

            dpValue.SelectedDateChanged -= SendPropertyChange;
            dpValue.SelectedDate = new DateTimeOffset (
                new DateTime (
                    value?.Year ?? new DateTime ().Year,
                    value?.Month ?? new DateTime ().Month,
                    value?.Day ?? new DateTime ().Day));
            dpValue.SelectedDateChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, DatePickerSelectedValueChangedEventArgs e) {
            PropertyDateEventArgs ea = new PropertyDateEventArgs ();
            ea.Key = Key;
            ea.Value = new DateOnly (
                e.NewDate?.Year ?? new DateTime ().Year,
                e.NewDate?.Month ?? new DateTime ().Month,
                e.NewDate?.Day ?? new DateTime ().Day);

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}