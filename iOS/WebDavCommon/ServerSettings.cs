using System;
using Foundation;

namespace WebDavCommon
{
    public class ServerSettings
    {
        private const string ServerUriKey = "ServerUri";
        private const string UserNameKey = "UserName";
        private const string PasswordKey = "PassWord";

        public Uri ServerUri { get; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public ServerSettings(string serverUri, string userName = "", string password = "")
        {
            if(string.IsNullOrEmpty(serverUri)) throw new ArgumentException(serverUri);
            ServerUri = new Uri(serverUri);
            UserName = userName;
            Password = password;
        }

        public bool HasCredential => !string.IsNullOrEmpty(this.Password) && !string.IsNullOrEmpty(this.UserName);

        public static ServerSettings CreateFromNsDictionary(NSDictionary userDataDictionary)
        {
            if(userDataDictionary != null)
            {
                NSObject serverUrl = userDataDictionary.ValueForKey((NSString) ServerUriKey);
                NSObject userName = userDataDictionary.ValueForKey((NSString) UserNameKey);
                NSObject passWord = userDataDictionary.ValueForKey((NSString) PasswordKey);
                return new ServerSettings(serverUrl.ToString(), userName.ToString(), passWord.ToString());
            }

            throw new ArgumentNullException(nameof(userDataDictionary));
        }

        public NSDictionary ToNsDictionary()
        {
            return new NSDictionary(new NSString(UserNameKey), new NSString(UserName),
                                    new NSString(ServerUriKey), new NSString(ServerUri.ToString()),
                                    new NSString(PasswordKey), new NSString(Password));
        }
    }
}