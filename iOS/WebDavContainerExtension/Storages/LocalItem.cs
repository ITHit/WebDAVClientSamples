﻿namespace WebDavContainerExtension.Storages
{
    public abstract class LocalItem
    {

        public string Path { get; set; }


        public bool IsExists { get; set; }


        public bool IsFolder => this is LocalFolder;

        public bool IsFile => this is LocalFile;

        protected LocalItem(string localPath, bool exists)
        {
            this.Path = localPath;
            this.IsExists = exists;
        }

        protected LocalItem()
        {
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