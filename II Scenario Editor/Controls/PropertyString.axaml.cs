using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISE.Controls {

    public partial class PropertyString : UserControl {
        private bool isInitiated = false;

        public Keys Key;

        public enum Keys {
            ScenarioAuthor,
            ScenarioName,
            ScenarioDescription,

            StepName,
            StepDescription,

            DemographicsName,
            DemographicsMRN,
            DemographicsHomeAddress,
            DemographicsTelephoneNumber,
            DemographicsInsuranceProvider,
            DemographicsInsuranceAccount,

            DoseComment
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

        public Task Init (Keys key) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyStringEventArgs>)d;
            }

            Label lblKey = this.GetControl<Label> ("lblKey");
            TextBox txtValue = this.GetControl<TextBox> ("txtValue");

            Key = key;
            switch (Key) {
                default: break;
                case Keys.ScenarioAuthor: lblKey.Content = "Author: "; break;
                case Keys.ScenarioName: lblKey.Content = "Title: "; break;
                case Keys.ScenarioDescription: lblKey.Content = "Description: "; break;

                case Keys.StepName: lblKey.Content = "Name: "; break;
                case Keys.StepDescription: lblKey.Content = "Description: "; break;

                case Keys.DemographicsName: lblKey.Content = "Name: "; break;
                case Keys.DemographicsMRN: lblKey.Content = "Medical Record Number: "; break;
                case Keys.DemographicsHomeAddress: lblKey.Content = "Home Address: "; break;
                case Keys.DemographicsTelephoneNumber: lblKey.Content = "Telephone Number: "; break;
                case Keys.DemographicsInsuranceProvider: lblKey.Content = "Insurance Provider: "; break;
                case Keys.DemographicsInsuranceAccount: lblKey.Content = "Insurance Account: "; break;

                case Keys.DoseComment: lblKey.Content = "Comment: "; break;
            }

            if (!isInitiated) {
                txtValue.KeyUp += SendPropertyChange;
                txtValue.LostFocus += SendPropertyChange;
            }

            isInitiated = true;

            return Task.CompletedTask;
        }

        public Task Set (string value) {
            TextBox txtValue = this.GetControl<TextBox> ("txtValue");

            txtValue.TextInput -= SendPropertyChange;
            txtValue.Text = value;
            txtValue.TextInput += SendPropertyChange;

            return Task.CompletedTask;
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            TextBox txtValue = this.GetControl<TextBox> ("txtValue");

            PropertyStringEventArgs ea = new PropertyStringEventArgs ();
            ea.Key = Key;
            ea.Value = txtValue?.Text ?? "";

            Debug.WriteLine ($"PropertyChanged: {ea.Key} '{ea.Value}'");
            PropertyChanged?.Invoke (this, ea);
        }
    }
}