using System;

using UIKit;
using WebDavCommon;
using WebDavCommon.Storages;

namespace WebDavContainer
{
    /// <inheritdoc />
    public partial class ViewController : UIViewController
    {
        private const string LoginSuccessfulTitle = "Login successful";

        private const string LoginSuccessfulMessage = "Now you can open documents from your server in MS Office Mobile or any other application via Location->Browse dialog and save back directly to server.";

        private const string ErrorTitle = "Error";

        private const string InvalidUriTitle = "Server URL is invalid";

        private const string InvalidUriMessage = "Input valid server URL";

        /// <inheritdoc />
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ServerSettings serverSettings = AppGroupSettings.GetServerSettings();
            if (serverSettings != null)
            {
                this.Server.Text = serverSettings.ServerUri.ToString();
                this.Username.Text = serverSettings.UserName ?? string.Empty;
                this.Password.Text = serverSettings.Password ?? string.Empty;
            }

#if DEBUG
            this.Server.Text = "http://webdavserver.net/User7bb0de4/";
#endif
        }

        async partial void Login_Clicked(UIButton sender)
        {
            string serverUri = this.Server.Text;
            string userName = this.Username.Text;
            string passWord = this.Password.Text;

            if (string.IsNullOrEmpty(serverUri) || string.IsNullOrWhiteSpace(serverUri))
            {
                this.ShowAlert(InvalidUriTitle, InvalidUriMessage);
            }


            try
            {
                this.LoginActivityIndicator.Hidden = false;
                this.LoginActivityIndicator.StartAnimating();
                sender.Enabled = false;
                var serverSettings = new ServerSettings(serverUri, userName, passWord);
                await SessionFactory.CheckConnectionAsync(serverSettings);
                var localStorage = new LocalStorage();
                localStorage.Clean();
                AppGroupSettings.SaveServerSettings(serverSettings);
                this.ShowAlert(LoginSuccessfulTitle, LoginSuccessfulMessage);
            }
            catch (Exception ex)
            {
                this.ShowAlert(ErrorTitle, ex.Message);
            }
            finally
            {
                sender.Enabled = true;
             }

            this.Editing = false;
            this.Server.ResignFirstResponder();
            this.Username.ResignFirstResponder();
            this.Password.ResignFirstResponder();
        }

        /// <summary>Displays alert to user.</summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException"> when <paramref name="title"/> or <paramref name="message"/> is null. </exception>
        private void ShowAlert(string title, string message)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (this.LoginActivityIndicator.IsAnimating)
            {
                this.LoginActivityIndicator.StopAnimating();
            }

            var alert = new UIAlertView() { Title = title, Message = message };
            alert.AddButton("Ok");
            alert.Show();
        }
    }
}