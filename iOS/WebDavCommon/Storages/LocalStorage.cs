using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using FileProvider;

using Foundation;

using WebDavCommon.Extensions;

namespace WebDavCommon.Storages
{
    /// <summary>This class provides methods to work with items on local filesystem.</summary>
    public class LocalStorage
    {
        /// <summary>The default storage folder.</summary>
        private const string DefaultStorageFolder = "Documents";

        /// <summary>The extended attribute storage.</summary>
        private readonly ExtendedAttributeStorage extendedAttributeStorage;

        /// <summary>Initializes a new instance of the <see cref="LocalStorage"/> class.</summary>
        public LocalStorage()
        {
            this.extendedAttributeStorage = new ExtendedAttributeStorage();
            using (NSUrl documentStorageUrl = NSFileProviderManager.DefaultManager.DocumentStorageUrl.Append(DefaultStorageFolder, true))
            {
               this.StorageRootPath = documentStorageUrl.Path;
            }

            this.InitLocalStorage();
        }

        /// <summary>Gets the storage root path.</summary>
        public string StorageRootPath { get; }

        /// <summary>Returns <see cref="LocalItem"/> for provided path.</summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>The <see cref="LocalItem"/>.</returns>
        public LocalItem GetItem(string localPath)
        {
            var isDirectory = false;
            bool isExists = NSFileManager.DefaultManager.FileExists(localPath, ref isDirectory);
            if (isDirectory) return new LocalFolder(localPath, isExists);

            return this.GetFile(localPath, isExists);
        }

        /// <summary>Gets folder content as <see cref="LocalItem"/> collection.</summary>
        /// <param name="folder">The folder.</param>
        /// <returns>The <see cref="LocalItem"/> array.</returns>
        /// <exception cref="NSErrorException"> if directory enumeration failed. </exception>
        public LocalItem[] GetFolderContent(LocalFolder folder)
        {
            if (!folder.IsExists) return new LocalItem[0];

            NSError error;
            NSUrl[] files = NSFileManager.DefaultManager.GetDirectoryContent(
                NSUrl.FromFilename(folder.Path),
                null,
                NSDirectoryEnumerationOptions.SkipsHiddenFiles,
                out error);

            error.ThrowNotNullAsException();
            return files.Select(f => this.GetItem(f.Path)).ToArray();
        }

        /// <summary>Gets folder content as <see cref="LocalItem"/> collection.</summary>
        /// <param name="localFolder">The local folder.</param>
        /// <returns>The <see cref="LocalItem"/> array.</returns>
        public LocalItem[] GetFolderContentOrEmpty(LocalFolder localFolder)
        {
            if (!localFolder.IsExists) return new LocalItem[0];
            return this.GetFolderContent(localFolder);
        }

        /// <summary>Deletes item.</summary>
        /// <param name="item">The item.</param>
        /// <exception cref="NSErrorException"> if deletion failed.</exception>
        public void Delete(LocalItem item)
        {
            using (NSUrl url = NSUrl.FromFilename(item.Path))
            {
                NSError removeError;
                NSFileManager.DefaultManager.Remove(url, out removeError);
                removeError.ThrowNotNullAsException();
            }
        }

        /// <summary>The update file data except content.</summary>
        /// <param name="localFile">The local file.</param>
        /// <returns>The <see cref="LocalFile"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="localFile"/> is null. </exception>
        public LocalFile UpdateFile(LocalFile localFile)
        {
            if (localFile == null) throw new ArgumentNullException(nameof(localFile));
            if (NSFileManager.DefaultManager.FileExists(localFile.Path))
            {
                this.extendedAttributeStorage.Write(localFile.Path, localFile.ExtendedAttribute);
            }

            return this.GetFile(localFile.Path);
        }

        /// <summary>Returns <see cref="LocalFile"/> for provided path.</summary>
        /// <param name="path">The local path.</param>
        /// <returns>The <see cref="LocalFile"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        /// <exception cref="ArgumentException"> if <paramref name="path"/> is not folder path. </exception>
        public LocalFile GetFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            bool isDirectory = false;
            bool isExists = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
            if (isDirectory)
            {
                throw new ArgumentException($"{path} is not a file path.", nameof(path));
            }

            return this.GetFile(path, isExists);
        }

        /// <summary>Returns <see cref="LocalFolder"/> for provided path.</summary>
        /// <param name="path">The local path.</param>
        /// <returns>The <see cref="LocalFolder"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        /// <exception cref="ArgumentException"> if <paramref name="path"/> is not folder path. </exception>
        public LocalFolder GetFolder(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            bool isDirectory = true;
            bool isExists = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
            if (!isDirectory)
            {
                throw new ArgumentException($"{path} is not a folder path.", nameof(path));
            }

            return new LocalFolder(path, isExists);
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
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (destinationFolder == null) throw new ArgumentNullException(nameof(destinationFolder));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!destinationFolder.IsExists)
            {
                Directory.CreateDirectory(destinationFolder.Path);
            }

            string newPath = Path.Combine(destinationFolder.Path, name);
            if (item is LocalFile) File.Move(item.Path, newPath);
            else Directory.Move(item.Path, newPath);
        }

        /// <summary>Cleans storage.</summary>
        public void Clean()
        {
            using (var url = NSUrl.FromFilename(this.StorageRootPath))
            {
                NSError error;
                NSFileManager.DefaultManager.Remove(url, out error);
                error.ThrowNotNullAsException();
            }
        }

        /// <summary>Returns <see cref="LocalFile"/> for provided path.</summary>
        /// <param name="path">The local path.</param>
        /// <param name="isExists">The value indicating whether is item exists. </param>
        /// <returns>The <see cref="LocalFile"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        private LocalFile GetFile(string path, bool isExists)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (!isExists) return LocalFile.CreateNotExist(path);
            ulong fileSize = NSFileManager.DefaultManager.GetAttributes(path).Size.GetValueOrDefault();
            try
            {
                ExtendedAttribute extendedAttribute = this.extendedAttributeStorage.Get(path);
                return LocalFile.CreateExists(path, fileSize, extendedAttribute);
            }
            catch (SerializationException)
            {
                this.extendedAttributeStorage.Delete(path);
                return LocalFile.CreateExists(path, fileSize);
            }
        }

        /// <summary>The init local storage.</summary>
        private void InitLocalStorage()
        {
            using (var url = NSUrl.FromFilename(this.StorageRootPath))
            {
                NSError error;
                NSFileManager.DefaultManager.CreateDirectory(url, true, null, out error);
                error.ThrowNotNullAsException();
            }
        }
    }
}
