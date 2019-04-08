using System;
using UIKit;
using WebDavCommon;

namespace WebDavContainer
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ServerSettings serverSettings = AppGroupSettings.GetServerSettings();
            if(serverSettings != null)
            {
                Server.Text = serverSettings.ServerUri.ToString() ?? string.Empty;
                Username.Text = serverSettings.UserName ?? string.Empty;
                Password.Text = serverSettings.Password ?? string.Empty;
            }

#if DEBUG
            Server.Text = "http://webdavserver.net/User7bb0de4/";
#endif
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void Login_Clicked(UIButton sender)
        {
            string serverUri = Server.Text;
            string userName = Username.Text;
            string passWord = Password.Text;


            if(String.IsNullOrEmpty(serverUri) || String.IsNullOrWhiteSpace(serverUri))
            {
                UIAlertView alert = new UIAlertView()
                {
                    Title = "Server URL is invalid",
                    Message = "Input valid server URL"
                };
                alert.AddButton("Ok");
                alert.Show();
            }


            try
            {
                var serverSettings = new ServerSettings(serverUri, userName, passWord);
                AppGroupSettings.SaveServerSettings(serverSettings);
                UIAlertView alert = new UIAlertView()
                {
                    Title = "Login successful",
                    Message = "Now you can open documents in MS Office or any other application from http://server/ via Location->Browse dialog and save back directly to server."
                };
                alert.AddButton("Ok");
                alert.Show();
            }
            catch(Exception ex)
            {
                UIAlertView doneAlert = new UIAlertView()
                {
                    Title = "Error",
                    Message = ex.Message
                };
                doneAlert.AddButton("Ok");
                doneAlert.Show();
            }

            this.Editing = false;
            Server.ResignFirstResponder();
            Username.ResignFirstResponder();
            Password.ResignFirstResponder();
        }
    }
}