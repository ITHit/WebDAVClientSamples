using ITHit.WebDAV.Client;

using WebDavCommon.Storages;

namespace WebDavCommon.Metadatas
{
    /// <summary>This class represents both local and remote state of folder.</summary>
    public class FolderMetadata : ItemMetadata
    {
        /// <summary>Initializes a new instance of the <see cref="FolderMetadata"/> class.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="parentIdentifier">The parent identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="localItem">The local item.</param>
        /// <param name="serverItem">The server item.</param>
        public FolderMetadata(
            string identifier,
            string parentIdentifier,
            string name,
            LocalFolder localItem,
            IFolderAsync serverItem = null)
            : base(identifier, parentIdentifier, name, localItem, serverItem)
        {
            this.LocalFolder = localItem;
            this.ServerFolder = serverItem;
        }

        /// <summary>Gets or sets the server folder.</summary>
        public IFolderAsync ServerFolder { get; set; }

        /// <summary>Gets or sets the local folder.</summary>
        public LocalFolder LocalFolder { get; set; }
    }
}
