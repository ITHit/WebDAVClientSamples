using System;
using Foundation;

namespace WebDavContainerExtension.Storages
{
    public class LocalFile : LocalItem
    {
        public FileExtendedAttribute ExtendedAttribute => new FileExtendedAttribute(this.UploadError, this.Etag);

        public ulong Size { get; }

        public string Etag { get; set; }

        public bool HasEtag => !string.IsNullOrEmpty(Etag);

        public NSError UploadError { get; set; }

        public LocalFile(string localPath, bool exists, FileExtendedAttribute fileExtendedAttribute = null) : base(localPath, exists)
        {
            if(fileExtendedAttribute == null)
            {
                return;
            }

            this.Etag = fileExtendedAttribute.Etag;
            this.UploadError = fileExtendedAttribute.CreateUploadError();
        }

        public LocalFile(string localPath, bool exists, ulong size, FileExtendedAttribute fileExtendedAttribute = null) : this(localPath, exists, fileExtendedAttribute)
        {
            this.Size = size;
        }

        public static LocalFile CreateNotExist(string localPath)
        {
            return new LocalFile(localPath, false);
        }

        public static LocalFile CreateExists(string localPath, ulong fileSize)
        {
            return new LocalFile(localPath, true, fileSize);
        }

        public static LocalFile CreateExists(string localPath, ulong fileSize, FileExtendedAttribute fileExtendedAttribute)
        {
            return new LocalFile(localPath, true, fileSize, fileExtendedAttribute);
        }
    }
}