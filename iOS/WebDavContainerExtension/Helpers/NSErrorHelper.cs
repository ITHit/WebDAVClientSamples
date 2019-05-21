using System;
using System.Collections.Generic;
using System.Linq;
using FileProvider;
using Foundation;

namespace WebDavContainerExtension.Helpers
{
    class NsErrorHelper
    {
        public static NSError GetFileProviderError(NSFileProviderError nsFileProviderError)
        {
            NSString errorDomain = nsFileProviderError.GetDomain();
            return new NSError(errorDomain, (int)nsFileProviderError);
        }

        public static NSError GetCocoaErrorWithMessage(NSCocoaError nsCocoaError, string message)
        {
            var userInfo = new NSDictionary(NSError.LocalizedDescriptionKey, message);
            return new NSError(NSError.CocoaErrorDomain, (int)nsCocoaError, userInfo);
        }

        internal static NSError GetFileProviderErrorWithError(NSFileProviderError nsFileProviderError, string message)
        {
            var userInfo = new NSDictionary(NSError.LocalizedDescriptionKey, message);
            var errorDomain = nsFileProviderError.GetDomain();
            return new NSError(errorDomain, (int)nsFileProviderError, userInfo);
        }

        public static NSError GetFileProviderNotFoundError(string id)
        {
            var userInfo = new NSDictionary(NSFileProviderErrorKeys.NonExistentItemIdentifierKey, id);
            NSFileProviderError errorCode = NSFileProviderError.NoSuchItem;
            return new NSError(errorCode.GetDomain(), (int)errorCode, userInfo);
        }

        public static NSError GetFileProviderNotFoundError()
        {
            return GetFileProviderError(NSFileProviderError.NoSuchItem);
        }

        public static NSError GetFileProviderUnauthorizedError()
        {
            return GetFileProviderError(NSFileProviderError.NotAuthenticated);
        }

        public static NSError GetUnspecifiedServerError()
        {
            return GetCocoaErrorWithMessage(NSCocoaError.None, "Network error happened.");
        }

        public static NSError GetUnspecifiedErrorError()
        {
            return GetCocoaErrorWithMessage(NSCocoaError.None, "Something went wrong");
        }

        public static NSError GetFileProviderDuplicateException()
        {
            return GetFileProviderError(NSFileProviderError.FilenameCollision);
        }

        public static NSError GetNSError(string errorDomain, long errorCode)
        {
            return new NSError(new NSString(errorDomain), new nint(errorCode));
        }

        public static NSError GetNsError(string errorDomain, long errorCode, Dictionary<string, string> uploadErrorInfo)
        {
            using (NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(uploadErrorInfo.Values.ToArray<object>(), uploadErrorInfo.Keys.ToArray<object>()))
            {
                return new NSError(new NSString(errorDomain), new nint(errorCode), userInfo);
            }
        }
    }
}