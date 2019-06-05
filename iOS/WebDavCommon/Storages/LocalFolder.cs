namespace WebDavCommon.Storages
{
    /// <inheritdoc />
    /// <summary>This class represents folder on local filesystem. </summary>
    public class LocalFolder : LocalItem
    {
        /// <summary>Initializes a new instance of the <see cref="LocalFolder"/> class.</summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="exists">The exists.</param>
        public LocalFolder(string localPath, bool exists)
            : base(localPath, exists)
        {
            if (!this.Path.EndsWith(System.IO.Path.DirectorySeparatorChar))
            {
                this.Path += System.IO.Path.DirectorySeparatorChar;
            }
        }
    }
}
