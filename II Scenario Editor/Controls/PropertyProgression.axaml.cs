using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyOptProgression : UserControl {
        public int Index;
        public int IndexStepTo;
        public string? Description;

        public new event EventHandler<PropertyOptProgressionEventArgs>? PropertyChanged;

        public class PropertyOptProgressionEventArgs : EventArgs {
            public int Index;
            public int IndexStepTo;
            public string? Description;
            public bool ToDelete = false;
        }

        public PropertyOptProgression () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (int index, int stepTo, string desc) {
            Index = index;
            IndexStepTo = stepTo;
            Description = desc;

            numStepTo.Value = IndexStepTo;
            txtDescription.Text = Description;

            lblProgressionProperty.Content = String.Format ("Edit Optional Progression To Step #{0:000}", IndexStepTo);

            numStepTo.ValueChanged += sendPropertyChange;
            numStepTo.LostFocus += sendPropertyChange;

            txtDescription.TextInput += sendPropertyChange;
            txtDescription.LostFocus += sendPropertyChange;
        }

        private void BtnDelete_Click (object? sender, RoutedEventArgs e) {
            PropertyOptProgressionEventArgs ea = new PropertyOptProgressionEventArgs ();
            ea.Index = Index;
            ea.IndexStepTo = IndexStepTo;
            ea.ToDelete = true;

            PropertyChanged?.Invoke (this, ea);
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            PropertyOptProgressionEventArgs ea = new PropertyOptProgressionEventArgs ();
            ea.Index = Index;
            ea.IndexStepTo = (int)(numStepTo.Value);
            ea.Description = txtDescription.Text;

            PropertyChanged?.Invoke (this, ea);
        }
    }
}