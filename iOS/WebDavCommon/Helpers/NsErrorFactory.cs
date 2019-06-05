using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;

namespace WebDavCommon.Helpers
{
    /// <summary>This class provides methods for <see cref="NSError"/> creation.</summary>
    public class NSErrorFactory
    {
        /// <summary>The description for unspecified network error.</summary>
        private const string NetworkErrorDescription = "Network error happened.";

        /// <summary>The description for unspecified error.</summary>
        private const string UnspecifiedErrorDescription = "Something went wrong";

        /// <summary>Creates <see cref="NSError"/> of Cocoa domain.</summary>
        /// <param name="cocoaErrorType">The cocoa error type.</param>
        /// <param name="description">The description.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateCocoaError(NSCocoaError cocoaErrorType, string description)
        {
            var userInfo = new NSDictionary(NSError.LocalizedDescriptionKey, description);
            return new NSError(NSError.CocoaErrorDomain, (int)cocoaErrorType, userInfo);
        }

        /// <summary>Creates <see cref="NSError"/> of FileProvider generic network error. </summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateUnspecifiedNetworkError()
        {
            return CreateCocoaError(NSCocoaError.None, NetworkErrorDescription);
        }

        /// <summary>Creates <see cref="NSError"/> of Cocoa generic error. </summary>
        /// <returns>The <see cref="NSError"/>.</returns>
        public static NSError CreateUnspecifiedError()
        {
            return CreateCocoaError(NSCocoaError.None, UnspecifiedErrorDescription);
        }

        /// <summary>Creates <see cref="NSError"/>.</summary>
        /// <param name="errorDomain">The error domain.</param>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="errorDomain"/> is null. </exception>
        public static NSError CreateNSError(string errorDomain, long errorCode)
        {
            if (errorDomain == null) throw new ArgumentNullException(nameof(errorDomain));
            return new NSError(new NSString(errorDomain), new nint(errorCode));
        }

        /// <summary>Creates <see cref="NSError"/>.</summary>
        /// <param name="errorDomain">The error domain.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorInfo">The error additional info.</param>
        /// <returns>The <see cref="NSError"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="errorDomain"/> is null. </exception>
        /// <exception cref="ArgumentNullException"> if <paramref name="errorInfo"/> is null. </exception>
        public static NSError CreateNSError(string errorDomain, long errorCode, Dictionary<string, string> errorInfo)
        {
            if (errorDomain == null) throw new ArgumentNullException(nameof(errorDomain));
            if (errorInfo == null) throw new ArgumentNullException(nameof(errorInfo));

            object[] errorValues = errorInfo.Values.ToArray<object>();
            object[] errorKeys = errorInfo.Keys.ToArray<object>();
            using (NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(errorValues, errorKeys))
            {
                return new NSError(new NSString(errorDomain), new nint(errorCode), userInfo);
            }
        }
    }
}
