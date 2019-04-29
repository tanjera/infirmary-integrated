using AppKit;
using II;
using II.Localization;
using II.Server;
using System;
using System.ComponentModel;


namespace II_MacOS
{
    public static class App
    {
		public static string [] Start_Args;

		public static Servers Server = new Servers ();
		public static Mirrors Mirror = new Mirrors ();
		public static Languages Language = new Languages ();

		public static Patient Patient;

		public static UI.PatientEditor Patient_Editor;

		public static UI.DeviceMonitor Device_Monitor;
		public static UI.DeviceECG Device_ECG;
		public static UI.DeviceDefib Device_Defib;
		public static UI.DeviceIABP Device_IABP;

		public static UI.DialogAbout Dialog_About;
		public static UI.DialogInitial Dialog_Initial;

		public static System.Timers.Timer Timer_Main = new System.Timers.Timer ();

		static void Main(string[] args) {
			Start_Args = args;

			Timer_Main.Interval = 100000; // q 10 milliseconds
			Timer_Main.Start ();

			// Send usage statistics to server in background
			BackgroundWorker bgw = new BackgroundWorker ();
			bgw.DoWork += delegate { Server.Post_UsageStatistics (); };
			bgw.RunWorkerAsync ();

			NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}