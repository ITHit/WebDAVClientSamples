using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;

using WebDavCommon.Helpers;

namespace WebDavCommon.Storages
{
    /// <summary>This class represents additional data in local filesystem.</summary>
    [Serializable]
    public class ExtendedAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="ExtendedAttribute"/> class.</summary>
        public ExtendedAttribute()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExtendedAttribute"/> class.</summary>
        /// <param name="uploadError">The uploadError.</param>
        public ExtendedAttribute(NSError uploadError)
        {
            if (uploadError == null) return;

            this.UploadErrorInfo = uploadError.UserInfo.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            this.UploadErrorCode = uploadError.Code;
            this.UploadErrorDomain = uploadError.Domain;
        }

        /// <summary>Initializes a new instance of the <see cref="ExtendedAttribute"/> class.</summary>
        /// <param name="uploadError">The uploadError.</param>
        /// <param name="etag">The etag.</param>
        public ExtendedAttribute(NSError uploadError, string etag)
            : this(uploadError)
        {
            this.Etag = etag;
        }


        /// <summary>Gets or sets the upload uploadError info.</summary>
        public Dictionary<string, string> UploadErrorInfo { get; set; }

        /// <summary>Gets or sets the upload uploadError code.</summary>
        public long UploadErrorCode { get; set; }

        /// <summary>Gets or sets the upload uploadError domain.</summary>
        public string UploadErrorDomain { get; set; }

        /// <summary>Gets or sets the etag.</summary>
        public string Etag { get; set; }

        /// <summary>Gets the upload uploadError.</summary>
        public NSError UploadError
        {
            get
            {
                return this.CreateUploadError();
            }
        }

        /// <summary>The create upload uploadError.</summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        private NSError CreateUploadError()
        {
            if (string.IsNullOrEmpty(this.UploadErrorDomain)) return null;

            if (this.UploadErrorInfo == null)
                return NSErrorFactory.CreateNSError(this.UploadErrorDomain, this.UploadErrorCode);

            Dictionary<string, string> uploadErrorInfo = this.UploadErrorInfo;
            return NSErrorFactory.CreateNSError(this.UploadErrorDomain, this.UploadErrorCode, uploadErrorInfo);
        }
    }
}
