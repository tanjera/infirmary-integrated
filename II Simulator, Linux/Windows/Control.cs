using System;
using System.Collections.Generic;
using System.IO;
using Cairo;
using Gtk;
using Pango;

using II;
using II.Localization;
using II.Server;

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
            
            /* GUI: Devices Section */
            
            Label lblDevices = new Label ();
            lblDevices.Halign = Align.Start;
            lblDevices.UseMarkup = true;
            lblDevices.Markup = "<span size='12pt' weight='bold'>Devices</span>";
            vboxDevices.PackStart (lblDevices, false, false, 6);
            
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
            
            /* GUI: Parameters */
            
            Table tableParameters = new Table (10, 4, true);
            tableParameters.RowSpacing = 4;
            tableParameters.ColumnSpacing = 4;
            tableParameters.Halign = Align.Fill;
            tableParameters.Valign = Align.Start;
            
            /* GUI: Parameters: Vital Signs */
     
            Label lblHR = new Label ($"{Instance.Language.Localize ("PE:HeartRate")}:");
            lblHR.Halign = Align.Start;
            tableParameters.Attach (lblHR, 0, 1, 0, 1);
            tableParameters.Attach (new SpinButton (0, 300, 5), 1, 4, 0, 1);
            
            Label lblNIBP = new Label ($"{Instance.Language.Localize ("PE:BloodPressure")}:");
            lblNIBP.Halign = Align.Start;
            tableParameters.Attach (lblNIBP, 0, 1, 1, 2);
            tableParameters.Attach (new SpinButton (0, 300, 5), 1, 2, 1, 2);
            tableParameters.Attach (new Label("/"), 2, 3, 1, 2);
            tableParameters.Attach (new SpinButton (0, 300, 5), 3, 4, 1, 2);

            Label lblRR = new Label ($"{Instance.Language.Localize ("PE:RespiratoryRate")}:");
            lblRR.Halign = Align.Start;
            tableParameters.Attach (lblRR, 0, 1, 2, 3);
            tableParameters.Attach (new SpinButton (0, 100, 2), 1, 4, 2, 3);

            Label lblSPO2 = new Label ($"{Instance.Language.Localize ("PE:PulseOximetry")}:");
            lblSPO2.Halign = Align.Start;
            tableParameters.Attach (lblSPO2, 0, 1, 3, 4);
            tableParameters.Attach (new SpinButton (0, 100, 2), 1, 4, 3, 4);
            
            Label lblT = new Label ($"{Instance.Language.Localize ("PE:Temperature")}:");
            lblT.Halign = Align.Start;
            tableParameters.Attach (lblT, 0, 1, 4, 5);
            tableParameters.Attach (new SpinButton (0, 100, 2), 1, 4, 4, 5);

            Label lblCardiacRhythm = new Label ($"{Instance.Language.Localize ("PE:CardiacRhythm")}:");
            lblCardiacRhythm.Halign = Align.Start;
            tableParameters.Attach (lblCardiacRhythm, 0, 1, 5, 6);

            List<string> listCardiacRhythm = new List<string> ();
            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof(Cardiac_Rhythms.Values)))
                listCardiacRhythm.Add (Instance.Language.Localize (Cardiac_Rhythms.LookupString (v)));
            ComboBox cmbCardiacRhythm = new ComboBox(listCardiacRhythm.ToArray ());
            tableParameters.Attach (cmbCardiacRhythm, 1, 4, 5, 6);
            
            CheckButton chkDefaultVitals = new CheckButton (Instance.Language.Localize ("PE:UseDefaultVitalSignRanges"));
            tableParameters.Attach (chkDefaultVitals, 0, 4, 6, 7);
            
            
            hboxMain.PackStart (vboxDevices, false, false, 10);
            hboxMain.PackStart (new Separator(Orientation.Vertical), false, false, 0);

            Label lblVitalSigns = new Label ();
            lblVitalSigns.UseMarkup = true;
            lblVitalSigns.Markup = $"<span size='12pt' weight='bold'>  {Instance.Language.Localize ("PE:VitalSigns")}</span>";
            lblVitalSigns.HeightRequest = 34;
            Expander expVitalSigns = new Expander ("Test");
            expVitalSigns.LabelWidget = lblVitalSigns;
            expVitalSigns.Add(tableParameters);
            expVitalSigns.Expanded = true;
            
            hboxMain.PackStart (expVitalSigns, false, false, 10);
            
            
            /* GUI: Assemble... */
            
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