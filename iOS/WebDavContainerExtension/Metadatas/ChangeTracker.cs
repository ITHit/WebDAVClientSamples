using System.Collections.Generic;
using System.Linq;


namespace WebDavContainerExtension.Metadatas
{
    public class ChangeTracker
    {
        private readonly IDictionary<uint, string[]> changeSets;

        private uint currentAnchor;

        public ChangeTracker()
        {
            this.changeSets = new Dictionary<uint, string[]>();
        }


        private string[] GetChangeSetOrEmpty(uint anchor)
        {
            if(!changeSets.ContainsKey(anchor)) return new string[] { };
            return changeSets[anchor];
        }

        public void TrimToChangeSet(uint anchor)
        {
            uint[] oldChangeSets = changeSets.Keys.Where(k => k < anchor).ToArray();
            foreach(uint changeSet in oldChangeSets)
            {
                changeSets.Remove(changeSet);
            }
        }

        public uint AddChangeSet(ItemMetadata[] metadatas)
        {
            string[] ids = metadatas.Select(m => m.Identifier).ToArray();
            this.changeSets.Add(this.currentAnchor, ids);
            return this.currentAnchor++;
        }

        public MetadataDiff GetDiff(uint anchor, ItemMetadata[] metadatas)
        {
            var changeSetContent = GetChangeSetOrEmpty(anchor);
            var removed = changeSetContent.Where(item => !metadatas.Any(ch => ch.Identifier == item)).ToArray();
            return new MetadataDiff(removed, metadatas);
        }

        public uint AddChangeSet(FileMetadata metadata)
        {
            return this.AddChangeSet(new ItemMetadata[] {metadata});
        }
    }
}