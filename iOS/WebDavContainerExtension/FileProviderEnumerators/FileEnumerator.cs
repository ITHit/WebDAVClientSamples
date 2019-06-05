using System;
using FileProvider;
using Foundation;
using ITHit.WebDAV.Client.Exceptions;

using WebDavCommon.Helpers;
using WebDavCommon.Metadatas;

using WebDavContainerExtension.FileProviderItems;

namespace WebDavContainerExtension.FileProviderEnumerators
{
    public class FileEnumerator : NSObject, INSFileProviderEnumerator
    {
        private readonly StorageManager storageManager;
        private readonly string EnumeratedItemIdentifier;

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
                var metadata = storageManager.GetFileMetadata(this.EnumeratedItemIdentifier);
                if(!metadata.IsExists)
                {
                    observer.FinishEnumerating(NSFileProviderErrorFactory.CreateNonExistentItemError(EnumeratedItemIdentifier));
                    return;
                }

                INSFileProviderItem item = ProviderItem.CreateFromMetadata(metadata);
                observer.DidEnumerateItems(new[] {item});
                observer.FinishEnumerating((NSData) null);
            }
            catch(UnauthorizedException)
            {
                observer.FinishEnumerating(NSFileProviderErrorFactory.CreateNonExistentItemError(this.EnumeratedItemIdentifier));
            }
            catch(WebDavHttpException)
            {
                observer.FinishEnumerating(NSErrorFactory.CreateUnspecifiedNetworkError());
            }
            catch(Exception)
            {
                observer.FinishEnumerating(NSErrorFactory.CreateUnspecifiedError());
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
            catch(UnauthorizedException)
            {
                observer.FinishEnumerating(NSFileProviderErrorFactory.CreatesNotAuthenticatedError());
            }
            catch(WebDavHttpException)
            {
                observer.FinishEnumerating(NSErrorFactory.CreateUnspecifiedNetworkError());
            }
            catch(Exception)
            {
                observer.FinishEnumerating(NSErrorFactory.CreateUnspecifiedError());
            }
        }

        private static uint GetUintFromNsData(NSData syncAnchor)
        {
            var anchorString = NSString.FromData(syncAnchor, NSStringEncoding.UTF8).ToString();
            var anchor = uint.Parse(anchorString);
            return anchor;
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

        public uint SyncAnchor { get; protected set; }
    }
}