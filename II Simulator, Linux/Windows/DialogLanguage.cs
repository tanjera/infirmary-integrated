using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Cairo;
using Gtk;
using Pango;

using II;
using II.Localization;
using II.Server;

namespace IISIM
{
    class DialogLanguage : Window
    {
        private App? Instance;
        
        /* GTK GUI Objects */
        private ComboBoxText cmbLanguages = new ComboBoxText ();
        
        public DialogLanguage(App inst) : base(WindowType.Toplevel) {
            Instance = inst;
            
            DeleteEvent += OnClose;
            this.Shown += OnShown;
        }

        private void OnShown (object? sender, EventArgs args) {
            InitInterface();
        }
        
        private void OnClose(object? sender, DeleteEventArgs a) {
         
        }
        
        private void InitInterface () {
            int sp = 2;                 // Spacing: General (int)
            uint usp = 3;               // Spacing: General (uint)
            uint upd = 5;               // Padding: General (uint)
            uint tlepd = 2;             // Padding: Top-level elements
            
            Box vbMain = new Box (Orientation.Vertical, sp);
            vbMain.BorderWidth = usp;
            
            Label lblChooseLanguage = new Label ($"{Instance?.Language.Localize ("LANGUAGE:Select")}") {
                Halign = Align.Center
            };
            vbMain.PackStart(lblChooseLanguage,false, false, upd);

            cmbLanguages = new ComboBoxText () {
                MarginStart = 6,
                MarginEnd = 6
            };
            
            foreach (string each in II.Localization.Language.Descriptions)
                cmbLanguages.AppendText (each);
            
            cmbLanguages.Active = Enum.Parse (typeof (II.Localization.Language.Values), Instance?.Settings.Language ?? "ENG").GetHashCode();
            vbMain.PackStart(cmbLanguages, false, false, upd);
            
            Box hbButtons = new Box(Orientation.Horizontal, sp) {
                Halign = Align.Center
            };
            
            Button btnCancel = new Button(new Label(Instance?.Language.Localize ("BUTTON:Cancel"))) {
                WidthRequest = 150
            };
            btnCancel.Pressed += OnClick_Cancel;
            
            Button btnContinue = new Button(new Label(Instance?.Language.Localize ("BUTTON:Continue"))) {
                WidthRequest = 150
            };
            btnContinue.Pressed += OnClick_Continue;
            
            hbButtons.PackStart(btnCancel,false, false, upd);
            hbButtons.PackStart(btnContinue,false, false, upd);
            vbMain.PackStart(hbButtons,false, false, tlepd);
            
            Add(vbMain);

            HeaderBar hb = new HeaderBar () {
                ShowCloseButton = false,
                Title = Instance?.Language.Localize ("LANGUAGE:Title")
            };
            this.Titlebar = hb;

            ShowAll();
        }
        
        private void OnClick_Cancel (object? sender, EventArgs e) {
            this.Close();
        }

        private void OnClick_Continue (object? sender, EventArgs e) {
            if (Instance is not null) {
                string language = cmbLanguages.ActiveText;
                int index = cmbLanguages.Active;
                
                Instance.Language.Value = Enum.GetValues<II.Localization.Language.Values> () [cmbLanguages.Active];
                Instance.Settings.Language = Instance.Language.Value.ToString ();
                Instance.Settings.Save ();
            }
            
            // Show messagebox prompting user to restart the program for changes to take effect
            MessageDialog dlgRestart = new MessageDialog (this, 0, MessageType.Info,
                ButtonsType.Ok, false, Instance?.Language.Localize ("MESSAGE:RestartForChanges"));
            dlgRestart.Response += (o, args) => { dlgRestart.Destroy (); };
            dlgRestart.Show();
            
            this.Close ();
            this.Destroy ();
        }
    }
}