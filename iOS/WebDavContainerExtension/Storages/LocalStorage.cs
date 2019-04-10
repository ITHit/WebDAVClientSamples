using System.Linq;
using WebDavContainerExtension.Extensions;
using Foundation;
using WebDavContainerExtension.Helpers;

namespace WebDavContainerExtension.Storages
{
    public class LocalStorage
    {

        public LocalStorage()
        {
        }

        private const string ExtendedAttributeKey = "FsExtensionMetadata";

        public LocalFile GetFile(string localPath)
        {
            if (!NSFileManager.DefaultManager.FileExists(localPath)) {
                return new LocalFile(localPath, false);
               }

            string extendedAttributes = NSFileManagerHelper.GetExtendedAttribute(localPath, ExtendedAttributeKey);

                return new LocalFile(localPath, true)
                {
                    Size = NSFileManager.DefaultManager.GetAttributes(localPath).Size.GetValueOrDefault(),
                    Etag = extendedAttributes
                };

        }

        public LocalFolder GetFolder(string localPath)
        {
            if (!NSFileManager.DefaultManager.FileExists(localPath))
            {
                return new LocalFolder(localPath, false);
            }

            return new LocalFolder(localPath, true);
        }

        public LocalItem GetItem(string localPath)
        {
            bool isDirectory = false;
            NSFileManager.DefaultManager.FileExists(localPath, ref isDirectory);
            if (isDirectory)
            {
                return GetFolder(localPath);
            }

            return GetFile(localPath);
        }

        public LocalItem[] GetFolderContent(LocalFolder folder)
        {
            if(!folder.IsExists) return new LocalItem[0];
            NSError error = null;
            NSUrl[] files = NSFileManager.DefaultManager.GetDirectoryContent(NSUrl.FromFilename(folder.Path), null, NSDirectoryEnumerationOptions.SkipsHiddenFiles, out error);
            if(error != null)
            {
                throw error.AsException();
            }


            return files.Select(f => GetItem(f.Path)).ToArray();
        }

        public void CoordinatedDelete(LocalItem item)
        {
            using (NSUrl url = NSUrl.FromFilename(item.Path))
            {
                NSError removeError;
                NSFileManager.DefaultManager.Remove(url, out removeError);
                if(removeError != null)
                {
                    throw removeError.AsException();
                }
            }
        }

        public LocalFile UpdateFile(LocalFile itemLocalItem)
        {
            string etag = itemLocalItem.Etag ?? string.Empty;
            NSFileManagerHelper.SetExtendedAttribute(itemLocalItem.Path, ExtendedAttributeKey, etag);
            return GetFile(itemLocalItem.Path);
        }

    }
}