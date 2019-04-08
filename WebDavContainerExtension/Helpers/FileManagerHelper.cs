﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WebDavContainerExtension.Helpers
{
    public class NSFileManagerHelper
    {
        const string BackupKey = "com.apple.MobileBackup";

        private const string LibsystemKernelDylib = "/usr/lib/system/libsystem_kernel.dylib";

        static bool _xattrCompatibility;

        /// <summary>
        /// Errno for not existing attribute.
        /// </summary>
        private const int AttributeNotFoundErrno = 93;

        /// <summary>
        /// Max size for error message buffer.
        /// </summary>
        private const int ErrorMessageBufferMaxSize = 255;


        static NSFileManagerHelper()
        {
            _xattrCompatibility = true; //!UIDevice.CurrentDevice.CheckSystemVersion (5, 1);
        }


        /// <summary>
        /// Determines whether extended attributes are supported.
        /// </summary>
        /// <param name="path">File or folder path.</param>
        /// <returns>True if extended attributes are supported, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Throw when path is null or empty.</exception>
        public static bool IsExtendedAttributesSupported(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            long attributeCount = ListXAattr(path, null, 0, 0);
            return attributeCount >= 0;
        }

        /// <summary>
        /// Reads extended attribute.
        /// </summary>
        /// <param name="path">File or folder path.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <returns>Attribute value.</returns>
        /// <exception cref="ArgumentNullException">Throw when path is null or empty or attrName is null or empty.</exception>
        /// <exception cref="IOException">Throw when file or attribute is no available.</exception>
        public static string GetExtendedAttribute(string path, string attrName)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if(string.IsNullOrEmpty(attrName))
            {
                throw new ArgumentNullException(nameof(attrName));
            }

            long attributeSize = GetXAttr(path, attrName, null, 0, 0, 0);
            if(attributeSize == -1)
            {
                if(Marshal.GetLastWin32Error() == AttributeNotFoundErrno)
                {
                    return null;
                }

                ThrowLastException(path, attrName);
            }

            byte[] buffer = new byte[attributeSize];
            long readLength = GetXAttr(path, attrName, buffer, attributeSize, 0, 0);

            if(readLength == -1)
            {
                ThrowLastException(path, attrName);
            }

            string attributeValue = Encoding.UTF8.GetString(buffer);
            return attributeValue;
        }

        /// <summary>
        /// Writes extended attribute.
        /// </summary>
        /// <param name="path">File or folder path.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="attrValue">Attribute value.</param>
        /// <exception cref="ArgumentNullException">Throw when path is null or empty or attrName is null or empty.</exception>
        /// <exception cref="IOException">Throw when file or attribute is no available.</exception>
        public static void SetExtendedAttribute(string path, string attrName, string attrValue)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if(string.IsNullOrEmpty(attrName))
            {
                throw new ArgumentNullException(nameof(attrName));
            }

            byte[] buffer = Encoding.UTF8.GetBytes(attrValue);
            long result = SetXAttr(path, attrName, buffer, buffer.Length, 0, 0);
            if(result == -1)
            {
                ThrowLastException(path, attrName);
            }
        }

        /// <summary>
        /// Deletes extended attribute.
        /// </summary>
        /// <param name="path">File or folder path.</param>
        /// <param name="attrName">Attribute name.</param>
        public static void DeleteExtendedAttribute(string path, string attrName)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if(string.IsNullOrEmpty(attrName))
            {
                throw new ArgumentNullException(nameof(attrName));
            }

            long result = RemoveXAttr(path, attrName, 0);
            if(result == -1)
            {
                ThrowLastException(path, attrName);
            }
        }

        /// <summary>
        /// Throws corresponding exception for last platform api call.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <exception cref="System.IO.IOException"></exception>
        private static void ThrowLastException(string fileName, string attrName)
        {
            int errno = Marshal.GetLastWin32Error(); // It returns glibc errno
            string message = GetMessageForErrno(errno);
            throw new IOException(string.Format("[{0}:{1}] {2} Errno {3}", fileName, attrName, message, errno));
        }

        /// <summary>
        /// Returns error message that described error number.
        /// </summary>
        /// <param name="errno">Error number.</param>
        /// <returns>Error message</returns>
        private static string GetMessageForErrno(int errno)
        {
            var buffer = new StringBuilder(ErrorMessageBufferMaxSize);
            StrErrorR(errno, buffer, ErrorMessageBufferMaxSize);
            return buffer.ToString();
        }

        /// <summary>
        /// External func getxattr from libc, what returns custom attribute by name.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="buffer">Buffer to collect attribute value.</param>
        /// <param name="bufferSize">Buffer size.</param>
        /// <param name="position">Position value.</param>
        /// <param name="options">Options value.</param>
        /// <returns>Attribute value size in bytes, when returning value -1 than some error occurred./// </returns>
        [DllImport(LibsystemKernelDylib, EntryPoint = "getxattr", SetLastError = true)]
        private static extern long GetXAttr(string filePath, string attrName, byte[] buffer, long bufferSize, long position, int options);

        /// <summary>
        /// External func setxattr from libc, sets attribute value for file by name. 
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="attrValue">Attribute value</param>
        /// <param name="size">Attribute value size</param>
        /// <param name="position">Position value.</param>
        /// <param name="options">Options value.</param>
        /// <returns>Status, when returning value -1 than some error occurred.</returns>
        [DllImport(LibsystemKernelDylib, EntryPoint = "setxattr", SetLastError = true)]
        private static extern long SetXAttr(string filePath, string attrName, byte[] attrValue, long size, long position, int options);

        /// <summary>
        /// Removes the extended attribute. 
        /// </summary>
        /// <param name="path">File or folder path.</param>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="options">Options value.</param>
        /// <returns>On success, zero is returned. On failure, -1 is returned.</returns>
        [DllImport(LibsystemKernelDylib, EntryPoint = "removexattr", SetLastError = true)]
        private static extern long RemoveXAttr(string path, string attrName, int options);

        /// <summary>
        /// External func listxattr from libc, what returns list of attributes separated null-terminated string.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="nameBuffer">Attribute name.</param>
        /// <param name="size">Buffer size</param>
        /// <param name="options">Options value.</param>
        /// <returns>Attributes bytes array size, when returning value -1 than some error occurred</returns>
        [DllImport(LibsystemKernelDylib, EntryPoint = "listxattr", SetLastError = true)]
        private static extern long ListXAattr(string filePath, StringBuilder nameBuffer, long size, int options);

        /// <summary>
        /// External func strerror_r from libc, what returns string that describes the error code passed in the argument.
        /// </summary>
        /// <param name="code">Error number.</param>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="bufferSize">Buffer size.</param>
        [DllImport(LibsystemKernelDylib, EntryPoint = "strerror_r", SetLastError = true)]
        private static extern IntPtr StrErrorR(int code, StringBuilder buffer, int bufferSize);
    }
}