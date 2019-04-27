using ITHit.WebDAV.Client;
using WebDavContainerExtension.Storages;

namespace WebDavContainerExtension.Metadatas
{
    public class FolderMetadata
        : ItemMetadata
    {
        public IFolderAsync ServerFolder { get; set; }
        public LocalFolder LocalFolder { get; set; }

        public FolderMetadata(string identifier, string parentIdentifier, string name, LocalFolder localItem, IFolderAsync serverItem = null) 
            : base(identifier, parentIdentifier, name, localItem, serverItem)
        {
            this.LocalFolder = localItem;
            this.ServerFolder = serverItem;
        }
    }
}