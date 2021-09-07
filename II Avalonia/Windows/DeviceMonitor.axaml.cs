using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using II;
using II.Rhythm;
using II.Waveform;

namespace II_Avalonia {

    public partial class DeviceMonitor : Window {

        public DeviceMonitor () {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
            //TODO Repopulate Code
        }
    }
}