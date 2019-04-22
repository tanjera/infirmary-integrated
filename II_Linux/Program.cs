using System;

using Gtk;

using II;
using II_Linux.UI;

namespace II_Linux {

	public class AppType {
		public string[] Start_Args;

		public II.Server.Connection Server_Connection = new II.Server.Connection();
		public II.Localization.Languages Language = new II.Localization.Languages();

		public Patient Patient;

		public PatientEditor Patient_Editor;

		public DeviceMonitor Device_Monitor;
		public DeviceECG Device_ECG;
		public DeviceDefib Device_Defib;
		public DeviceIABP Device_IABP;

		public DialogAbout Dialog_About;
		public DialogInitial Dialog_Initial;

		public System.Timers.Timer Timer_Main = new System.Timers.Timer();
	}

	class MainClass {
		public static void Main(string[] args) {
			Application.Init();
			PatientEditor Patient_Editor = new PatientEditor(args);
			Patient_Editor.Show();
			Application.Run();
		}

	}
}
