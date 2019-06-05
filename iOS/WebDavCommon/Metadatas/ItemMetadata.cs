using ITHit.WebDAV.Client;

using WebDavCommon.Storages;

namespace WebDavCommon.Metadatas
{
    /// <summary>This class represents both local and remote state.</summary>
    public abstract class ItemMetadata
    {
        /// <summary>Initializes a new instance of the <see cref="ItemMetadata"/> class.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="parentIdentifier">The parent identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="localItem">The local item.</param>
        /// <param name="serverItem">The server item.</param>
        protected ItemMetadata(
            string identifier,
            string parentIdentifier,
            string name,
            LocalItem localItem,
            IHierarchyItemAsync serverItem = null)
        {
            this.Identifier = identifier;
            this.ParentIdentifier = parentIdentifier;
            this.Name = name;
            this.LocalItem = localItem;
            this.ServerItem = serverItem;
        }

        /// <summary>Gets or sets the identifier.</summary>
        public string Identifier { get; set; }

        /// <summary>Gets or sets the parent identifier.</summary>
        public string ParentIdentifier { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the local metadata.</summary>
        public LocalItem LocalItem { get; set; }

        /// <summary>Gets or sets the server metadata.</summary>
        public IHierarchyItemAsync ServerItem { get; set; }

        /// <summary>Gets a value indicating whether is item exists local.</summary>
        public bool ExistsLocal => this.LocalItem.IsExists;

        /// <summary>Gets a value indicating whether is item exists remote.</summary>
        public bool ExistsOnServer => this.ServerItem != null;

        /// <summary>Gets a value indicating whether is item exists anywhere.</summary>
        public bool IsExists => this.ExistsLocal || this.ExistsOnServer;
    }
}
