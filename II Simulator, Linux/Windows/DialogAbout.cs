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
using WrapMode = Gtk.WrapMode;

namespace IISIM
{
    class DialogAbout : Window
    {
        private App? Instance;
        
        public DialogAbout(App inst) : base(Gtk.WindowType.Toplevel) {
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
            uint upd = 0;               // Padding: General (uint)
            int mb = 5;
            
            Box hbMain1 = new Box (Orientation.Horizontal, sp);
            hbMain1.BorderWidth = usp;
            
            Image imgIcon = new Image (new Gdk.Pixbuf ("Resources/Icon_Infirmary_128.png", 100, 100, true));
            imgIcon.Halign = Align.Center;
            imgIcon.Valign = Align.Center;
            imgIcon.Margin = 14;
            hbMain1.PackStart(imgIcon, false, false, upd);
            
            Box vbMain2 = new Box (Orientation.Vertical, sp);
            vbMain2.BorderWidth = usp;
            vbMain2.MarginTop = sp;
            vbMain2.MarginBottom = sp;
            vbMain2.MarginEnd = mb;

            Label lblTitle = new Label ();
            lblTitle.UseMarkup = true;
            lblTitle.Markup =
                $"<span size='18pt' weight='bold'>{Instance?.Language.Localize ("II:InfirmaryIntegrated")}</span>";
            lblTitle.Halign = Align.Start;
            lblTitle.MarginBottom = mb;
            
            Label lblVersion = new Label (String.Format (Instance?.Language.Localize ("ABOUT:Version") ?? "",
                Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0"));
            lblVersion.Halign = Align.Start;
            
            Label lblAuthor = new Label ("Ibi Keller, (c) 2017-2026");
            lblAuthor.Halign = Align.Start;
            lblAuthor.MarginBottom = mb;
            
            Label lblDescription = new Label (Instance?.Language.Localize ("ABOUT:Description"));
            lblDescription.Wrap = true;
            lblDescription.LineWrapMode = Pango.WrapMode.Word;
            lblDescription.Halign = Align.Start;
            lblDescription.MarginBottom = mb;
            
            Button btnWebsite = new Button ();
            Label lblWebsite = new Label ();
            lblWebsite.UseMarkup = true;
            lblWebsite.Markup =
                $"<span foreground='blue' underline='single'>https://www.infirmary-integrated.com</span>";
            btnWebsite.Halign = Align.Center;
            btnWebsite.Child = lblWebsite;
            btnWebsite.Relief = ReliefStyle.None;
            
            btnWebsite.Pressed += delegate (object? sender, EventArgs e) {
                Process.Start (new ProcessStartInfo ("xdg-open", 
                    "http://www.infirmary-integrated.com/".Replace ("&", "^&")) { CreateNoWindow = true });
            };
            
            Button btnRepo = new Button ();
            Label lblRepo = new Label ();
            lblRepo.UseMarkup = true;
            lblRepo.Markup =
                $"<span foreground='blue' underline='single'>https://github.com/tanjera/infirmary-integrated</span>";
            btnRepo.Halign =  Align.Center;
            btnRepo.Child = lblRepo;
            btnRepo.Relief = ReliefStyle.None;
            
            btnRepo.Pressed += delegate (object? sender, EventArgs e) {
                Process.Start (new ProcessStartInfo ("xdg-open", 
                    "https://github.com/tanjera/infirmary-integrated".Replace ("&", "^&")) { CreateNoWindow = true });
            };
            
            vbMain2.PackStart(lblTitle,false, false, upd);
            vbMain2.PackStart(lblVersion,false, false, upd);
            vbMain2.PackStart(lblAuthor,false, false, upd);
            vbMain2.PackStart(lblDescription,false, false, upd);
            vbMain2.PackStart(btnWebsite,false, false, 0);
            vbMain2.PackStart(btnRepo,true, false, 0);
            
            hbMain1.PackStart(vbMain2, false, false, upd);
            
            Add(hbMain1);

            HeaderBar hb = new HeaderBar () {
                ShowCloseButton = true,
                Title = Instance?.Language.Localize ("ABOUT:AboutProgram")
            };
            this.Titlebar = hb;
            
            ShowAll();
        }
    }
}