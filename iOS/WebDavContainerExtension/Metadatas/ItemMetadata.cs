using ITHit.WebDAV.Client;
using WebDavContainerExtension.Storages;

namespace WebDavContainerExtension.Metadatas
{
    public abstract class ItemMetadata
    {
        public string Identifier { get; set; }
        public string ParentIdentifier { get; set; }
        public string Name { get; set; }
        public LocalItem LocalItem { get; set; }
        public IHierarchyItemAsync ServerItem { get; set; }
        public bool ExistsLocal => LocalItem.IsExists;
        public bool ExistsOnServer => ServerItem != null;
        public bool IsExists => ExistsLocal || ExistsOnServer;
        public bool IsFolder => this is FolderMetadata;
        public bool IsFile => this is FileMetadata;

        public ItemMetadata(string identifier, string parentIdentifier, string name, LocalItem localItem, IHierarchyItemAsync serverItem = null)
        {
            this.Identifier = identifier;
            this.ParentIdentifier = parentIdentifier;
            this.Name = name;
            this.LocalItem = localItem;
            this.ServerItem = serverItem;
        }

        public FolderMetadata AsFolder()
        {
            return this as FolderMetadata;
        }
        public FileMetadata AsFile()
        {
            return this as FileMetadata;
        }



    }
}