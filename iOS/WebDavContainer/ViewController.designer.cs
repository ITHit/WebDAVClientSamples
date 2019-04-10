// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace WebDavContainer
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Login { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Password { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Server { get; set; }

        [Outlet]
        UIKit.UILabel SuccessLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Username { get; set; }

        [Action ("Login_Clicked:")]
        partial void Login_Clicked (UIKit.UIButton sender);
        
        void ReleaseDesignerOutlets ()
        {
            if (Login != null) {
                Login.Dispose ();
                Login = null;
            }

            if (Password != null) {
                Password.Dispose ();
                Password = null;
            }

            if (Server != null) {
                Server.Dispose ();
                Server = null;
            }

            if (SuccessLabel != null) {
                SuccessLabel.Dispose ();
                SuccessLabel = null;
            }

            if (Username != null) {
                Username.Dispose ();
                Username = null;
            }
        }
    }
}
