using FileProvider;
using Foundation;

namespace WebDavContainerExtension.Helpers
{
    class NsErrorHelper
    {
        public static NSError GetFileProviderError(NSFileProviderError nsFileProviderError)
        {
            var errorDomain = nsFileProviderError.GetDomain();
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
            var errorCode = NSFileProviderError.NoSuchItem;
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
    }
}