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
    class DialogMirrorBroadcast : Window
    {
        private App? Instance;
        
        /* GTK GUI Objects */
        private Entry tbAccessionKey = new Entry ();
        private Entry tbAccessPassword = new Entry ();
        private Entry tbAdminPassword = new Entry ();
        
        public DialogMirrorBroadcast(App inst) : base(WindowType.Toplevel) {
            
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
            int sp = 2;                 // Spacing: General (int)
            uint usp = 3;               // Spacing: General (uint)
            uint upd = 5;               // Padding: General (uint)
            uint tlepd = 2;             // Padding: Top-level elements

            tbAccessionKey.IsEditable = true;
            
            VBox vbMain1 = new VBox (false, sp);
            vbMain1.BorderWidth = usp;
            vbMain1.MarginBottom = 2;
            
            Label txtMessage = new Label (Instance.Language.Localize ("MIRROR:EnterSettings"));
            txtMessage.Halign = Align.Center;
            vbMain1.PackStart(txtMessage,false, false, upd);
            
            HBox hbMain2 = new HBox();
            hbMain2.Halign = Align.Center;
            hbMain2.MarginEnd = 6;

            Image imgClipboard = new Image (new Gdk.Pixbuf ("Third_Party/Icon_Clipboard_128.png", 48, 48, true));
            imgClipboard.Halign = Align.Center;
            imgClipboard.Valign = Align.Center;
            imgClipboard.Margin = 6;
            hbMain2.PackStart(imgClipboard, false, false, upd);

            Grid gdKeys = new Grid ();
            
            gdKeys.RowSpacing = usp;
            gdKeys.ColumnSpacing = usp;

            Label lblAccessionKey = new Label (Instance.Language.Localize ("MIRROR:AccessionKey"));
            lblAccessionKey.Halign = Align.Start;
            
            Button btnGenAccessionKey = new Button ();
            btnGenAccessionKey.Label = "+";
            btnGenAccessionKey.WidthRequest = 30;
            btnGenAccessionKey.Pressed += (sender, args) => {
                tbAccessionKey.Text = Utility.RandomString (8);
            };

            gdKeys.Attach (lblAccessionKey, 0, 0, 1, 1);
            gdKeys.Attach (tbAccessionKey, 1, 0, 4, 1);
            gdKeys.Attach (btnGenAccessionKey, 5, 0, 1, 1);

            Label lblAccessPassword = new Label (Instance.Language.Localize ("MIRROR:AccessPassword"));
            lblAccessPassword.Halign = Align.Start;
            
            Button btnGenAccessPassword = new Button ();
            btnGenAccessPassword.Label = "+";
            btnGenAccessPassword.WidthRequest = 30;
            btnGenAccessPassword.Pressed += (sender, args) => {
                tbAccessPassword.Text = Utility.RandomString (8);
            };

            gdKeys.Attach (lblAccessPassword, 0, 1, 1, 1);
            gdKeys.Attach (tbAccessPassword, 1, 1, 4, 1);
            gdKeys.Attach (btnGenAccessPassword, 5, 1, 1, 1);
            
            Label lblAdminPassword = new Label (Instance.Language.Localize ("MIRROR:AdminPassword"));
            lblAccessPassword.Halign = Align.Start;

            gdKeys.Attach (lblAdminPassword, 0, 2, 1, 1);
            gdKeys.Attach (tbAdminPassword, 1, 2, 5, 1);
            
            hbMain2.PackStart(gdKeys,false, false, tlepd);
            vbMain1.PackStart(hbMain2,false, false, tlepd);
            
            HBox hbButtons = new HBox ();
            hbButtons.Halign = Align.Center;
            
            Button btnCancel = new Button(new Label(Instance.Language.Localize ("BUTTON:Cancel")));
            btnCancel.WidthRequest = 150;
            btnCancel.Pressed += OnClick_Cancel;
            
            Button btnContinue = new Button(new Label(Instance.Language.Localize ("BUTTON:Continue")));
            btnContinue.WidthRequest = 150;
            btnContinue.Pressed += OnClick_Continue;
            
            hbButtons.PackStart(btnCancel,false, false, upd);
            hbButtons.PackStart(btnContinue,false, false, upd);
            
            vbMain1.PackStart(hbButtons,false, false, tlepd);
            
            Add(vbMain1);
            Title = Instance.Language.Localize ("MIRROR:BroadcastTitle");
            ShowAll();
        }
        
        private void OnClick_Cancel (object sender, EventArgs e) {
            this.Close();
        }

        private void OnClick_Continue (object sender, EventArgs e) {
            if (Instance is not null) {
                Regex regex = new ("^[a-zA-Z0-9]*$");
                if ((tbAccessionKey.Text ?? "").Length > 0
                    && regex.IsMatch (tbAccessionKey.Text ?? "")) {
                    Instance.Mirror.Status = II.Server.Mirror.Statuses.HOST;
                    Instance.Mirror.Accession = tbAccessionKey.Text ?? "";
                    Instance.Mirror.PasswordAccess = tbAccessPassword.Text ?? "";
                    Instance.Mirror.PasswordEdit = tbAdminPassword.Text ?? "";

                    this.Close ();
                } else {
                    MessageDialog dlgLoadFail = new MessageDialog (this, 0, MessageType.Error,
                        ButtonsType.Ok, false, Instance.Language.Localize ("MIRROR:SettingsInvalid"));
                    dlgLoadFail.Response += (o, args) => { dlgLoadFail.Destroy (); };
                    dlgLoadFail.Show();
                }
            }
            
            this.Close ();
        }
    }
}