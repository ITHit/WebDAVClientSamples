using FileProvider;

using Foundation;

namespace WebDavCommon.Helpers
{
    /// <summary>Creates <see cref="NSError"/> of FileProvider domain.</summary>
    public class NSFileProviderErrorFactory
    {
        /// <summary>Creates <see cref="NSError"/> of FileProvider domain.</summary>
        /// <param name="fileProviderErrorType">The FileProvider error type.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateError(NSFileProviderError fileProviderErrorType)
        {
            NSString errorDomain = fileProviderErrorType.GetDomain();
            return new NSError(errorDomain, (int)fileProviderErrorType);
        }

        /// <summary>Creates <see cref="NSError"/> of FileProvider domain.</summary>
        /// <param name="fileProviderErrorType">The FileProvider error type.</param>
        /// <param name="description">The description.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateError(NSFileProviderError fileProviderErrorType, string description)
        {
            var userInfo = new NSDictionary(NSError.LocalizedDescriptionKey, description);
            NSString errorDomain = fileProviderErrorType.GetDomain();
            return new NSError(errorDomain, (int)fileProviderErrorType, userInfo);
        }

        /// <summary>Creates non existent item <see cref="NSError"/>. </summary>
        /// <param name="identifier">The <see cref="NSFileProviderItemIdentifier"/>.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateNonExistentItemError(string identifier)
        {
            var userInfo = new NSDictionary(NSFileProviderErrorKeys.NonExistentItemIdentifierKey, identifier);
            var errorCode = NSFileProviderError.NoSuchItem;
            return new NSError(errorCode.GetDomain(), (int)errorCode, userInfo);
        }

        /// <summary>Creates non existent item <see cref="NSError"/>. </summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateNonExistentItemError()
        {
            return CreateError(NSFileProviderError.NoSuchItem);
        }

        /// <summary>Creates non authenticated request <see cref="NSError"/>. </summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreatesNotAuthenticatedError()
        {
            return CreateError(NSFileProviderError.NotAuthenticated);
        }

        /// <summary>Creates filename collision <see cref="NSError"/>. </summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateFilenameCollisionError()
        {
            return CreateError(NSFileProviderError.FilenameCollision);
        }
    }
}
