using FileProvider;

using WebDavCommon.Helpers;
using WebDavCommon.Metadatas;

namespace WebDavContainerExtension.FileProviderItems
{
    public class FolderItem : ProviderItem
    {

        public FolderItem(FolderMetadata createdFolder) : base(createdFolder)
        {
            TypeIdentifier = UTTypeHelper.GetFolderTypeIdentifier();
            this.Capabilities = NSFileProviderItemCapabilities.AddingSubItems
                               | NSFileProviderItemCapabilities.ContentEnumerating
                               | NSFileProviderItemCapabilities.Reading
                               | NSFileProviderItemCapabilities.Renaming
                               | NSFileProviderItemCapabilities.Deleting;
        }
    }
}