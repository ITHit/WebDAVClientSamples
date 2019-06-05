using System;

using Foundation;

namespace WebDavCommon.Storages
{
    /// <summary>This class represents file on local filesystem. </summary>
    public class LocalFile : LocalItem
    {
        /// <summary>Initializes a new instance of the <see cref="LocalFile"/> class.</summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="exists">The exists.</param>
        /// <param name="extendedAttribute">The file extended attribute.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="localPath"/> is null. </exception>
        public LocalFile(string localPath, bool exists, ExtendedAttribute extendedAttribute = null)
            : base(localPath, exists)
        {
            if (extendedAttribute == null) return;

            this.Etag = extendedAttribute.Etag;
            this.UploadError = extendedAttribute.UploadError;
        }

        /// <summary>Initializes a new instance of the <see cref="LocalFile"/> class.</summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="exists">The exists.</param>
        /// <param name="size">The size.</param>
        /// <param name="extendedAttribute">The file extended attribute.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="localPath"/> is null. </exception>
        public LocalFile(string localPath, bool exists, ulong size, ExtendedAttribute extendedAttribute = null)
            : this(localPath, exists, extendedAttribute)
        {
            this.Size = size;
        }

        /// <summary>The extended attribute instance.</summary>
        public ExtendedAttribute ExtendedAttribute => new ExtendedAttribute(this.UploadError, this.Etag);

        /// <summary>Gets the size.</summary>
        public ulong Size { get; }

        /// <summary>Gets or sets the etag.</summary>
        public string Etag { get; set; }

        /// <summary>Gets a value indicating whether is etag present.</summary>
        public bool HasEtag => !string.IsNullOrEmpty(this.Etag);

        /// <summary>Gets or sets the upload error.</summary>
        public NSError UploadError { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LocalFile"/> class that's not exists.</summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>The <see cref="LocalFile"/>.</returns>
        public static LocalFile CreateNotExist(string localPath)
        {
            return new LocalFile(localPath, false);
        }

        /// <summary>Initializes a new instance of the <see cref="LocalFile"/> class that exists.</summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="fileSize">The file size.</param>
        /// <param name="extendedAttribute">The file extended attribute.</param>
        /// <returns>The <see cref="LocalFile"/>.</returns>
        public static LocalFile CreateExists(
            string localPath,
            ulong fileSize,
            ExtendedAttribute extendedAttribute = null)
        {
            return new LocalFile(localPath, true, fileSize, extendedAttribute);
        }
    }
}
