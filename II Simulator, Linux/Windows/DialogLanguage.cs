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
        private App Instance;
        
        /* GTK GUI Objects */
        private ComboBoxText cmbLanguages = new ComboBoxText ();
        
        public DialogLanguage(App inst) : base(WindowType.Popup) {
            
            this.Titlebar = new HeaderBar ();
            
            Instance = inst;
            
            DeleteEvent += OnClose;
            this.Shown += OnShown;
        }

        private void OnShown (object sender, EventArgs args) {
            InitInterface();
        }
        
        private void OnClose(object sender, DeleteEventArgs a) {
         
        }
        
        private void InitInterface () {
            VBox vbMain = new VBox (false, 10);
            vbMain.BorderWidth = 10;
            
            Label lblChooseLanguage = new Label ($"{Instance.Language.Localize ("LANGUAGE:Select")}");
            lblChooseLanguage.Halign = Align.Center;
            vbMain.PackStart(lblChooseLanguage,false, false, 6);
            
            foreach (string each in II.Localization.Language.Descriptions)
                cmbLanguages.AppendText (each);
            cmbLanguages.Active = Enum.Parse (typeof (II.Localization.Language.Values), Instance.Settings.Language).GetHashCode();
            vbMain.PackStart(cmbLanguages, false, false, 6);
            
            HBox hbButtons = new HBox();
            hbButtons.Halign = Align.Center;
            
            Button btnCancel = new Button(new Label(Instance.Language.Localize ("BUTTON:Cancel")));
            btnCancel.WidthRequest = 150;
            btnCancel.Pressed += OnClick_Cancel;
            
            Button btnContinue = new Button(new Label(Instance.Language.Localize ("BUTTON:Continue")));
            btnContinue.WidthRequest = 150;
            btnContinue.Pressed += OnClick_Continue;
            
            hbButtons.PackStart(btnCancel,false, false, 6);
            hbButtons.PackStart(btnContinue,false, false, 6);
            vbMain.PackStart(hbButtons,false, false, 6);
            
            Add(vbMain);
            Title = Instance.Language.Localize ("LANGUAGE:Title");
            ShowAll();
        }
        
        private void OnClick_Cancel (object sender, EventArgs e) {
            this.Close();
        }

        private void OnClick_Continue (object sender, EventArgs e) {
            if (Instance is not null) {
                
                string language = cmbLanguages.ActiveText;
                int index = cmbLanguages.Active;
                
                Instance.Language.Value = Enum.GetValues<II.Localization.Language.Values> () [cmbLanguages.Active];
                Instance.Settings.Language = Instance.Language.Value.ToString ();
                Instance.Settings.Save ();
            }
            
            // Show messagebox prompting user to restart the program for changes to take effect
            MessageDialog dlgRestart = new MessageDialog (this, 0, MessageType.Info,
                ButtonsType.Ok, false, Instance.Language.Localize ("MESSAGE:RestartForChanges"));
            dlgRestart.Response += (o, args) => { dlgRestart.Destroy (); };
            dlgRestart.Show();
            
            this.Close ();
        }
    }
}