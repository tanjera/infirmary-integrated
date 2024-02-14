using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using II;

namespace IISE.Controls {

    public partial class PropertyAllergy : UserControl {
        private bool isInitiated = false;

        public new event EventHandler<PropertyAllergyEventArgs>? PropertyChanged;

        public class PropertyAllergyEventArgs : EventArgs {
            public II.Allergy Allergy = new ();
        }

        public PropertyAllergy () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (II.Allergy allergy) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyAllergyEventArgs>)d;
            }

            TextBox? ptxtAllergen = this.FindControl<TextBox> ("txtAllergen");
            TextBox? ptxtReaction = this.FindControl<TextBox> ("txtReaction");
            ComboBox? pcmbIntensity = this.FindControl<ComboBox> ("cmbIntensity");

            // Populate enum string lists for readable display
            List<string> intensities = new List<string> ();

            if (App.Language != null) {
                foreach (var v in Enum.GetValues<II.Scales.Intensity.Values> ())
                    intensities.Add (App.Language.Dictionary [II.Scales.Intensity.LookupString (v)]);
            }

            pcmbIntensity.Items = intensities;

            ptxtAllergen.Text = allergy.Allergen;
            ptxtReaction.Text = allergy.Reaction;
            pcmbIntensity.SelectedIndex = allergy.Intensity.GetHashCode ();

            if (!isInitiated) {
                ptxtAllergen.TextInput += SendPropertyChange;
                ptxtAllergen.LostFocus += SendPropertyChange;

                ptxtReaction.TextInput += SendPropertyChange;
                ptxtReaction.LostFocus += SendPropertyChange;

                pcmbIntensity.SelectionChanged += SendPropertyChange;
            }

            isInitiated = true;
        }

        private void UpdateView (II.Allergy allergy) {
            TextBox? ptxtAllergen = this.FindControl<TextBox> ("txtAllergen");
            TextBox? ptxtReaction = this.FindControl<TextBox> ("txtReaction");
            ComboBox? pcmbIntensity = this.FindControl<ComboBox> ("cmbIntensity");

            // Nothing to do... keep this in case that changes in the future
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            TextBox? ptxtAllergen = this.FindControl<TextBox> ("txtAllergen");
            TextBox? ptxtReaction = this.FindControl<TextBox> ("txtReaction");
            ComboBox? pcmbIntensity = this.FindControl<ComboBox> ("cmbIntensity");

            PropertyAllergyEventArgs ea = new PropertyAllergyEventArgs ();

            ea.Allergy.Allergen = ptxtAllergen.Text;
            ea.Allergy.Reaction = ptxtReaction.Text;
            ea.Allergy.Intensity = Enum.GetValues<II.Scales.Intensity.Values> () [
                pcmbIntensity.SelectedIndex < 0 ? 0 : pcmbIntensity.SelectedIndex];

            Debug.WriteLine ($"PropertyChanged: Allergy");

            PropertyChanged?.Invoke (this, ea);

            UpdateView (ea.Allergy);
        }

        private void SendPropertyChange (object? sender, DatePickerSelectedValueChangedEventArgs e)
            => SendPropertyChange (sender, new EventArgs ());

        private void SendPropertyChange (object? sender, TimePickerSelectedValueChangedEventArgs e)
            => SendPropertyChange (sender, new EventArgs ());
    }
}