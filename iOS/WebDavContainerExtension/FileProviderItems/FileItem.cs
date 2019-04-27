using System;
using WebDavContainerExtension.Metadatas;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client.Exceptions;
using WebDavContainerExtension.Helpers;
using WebDavContainerExtension.Storages;

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
            TypeIdentifier = Extension.GetFileTypeIdentifier(fileMetadata.Name);
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
            IsMostRecentVersionDownloaded = fileMetadata.IsSyncByEtag && IsDownloaded;
            IsUploaded = !fileMetadata.HasUploadError || fileMetadata.IsSyncByEtag;
            UploadingError = fileMetadata.LocalFile.UploadError;
        }
    }
}