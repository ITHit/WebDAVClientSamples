using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using FileProvider;
using Foundation;
using MobileCoreServices;
using ObjCRuntime;

namespace WebDavContainerExtension
{
    /// <summary>
    /// Extensions.
    /// class implement additional methods that are not relevant to any class
    /// </summary>
    public static class Extension
    {
        [DllImport(Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCreatePreferredIdentifierForTag")]
        public static extern IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);


        public static string GetUTType(string fileExtension)
        {
            fileExtension = fileExtension.Substring(1);
            NSString classRef = new NSString(UTType.TagClassFilenameExtension);
            NSString mimeRef = new NSString(fileExtension);

            IntPtr utiRef = UTTypeCreatePreferredIdentifierForTag(classRef.Handle, mimeRef.Handle, IntPtr.Zero);

            string uti = NSString.FromHandle(utiRef);

            return uti;
        }

        public static string Decode(string source)
        {
            string[] sourceElements = source.Split('/');
            List<string> resultElements = new List<string>();
            foreach(string element in sourceElements)
            {
                resultElements.Add(WebUtility.UrlDecode(element));
            }

            string[] resultArray = resultElements.ToArray();
            string result = String.Join("/", resultArray);
            return result;
        }

        public static string Encode(string source)
        {
            string[] sourceElements = source.Split('/');
            List<string> resultElements = new List<string>();
            foreach(string element in sourceElements)
            {
                resultElements.Add(WebUtility.UrlEncode(element));
            }

            string[] resultArray = resultElements.ToArray();
            string result = String.Join("/", resultArray);
            return result;
        }

        public static string GetTypeIdentifier(string type, string itemName)
        {
            string baseType = "public";

            switch(type)
            {
                case "Folder":
                {
                    baseType += ".folder";
                    break;
                }
                    ;
                default:
                {
                    string fileExtension = Path.GetExtension(itemName);
                    baseType = GetUTType(fileExtension);
                    break;
                }
            }

            return baseType;
        }


        public static string GetFolderTypeIdentifier()
        {
            return "public.folder";
        }

        public static string GetFileTypeIdentifier(string itemName)
        {

                        string fileExtension = Path.GetExtension(itemName);
                       return GetUTType(fileExtension);

        }
    }
}