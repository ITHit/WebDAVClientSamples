using System;
using System.Collections.Generic;
using System.Linq;

using FileProvider;

using Foundation;

using ITHit.WebDAV.Client;
using ITHit.WebDAV.Client.Exceptions;

using WebDavCommon.Storages;

namespace WebDavCommon.Metadatas
{
    /// <summary>This class provides methods for manage local and remote storages.</summary>
    public class StorageManager
    {
        /// <summary>The session.</summary>
        private readonly WebDavSessionAsync session;

        /// <summary>Initializes a new instance of the <see cref="StorageManager"/> class.</summary>
        /// <param name="locationMapper">The location mapper.</param>
        /// <param name="session">The session.</param>
        /// <param name="localStorage">The local storage.</param>
        public StorageManager(LocationMapper locationMapper, WebDavSessionAsync session, LocalStorage localStorage)
        {
            this.LocationMapper = locationMapper;
            this.session = session;
            this.LocalStorage = localStorage;
        }

        /// <summary>Gets the location mapper.</summary>
        public LocationMapper LocationMapper { get; }

        /// <summary>Gets the local storage.</summary>
        public LocalStorage LocalStorage { get; }

        /// <summary>Notifies system that items with <paramref name="itemIdentifiers"/> changed. </summary>
        /// <param name="itemIdentifiers">The item identifiers.</param>
        public void NotifyEnumerator(params string[] itemIdentifiers)
        {
            foreach (string itemIdentifier in itemIdentifiers)
            {
                NSFileProviderManager.DefaultManager.SignalEnumerator(itemIdentifier, error => { });
            }
        }

        /// <summary>Gets folder's local and remote state. </summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="FolderMetadata"/>.</returns>
        public FolderMetadata GetFolderMetadata(string itemIdentifier)
        {
            try
            {
                Uri serverUri = this.LocationMapper.GetServerUriFromIdentifier(itemIdentifier);
                IFolderAsync serverItem = this.session.OpenFolderAsync(serverUri).GetAwaiter().GetResult();

                string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
                return this.CreateFolderMetadata(itemIdentifier, localItem, serverItem);
            }
            catch (NotFoundException)
            {
                string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
                return this.CreateFolderMetadata(itemIdentifier, localItem);
            }
        }

        /// <summary>Creates folder on server only.</summary>
        /// <param name="folderMetadata">The folder metadata.</param>
        /// <param name="folderName">The directory name.</param>
        /// <returns>The <see cref="FolderMetadata"/>.</returns>
        public FolderMetadata CreateFolderOnServer(FolderMetadata folderMetadata, string folderName)
        {
            IFolderAsync newFolder = folderMetadata.ServerFolder.CreateFolderAsync(folderName).GetAwaiter().GetResult();

            string id = this.LocationMapper.GetIdentifierFromServerUri(newFolder.Href);
            string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(id);
            LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
            return this.CreateFolderMetadata(id, localItem, newFolder);
        }

        /// <summary>Gets item's local and remote state. </summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="ItemMetadata"/>.</returns>
        public ItemMetadata GetItemMetadata(string itemIdentifier)
        {
            if (this.LocationMapper.IsFolderIdentifier(itemIdentifier))
            {
                return this.GetFolderMetadata(itemIdentifier);
            }

            return this.GetFileMetadata(itemIdentifier);
        }

        /// <summary>Gets file's local and remote state. </summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata GetFileMetadata(string itemIdentifier)
        {
            try
            {
                Uri serverUri = this.LocationMapper.GetServerUriFromIdentifier(itemIdentifier);
                IFileAsync serverItem = this.session.OpenFileAsync(serverUri).GetAwaiter().GetResult();

                string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFile localItem = this.LocalStorage.GetFile(localPath);
                return this.CreateFileMetadata(itemIdentifier, localItem, serverItem);
            }
            catch (NotFoundException)
            {
                string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFile localItem = this.LocalStorage.GetFile(localPath);
                return this.CreateFileMetadata(itemIdentifier, localItem);
            }
        }

        /// <summary>
        /// Moves item on local filesystem and remote if it exists.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="destinationFolder"> The destination folder.</param>
        /// <param name="name">New item's name.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="item"/> is null or <paramref name="destinationFolder"/> is null.
        /// </exception>
        public void MoveItem(ItemMetadata item, FolderMetadata destinationFolder, string name)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (destinationFolder == null) throw new ArgumentNullException(nameof(destinationFolder));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (item.ExistsOnServer)
            {
                this.MoveItemOnServer(item, destinationFolder, name);
            }

            if (item.ExistsLocal)
            {
                this.LocalStorage.Move(item.LocalItem, destinationFolder.LocalFolder, name);
            }
        }

        /// <summary>Deletes item in all locations</summary>
        /// <param name="itemMetadata">The item metadata.</param>
        public void Delete(ItemMetadata itemMetadata)
        {
            if (itemMetadata.ExistsLocal)
            {
                this.LocalStorage.Delete(itemMetadata.LocalItem);
            }

            if (!itemMetadata.ExistsOnServer) return;
            try
            {
                itemMetadata.ServerItem.DeleteAsync().GetAwaiter().GetResult();
            }
            catch (NotFoundException)
            {
            }
        }

        /// <summary>Retrieves folder's local and remote content.</summary>
        /// <param name="folderMetadata">The folder metadata.</param>
        /// <returns>The <see cref="ItemMetadata"/> array.</returns>
        public ItemMetadata[] GetFolderChildrenMetadatas(FolderMetadata folderMetadata)
        {
            IHierarchyItemAsync[] serverItems = GetRemoteFolderContentOrEmpty(folderMetadata);
            LocalItem[] localItems = this.LocalStorage.GetFolderContentOrEmpty(folderMetadata.LocalFolder);
            return this.CreateItemMetadatas(folderMetadata.Identifier, localItems, serverItems);
        }

        /// <summary>Creates file on server.</summary>
        /// <param name="parentMetadata">The parent metadata.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata CreateFileOnServer(FolderMetadata parentMetadata, string fileName)
        {
            IFileAsync newFile = parentMetadata.ServerFolder.CreateFileAsync(fileName, null).GetAwaiter().GetResult();
            string identifier = this.LocationMapper.GetIdentifierFromServerUri(newFile.Href);
            string localPath = this.LocationMapper.GetLocalUrlFromIdentifier(identifier);
            LocalFile localItem = this.LocalStorage.GetFile(localPath);
            string name = this.GetItemDisplayName(identifier, newFile);
            return new FileMetadata(identifier, parentMetadata.Identifier, name, localItem, newFile);
        }

        /// <summary>Writes content on server.</summary>
        /// <param name="fileMetadata">The created file.</param>
        /// <param name="fileUrlPath">The file url path.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata WriteFileContentOnServer(FileMetadata fileMetadata, string fileUrlPath)
        {
            IFileAsync serverItem = fileMetadata.ServerFile;
            serverItem.TimeOut = 36000000;
            serverItem.UploadAsync(fileUrlPath).GetAwaiter().GetResult();
            return this.GetFileMetadata(fileMetadata.Identifier);
        }

        /// <summary>Updates file local state.</summary>
        /// <param name="file">The item.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata UpdateFileLocal(FileMetadata file)
        {
            LocalFile localFile = this.LocalStorage.UpdateFile(file.LocalFile);
            string name = this.GetItemDisplayName(file.Identifier, file.ServerFile);
            return new FileMetadata(file.Identifier, file.ParentIdentifier, name, localFile, file.ServerFile);
        }

        /// <summary>Push file local content to server.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata PushToServer(FileMetadata item)
        {
            if (!item.ExistsOnServer)
            {
                item = this.CreateOnServer(item);
            }

            item = this.WriteFileContentOnServer(item, item.LocalFile.Path);
            item.LocalFile.Etag = item.ServerFile.Etag;
            item.LocalFile.UploadError = null;
            this.LocalStorage.UpdateFile(item.LocalFile);
            return item;
        }

        /// <summary>Moves item on server only.</summary>
        /// <param name="item">The item.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="name">The name.</param>
        public void MoveItemOnServer(ItemMetadata item, FolderMetadata destinationFolder, string name)
        {
            item.ServerItem.MoveToAsync(destinationFolder.ServerFolder, name, false, null).GetAwaiter().GetResult();
        }

        /// <summary>Get items from server and saves local.</summary>
        /// <param name="item">The item.</param>
        public void PullFromServer(FileMetadata item)
        {
            IFileAsync serverItem = item.ServerFile;
            item.ServerFile.TimeOut = 36000000;
            serverItem.DownloadAsync(item.LocalFile.Path).GetAwaiter().GetResult();
            item.LocalFile.Etag = item.ServerFile.Etag;
            this.LocalStorage.UpdateFile(item.LocalFile);
        }

        /// <summary>Deletes local copy of item.</summary>
        /// <param name="item">The item.</param>
        public void DeleteLocal(ItemMetadata item)
        {
            this.LocalStorage.Delete(item.LocalItem);
        }

        /// <summary>Mark item with upload error.</summary>
        /// <param name="url">The url.</param>
        /// <param name="localFileUploadError">The local file upload error.</param>
        public void SetUploadError(NSUrl url, NSError localFileUploadError)
        {
            LocalFile localFile = this.LocalStorage.GetFile(url.Path);
            localFile.UploadError = localFileUploadError;
            localFile.Etag = null;
            this.LocalStorage.UpdateFile(localFile);
        }

        /// <summary>Resets version of local copy.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        public FileMetadata ResetLocalVersion(FileMetadata item)
        {
            item.LocalFile.Etag = null;
            item = this.UpdateFileLocal(item);
            return item;
        }

        /// <summary>Returns folder's remote content or empty if not exists.</summary>
        /// <param name="folderMetadata">The folder metadata.</param>
        /// <returns>The <see cref="IHierarchyItemAsync"/> array.</returns>
        private static IHierarchyItemAsync[] GetRemoteFolderContentOrEmpty(FolderMetadata folderMetadata)
        {
            if (!folderMetadata.ExistsOnServer)
            {
                return new IHierarchyItemAsync[0];
            }

            try
            {
                return folderMetadata.ServerFolder.GetChildrenAsync(false).GetAwaiter().GetResult();
            }
            catch (NotFoundException)
            {
                return new IHierarchyItemAsync[0];
            }
        }

        /// <summary>Creates file on server without local copy.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        private FileMetadata CreateOnServer(FileMetadata item)
        {
            FolderMetadata parent = this.GetFolderMetadata(item.ParentIdentifier);
            return this.CreateFileOnServer(parent, item.Name);
        }

        /// <summary>Creates folder on server without local copy.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FolderMetadata"/>.</returns>
        private FolderMetadata CreateOnServer(FolderMetadata item)
        {
            FolderMetadata parent = this.GetFolderMetadata(item.ParentIdentifier);
            return this.CreateFolderOnServer(parent, item.Name);
        }

        /// <summary>Create item metadatas.</summary>
        /// <param name="parentIdentifier">The parent identifier.</param>
        /// <param name="localItems">The local items.</param>
        /// <param name="serverItems">The server items.</param>
        /// <returns>The <see cref="ItemMetadata"/> array.</returns>
        private ItemMetadata[] CreateItemMetadatas(
            string parentIdentifier,
            LocalItem[] localItems,
            IHierarchyItemAsync[] serverItems)
        {
            Dictionary<string, LocalItem> localDictionary = localItems.ToDictionary(k => k.Path);
            var result = new List<ItemMetadata>();
            foreach (IHierarchyItemAsync hierarchyItemAsync in serverItems)
            {
                string localUrl = this.LocationMapper.GetLocalUrlFromServerUri(hierarchyItemAsync.Href);
                string identifier = this.LocationMapper.GetIdentifierFromServerUri(hierarchyItemAsync.Href);
                if (hierarchyItemAsync is IFolderAsync folder)
                {
                    if (localDictionary.ContainsKey(localUrl) && localDictionary[localUrl] is LocalFolder)
                    {
                        var localItem = localDictionary[localUrl] as LocalFolder;
                        string name = this.GetItemDisplayName(identifier, folder);
                        result.Add(new FolderMetadata(identifier, parentIdentifier, name, localItem, folder));
                        localDictionary.Remove(localUrl);
                    }

                    LocalFolder localItem1 = this.LocalStorage.GetFolder(localUrl);
                    string name1 = this.GetItemDisplayName(identifier, folder);
                    result.Add(new FolderMetadata(identifier, parentIdentifier, name1, localItem1, folder));
                }

                if (hierarchyItemAsync is IFileAsync file)
                {
                    if (localDictionary.ContainsKey(localUrl) && localDictionary[localUrl] is LocalFile)
                    {
                        var localItem = localDictionary[localUrl] as LocalFile;
                        string name = this.GetItemDisplayName(identifier, file);
                        result.Add(new FileMetadata(identifier, parentIdentifier, name, localItem, file));
                        localDictionary.Remove(localUrl);
                    }

                    LocalFile localItem1 = this.LocalStorage.GetFile(localUrl);
                    string name1 = this.GetItemDisplayName(identifier, file);
                    result.Add(new FileMetadata(identifier, parentIdentifier, name1, localItem1, file));
                }
            }

            foreach (KeyValuePair<string, LocalItem> dictionaryItem in localDictionary)
            {
                string identifier = this.LocationMapper.GetIdentifierFromLocalPath(dictionaryItem.Key);
                if (dictionaryItem.Value is LocalFile)
                {
                    result.Add(this.CreateFileMetadata(identifier, dictionaryItem.Value as LocalFile));
                }

                if (dictionaryItem.Value is LocalFolder)
                {
                    result.Add(this.CreateFolderMetadata(identifier, dictionaryItem.Value as LocalFolder));
                }
            }

            return result.ToArray();
        }

        /// <summary>Calculate item's name.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <param name="serverItem">The server item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetItemDisplayName(string itemIdentifier, IHierarchyItemAsync serverItem)
        {
            return serverItem?.DisplayName ?? this.LocationMapper.GetNameFromIdentifier(itemIdentifier);
        }

        /// <summary>Creates file metadata.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <param name="localItem">The local item.</param>
        /// <param name="serverItem">The server item.</param>
        /// <returns>The <see cref="FileMetadata"/>.</returns>
        private FileMetadata CreateFileMetadata(
            string itemIdentifier,
            LocalFile localItem,
            IFileAsync serverItem = null)
        {
            string parentIdentifier = this.LocationMapper.GetParentIdentifier(itemIdentifier);
            string name = this.GetItemDisplayName(itemIdentifier, serverItem);
            return new FileMetadata(itemIdentifier, parentIdentifier, name, localItem, serverItem);
        }

        /// <summary>Creates folder metadata.</summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <param name="localItem">The local item.</param>
        /// <param name="serverItem">The server item.</param>
        /// <returns>The <see cref="FolderMetadata"/>.</returns>
        private FolderMetadata CreateFolderMetadata(
            string itemIdentifier,
            LocalFolder localItem,
            IFolderAsync serverItem = null)
        {
            string parentIdentifier = this.LocationMapper.GetParentIdentifier(itemIdentifier);
            string name = this.GetItemDisplayName(itemIdentifier, serverItem);
            return new FolderMetadata(itemIdentifier, parentIdentifier, name, localItem, serverItem);
        }
    }
}
