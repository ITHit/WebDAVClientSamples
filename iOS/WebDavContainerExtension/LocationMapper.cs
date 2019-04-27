using System;
using System.IO;
using System.Linq;
using FileProvider;

namespace WebDavContainerExtension
{
    public class LocationMapper
    {
        public string LocalStorageRoot { get; }
        public Uri ServerRoot { get; }

        public LocationMapper(Uri serverRootUri, string localStorageRoot)
        {
            ServerRoot = serverRootUri ?? throw new ArgumentNullException(nameof(serverRootUri));
            LocalStorageRoot = localStorageRoot ?? throw new ArgumentNullException(nameof(localStorageRoot));
        }

        public string GetIdentifierFromLocalPath(string localPath)
        {
            if(localPath == null) throw new ArgumentNullException(nameof(localPath));
            if(LocalStorageRoot.StartsWith("/private") && !localPath.StartsWith("/private"))
            {
                localPath = "/private" + localPath;
            }

            var relativeUrl = localPath.Replace(LocalStorageRoot, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            if(!string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }

            return NSFileProviderItemIdentifier.RootContainer.ToString();
        }

        public string GetLocalUrlFromIdentifier(string persistentIdentifier)
        {
            if(persistentIdentifier == null) throw new ArgumentNullException(nameof(persistentIdentifier));
            if(persistentIdentifier == NSFileProviderItemIdentifier.RootContainer.ToString())
            {
                return LocalStorageRoot;
            }

            return Path.Combine(LocalStorageRoot, persistentIdentifier);
        }

        public Uri GetServerUriFromIdentifier(string itemIdentifier)
        {
            if (itemIdentifier == NSFileProviderItemIdentifier.RootContainer.ToString())
            {
                return ServerRoot;
            }

            return new Uri(ServerRoot, itemIdentifier);
        }

        public string GetIdentifierFromServerUri(Uri serverUri)
        {
            if(serverUri == null) throw new ArgumentNullException(nameof(serverUri));

            var relative = serverUri.AbsoluteUri.Replace(ServerRoot.AbsoluteUri, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            if(relative == string.Empty)
            {
                return NSFileProviderItemIdentifier.RootContainer.ToString();
            }

            return Extension.Decode(relative);
        }

        public string GetParentIdentifier(string itemIdentifier)
        {
            itemIdentifier = itemIdentifier.TrimEnd(Path.DirectorySeparatorChar);
            itemIdentifier = itemIdentifier.Remove(itemIdentifier.LastIndexOf(Path.DirectorySeparatorChar) + 1).TrimStart(Path.DirectorySeparatorChar);
            if (itemIdentifier == string.Empty)
            {
                return NSFileProviderItemIdentifier.RootContainer.ToString();
            }

            return itemIdentifier;
        }


        public string GetParentIdentifierFromServerUri(Uri serverUri)
        {
            string id = GetIdentifierFromServerUri(serverUri);
            return GetParentIdentifier(id);
        }

        public Uri GetServerUriFromLocalUrl(string url)
        {
            string fileIdentifier = GetIdentifierFromLocalPath(url);
            return GetServerUriFromIdentifier(fileIdentifier);
        }

        public string GetLocalUrlFromServerUri(Uri serverUri)
        {
            string id = GetIdentifierFromServerUri(serverUri);
            return GetLocalUrlFromIdentifier(id);
        }

        public string GetNameFromIdentifier(string itemIdentifier)
        {
            if(itemIdentifier == NSFileProviderItemIdentifier.RootContainer)
            {
                return null;
            }

            return itemIdentifier.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).LastOrDefault();
        }

        public bool IsFolderIdentifier(string itemIdentifier)
        {
            return itemIdentifier == NSFileProviderItemIdentifier.RootContainer || itemIdentifier.EndsWith(Path.DirectorySeparatorChar);
        }
    }
}