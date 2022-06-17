using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyOptProgression : UserControl {
        public int Index;
        public string? StepToUUID;
        public string? Description;

        public new event EventHandler<PropertyOptProgressionEventArgs>? PropertyChanged;

        public class PropertyOptProgressionEventArgs : EventArgs {
            public int Index;
            public string? StepToUUID;
            public string? Description;
            public bool ToDelete = false;
        }

        public PropertyOptProgression () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (int index, string? stepTo, string? desc) {
            Index = index;
            StepToUUID = stepTo;
            Description = desc;

            Label lblProgressionProperty = this.FindControl<Label> ("lblProgressionProperty");
            TextBox txtStepTo = this.FindControl<TextBox> ("txtStepTo");
            TextBox txtDescription = this.FindControl<TextBox> ("txtDescription");

            txtStepTo.Text = StepToUUID;
            txtDescription.Text = Description;

            lblProgressionProperty.Content = String.Format ("Edit Optional Progression To Step #{0:000}", StepToUUID);

            txtStepTo.TextInput += sendPropertyChange;
            txtStepTo.LostFocus += sendPropertyChange;

            txtDescription.TextInput += sendPropertyChange;
            txtDescription.LostFocus += sendPropertyChange;
        }

        private void BtnDelete_Click (object? sender, RoutedEventArgs e) {
            PropertyOptProgressionEventArgs ea = new PropertyOptProgressionEventArgs ();
            ea.Index = Index;
            ea.StepToUUID = StepToUUID;
            ea.ToDelete = true;

            PropertyChanged?.Invoke (this, ea);
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            NumericUpDown numStepTo = this.FindControl<NumericUpDown> ("numStepTo");
            TextBox txtDescription = this.FindControl<TextBox> ("txtDescription");

            PropertyOptProgressionEventArgs ea = new PropertyOptProgressionEventArgs ();
            ea.Index = Index;
            ea.StepToUUID = txtStepTo.Text;
            ea.Description = txtDescription.Text;

            PropertyChanged?.Invoke (this, ea);
        }
    }
}