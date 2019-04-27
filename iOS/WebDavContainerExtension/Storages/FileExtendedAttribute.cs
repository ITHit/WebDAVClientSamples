using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Foundation;
using WebDavContainerExtension.Helpers;

namespace WebDavContainerExtension.Storages
{
    [Serializable]
    [DataContract(Name = nameof(FileExtendedAttribute))]
    public class FileExtendedAttribute
    {
        public FileExtendedAttribute()
        {
        }

        [DataMember]
        public Dictionary<string, string> UploadErrorInfo { get; set; }

        [DataMember]
        public long UploadErrorCode { get; set; }

        [DataMember]
        public string UploadErrorDomain { get; set; }

        [DataMember]
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