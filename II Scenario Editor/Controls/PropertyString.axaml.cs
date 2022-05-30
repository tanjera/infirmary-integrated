using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;

namespace II_Scenario_Editor.Controls {

    public partial class PropertyString : UserControl {
        public Keys Key;

        public enum Keys {
            ScenarioAuthor,
            ScenarioName,
            ScenarioDescription,
            StepName,
            StepDescription
        }

        public new event EventHandler<PropertyStringEventArgs>? PropertyChanged;

        public class PropertyStringEventArgs : EventArgs {
            public Keys Key;
            public string? Value;
        }

        public PropertyString () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (Keys key) {
            Key = key;

            Label lblKey = this.FindControl<Label> ("lblKey");
            TextBox txtValue = this.FindControl<TextBox> ("txtValue");

            switch (Key) {
                default: break;
                case Keys.ScenarioAuthor: lblKey.Content = "Author: "; break;
                case Keys.ScenarioName: lblKey.Content = "Title: "; break;
                case Keys.ScenarioDescription: lblKey.Content = "Description: "; break;
                case Keys.StepName: lblKey.Content = "Name: "; break;
                case Keys.StepDescription: lblKey.Content = "Description: "; break;
            }

            txtValue.TextInput += sendPropertyChange;
            txtValue.LostFocus += sendPropertyChange;
        }

        public void Set (string value) {
            TextBox txtValue = this.FindControl<TextBox> ("txtValue");

            txtValue.TextInput -= sendPropertyChange;
            txtValue.Text = value;
            txtValue.TextInput += sendPropertyChange;
        }

        private void sendPropertyChange (object? sender, EventArgs e) {
            TextBox txtValue = this.FindControl<TextBox> ("txtValue");

            PropertyStringEventArgs ea = new PropertyStringEventArgs ();
            ea.Key = Key;
            ea.Value = txtValue?.Text ?? "";

            PropertyChanged?.Invoke (this, ea);
        }
    }
}