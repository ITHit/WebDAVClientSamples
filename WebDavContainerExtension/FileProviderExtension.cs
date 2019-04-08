using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client;
using ITHit.WebDAV.Client.Exceptions;
using ITHit.WebDAV.Client.Logger;
using UIKit;
using WebDavCommon;
using WebDavContainerExtension.Extensions;
using WebDavContainerExtension.FileProviderEnumerators;
using WebDavContainerExtension.FileProviderItems;
using WebDavContainerExtension.Helpers;
using WebDavContainerExtension.Metadatas;
using WebDavContainerExtension.Storages;

namespace WebDavContainerExtension
{
    [Register(nameof(FileProviderExtension))]
    class FileProviderExtension : NSFileProviderExtension
    {
        private const string StorageFolder = "Documents";

        public LocationMapper LocationMapper { get; }

        public WebDavSessionAsync Session { get; }

        public StorageManager StorageManager { get; }

        public FileProviderExtension()
        {
            var serverSettings = AppGroupSettings.GetServerSettings();
            Session = InitSession(serverSettings);
            string documentStoragePath = InitLocalStorage();
            LocationMapper = new LocationMapper(serverSettings.ServerUri, documentStoragePath);
            this.StorageManager = new StorageManager(LocationMapper, Session, new LocalStorage());

        }

        private WebDavSessionAsync InitSession(ServerSettings serverSettings)
        {
#if !DEBUG
            FileLogger.Level = LogLevel.Off;
#endif
            string license = @"<?xml version='1.0'...";
            WebDavSessionAsync session = new WebDavSessionAsync(license);

            if(serverSettings != null && serverSettings.HasCredential)
                Session.Credentials = new NetworkCredential(serverSettings.UserName, serverSettings.Password);

            return session;
        }

        private string InitLocalStorage()
        {
            using (NSUrl documentStorageUrl = NSFileProviderManager.DefaultManager.DocumentStorageUrl.Append(StorageFolder, true))
            {
                NSError error;
                NSFileManager.DefaultManager.CreateDirectory(documentStorageUrl, true, null, out error);
                if(error != null)
                {
                    throw error.AsException();
                }

                return documentStorageUrl.Path;
            }
        }

        /// <param name="url">The shared document's URL.</param>
        /// <param name="completionHandler">
        ///   <para>An action the system calls subseqent to the creation of a placeholder.</para>
        ///   <para tool="nullallowed">This parameter can be <see langword="null" />.</para>
        /// </param>
        /// <summary>When implemented by the developer, creates a specified placeholder for a previously defined URL.</summary>
        /// <remarks>
        ///   <para>The developer must override this method. This method is called to provide a placeholder for documents that are returned by the Document Picker but that are not locally stored.</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void ProvidePlaceholderAtUrl(NSUrl url, Action<NSError> completionHandler)
        {
            try
            {
                string identifier = this.GetPersistentIdentifier(url);
                ItemMetadata itemMetadata = this.StorageManager.GetItemMetadata(identifier);
                if (!itemMetadata.IsExists)
                {
                    completionHandler?.Invoke(NsErrorHelper.GetFileProviderNotFoundError(identifier));
                    return;
                }
                var placeholderUrl = NSFileProviderManager.GetPlaceholderUrl(url);
                NSError error = null;
                NSFileManager.DefaultManager.CreateDirectory(placeholderUrl.RemoveLastPathComponent(), true, null, out error);
                if(error != null)
                {
                    completionHandler?.Invoke(error);
                    return;
                }

                var providerItem = ProviderItem.CreateFromMetadata(itemMetadata);
                NSFileProviderManager.WritePlaceholder(placeholderUrl, providerItem, out error);
                completionHandler?.Invoke(error);
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }


        public override INSFileProviderItem GetItem(NSString identifier, out NSError error)
        {
            error = null;
            try
            {
                ItemMetadata itemMetadata = this.StorageManager.GetItemMetadata(identifier);
                if (!itemMetadata.IsExists)
                {
                    error = NsErrorHelper.GetFileProviderNotFoundError(identifier);
                    return null;
                }

                return ProviderItem.CreateFromMetadata(itemMetadata);
            }
            catch (UnauthorizedException ex)
            {
                error = NsErrorHelper.GetFileProviderUnauthorizedError();
            }
            catch (WebDavHttpException ex)
            {
                error = NsErrorHelper.GetUnspecifiedServerError();
            }
            catch (Exception ex)
            {
                error = NsErrorHelper.GetUnspecifiedErrorError();
            }

            return null;
        }

        /// <param name="itemUrl">The URL for the shared document.</param>
        /// <summary>When implemented by the developer, returns a specified identifier for a given URL.</summary>
        /// <returns>String that specifically identifies a document with reference to its URL.</returns>
        /// <remarks>
        ///   <para>The identifier is defined by relative path of the document from the root URL address that was returned by the DocumentStorageURL method.</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override string GetPersistentIdentifier(NSUrl itemUrl)
        {
            string identifierFromLocalPath = this.LocationMapper.GetIdentifierFromLocalPath(itemUrl.Path);
            return identifierFromLocalPath;
        }

        /// <inheritdoc />
        /// <param name="persistentIdentifier">Persistent identifier for a document that is shared .</param>
        /// <summary>When implemented by the developer, returns the URL for a specified persistent identifier.</summary>
        /// <returns>The shared document's URL.</returns>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override NSUrl GetUrlForItem(string persistentIdentifier)
        {
            return NSUrl.FromFilename(this.LocationMapper.GetLocalUrlFromIdentifier(persistentIdentifier));
        }

        public override INSFileProviderEnumerator GetEnumerator(string containerItemIdentifier, out NSError error)
        {
            error = null;
            if(containerItemIdentifier == NSFileProviderItemIdentifier.WorkingSetContainer)
            {
                return new EmptyEnumerator();
            }

            if(LocationMapper.IsFolderIdentifier(containerItemIdentifier))
            {
                return new FolderEnumerator(containerItemIdentifier, StorageManager);
            }


            return new FileEnumerator(containerItemIdentifier, StorageManager);

        }

        /// <param name="url">The shared document's URL.</param>
        /// <summary>When implemented by the developer, informs a file provider extension that there has been a change in a document.</summary>
        /// <remarks>
        ///   <para>You must override this method.Do not call super.</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void ItemChangedAtUrl(NSUrl url)
        {
            var identifier = this.GetPersistentIdentifier(url);
            FileMetadata item = this.StorageManager.GetFileMetadata(identifier);
            if(!item.ExistsLocal) throw NsErrorHelper.GetFileProviderNotFoundError().AsException();
            item.LocalFile.Etag = null;
            item = this.StorageManager.UpdateFileLocal(item);
            this.StorageManager.PushToServer(item);
        }

        /// <param name="url">The shared document's URL.</param>
        /// <param name="completionHandler">
        ///   <para>An action the system calls when the refernced file becomes available.</para>
        ///   <para tool="nullallowed">This parameter can be <see langword="null" />.</para>
        /// </param>
        /// <summary>When implemented by the developer, supplies an actual file on a disk in place of a placeholder.</summary>
        /// <remarks>
        ///   <para>You have to override this method. Do not call super in an implementation.</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void StartProvidingItemAtUrl(NSUrl url, Action<NSError> completionHandler)
        {
            try
            {
                var identifier = this.GetPersistentIdentifier(url);
                FileMetadata item = this.StorageManager.GetFileMetadata(identifier);
                if (!item.ExistsOnServer || item.LocalFile.Etag == item.ServerFile.Etag)
                {
                    completionHandler?.Invoke(null);
                    return;
                }

                this.StorageManager.PullFromServer(item);
                completionHandler?.Invoke(null);
            }
            catch(NotFoundException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetFileProviderNotFoundError());
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        /// <param name="url">The shared document's URL.</param>
        /// <summary>When implemented by the developer, informs a file provider extension when a specified document is no longer being accessed</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void StopProvidingItemAtUrl(NSUrl url)
        {
            try
            {
                var identifier = this.GetPersistentIdentifier(url);
                FileMetadata item = this.StorageManager.GetFileMetadata(identifier);
                if(!item.LocalFile.HasEtag)
                {
                    this.StorageManager.PushToServer(item);
                }
                this.StorageManager.DeleteLocal(item);
                ProvidePlaceholderAtUrl(url, null);
            }
            catch(Exception ex)
            {
                throw NsErrorHelper.GetUnspecifiedErrorError().AsException();
            }
        }

        /// <param name="directoryName">The directory name.</param>
        /// <param name="parentItemIdentifier">The parent directory's persistent identifier.</param>
        /// <param name="completionHandler">A handler to run after the operation completes.</param>
        /// <summary>When implemented by the developer, creates a new directory in the specified location and runs a handler when the operation is complete.</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void CreateDirectory(string directoryName, string parentItemIdentifier, Action<INSFileProviderItem, NSError> completionHandler)
        {
            try
            {
                FolderMetadata folderMetadata = this.StorageManager.GetFolderMetadata(parentItemIdentifier);
                if(folderMetadata.ServerItem != null)
                {
                    completionHandler?.Invoke(ProviderItem.CreateFromMetadata(folderMetadata), null);
                }
                FolderMetadata createdFolder = this.StorageManager.CreateFolderOnServer(folderMetadata, directoryName);
                completionHandler?.Invoke(ProviderItem.CreateFromMetadata(createdFolder), null);
                this.StorageManager.NotifyEnumerator(parentItemIdentifier);
            }
            catch (MethodNotAllowedException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderDuplicateException());
            }
            catch (NotFoundException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderNotFoundError());
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        /// <param name="itemIdentifier">The persistent identifier for the item.</param>
        /// <param name="completionHandler">A handler to run after the operation completes.</param>
        /// <summary>When implemented by the developer, deletes the identified item and runs a handler when the operation is complete.</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void DeleteItem(string itemIdentifier, Action<NSError> completionHandler)
        {
            try
            {
                ItemMetadata metadata = this.StorageManager.GetItemMetadata(itemIdentifier);
                if(!metadata.IsExists)
                {
                    completionHandler?.Invoke(null);
                    return;
                }

                this.StorageManager.DeleteEveryWhere(metadata);
                completionHandler?.Invoke(null);
            }
            catch (MethodNotAllowedException ex)
            {
                completionHandler?.Invoke( NsErrorHelper.GetFileProviderDuplicateException());
            }
            catch (NotFoundException ex)
            {
                completionHandler?.Invoke(null);
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke( NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        /// <param name="fileUrl">The URL for the file.</param>
        /// <param name="parentItemIdentifier">The parent directory's persistent identifier.</param>
        /// <param name="completionHandler">A handler to run after the operation completes.</param>
        /// <summary>When implemented by the developer, imports the resource at the specified <paramref name="fileURL" /> into the directory that is identified by <paramref name="parentItemIdentifier" />.</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void ImportDocument(NSUrl fileUrl, string parentItemIdentifier, Action<INSFileProviderItem, NSError> completionHandler)
        {
            try
            {
                fileUrl.StartAccessingSecurityScopedResource();
                var parentMetadata = StorageManager.GetFolderMetadata(parentItemIdentifier);
                if(!parentMetadata.IsExists)
                {
                    completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderNotFoundError(parentItemIdentifier));
                    return;
                }

                var existsNames = StorageManager.GetFolderChildrenMetadatas(parentMetadata).Select(x => x.Name);
                string fileName = GetNewFileName(fileUrl.LastPathComponent, existsNames);
                FileMetadata createdFile = StorageManager.CreateFileOnServer(parentMetadata, fileName);
                createdFile = StorageManager.WriteContentOnServer(createdFile, fileUrl.Path);
                completionHandler?.Invoke(ProviderItem.CreateFromMetadata(createdFile), null);
                this.StorageManager.NotifyEnumerator(parentItemIdentifier);
            }
            catch (NotFoundException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderNotFoundError());
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedErrorError());
            }
            finally
            {
                fileUrl.StopAccessingSecurityScopedResource();
            }
        }

        private string GetNewFileName(string fileName, IEnumerable<string> existsNames)
        {
            HashSet<string> nameSet = existsNames.ToHashSet();
            if (!nameSet.Contains(fileName))
            {
                return fileName;
            }

            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            int i = 1;
            string currentCopyName = $"{filenameWithoutExtension}-Copy({i}){extension}";
            while(nameSet.Contains(currentCopyName))
            {
                i++;
                currentCopyName = $"{filenameWithoutExtension}-Copy({i}){extension}";
            }

            return currentCopyName;
        }

        /// <param name="itemIdentifier">The persistent identifier for the item.</param>
        /// <param name="itemName">The new name for the item.</param>
        /// <param name="completionHandler">A handler to run after the operation completes.</param>
        /// <summary>When implemented by the developer, changes the name of the identified item.</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void RenameItem(string itemIdentifier, string itemName, Action<INSFileProviderItem, NSError> completionHandler)
        {
           this.ReparentItem(itemIdentifier, LocationMapper.GetParentIdentifier(itemIdentifier), itemName, completionHandler);
        }

        /// <param name="itemIdentifier">The persistent identifier for the item.</param>
        /// <param name="destParentItemIdentifier">The parent directory's persistent identifier.</param>
        /// <param name="newName">
        ///   <para>The new name for the item.</para>
        ///   <para tool="nullallowed">This parameter can be <see langword="null" />.</para>
        /// </param>
        /// <param name="completionHandler">A handler to run after the operation completes.</param>
        /// <summary>When implemented by the developer, moves the identified item to a new name under a new parent.</summary>
        /// <remarks>
        ///   <para>(More documentation for this node is coming)</para>
        ///   <para tool="threads">This can be used from a background thread.</para>
        /// </remarks>
        public override void ReparentItem(string itemIdentifier, string destParentItemIdentifier, string newName, Action<INSFileProviderItem, NSError> completionHandler)
        {
            try
            {
                ItemMetadata item = this.StorageManager.GetItemMetadata(itemIdentifier);
                FolderMetadata destinationFolder = this.StorageManager.GetFolderMetadata(destParentItemIdentifier);
                string name = newName ?? item.Name;
                this.StorageManager.MoveItemOnServer(item, destinationFolder, name);
                var oldParentIdentifier = item.ParentIdentifier;
                item.ParentIdentifier = destParentItemIdentifier;
                item.Name = name;
                completionHandler(ProviderItem.CreateFromMetadata(item), null);
                this.StorageManager.NotifyEnumerator(oldParentIdentifier, item.ParentIdentifier);
            }
            catch (PreconditionFailedException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderDuplicateException());
            }
            catch (ForbiddenException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderDuplicateException());
            }
            catch (NotFoundException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderNotFoundError(LocationMapper.GetLocalUrlFromServerUri(ex.Uri)));
            }
            catch (UnauthorizedException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                completionHandler?.Invoke(null, NsErrorHelper.GetUnspecifiedErrorError());
            }
        }
    }
}