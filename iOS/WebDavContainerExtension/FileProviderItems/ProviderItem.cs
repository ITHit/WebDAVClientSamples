using System.Linq;
using FileProvider;
using Foundation;

using WebDavCommon.Metadatas;

namespace WebDavContainerExtension.FileProviderItems
{
    public abstract class ProviderItem : NSObject, INSFileProviderItem
    {
        public string Identifier { get; }
        public string ParentIdentifier { get; }
        public string Filename { get; }
        public string TypeIdentifier { get; protected set; }

        [Export("capabilities")]
        public NSFileProviderItemCapabilities Capabilities { get; protected set; }

        [Export("isShared")]
        public bool IsShared => true;

    protected ProviderItem(ItemMetadata createdFolder)
        {
            this.Identifier = createdFolder.Identifier;
            this.ParentIdentifier = createdFolder.ParentIdentifier;
            this.Filename = createdFolder.Name;
        }

        public static INSFileProviderItem[] CreateFromMetadatas(ItemMetadata[] metadatas)
        {
            return metadatas.Select(CreateFromMetadata).ToArray();
        }

        public static INSFileProviderItem CreateFromMetadata(ItemMetadata itemMetadata)
        {
            if(itemMetadata is FileMetadata)
            {
                return new FileItem(itemMetadata as FileMetadata);
            }

            return new FolderItem(itemMetadata as FolderMetadata);
        }
    }
}