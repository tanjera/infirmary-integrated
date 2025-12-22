using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Cairo;
using Gtk;
using Pango;

using II;
using II.Localization;
using II.Server;

namespace IISIM
{
    class DialogMirrorReceive : Window
    {
        private App? Instance;
        
        /* GTK GUI Objects */
        private Entry tbAccessionKey = new Entry ();
        private Entry tbAccessPassword = new Entry ();
        
        public DialogMirrorReceive(App inst) : base(WindowType.Toplevel) {
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

            tbAccessionKey.IsEditable = true;
            
            Box vbMain1 = new Box (Orientation.Vertical, sp) {
                BorderWidth = usp,
                MarginBottom = 2
            };
            
            Label txtMessage = new Label (Instance?.Language.Localize ("MIRROR:EnterSettings")) {
                Halign = Align.Center
            };
            vbMain1.PackStart(txtMessage,false, false, upd);
            
            Box hbMain2 = new Box(Orientation.Horizontal, sp) {
                Halign = Align.Center,
                MarginEnd = 6
            };

            Image imgClipboard = new Image (new Gdk.Pixbuf ("Third_Party/Icon_Clipboard_128.png", 48, 48, true)) {
                Halign = Align.Center,
                Valign = Align.Center,
                Margin = 6
            };
            hbMain2.PackStart(imgClipboard, false, false, upd);

            Grid gdKeys = new Grid () {
                RowSpacing = usp,
                ColumnSpacing = usp
            };

            Label lblAccessionKey = new Label (Instance.Language.Localize ("MIRROR:AccessionKey")) {
                Halign = Align.Start
            };
            
            gdKeys.Attach (lblAccessionKey, 0, 0, 1, 1);
            gdKeys.Attach (tbAccessionKey, 1, 0, 5, 1);
            
            Label lblAccessPassword = new Label (Instance.Language.Localize ("MIRROR:AccessPassword")) {
                Halign = Align.Start
            };
            
            gdKeys.Attach (lblAccessPassword, 0, 1, 1, 1);
            gdKeys.Attach (tbAccessPassword, 1, 1, 5, 1);
            
            hbMain2.PackStart(gdKeys,false, false, tlepd);
            vbMain1.PackStart(hbMain2,false, false, tlepd);

            Box hbButtons = new Box (Orientation.Horizontal, sp) {
                Halign = Align.Center
            };
            
            Button btnCancel = new Button(new Label(Instance.Language.Localize ("BUTTON:Cancel"))) {
                WidthRequest = 150
            };
            btnCancel.Pressed += OnClick_Cancel;
            
            Button btnContinue = new Button(new Label(Instance.Language.Localize ("BUTTON:Continue"))) {
                WidthRequest = 150
            };
            btnContinue.Pressed += OnClick_Continue;
            
            hbButtons.PackStart(btnCancel,false, false, upd);
            hbButtons.PackStart(btnContinue,false, false, upd);
            
            vbMain1.PackStart(hbButtons,false, false, tlepd);
            
            Add(vbMain1);
            
            HeaderBar hb = new HeaderBar() {
                ShowCloseButton = false,
                Title = Instance?.Language.Localize ("MIRROR:ReceiveTitle")
            };
            this.Titlebar = hb;

            ShowAll();
        }
        
        private void OnClick_Cancel (object sender, EventArgs e) {
            this.Close();
        }

        private void OnClick_Continue (object sender, EventArgs e) {
            if (Instance is not null) {


                Regex regex = new("^[a-zA-Z0-9]*$");
                if ((tbAccessionKey.Text ?? "").Length > 0
                    && regex.IsMatch (tbAccessionKey.Text ?? "")) {
                    Instance.Mirror.Status = II.Server.Mirror.Statuses.CLIENT;
                    Instance.Mirror.Accession = tbAccessionKey.Text ?? "";
                    Instance.Mirror.PasswordAccess = tbAccessPassword.Text ?? "";

                    this.Close ();
                } else {
                    MessageDialog dlgSettingsInvalid = new MessageDialog (this, 0, MessageType.Error,
                        ButtonsType.Ok, false, Instance.Language.Localize ("MIRROR:SettingsInvalid"));
                    dlgSettingsInvalid.Response += (o, args) => { dlgSettingsInvalid.Destroy (); };
                    dlgSettingsInvalid.Show();
                    
                    this.Close ();
                }
            }

            this.Destroy ();
        }
    }
}