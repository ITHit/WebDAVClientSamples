namespace WebDavContainerExtension.Storages
{
    public class LocalFolder: LocalItem
    {
        public LocalFolder(string localPath, bool exists) : base(localPath, exists)
        {
            if(!Path.EndsWith(System.IO.Path.DirectorySeparatorChar))
            {
                Path = Path += System.IO.Path.DirectorySeparatorChar;
            }
        }
    }
}