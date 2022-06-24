using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyProgression : UserControl {
        public string? UUID;
        public string? StepToUUID;
        public string? StepToName;
        public string? Description;

        public new event EventHandler<PropertyProgressionEventArgs>? PropertyChanged;

        public class PropertyProgressionEventArgs : EventArgs {
            public string? UUID;
            public string? StepToUUID;
            public string? StepToName;
            public string? Description;
            public bool ToDelete = false;
        }

        public PropertyProgression () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (string? uuid, string? stepToUUID, string? stepToName, string? desc) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyProgressionEventArgs>)d;
            }

            UUID = uuid;
            StepToUUID = stepToUUID;
            StepToName = stepToName;
            Description = desc;

            Label lblProgressionProperty = this.FindControl<Label> ("lblProgressionProperty");
            TextBox txtDescription = this.FindControl<TextBox> ("txtDescription");

            txtDescription.Text = Description;
            lblProgressionProperty.Content = $"Progression To: {StepToName}";

            txtDescription.TextInput += SendPropertyChange;
            txtDescription.LostFocus += SendPropertyChange;
        }

        private void BtnDelete_Click (object? sender, RoutedEventArgs e) {
            PropertyProgressionEventArgs ea = new PropertyProgressionEventArgs ();
            ea.UUID = UUID;
            ea.StepToUUID = StepToUUID;
            ea.StepToName = StepToName;
            ea.ToDelete = true;

            PropertyChanged?.Invoke (this, ea);
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            TextBox txtStepTo = this.FindControl<TextBox> ("txtStepTo");
            TextBox txtDescription = this.FindControl<TextBox> ("txtDescription");

            PropertyProgressionEventArgs ea = new PropertyProgressionEventArgs ();
            ea.UUID = UUID;
            ea.StepToUUID = StepToUUID;
            ea.StepToName = StepToName;
            ea.Description = txtDescription.Text;

            Debug.WriteLine ($"PropertyChanged: Progression {ea.UUID} -> {ea.StepToUUID} ({ea.StepToName}) {ea.Description}");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}