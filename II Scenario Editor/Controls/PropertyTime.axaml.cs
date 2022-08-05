using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyTime : UserControl {
        private bool isInitiated = false;

        public Keys Key;

        public enum Keys {
            SimulationTime
        }

        public new event EventHandler<PropertyTimeEventArgs>? PropertyChanged;

        public class PropertyTimeEventArgs : EventArgs {
            public Keys Key;
            public TimeOnly? Value;
        }

        public PropertyTime () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyTimeEventArgs>)d;
            }

            Label lblKey = this.FindControl<Label> ("lblKey");
            TimePicker tpValue = this.FindControl<TimePicker> ("tpValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.SimulationTime: lblKey.Content = "Simulation Time: "; break;
            }

            if (!isInitiated) {
                tpValue.SelectedTimeChanged += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public async Task Init (Keys key, TimeOnly value) {
            await Init (key);
            await Set (value);
        }

        public Task Set (DateTime? value)
            => Set (TimeOnly.FromDateTime (value ?? new DateTime ()));

        public Task Set (TimeOnly? value) {
            if (value is null)
                return Task.CompletedTask;

            TimePicker tpValue = this.FindControl<TimePicker> ("tpValue");

            tpValue.SelectedTimeChanged -= SendPropertyChange;
            tpValue.SelectedTime = new TimeSpan (
                    value?.Hour ?? 0,
                    value?.Minute ?? 0,
                    0);
            tpValue.SelectedTimeChanged += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, TimePickerSelectedValueChangedEventArgs e) {
            PropertyTimeEventArgs ea = new PropertyTimeEventArgs ();
            ea.Key = Key;
            ea.Value = new TimeOnly (
                e.NewTime?.Hours ?? 0,
                e.NewTime?.Minutes ?? 0,
                0);

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}