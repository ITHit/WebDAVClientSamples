using System;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client.Exceptions;
using WebDavContainerExtension.FileProviderItems;
using WebDavContainerExtension.Helpers;
using WebDavContainerExtension.Metadatas;

namespace WebDavContainerExtension.FileProviderEnumerators
{
    public class FolderEnumerator : NSObject, INSFileProviderEnumerator
    {
        private readonly StorageManager storageManager;

        private readonly string enumeratedItemIdentifier;

        private readonly ChangeTracker changeTracker;

        public uint SyncAnchor { get; protected set; }

        public FolderEnumerator(string enumeratedItemIdentifier, StorageManager storageManager)
        {
            this.storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            this.enumeratedItemIdentifier = enumeratedItemIdentifier ?? throw new ArgumentNullException(nameof(enumeratedItemIdentifier));
            this.changeTracker = new ChangeTracker();
        }

        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        public void Invalidate()
        {
        }

        /// <param name="observer">To be added.</param>
        /// <param name="startPage">To be added.</param>
        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        public void EnumerateItems(INSFileProviderEnumerationObserver observer, NSData startPage)
        {
            try
            {
                FolderMetadata metadata = storageManager.GetFolderMetadata(this.enumeratedItemIdentifier);
                if (!metadata.IsExists)
                {
                    observer.FinishEnumerating(NsErrorHelper.GetFileProviderNotFoundError(enumeratedItemIdentifier));
                    return;
                }

                ItemMetadata[] metadatas = this.storageManager.GetFolderChildrenMetadatas(metadata);
                this.SyncAnchor = this.changeTracker.AddChangeSet(metadatas);

                INSFileProviderItem[] items = ProviderItem.CreateFromMetadatas(metadatas);
                observer.DidEnumerateItems(items);
                observer.FinishEnumerating((NSData) null);
            }
            catch (UnauthorizedException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        [Export("enumerateChangesForObserver:fromSyncAnchor:")]
        public void EnumerateChanges(INSFileProviderChangeObserver observer, NSData syncAnchor)
        {
            
            uint anchor = GetAnchorFromNsData(syncAnchor);
            try
            {
                FolderMetadata metadata = storageManager.GetFolderMetadata(this.enumeratedItemIdentifier);
                if (!metadata.IsExists)
                {
                    observer.FinishEnumerating(NsErrorHelper.GetFileProviderNotFoundError(enumeratedItemIdentifier));
                    return;
                }

                ItemMetadata[] metadatas = this.storageManager.GetFolderChildrenMetadatas(metadata);
                MetadataDiff diff = this.changeTracker.GetDiff(anchor, metadatas);
                observer.DidDeleteItems(diff.DeletedId);
                INSFileProviderItem[] updatedItems = ProviderItem.CreateFromMetadatas(diff.Updated);
                observer.DidUpdateItems(updatedItems);

                this.SyncAnchor = this.changeTracker.AddChangeSet(metadatas);
                observer.FinishEnumeratingChanges(this.GetCurrentAnchorNsData(this.SyncAnchor), false);
            }
            catch (UnauthorizedException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch (WebDavHttpException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch (Exception ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        private static uint GetAnchorFromNsData(NSData syncAnchor)
        {
            string anchorString = NSString.FromData(syncAnchor, NSStringEncoding.UTF8).ToString();
            uint anchor = uint.Parse(anchorString);
            return anchor;
        }

        [Export("currentSyncAnchorWithCompletionHandler:")]
        public void CurrentSyncAnchor(Action<NSData> completionHandler)
        {
            completionHandler?.Invoke(GetCurrentAnchorNsData(this.SyncAnchor));
        }

        private NSData GetCurrentAnchorNsData(uint anchorNumber)
        {
            return NSData.FromString(anchorNumber.ToString(), NSStringEncoding.UTF8);
        }
    }
}