using System;
using System.Collections.Generic;
using System.Linq;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client;
using ITHit.WebDAV.Client.Exceptions;
using WebDavContainerExtension.Storages;

namespace WebDavContainerExtension.Metadatas
{
    public class StorageManager
    {
        private readonly WebDavSessionAsync Session;
        public LocationMapper LocationMapper { get; }
        public LocalStorage LocalStorage { get; }

        public StorageManager(LocationMapper locationMapper, WebDavSessionAsync session, LocalStorage localStorage)
        {
            LocationMapper = locationMapper;
            Session = session;
            LocalStorage = localStorage;
        }


        public void NotifyEnumerator(params string[] itemIdentifiers)
        {
            foreach(string itemIdentifier in itemIdentifiers)
            {
                NSFileProviderManager.DefaultManager.SignalEnumerator(itemIdentifier, error => { });
            }
        }

        public FolderMetadata GetFolderMetadata(string itemIdentifier)
        {
            try
            {
                Uri serverUri = LocationMapper.GetServerUriFromIdentifier(itemIdentifier);
                IFolderAsync serverItem = Session.OpenFolderAsync(serverUri)
                                                 .GetAwaiter()
                                                 .GetResult();

                string localPath = LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
                return this.CreateFolderMetadata(itemIdentifier, localItem, serverItem);
            }
            catch(NotFoundException)
            {
                var localPath = LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
                return this.CreateFolderMetadata(itemIdentifier, localItem);
            }
        }

        private FolderMetadata CreateFolderMetadata(string itemIdentifier, LocalFolder localItem, IFolderAsync serverItem = null)
        {
            string parentIdentifier = LocationMapper.GetParentIdentifier(itemIdentifier);
            string name = GetItemDisplayName(itemIdentifier, serverItem);
            return new FolderMetadata(itemIdentifier, parentIdentifier, name, localItem, serverItem);
        }

        public FolderMetadata CreateFolderOnServer(FolderMetadata folderMetadata, string directoryName)
        {
            IFolderAsync newFolder = folderMetadata.ServerFolder.CreateFolderAsync(directoryName)
                                                   .GetAwaiter()
                                                   .GetResult();

            string id = LocationMapper.GetIdentifierFromServerUri(newFolder.Href);
            string localPath = LocationMapper.GetLocalUrlFromIdentifier(id);
            LocalFolder localItem = this.LocalStorage.GetFolder(localPath);
            return CreateFolderMetadata(id, localItem, newFolder);
        }

        public ItemMetadata GetItemMetadata(string itemIdentifier)
        {
            if(LocationMapper.IsFolderIdentifier(itemIdentifier))
            {
                return GetFolderMetadata(itemIdentifier);
            }

            return GetFileMetadata(itemIdentifier);
        }

        public FileMetadata GetFileMetadata(string itemIdentifier)
        {
            try
            {
                Uri serverUri = LocationMapper.GetServerUriFromIdentifier(itemIdentifier);
                IFileAsync serverItem = Session.OpenFileAsync(serverUri)
                                               .GetAwaiter()
                                               .GetResult();

                string localPath = LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFile localItem = this.LocalStorage.GetFile(localPath);
                return this.CreateFileMetadata(itemIdentifier, localItem, serverItem);
            }
            catch(NotFoundException)
            {
                string localPath = LocationMapper.GetLocalUrlFromIdentifier(itemIdentifier);
                LocalFile localItem = this.LocalStorage.GetFile(localPath);
                return this.CreateFileMetadata(itemIdentifier, localItem);
            }
        }

        private FileMetadata CreateFileMetadata(string itemIdentifier, LocalFile localItem, IFileAsync serverItem = null)
        {
            string parentIdentifier = LocationMapper.GetParentIdentifier(itemIdentifier);
            string name = GetItemDisplayName(itemIdentifier, serverItem);
            return new FileMetadata(itemIdentifier, parentIdentifier, name, localItem, serverItem);
        }


        public void DeleteEveryWhere(ItemMetadata itemMetadata)
        {
            if(itemMetadata.ExistsLocal)
            {
                LocalStorage.Delete(itemMetadata.LocalItem);
            }

            if(itemMetadata.ExistsOnServer)
            {
                try
                {
                    itemMetadata.ServerItem.DeleteAsync().GetAwaiter().GetResult();
                }
                catch(NotFoundException)
                {
                }
            }
        }

        public ItemMetadata[] GetFolderChildrenMetadatas(FolderMetadata folderMetadata)
        {
            IHierarchyItemAsync[] serverItems;
            if(folderMetadata.ExistsOnServer)
            {
                try
                {
                    serverItems = folderMetadata.ServerFolder.GetChildrenAsync(false)
                                                .GetAwaiter()
                                                .GetResult();
                }
                catch(NotFoundException)
                {
                    serverItems = new IHierarchyItemAsync[0];
                }
            }
            else
            {
                serverItems = new IHierarchyItemAsync[0];
            }

            LocalItem[] localItems;
            if(folderMetadata.ExistsLocal)
            {
                localItems = LocalStorage.GetFolderContent(folderMetadata.LocalFolder);
            }
            else
            {
                localItems = new LocalItem[0];
            }

            return CreateItemMetadatas(folderMetadata.Identifier, localItems, serverItems);
        }

        private ItemMetadata[] CreateItemMetadatas(string parentIdentifier, LocalItem[] localItems, IHierarchyItemAsync[] serverItems)
        {
            var localDictionary = localItems.ToDictionary(k => k.Path);
            var result = new List<ItemMetadata>();
            foreach(IHierarchyItemAsync hierarchyItemAsync in serverItems)
            {
                string localUrl = LocationMapper.GetLocalUrlFromServerUri(hierarchyItemAsync.Href);
                string identifier = LocationMapper.GetIdentifierFromServerUri(hierarchyItemAsync.Href);
                if(hierarchyItemAsync is IFolderAsync folder)
                {
                    if(localDictionary.ContainsKey(localUrl) && localDictionary[localUrl].IsFolder)
                    {
                        LocalFolder localItem = localDictionary[localUrl].AsFolder();
                        string name = GetItemDisplayName(identifier, folder);
                        result.Add(new FolderMetadata(identifier, parentIdentifier, name, localItem, folder));
                        localDictionary.Remove(localUrl);
                    }

                    LocalFolder localItem1 = LocalStorage.GetFolder(localUrl);
                    string name1 = GetItemDisplayName(identifier, folder);
                    result.Add(new FolderMetadata(identifier, parentIdentifier, name1, localItem1, folder));
                }

                if(hierarchyItemAsync is IFileAsync file)
                {
                    if(localDictionary.ContainsKey(localUrl) && localDictionary[localUrl].IsFile)
                    {
                        LocalFile localItem = localDictionary[localUrl].AsFile();
                        string name = GetItemDisplayName(identifier, file);
                        result.Add(new FileMetadata(identifier, parentIdentifier, name, localItem, file));
                        localDictionary.Remove(localUrl);
                    }

                    LocalFile localItem1 = LocalStorage.GetFile(localUrl);
                    string name1 = GetItemDisplayName(identifier, file);
                    result.Add(new FileMetadata(identifier, parentIdentifier, name1, localItem1, file));
                }
            }

            foreach(KeyValuePair<string, LocalItem> dictionaryItem in localDictionary)
            {
                string identifier = LocationMapper.GetIdentifierFromLocalPath(dictionaryItem.Key);
                if(dictionaryItem.Value.IsFile)
                {
                    result.Add(CreateFileMetadata(identifier, dictionaryItem.Value.AsFile()));
                }

                if(dictionaryItem.Value.IsFolder)
                {
                    result.Add(CreateFolderMetadata(identifier, dictionaryItem.Value.AsFolder()));
                }
            }

            return result.ToArray();
        }

        private string GetItemDisplayName(string itemIdentifier, IHierarchyItemAsync serverItem)
        {
            return serverItem?.DisplayName ?? LocationMapper.GetNameFromIdentifier(itemIdentifier);
        }

        public FileMetadata CreateFileOnServer(FolderMetadata parentMetadata, string fileName)
        {
            IFileAsync newFile = parentMetadata.ServerFolder.CreateFileAsync(fileName, null).GetAwaiter().GetResult();
            string identifier = LocationMapper.GetIdentifierFromServerUri(newFile.Href);
            string localPath = LocationMapper.GetLocalUrlFromIdentifier(identifier);
            LocalFile localItem = LocalStorage.GetFile(localPath);
            string name = GetItemDisplayName(identifier, newFile);
            return new FileMetadata(identifier, parentMetadata.Identifier, name, localItem, newFile);
        }

        public FileMetadata WriteContentOnServer(FileMetadata createdFile, string fileUrlPath)
        {
            IFileAsync serverItem = createdFile.ServerFile;
            serverItem.TimeOut = 36000000;
            serverItem.UploadAsync(fileUrlPath).GetAwaiter().GetResult();
            return GetFileMetadata(createdFile.Identifier);
        }

        public FileMetadata UpdateFileLocal(FileMetadata item)
        {
            LocalFile localFile = LocalStorage.UpdateFile(item.LocalFile);
            string name = GetItemDisplayName(item.Identifier, item.ServerFile);
            return new FileMetadata(item.Identifier, item.ParentIdentifier, name, localFile, item.ServerFile);
        }

        public FileMetadata PushToServer(FileMetadata item)
        {
            if(!item.ExistsOnServer)
            {
                item = this.CreateOnServer(item);
            }

            item = WriteContentOnServer(item, item.LocalFile.Path);
            item.LocalFile.Etag = item.ServerFile.Etag;
            item.LocalFile.UploadError = null;
            LocalStorage.UpdateFile(item.LocalFile);
            return item;
        }

        private FileMetadata CreateOnServer(FileMetadata item)
        {
            FolderMetadata parent = GetFolderMetadata(item.ParentIdentifier);
            return CreateFileOnServer(parent, item.Name);
        }

        private FolderMetadata CreateOnServer(FolderMetadata item)
        {
            FolderMetadata parent = GetFolderMetadata(item.ParentIdentifier);
            return CreateFolderOnServer(parent, item.Name);
        }


        public void MoveItemOnServer(ItemMetadata item, FolderMetadata destinationFolder, string name)
        {
            item.ServerItem.MoveToAsync(destinationFolder.ServerFolder, name, false, null).GetAwaiter().GetResult();
        }

        public void PullFromServer(FileMetadata item)
        {
            IFileAsync serverItem = item.ServerFile;
            item.ServerFile.TimeOut = 36000000;
            serverItem.DownloadAsync(item.LocalFile.Path).GetAwaiter().GetResult();
            item.LocalFile.Etag = item.ServerFile.Etag;
            LocalStorage.UpdateFile(item.LocalFile);
        }

        public void DeleteLocal(ItemMetadata item)
        {
            this.LocalStorage.Delete(item.LocalItem);
        }

        public void SetUploadError(NSUrl url, NSError localFileUploadError)
        {
            LocalFile localFile = this.LocalStorage.GetFile(url.Path);
            localFile.UploadError = localFileUploadError;
            localFile.Etag = null;
            this.LocalStorage.UpdateFile(localFile);
        }

        public FileMetadata ResetLocalVersion(FileMetadata item)
        {
            item.LocalFile.Etag = null;
            item = UpdateFileLocal(item);
            return item;
        }
    }
}