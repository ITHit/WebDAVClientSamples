using System.Collections.Generic;
using System.Linq;

namespace WebDavCommon.Metadatas
{
    /// <summary>This class provides simulation of synchronization api.</summary>
    /// <remarks>All not deleted items marks as changed. </remarks>
    public class SynchronizationSimulator
    {
        /// <summary>The dictionary of saved states.</summary>
        private readonly IDictionary<uint, string[]> changeSets;

        /// <summary>The current version of state.</summary>
        private uint currentAnchor;

        /// <summary>Initializes a new instance of the <see cref="SynchronizationSimulator"/> class.</summary>
        public SynchronizationSimulator()
        {
            this.changeSets = new Dictionary<uint, string[]>();
        }

        /// <summary>Trims local history to provided version</summary>
        /// <param name="version">The target version. </param>
        public void TrimToChangeSet(uint version)
        {
            uint[] oldChangeSets = this.changeSets.Keys.Where(k => k < version).ToArray();
            foreach (uint changeSet in oldChangeSets) this.changeSets.Remove(changeSet);
        }

        /// <summary>Add <paramref name="metadata"/> to history.</summary>
        /// <param name="metadata">The metadatas.</param>
        /// <returns>The <see cref="uint"/> new latest version.</returns>
        public uint AddChangeSet(ItemMetadata[] metadata)
        {
            string[] ids = metadata.Select(m => m.Identifier).ToArray();
            this.changeSets.Add(this.currentAnchor, ids);
            return this.currentAnchor++;
        }

        /// <summary>Returns diff between <paramref name="version"/> version in history with <paramref name="metadata"/>.</summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="metadata">The metadatas.</param>
        /// <returns>The <see cref="ChangeSet"/>.</returns>
        /// <remarks>All not deleted items marks as changed. </remarks>
        public ChangeSet GetDiff(uint anchor, ItemMetadata[] metadata)
        {
            string[] changeSetContent = this.GetChangeSetOrEmpty(anchor);
            string[] removed = changeSetContent.Where(item => !metadata.Any(ch => ch.Identifier == item)).ToArray();
            return new ChangeSet(removed, metadata);
        }

        /// <summary>Add <paramref name="metadata"/> to history.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns>The <see cref="uint"/>.</returns>
        public uint AddChangeSet(FileMetadata metadata)
        {
            return this.AddChangeSet(new ItemMetadata[] { metadata });
        }

        /// <summary>Returns diff between <paramref name="version"/> version to current.</summary>
        /// <param name="version">The version.</param>
        /// <returns>The <see cref="string"/> array of identifiers.</returns>
        private string[] GetChangeSetOrEmpty(uint version)
        {
            if (!this.changeSets.ContainsKey(version)) return new string[] { };
            return this.changeSets[version];
        }
    }
}
