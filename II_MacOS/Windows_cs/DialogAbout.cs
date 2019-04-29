using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace II_MacOS.UI {
	public partial class DialogAbout : AppKit.NSWindow {
		#region Constructors

		// Called when created from unmanaged code
		public DialogAbout (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public DialogAbout (NSCoder coder) : base (coder)
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
