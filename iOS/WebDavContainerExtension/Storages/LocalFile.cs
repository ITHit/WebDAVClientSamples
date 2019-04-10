using System;
using Foundation;

namespace WebDavContainerExtension.Storages
{
    public class LocalFile: LocalItem
    {

        public LocalFile(string localPath, bool exists) : base(localPath, exists)
        {
        }

        public string Etag { get; set; }
        public ulong Size { get; set; }
        public bool HasEtag => !string.IsNullOrEmpty(Etag);
        public Exception DownLoadError { get; set; }
        public Exception UploadError { get; set; }
    }
}