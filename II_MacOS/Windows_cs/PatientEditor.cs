using System;

using AppKit;
using Foundation;

namespace II_MacOS
{
    [Register("PatientEditor")]
    public class PatientEditor : NSDocument
    {
        public PatientEditor(IntPtr handle) : base(handle)
        {
            // Add your subclass-specific initialization here.
        }

        public override void WindowControllerDidLoadNib(NSWindowController windowController)
        {
            base.WindowControllerDidLoadNib(windowController);
            // Add any code here that needs to be executed once the windowController has loaded the document's window.
        }

        [Export("autosavesInPlace")]
        public static bool AutosaveInPlace()
        {
            return true;
        }

        public override string WindowNibName
        {
            get
            {
                return "PatientEditor";
            }
        }

        public override NSData GetAsData(string typeName, out NSError outError)
        {
            // Insert code here to write your document to data of the specified type. 
            // If outError != NULL, ensure that you create and set an appropriate error when returning nil.
            throw new NotImplementedException();
        }

        public override bool ReadFromData(NSData data, string typeName, out NSError outError)
        {
            // Insert code here to read your document from the given data of the specified type. 
            // If outError != NULL, ensure that you create and set an appropriate error when returning NO.
            throw new NotImplementedException();
        }
    }
}
