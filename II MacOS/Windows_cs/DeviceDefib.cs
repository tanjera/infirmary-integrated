using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace II_MacOS.UI {
	public partial class DeviceDefib : AppKit.NSWindow {
		#region Constructors

		// Called when created from unmanaged code
		public DeviceDefib (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public DeviceDefib (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion
	}
}
