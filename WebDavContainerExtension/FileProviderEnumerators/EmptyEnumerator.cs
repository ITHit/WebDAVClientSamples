using System;
using FileProvider;
using Foundation;

namespace WebDavContainerExtension.FileProviderEnumerators
{
    class EmptyEnumerator : NSObject, INSFileProviderEnumerator
    {
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
            observer.FinishEnumerating((NSData) null);
        }
    }
}