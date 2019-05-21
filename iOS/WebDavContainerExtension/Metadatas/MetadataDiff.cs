namespace WebDavContainerExtension.Metadatas
{
    public class MetadataDiff
    {
        public string[] DeletedId { get; set; }
        public ItemMetadata[] Updated { get; set; }

        public MetadataDiff(string[] deleted, ItemMetadata[] updated)
        {
            DeletedId = deleted;
            Updated = updated;
        }

        public MetadataDiff()
        {
            DeletedId =new string[0];
            Updated = new ItemMetadata[0];
        }
    }
}