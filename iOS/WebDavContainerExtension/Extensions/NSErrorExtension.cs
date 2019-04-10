using System;
using Foundation;

namespace WebDavContainerExtension.Extensions
{
    public static class NSErrorExtension
    {
        public static NSErrorException AsException(this NSError error)
        {
            if(error == null) throw new ArgumentNullException(nameof(error));
            return new NSErrorException(error);
        }
    }
}