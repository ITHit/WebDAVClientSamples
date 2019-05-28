using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Foundation;

using WebDavContainerExtension.Extensions;
using WebDavContainerExtension.Helpers;

namespace WebDavContainerExtension.Storages
{
    public class LocalStorage
    {
        private readonly IFormatter _fileExtendedAttributeSerializer;
        private const string ExtendedAttributeKey = "FsExtensionMetadata";

        public LocalStorage()
        {
            _fileExtendedAttributeSerializer = new BinaryFormatter();
        }

        private LocalFile GetFile(string localPath, bool isExists)
        {
            if(localPath == null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            if(!isExists)
        {
                return LocalFile.CreateNotExist(localPath);
               }

            ulong fileSize = NSFileManager.DefaultManager.GetAttributes(localPath).Size.GetValueOrDefault();
            try
            {
                FileExtendedAttribute fileExtendedAttribute = this.GetFileExtendedAttribute(localPath);
                return LocalFile.CreateExists(localPath, fileSize, fileExtendedAttribute);

                    }
            catch (SerializationException)
            {
                NSFileManagerHelper.DeleteExtendedAttribute(localPath, ExtendedAttributeKey);
                return LocalFile.CreateExists(localPath, fileSize);
            }
        }

        protected FileExtendedAttribute  GetFileExtendedAttribute(string localPath)
        {
            if (localPath == null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            byte[] extendedAttribute = NSFileManagerHelper.GetExtendedAttributeBytes(localPath, ExtendedAttributeKey);
            if (extendedAttribute == null)
            {
                return null;
            }
        
            using (MemoryStream stream = new MemoryStream(extendedAttribute))
            {
                return (FileExtendedAttribute)_fileExtendedAttributeSerializer.Deserialize(stream);
            }
        }

        public LocalItem GetItem(string localPath)
        {
            bool isDirectory = false;
            bool isExists = NSFileManager.DefaultManager.FileExists(localPath, ref isDirectory);
            if (isDirectory)
            {
                return new LocalFolder(localPath, isExists);
            }

            return GetFile(localPath, isExists);
        }

        public LocalItem[] GetFolderContent(LocalFolder folder)
        {
            if(!folder.IsExists) return new LocalItem[0];
            NSError error;
            NSUrl[] files = NSFileManager.DefaultManager.GetDirectoryContent(NSUrl.FromFilename(folder.Path),
                                                                             null,
                                                                             NSDirectoryEnumerationOptions.SkipsHiddenFiles,
                                                                             out error);

            if(error != null)
            {
                throw error.AsException();
            }

            return files.Select(f => GetItem(f.Path)).ToArray();
        }

        public void Delete(LocalItem item)
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

        public LocalFile UpdateFile(LocalFile localFile)
        {
            if (NSFileManager.DefaultManager.FileExists(localFile.Path))
            {
                WriteFileExtendedAttribute(localFile, localFile.ExtendedAttribute);
            }

            return GetFile(localFile.Path);
        }

        private void WriteFileExtendedAttribute(LocalFile itemLocalItem, FileExtendedAttribute fileExtendedAttribute)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                _fileExtendedAttributeSerializer.Serialize(stream, fileExtendedAttribute);
                stream.Flush();
                stream.Position = 0;
                NSFileManagerHelper.SetExtendedAttributeBytes(itemLocalItem.Path, ExtendedAttributeKey, stream.ToArray());
            }
        }

        public LocalFile GetFile(string localPath)
        {
            bool isExists = NSFileManager.DefaultManager.FileExists(localPath);
            return GetFile(localPath, isExists);
    }

        public LocalFolder GetFolder(string localPath)
        {
            bool isExists = NSFileManager.DefaultManager.FileExists(localPath);
            return new LocalFolder(localPath, isExists);
        }

        /// <summary>
        /// Moves item in local storage. Creates <paramref name="destinationFolder"/> if not exists.
        /// </summary>
        /// <param name="item"> The item to move. </param>
        /// <param name="destinationFolder"> The destination folder. </param>
        /// <param name="name"> New item name. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="item"/> is null or <paramref name="destinationFolder"/> is null or if <paramref name="name"/> is null or empty.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="IOException">
        /// The destination item already exists or <paramref name="item"/> not found or destination item already exists.
        /// </exception>
        public void Move(LocalItem item, LocalFolder destinationFolder, string name)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!destinationFolder.IsExists)
            {
                Directory.CreateDirectory(destinationFolder.Path);
            }

            string newPath = Path.Combine(destinationFolder.Path, name);
            if (item.IsFile)
            {
                File.Move(item.Path, newPath);
            }
            else
            {
                Directory.Move(item.Path, newPath);
            }
        }
    }
}