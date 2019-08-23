using System;
using System.ComponentModel;

namespace II_Linux.UI
{
    public partial class PatientEditor : Gtk.Window
    {
		AppType App = new AppType();

		public PatientEditor(string[] args) : base(Gtk.WindowType.Toplevel) {
			this.Build();

			App.Start_Args = args;
			App.Patient_Editor = this;

			App.Timer_Main.Interval = 10d; // q 10 milliseconds
			App.Timer_Main.Start();

			// Send usage statistics to server in background
			BackgroundWorker bgw = new BackgroundWorker();
			bgw.DoWork += delegate { App.Server_Connection.Send_UsageStatistics(); };
			bgw.RunWorkerAsync();

			//InitInitialRun();
			//InitInterface();
			//InitPatient();

			if (App.Start_Args.Length > 0)
				throw new NotImplementedException();
				//LoadOpen(App.Start_Args[0]);

		}

		private void InitInitialRun() {
			//string setLang = Properties.Settings.Default.Language;

			/*
			if (setLang == null || setLang == ""
				|| !Enum.TryParse<Languages.Values>(setLang, out App.Language.Value)) {
				App.Language = new Languages();
				DialogInitial();
			}
			*/
		}
	}
}
