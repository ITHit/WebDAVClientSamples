using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using WebDavContainerExtension.Extensions;
using Foundation;
using WebDavContainerExtension.Helpers;
using System.Runtime.Serialization.Formatters.Binary;

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
    }
}