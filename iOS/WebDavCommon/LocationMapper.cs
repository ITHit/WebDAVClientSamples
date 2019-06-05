using System;
using System.IO;
using System.Linq;

using FileProvider;

using WebDavCommon.Helpers;

namespace WebDavCommon
{
    /// <summary>This class provides methods for mapping between local path, server uri and identifier.</summary>
    public class LocationMapper
    {
        /// <summary>The private prefix.</summary>
        private const string PrivatePrefix = "/private";

        /// <summary>Initializes a new instance of the <see cref="LocationMapper" /> class.</summary>
        /// <param name="serverRootUri">The server root uri.</param>
        /// <param name="localStorageRoot">The local storage root.</param>
        public LocationMapper(Uri serverRootUri, string localStorageRoot)
        {
            this.ServerRoot = serverRootUri ?? throw new ArgumentNullException(nameof(serverRootUri));
            this.LocalStorageRoot = localStorageRoot ?? throw new ArgumentNullException(nameof(localStorageRoot));
        }

        /// <summary>Gets the local storage root.</summary>
        public string LocalStorageRoot { get; }

        /// <summary>Gets the server root.</summary>
        public Uri ServerRoot { get; }

        /// <summary>Gets identifier by local path.</summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="localPath"/> is null. </exception>
        public string GetIdentifierFromLocalPath(string localPath)
        {
            if (localPath == null) throw new ArgumentNullException(nameof(localPath));
            if (this.LocalStorageRoot.StartsWith(PrivatePrefix) && !localPath.StartsWith(PrivatePrefix))
            {
                localPath = PrivatePrefix + localPath;
            }

            string relativeUrl = localPath.Replace(this.LocalStorageRoot, string.Empty)
                                          .TrimStart(Path.DirectorySeparatorChar);

            if (!string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }

            return NSFileProviderItemIdentifier.RootContainer.ToString();
        }

        /// <summary>Gets local path by identifier.</summary>
        /// <param name="persistentIdentifier">The persistent identifier.</param>
        /// <returns>The <see cref="string"/> contains item identifier.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="persistentIdentifier"/> is null. </exception>
        public string GetLocalUrlFromIdentifier(string persistentIdentifier)
        {
            if (persistentIdentifier == null) throw new ArgumentNullException(nameof(persistentIdentifier));
            if (persistentIdentifier == NSFileProviderItemIdentifier.RootContainer.ToString())
            {
                return this.LocalStorageRoot;
            }

            return Path.Combine(this.LocalStorageRoot, persistentIdentifier);
        }

        /// <summary>Gets server uri by identifier.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="Uri"/>.</returns>
        public Uri GetServerUriFromIdentifier(string itemIdentifier)
        {
            if (itemIdentifier == NSFileProviderItemIdentifier.RootContainer.ToString())
            {
                return this.ServerRoot;
            }

            return new Uri(this.ServerRoot, itemIdentifier);
        }

        /// <summary>Gets identifier from server uri.</summary>
        /// <param name="serverUri">The server uri.</param>
        /// <returns>The <see cref="string"/> contains item identifier.</returns>
        /// <exception cref="ArgumentNullException"> is <paramref name="serverUri"/> is null. </exception>
        public string GetIdentifierFromServerUri(Uri serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException(nameof(serverUri));
            string relative = serverUri.AbsoluteUri.Replace(this.ServerRoot.AbsoluteUri, string.Empty)
                                                   .TrimStart(Path.DirectorySeparatorChar);

            if (relative == string.Empty)
            {
                return NSFileProviderItemIdentifier.RootContainer.ToString();
            }

            return UrlHelper.Decode(relative);
        }

        /// <summary>Gets parent identifier.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="string"/> contains item identifier.</returns>
        public string GetParentIdentifier(string itemIdentifier)
        {
            itemIdentifier = itemIdentifier.TrimEnd(Path.DirectorySeparatorChar);
            itemIdentifier = itemIdentifier.Remove(itemIdentifier.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                                           .TrimStart(Path.DirectorySeparatorChar);
            if (itemIdentifier == string.Empty) return NSFileProviderItemIdentifier.RootContainer.ToString();

            return itemIdentifier;
        }

        /// <summary>Gets local url by server uri.</summary>
        /// <param name="serverUri">The server uri.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetLocalUrlFromServerUri(Uri serverUri)
        {
            string id = this.GetIdentifierFromServerUri(serverUri);
            return this.GetLocalUrlFromIdentifier(id);
        }

        /// <summary>Gets name from identifier.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetNameFromIdentifier(string itemIdentifier)
        {
            if (itemIdentifier == NSFileProviderItemIdentifier.RootContainer)
            {
                return null;
            }

            return itemIdentifier.TrimEnd(Path.DirectorySeparatorChar)
                                 .Split(Path.DirectorySeparatorChar)
                                 .LastOrDefault();
        }

        /// <summary>Returns a value indicating whether is identifier is folder.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsFolderIdentifier(string itemIdentifier)
        {
            return itemIdentifier == NSFileProviderItemIdentifier.RootContainer
                                    || itemIdentifier.EndsWith(Path.DirectorySeparatorChar);
        }
    }
}
