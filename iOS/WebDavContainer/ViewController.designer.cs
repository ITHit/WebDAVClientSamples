// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace WebDavContainer
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        UIKit.UILabel SuccessLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Login { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView LoginActivityIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Password { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Server { get; set; }

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

            if (LoginActivityIndicator != null) {
                LoginActivityIndicator.Dispose ();
                LoginActivityIndicator = null;
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