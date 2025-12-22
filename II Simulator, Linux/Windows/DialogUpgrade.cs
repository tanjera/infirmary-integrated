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
    class DialogUpgrade : Window
    {
        private App? Instance;
        
        public enum UpgradeOptions {
            None,
            Website,
            Delay,
            Mute
        }
        
        public event EventHandler<UpgradeEventArgs>? OnUpgradeRoute;

        public class UpgradeEventArgs : EventArgs {
            public UpgradeOptions Route;

            public UpgradeEventArgs (UpgradeOptions d)
                => Route = d;
        }

        
        public DialogUpgrade(App inst) : base(Gtk.WindowType.Toplevel) {
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
            
            Box bxMain1 = new Box (Orientation.Vertical, sp) {
                BorderWidth = usp,
                Margin = 6
            };
            
            Label lblUpgradeAvailable = new Label (Instance?.Language.Localize ("UPGRADE:UpdateAvailable")) {
                Wrap = true,
                Justify = Justification.Center,
                MarginStart = 8,
                MarginEnd = 8
            };
            bxMain1.PackStart (lblUpgradeAvailable,false, false, upd);
            
            /* Upgrade via Website Button Option */
            Box bxWebsite =  new Box (Orientation.Horizontal, sp) {
                MarginStart = 6, 
                MarginEnd = 6,
                Halign = Align.Center
            };
            
            Image imgWebsite = new Image (new Gdk.Pixbuf ("Third_Party/Icon_UpgradeWebsite_128.png", 48, 48, true));
            Label lblWebsite = new Label (Instance?.Language.Localize ("UPGRADE:OpenDownloadPage")) {
                MarginStart = 15
            };
            bxWebsite.PackStart(imgWebsite,false, false, upd);
            bxWebsite.PackStart(lblWebsite,false, false, upd);

            Button btnWebsite = new Button () {
                Child = bxWebsite,
                Halign = Align.Fill,
                Relief = ReliefStyle.None
            };
            
            btnWebsite.Pressed += delegate (object? sender, EventArgs e) {
                OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Website));
                this.Close();
                this.Destroy ();
            };
            
            bxMain1.PackStart(btnWebsite, false, false, upd);
            
            /* Delay Button Option */
            Box bxDelay =  new Box (Orientation.Horizontal, sp) {
                MarginStart = 6, 
                MarginEnd = 6,
                Halign = Align.Center
            };
            
            Image imgDelay = new Image (new Gdk.Pixbuf ("Third_Party/Icon_UpgradeLater_128.png", 48, 48, true));
            Label lblDelay = new Label (Instance?.Language.Localize ("UPGRADE:Later")) {
                MarginStart = 15
            };
            bxDelay.PackStart(imgDelay,false, false, upd);
            bxDelay.PackStart(lblDelay,false, false, upd);

            Button btnDelay = new Button () {
                Child = bxDelay,
                Halign = Align.Fill,
                Relief = ReliefStyle.None
            };
            
            btnDelay.Pressed += delegate (object? sender, EventArgs e) {
                OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Delay));
                this.Close();
                this.Destroy ();
            };
            
            bxMain1.PackStart(btnDelay, false, false, upd);
            
            /* Mute Button Option */
            Box bxMute =  new Box (Orientation.Horizontal, sp) {
                MarginStart = 6, 
                MarginEnd = 6,
                Halign = Align.Center
            };
            
            Image imgMute = new Image (new Gdk.Pixbuf ("Third_Party/Icon_UpgradeMute_128.png", 48, 48, true));
            Label lblMute = new Label (Instance?.Language.Localize ("UPGRADE:Mute")) {
                MarginStart = 15
            };
            bxMute.PackStart(imgMute,false, false, upd);
            bxMute.PackStart(lblMute,false, false, upd);

            Button btnMute = new Button () {
                Child = bxMute,
                Halign = Align.Fill,
                Relief = ReliefStyle.None
            };
            
            btnMute.Pressed += delegate (object? sender, EventArgs e) {
                OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Mute));
                this.Close();
                this.Destroy ();
            };
            
            bxMain1.PackStart(btnMute, false, false, upd);
            
            /* Finish packing & show */
            
            Add(bxMain1);
            
            HeaderBar hb = new HeaderBar() {
                ShowCloseButton = true,
                Title = Instance?.Language.Localize ("UPGRADE:Upgrade") 
            };
            this.Titlebar = hb;
            
            ShowAll();
        }
        

        private void btnDelay_Click (object sender, EventArgs e) {
            OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Delay));
            Close ();
        }

        private void btnMute_Click (object sender, EventArgs e) {
            OnUpgradeRoute?.Invoke (this, new UpgradeEventArgs (UpgradeOptions.Mute));
            Close ();
        }
    }
}