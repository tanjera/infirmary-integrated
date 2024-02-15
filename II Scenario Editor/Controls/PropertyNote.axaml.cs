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

    public partial class PropertyNote : UserControl {
        private bool isInitiated = false;

        public new event EventHandler<PropertyNoteEventArgs>? PropertyChanged;

        public class PropertyNoteEventArgs : EventArgs {
            public II.Record.Note Note = new ();
        }

        public PropertyNote () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Init (II.Record.Note note) {
            if (PropertyChanged != null) {              // In case of re-initiation, need to wipe all subscriptions
                foreach (Delegate d in PropertyChanged.GetInvocationList ())
                    PropertyChanged -= (EventHandler<PropertyNoteEventArgs>)d;
            }

            DatePicker pdpDate = this.FindControl<DatePicker> ("dpDate");
            TimePicker ptpTime = this.FindControl<TimePicker> ("tpTime");
            TextBox? ptxtTitle = this.FindControl<TextBox> ("txtTitle");
            TextBox? ptxtAuthor = this.FindControl<TextBox> ("txtAuthor");
            TextBox? ptxtContent = this.FindControl<TextBox> ("txtContent");

            if (note.Timestamp is not null) {
                pdpDate.SelectedDate = new DateTimeOffset (note.Timestamp.Value);
                ptpTime.SelectedTime = new TimeSpan (note.Timestamp.Value.Hour, note.Timestamp.Value.Minute, 0);
            }

            ptxtTitle.Text = note.Title;
            ptxtAuthor.Text = note.Author;
            ptxtContent.Text = note.Content;

            if (!isInitiated) {
                pdpDate.SelectedDateChanged += SendPropertyChange;
                pdpDate.LostFocus += SendPropertyChange;

                ptpTime.SelectedTimeChanged += SendPropertyChange;
                ptpTime.LostFocus += SendPropertyChange;

                ptxtTitle.TextInput += SendPropertyChange;
                ptxtTitle.LostFocus += SendPropertyChange;

                ptxtAuthor.TextInput += SendPropertyChange;
                ptxtAuthor.LostFocus += SendPropertyChange;

                ptxtContent.TextInput += SendPropertyChange;
                ptxtContent.LostFocus += SendPropertyChange;
            }

            isInitiated = true;
        }

        private void UpdateView (II.Record.Note note) {
            DatePicker pdpDate = this.FindControl<DatePicker> ("dpDate");
            TimePicker ptpTime = this.FindControl<TimePicker> ("tpTime");
            TextBox? ptxtTitle = this.FindControl<TextBox> ("txtTitle");
            TextBox? ptxtAuthor = this.FindControl<TextBox> ("txtAuthor");
            TextBox? ptxtContent = this.FindControl<TextBox> ("txtContent");

            // Nothing to do... keep this in case that changes in the future
        }

        private void SendPropertyChange (object? sender, EventArgs e) {
            DatePicker pdpDate = this.FindControl<DatePicker> ("dpDate");
            TimePicker ptpTime = this.FindControl<TimePicker> ("tpTime");
            TextBox? ptxtTitle = this.FindControl<TextBox> ("txtTitle");
            TextBox? ptxtAuthor = this.FindControl<TextBox> ("txtAuthor");
            TextBox? ptxtContent = this.FindControl<TextBox> ("txtContent");

            PropertyNoteEventArgs ea = new PropertyNoteEventArgs ();

            ea.Note.Timestamp = new DateTime (
                pdpDate?.SelectedDate?.Year ?? new DateTime ().Year,
                pdpDate?.SelectedDate?.Month ?? new DateTime ().Month,
                pdpDate?.SelectedDate?.Day ?? new DateTime ().Day,
                ptpTime?.SelectedTime?.Hours ?? new DateTime ().Hour,
                ptpTime?.SelectedTime?.Minutes ?? new DateTime ().Minute,
                0);

            ea.Note.Title = ptxtTitle.Text;
            ea.Note.Author = ptxtAuthor.Text;
            ea.Note.Content = ptxtContent.Text;

            Debug.WriteLine ($"PropertyChanged: Note");

            PropertyChanged?.Invoke (this, ea);

            UpdateView (ea.Note);
        }

        private void SendPropertyChange (object? sender, DatePickerSelectedValueChangedEventArgs e) {
            SendPropertyChange (sender, new EventArgs ());
        }

        private void SendPropertyChange (object? sender, TimePickerSelectedValueChangedEventArgs e) {
            SendPropertyChange (sender, new EventArgs ());
        }
    }
}