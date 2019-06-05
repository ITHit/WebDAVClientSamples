namespace WebDavCommon.Metadatas
{
    public class ChangeSet
    {
        public string[] DeletedId { get; set; }
        public ItemMetadata[] Updated { get; set; }

        public ChangeSet(string[] deleted, ItemMetadata[] updated)
        {
            this.DeletedId = deleted;
            this.Updated = updated;
        }

        public ChangeSet()
        {
            this.DeletedId =new string[0];
            this.Updated = new ItemMetadata[0];
        }
    }
}