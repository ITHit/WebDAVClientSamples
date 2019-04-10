﻿using ITHit.WebDAV.Client;

namespace WebDavContainerExtension.Storages
{
    public abstract class LocalItem
    {
        public string Path { get; set; }
        public bool IsExists { get; set; }
        public bool IsFolder => this is LocalFolder;
        public bool IsFile => this is LocalFile;

        public LocalItem(string localPath, bool exists)
        {
            this.Path = localPath;
            this.IsExists = exists;
        }

        public LocalFolder AsFolder()
        {
            return this as LocalFolder;
        }

        public LocalFile AsFile()
        {
            return this as LocalFile;
        }
      
    }
}