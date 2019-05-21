using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using WebDavContainerExtension.Helpers;

namespace WebDavContainerExtension.Storages
{
    [Serializable]
    public class FileExtendedAttribute
    {
        public FileExtendedAttribute()
        {
        }

        public Dictionary<string, string> UploadErrorInfo { get; set; }

        public long UploadErrorCode { get; set; }

        public string UploadErrorDomain { get; set; }

        public string Etag { get; set; }

        public FileExtendedAttribute(NSError error)
        {
            if(error == null)
            {
                return;
            }

            this.UploadErrorInfo = error.UserInfo.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            this.UploadErrorCode = error.Code;
            this.UploadErrorDomain = error.Domain;
        }

        public FileExtendedAttribute(NSError error, string etag) : this(error)
        {
            this.Etag = etag;
        }

        public NSError CreateUploadError()
        {
            if(string.IsNullOrEmpty(UploadErrorDomain))
            {
                return null;
            }

            if(UploadErrorInfo == null)
            {
                return NsErrorHelper.GetNSError(UploadErrorDomain, UploadErrorCode);
            }

            Dictionary<string, string> uploadErrorInfo = UploadErrorInfo;
            return NsErrorHelper.GetNsError(UploadErrorDomain, UploadErrorCode, uploadErrorInfo);
        }
    }
}