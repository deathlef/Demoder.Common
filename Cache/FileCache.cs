/*
Demoder.Common
Copyright (c) 2010-2012 Demoder <demoder@demoder.me>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using Demoder.Common.Hash;
using Demoder.Common.Serialization;


namespace Demoder.Common.Cache
{
    public class FileCache
    {
        #region members
        /// <summary>
        /// This is an index of all the cached data
        /// </summary>
        private Dictionary<string, CacheInfo> cacheIndex = new Dictionary<string, CacheInfo>();
        /// <summary>
        /// Monitors the cache directory for changes
        /// </summary>
        private FileSystemWatcher fsWatcher;

        private DirectoryInfo cacheRootDirectory;
        private DirectoryInfo cacheIndexDirectory;
        private DirectoryInfo cacheDataDirectory;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes the URL cache
        /// </summary>
        /// <param name="rootDirectory">Directory used for storage</param>
        public FileCache(DirectoryInfo rootDirectory)
        {
            //Define cache directories
            this.cacheRootDirectory = rootDirectory;
            this.cacheIndexDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Index"));
            this.cacheDataDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Data"));
            //Check if cache directories exist
            if (!this.cacheRootDirectory.Exists)
                this.cacheRootDirectory.Create();
            if (!this.cacheIndexDirectory.Exists)
                this.cacheIndexDirectory.Create();
            if (!this.cacheDataDirectory.Exists)
                this.cacheDataDirectory.Create();

            //initialize the fsWatcher
            this.fsWatcher = new FileSystemWatcher(rootDirectory.FullName);
        }
        #endregion
        #region fsWatcher implementation

        #endregion

        #region Methods
        public void Cache(string key, byte[] data)
        {
            lock (this.cacheIndex)
            {
                string md5 = MD5Checksum.Generate(data).String;
                //Is this data the same as the old?
                if (this.cacheIndex.ContainsKey(key))
                    if (this.cacheIndex[key].Hash == md5)
                        return;

                //If we made it here, it's a change to the old value.
                CacheInfo ci = new CacheInfo(key, md5);
                FileInfo dataFile = new FileInfo(this.GetDataFileName(key));
                FileInfo indexFile = new FileInfo(this.GetIndexFileName(key));
                try
                {
                    //Update the cache.
                    File.WriteAllBytes(dataFile.FullName, data); //Write the data
                    Xml.Serialize<CacheInfo>(indexFile, ci, false); //Write the index
                    this.cacheIndex[key] = ci; //Store index in memory
                }
                catch
                {
                    //If something fails with the caching, remove the cache entry.
                    if (this.cacheIndex.ContainsKey(key))
                    {
                        this.cacheIndex.Remove(key);
                    }
                    try
                    {
                        indexFile.Delete();
                        dataFile.Delete();
                    }
                    catch { return; }
                    return;
                }
            }
        }

        /// <summary>
        /// Reads a file from cache.
        /// </summary>
        /// <param name="key">ID of file to read</param>
        /// <returns></returns>
        public byte[] Read(string key)
        {
            lock (this.cacheIndex)
            {
                if (!this.cacheIndex.ContainsKey(key))
                    return null;
                else
                {
                    byte[] bytes = File.ReadAllBytes(this.GetDataFileName(key));
                    //Ensure the index is up to date.
                    string md5 = MD5Checksum.Generate(bytes).String;
                    if (md5 != this.cacheIndex[key].Hash)
                        this.Cache(key, bytes);
                    return bytes;
                }
            }
        }

        public DateTime Time(string key)
        {
            lock (this.cacheIndex)
            {
                if (this.cacheIndex.ContainsKey(key))
                    return new FileInfo(this.GetDataFileName(key)).LastWriteTime;
                else
                    return default(DateTime);
            }
        }

        private string GetIndexFileName(string key)
        {
            return Path.Combine(this.cacheIndexDirectory.FullName, MD5Checksum.Generate(key) + ".xml");
        }
        private string GetDataFileName(string key)
        {
            return Path.Combine(this.cacheDataDirectory.FullName, MD5Checksum.Generate(key) + ".data");
        }
        #endregion

        #region data classes
        public class CacheInfo
        {
            #region members
            private string key = default(string);
            private string hash = default(string);
            #endregion
            #region constructors
            public CacheInfo()
            {
            }
            public CacheInfo(string key, string md5Hash)
            {
                this.key = key;
                this.hash = md5Hash;
            }
            #endregion
            #region accessors
            /// <summary>
            /// Unique key of the data
            /// </summary>
            public string Key
            {
                get { return this.key; }
                set
                {
                    if (this.key == default(string))
                        this.key = value;
                    else
                        throw new InvalidOperationException("Key may not be changed after initialization of object.");
                }
            }

            /// <summary>
            /// Hash value of the data
            /// </summary>
            public string Hash
            {
                get { return this.hash; }
                set { this.hash = value; }
            }
            #endregion
        }
        #endregion
    }
}
