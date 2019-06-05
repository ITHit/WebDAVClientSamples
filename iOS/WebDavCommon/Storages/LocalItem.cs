using System;

namespace WebDavCommon.Storages
{
    /// <summary>This class represents item on local filesystem. </summary>
    public abstract class LocalItem
    {
        /// <summary>Initializes a new instance of the <see cref="LocalItem"/> class.</summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="exists">The value indicating whether is item exists.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="localPath"/> is null. </exception>
        protected LocalItem(string localPath, bool exists)
        {
            this.Path = localPath ?? throw new ArgumentNullException(nameof(localPath));
            this.IsExists = exists;
        }

        /// <summary>Gets or sets the path.</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets a value indicating whether is item exists.</summary>
        public bool IsExists { get; set; }
    }
}
