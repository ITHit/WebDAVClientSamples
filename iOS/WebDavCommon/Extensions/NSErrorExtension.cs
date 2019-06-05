using System;

using Foundation;

namespace WebDavCommon.Extensions
{
    /// <summary>This class extends functionality of <see cref="NSError"/>. </summary>
    public static class NSErrorExtension
    {
        /// <summary>Converts <paramref name="error"/> to <see cref="NSErrorException"/></summary>
        /// <param name="error">The error.</param>
        /// <returns>The instance of <see cref="NSErrorException"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="error"/> is null. </exception>
        public static NSErrorException AsException(this NSError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));

            return new NSErrorException(error);
        }

        /// <summary>Throws <paramref name="error"/> as <see cref="NSErrorException"/> if not null. </summary>
        /// <param name="error">The error.</param>
        /// <exception cref="NSErrorException"> if <paramref name="error"/> is not null. </exception>
        public static void ThrowNotNullAsException(this NSError error)
        {
            if (error == null) return;
            throw error.AsException();
        }
    }
}
