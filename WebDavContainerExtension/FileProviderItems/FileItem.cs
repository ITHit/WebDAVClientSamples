using WebDavContainerExtension.Metadatas;
using FileProvider;
using Foundation;

namespace WebDavContainerExtension.FileProviderItems
{
    public class FileItem : ProviderItem
    {

        [Export("documentSize")]
        public NSNumber Size { get; protected set; }

        public FileItem(FileMetadata itemMetadata) : base(itemMetadata)
        {

                this.Size = new NSNumber(itemMetadata.ExistsLocal ? itemMetadata.LocalFile.Size : (ulong)itemMetadata.ServerFile.ContentLength);
                TypeIdentifier = Extension.GetFileTypeIdentifier(itemMetadata.Name);
                this.Capabilities = NSFileProviderItemCapabilities.Writing
                                   | NSFileProviderItemCapabilities.Deleting
                                   | NSFileProviderItemCapabilities.Reading
                                   | NSFileProviderItemCapabilities.Renaming
                                   | NSFileProviderItemCapabilities.Reparenting;
        }
    }
}