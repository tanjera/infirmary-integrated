using System;
using System.IO;
using Cairo;
using Gtk;
using Pango;

namespace IISIM
{
    class Control : Window
    {
        private App Instance;
        
        public Control(App inst) : base("Infirmary Integrated") {
            Instance = inst;

            DeleteEvent += Window_DeleteEvent;
            this.Shown += Init;

            SetDefaultSize(640, 480);

            HBox hboxMain = new HBox (false, 10);
            hboxMain.BorderWidth = 10;
            
            VBox vboxDevices = new VBox (false, 10);
            
            Label labelDevices = new Label ();
            labelDevices.Halign = Align.Start;
            labelDevices.UseMarkup = true;
            labelDevices.Markup = "<span size='12pt' weight='bold'>Devices</span>";
            vboxDevices.PackStart (labelDevices, false, false, 6);
            
            HBox hboxDeviceCardiacMonitor = new HBox (false, 6);
            hboxDeviceCardiacMonitor.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceMonitor_128.png", 40, 40, true))
                , false, false, 0);
            hboxDeviceCardiacMonitor.PackStart(new Label("Cardiac Monitor"), false, false, 6);
            
            HBox hboxDeviceDefibrillator = new HBox (false, 6);
            hboxDeviceDefibrillator.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceDefibrillator_128.png", 40, 40, true))
                , false, false, 0);
            hboxDeviceDefibrillator.PackStart(new Label("Defibrillator"), false, false, 6);
            
            HBox hbox12LeadECG = new HBox (false, 6);
            hbox12LeadECG.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Device12LeadECG_128.png", 40, 40, true))
                , false, false, 0);
            hbox12LeadECG.PackStart(new Label("12 Lead ECG"), false, false, 6);
            
            vboxDevices.PackStart (hboxDeviceCardiacMonitor, false, false, 0);
            vboxDevices.PackStart (hboxDeviceDefibrillator, false, false, 0);
            vboxDevices.PackStart (hbox12LeadECG, false, false, 0);
            
            
            Table tableParameters = new Table (10, 4, true);
            tableParameters.RowSpacing = 4;
            tableParameters.ColumnSpacing = 4;
            tableParameters.Halign = Align.Fill;
            tableParameters.Valign = Align.Start;
            
            Label labelHR = new Label ("Heart Rate");
            labelHR.Halign = Align.Start;
            tableParameters.Attach (labelHR, 0, 1, 0, 1);
            tableParameters.Attach (new SpinButton (0, 300, 5), 1, 4, 0, 1);
            
            Label labelNIBP = new Label ("Blood Pressure");
            labelNIBP.Halign = Align.Start;
            tableParameters.Attach (labelNIBP, 0, 1, 1, 2);
            tableParameters.Attach (new SpinButton (0, 300, 5), 1, 2, 1, 2);
            tableParameters.Attach (new Label("/"), 2, 3, 1, 2);
            tableParameters.Attach (new SpinButton (0, 300, 5), 3, 4, 1, 2);

            Label labelRR = new Label ("Respiratory Rate");
            labelRR.Halign = Align.Start;
            tableParameters.Attach (labelRR, 0, 1, 2, 3);
            tableParameters.Attach (new SpinButton (0, 100, 2), 1, 4, 2, 3);

            hboxMain.PackStart (vboxDevices, false, false, 10);
            hboxMain.PackStart (new Separator(Orientation.Vertical), false, false, 0);

            Label labelVitalSigns = new Label ();
            labelVitalSigns.UseMarkup = true;
            labelVitalSigns.Markup = "<span size='12pt' weight='bold'>    Vital Signs</span>";
            labelVitalSigns.HeightRequest = 34;
            Expander expVitalSigns = new Expander ("Test");
            expVitalSigns.LabelWidget = labelVitalSigns;
            expVitalSigns.Add(tableParameters);
            expVitalSigns.Expanded = true;
            
            hboxMain.PackStart (expVitalSigns, false, false, 10);
            
            Add (hboxMain);
            ShowAll ();
        }

        private void Init (object sender, EventArgs args) {
            
        }
        
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}