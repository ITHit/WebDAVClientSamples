using System;
using System.Linq;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client.Exceptions;
using WebDavContainerExtension.FileProviderItems;
using WebDavContainerExtension.Helpers;
using WebDavContainerExtension.Metadatas;

namespace WebDavContainerExtension.FileProviderEnumerators
{
    public class FileEnumerator : NSObject, INSFileProviderEnumerator
    {
        private readonly StorageManager storageManager;

        private readonly string EnumeratedItemIdentifier;

        public uint SyncAnchor { get; protected set; }

        public FileEnumerator(string enumeratedItemIdentifier, StorageManager storageManager)
        {
            this.EnumeratedItemIdentifier = enumeratedItemIdentifier ?? throw new ArgumentNullException(nameof(enumeratedItemIdentifier));
            this.storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
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
                FileMetadata metadata = storageManager.GetFileMetadata(this.EnumeratedItemIdentifier);
                if(!metadata.IsExists)
                {
                    observer.FinishEnumerating(NsErrorHelper.GetFileProviderNotFoundError(EnumeratedItemIdentifier));
                    return;
                }

                INSFileProviderItem item = ProviderItem.CreateFromMetadata(metadata);
                observer.DidEnumerateItems(new[] {item});
                observer.FinishEnumerating((NSData) null);
            }
            catch(UnauthorizedException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetFileProviderNotFoundError(this.EnumeratedItemIdentifier));
            }
            catch(WebDavHttpException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch(Exception ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        [Export("enumerateChangesForObserver:fromSyncAnchor:")]
        public void EnumerateChanges(INSFileProviderChangeObserver observer, NSData syncAnchor)
        {
            try
            {
                FileMetadata metadata = storageManager.GetFileMetadata(this.EnumeratedItemIdentifier);
                if(!metadata.IsExists)
                {
                    observer.DidDeleteItems(new[] {EnumeratedItemIdentifier});
                    observer.FinishEnumeratingChanges(this.GetNsDataFromUint(this.SyncAnchor++), false);
                    return;
                }

                observer.DidUpdateItems(new[] { ProviderItem.CreateFromMetadata(metadata) });
                observer.FinishEnumeratingChanges(this.GetNsDataFromUint(this.SyncAnchor++), false);
            }
            catch(UnauthorizedException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetFileProviderUnauthorizedError());
            }
            catch(WebDavHttpException ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedServerError());
            }
            catch(Exception ex)
            {
                observer.FinishEnumerating(NsErrorHelper.GetUnspecifiedErrorError());
            }
        }

        private static uint GetUintFromNsData(NSData syncAnchor)
        {
            string anchorString = NSString.FromData(syncAnchor, NSStringEncoding.UTF8).ToString();
            return uint.Parse(anchorString);
        }

        [Export("currentSyncAnchorWithCompletionHandler:")]
        public void CurrentSyncAnchor(Action<NSData> completionHandler)
        {
            completionHandler?.Invoke(GetNsDataFromUint(this.SyncAnchor));
        }

        private NSData GetNsDataFromUint(uint anchorNumber)
        {
            return NSData.FromString(anchorNumber.ToString(), NSStringEncoding.UTF8);
        }
    }
}