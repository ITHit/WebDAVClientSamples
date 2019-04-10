using ITHit.WebDAV.Client;
using WebDavContainerExtension.Storages;

namespace WebDavContainerExtension.Metadatas
{
    public class FileMetadata: ItemMetadata
    {
        public LocalFile LocalFile { get; set; }

        public IFileAsync ServerFile { get; set; }

        public FileMetadata(string identifier, string parentIdentifier, string name, LocalFile localItem, IFileAsync serverItem = null) 
            : base(identifier, parentIdentifier, name, localItem, serverItem)
        {
            this.LocalFile = localItem;
            this.ServerFile = serverItem;
        }
    }
}