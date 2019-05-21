using System;
using Foundation;

namespace WebDavCommon
{
    public class ServerSettings
    {
        public Uri ServerUri { get; }

        public string Password { get; set; }
        public string UserName { get; set; }

        public ServerSettings(string serverUri, string userName = "", string password = "")
        {
            if(string.IsNullOrEmpty(serverUri))
            {
                throw new ArgumentException(serverUri);
            }

            ServerUri = new Uri(serverUri);
            UserName = userName;
            Password = password;
        }

        public bool HasCredential => !string.IsNullOrEmpty(this.Password) && !String.IsNullOrEmpty(this.UserName);

        public static ServerSettings CreateFromNsDictionary(NSDictionary userDataDictionary)
        {
            if(userDataDictionary == null)
            {
                throw new ArgumentNullException(nameof(userDataDictionary));
            }

            NSObject serverUrl = userDataDictionary.ValueForKey((NSString) "ServerUri");
            NSObject userName = userDataDictionary.ValueForKey((NSString) "UserName");
            NSObject passWord = userDataDictionary.ValueForKey((NSString) "PassWord");
            return new ServerSettings(serverUrl.ToString(), userName.ToString(), passWord.ToString());
        }

        public NSDictionary ToNsDictionary()
        {
            return new NSDictionary(new NSString("UserName"), new NSString(UserName),
                                    new NSString("ServerUri"), new NSString(ServerUri.ToString()),
                                    new NSString("PassWord"), new NSString(Password));
        }
    }
}