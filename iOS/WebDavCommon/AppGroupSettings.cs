using System;
using System.IO;
using Foundation;

namespace WebDavCommon
{
    public static class AppGroupSettings
    {
        private const string AppGroupId = "group.com.WebDAV.Client.Container";
        private const string ServerSettingFile = "data.out";

        public static ServerSettings GetServerSettings()
        {
            using (NSUrl userDataPath = GetSharedContainerUrl())
            using (var userData = NSDictionary.FromFile(Path.Combine(userDataPath.Path, ServerSettingFile)))
            {
                if (userData == null)
                {
                    return null;
                }

                return ServerSettings.CreateFromNsDictionary(userData);
            }
        }

        private static NSUrl GetSharedContainerUrl()
        {
            NSUrl userDataPath = NSFileManager.DefaultManager.GetContainerUrl(AppGroupId);
            if (userDataPath == null)
            {
                throw new AccessViolationException("Group container is null");
            }

            return userDataPath;
        }

        public static void SaveServerSettings(ServerSettings serverSettings)
        {
            if (serverSettings == null)
            {
                throw new ArgumentNullException(nameof(serverSettings));
            }

            using (NSUrl userDataPath = GetSharedContainerUrl())
            using (NSDictionary data = serverSettings.ToNsDictionary())
            {
                if (!data.WriteToFile(Path.Combine(userDataPath.Path, ServerSettingFile), true))
                {
                    throw new Exception("Failed to save server setting");
                }
            }
        }
    }
}