using System;
using System.IO;
using System.Runtime.InteropServices;

using Foundation;

using MobileCoreServices;

using ObjCRuntime;

namespace WebDavCommon.Helpers
{
    /// <summary>This class provide methods to retrieve UTType. </summary>
    public static class UTTypeHelper
    {
        /// <summary>Returns UTType.</summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetUTType(string fileExtension)
        {
            fileExtension = fileExtension.Substring(1);
            var classRef = new NSString(UTType.TagClassFilenameExtension);
            var mimeRef = new NSString(fileExtension);
            IntPtr utiRef = UTTypeCreatePreferredIdentifierForTag(classRef.Handle, mimeRef.Handle, IntPtr.Zero);
            return NSString.FromHandle(utiRef);
        }

        /// <summary>Returns folder UTType. </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetFolderTypeIdentifier()
        {
            return "public.folder";
        }

        /// <summary>Returns folder UTType. </summary>
        /// <param name="itemName">The item name.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetFileTypeIdentifier(string itemName)
        {
            string fileExtension = Path.GetExtension(itemName);
            return GetUTType(fileExtension);
        }

        [DllImport(Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCreatePreferredIdentifierForTag")]
        private static extern IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
    }
}
