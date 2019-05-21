using WebDavContainerExtension.Metadatas;
using FileProvider;

namespace WebDavContainerExtension.FileProviderItems
{
    public class FolderItem : ProviderItem
    {

        public FolderItem(FolderMetadata createdFolder) : base(createdFolder)
        {
            TypeIdentifier = Extension.GetFolderTypeIdentifier();
            this.Capabilities = NSFileProviderItemCapabilities.AddingSubItems
                               | NSFileProviderItemCapabilities.ContentEnumerating
                               | NSFileProviderItemCapabilities.Reading
                               | NSFileProviderItemCapabilities.Renaming
                               | NSFileProviderItemCapabilities.Deleting;
        }
    }
}