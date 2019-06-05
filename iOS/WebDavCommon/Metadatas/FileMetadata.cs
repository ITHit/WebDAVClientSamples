using ITHit.WebDAV.Client;

using WebDavCommon.Storages;

namespace WebDavCommon.Metadatas
{
    /// <summary>This class represents both local and remote state of file.</summary>
    public class FileMetadata : ItemMetadata
    {
        /// <summary>Initializes a new instance of the <see cref="FileMetadata"/> class.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="parentIdentifier">The parent identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="localItem">The local item.</param>
        /// <param name="serverItem">The server item.</param>
        public FileMetadata(
            string identifier,
            string parentIdentifier,
            string name,
            LocalFile localItem,
            IFileAsync serverItem = null)
            : base(identifier, parentIdentifier, name, localItem, serverItem)
        {
            this.LocalFile = localItem;
            this.ServerFile = serverItem;
        }

        /// <summary>Gets or sets the local file.</summary>
        public LocalFile LocalFile { get; set; }

        /// <summary>Gets or sets the server file.</summary>
        public IFileAsync ServerFile { get; set; }

        /// <summary>Gets a value indicating whether is item Etag is same local and on server. </summary>
        public bool IsSyncByEtag => this.ExistsOnServer && this.LocalFile.Etag == this.ServerFile.Etag;

        /// <summary>Gets or sets file size.</summary>
        /// <remarks>Return local size if exists and remote size otherwise. </remarks>
        public ulong Size => this.ExistsLocal ? this.LocalFile.Size : (ulong)this.ServerFile.ContentLength;

        /// <summary>Gets a value indicating whether is item fails upload. </summary>
        public bool HasUploadError => this.LocalFile.UploadError != null;
    }
}
