using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace II_MacOS.Resources {
	public partial class DialogInitial : AppKit.NSWindow {
		#region Constructors

		// Called when created from unmanaged code
		public DialogInitial (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public DialogInitial (NSCoder coder) : base (coder)
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
