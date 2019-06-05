using FileProvider;
using Foundation;

using WebDavCommon.Helpers;
using WebDavCommon.Metadatas;

namespace WebDavContainerExtension.FileProviderItems
{
    public class FileItem : ProviderItem
    {

        [Export("documentSize")]
        public NSNumber Size { get; protected set; }

        [Export("isUploaded")]
        public bool IsUploaded { get; protected set; }

        [Export("isDownloaded")]
        public bool IsDownloaded { get; protected set; }

        [Export("isMostRecentVersionDownloaded")]
        public bool IsMostRecentVersionDownloaded { get; protected set; }

        [Export("uploadingError")]
        public NSError UploadingError { get; protected set; }

        public FileItem(FileMetadata fileMetadata) : base(fileMetadata)
        {
            Size = new NSNumber(fileMetadata.Size);
            TypeIdentifier = UTTypeHelper.GetFileTypeIdentifier(fileMetadata.Name);
            this.Capabilities = NSFileProviderItemCapabilities.Writing
                                | NSFileProviderItemCapabilities.Deleting
                                | NSFileProviderItemCapabilities.Reading
                                | NSFileProviderItemCapabilities.Renaming
                                | NSFileProviderItemCapabilities.Reparenting;

            if(fileMetadata.HasUploadError)
            {
                this.Capabilities &= ~(NSFileProviderItemCapabilities.Renaming 
                                       | NSFileProviderItemCapabilities.Reparenting);
            }

            IsDownloaded = fileMetadata.ExistsLocal;
            IsMostRecentVersionDownloaded = (fileMetadata.IsSyncByEtag && fileMetadata.ExistsLocal) || !fileMetadata.ExistsOnServer;
            IsUploaded = !fileMetadata.HasUploadError && (fileMetadata.IsSyncByEtag || !fileMetadata.ExistsLocal);
            UploadingError = fileMetadata.LocalFile.UploadError;
        }
    }
}