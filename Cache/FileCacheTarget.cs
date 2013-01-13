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
using System.Linq;
using System.Text;
using System.IO;
using Demoder.Common.Hash;
using Demoder.Common.Serialization;

namespace Demoder.Common.Cache
{
    public class FileCacheTarget : ICacheTarget
    {
        private DirectoryInfo CacheDirectory { get; set; }
        public static DirectoryInfo DefaultCacheRootDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(Misc.MyTemporaryDirectory, "FileCache"));
            }
        }

        public FileCacheTarget(string cacheName)
            : this(DefaultCacheRootDirectory, cacheName)
        {
        }

        public FileCacheTarget(DirectoryInfo cacheRootDirectory, string cacheName = "")
        {
            if (String.IsNullOrWhiteSpace(cacheName))
            {
                this.CacheDirectory = cacheRootDirectory;
            }
            else
            {
                this.CacheDirectory = new DirectoryInfo(Path.Combine(cacheRootDirectory.FullName, cacheName));
            }
            if (!this.CacheDirectory.Exists)
            {
                this.CacheDirectory.Create();
            }
        }

        /// <summary>
        /// Retrieves the path & name of the cache entry file
        /// </summary>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        private FileInfo GetCacheEntryFile(params object[] identifiers)
        {
            var md5 = CacheBase.GetChecksum(identifiers).ToString();
            return new FileInfo(
                Path.Combine(
                    this.CacheDirectory.FullName,
                    md5.Substring(0, 2),
                    md5 + ".cachefile"));
        }

        public void Store(CacheEntry cacheEntry, params object[] identifiers)
        {
            lock (this.CacheDirectory)
            {
                var file = this.GetCacheEntryFile(identifiers);
                if (!file.Directory.Exists) { file.Directory.Create(); }

                using (var stream = new SuperStream(
                                        new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None), 
                                        Endianess.Little) 
                                        { 
                                             DisposeBaseStream = true 
                                        })
                {
                    StreamData.Serialize(cacheEntry, stream);
                }
            }
        }

        public CacheEntry Retrieve(params object[] identifiers)
        {
            lock (this.CacheDirectory)
            {
                var file = this.GetCacheEntryFile(identifiers);
                if (!file.Exists)
                {
                    return CacheEntry.Empty;
                }
                using (var stream = new SuperStream(file.OpenRead(), Endianess.Little) { DisposeBaseStream = true })
                {
                    return StreamData.Create<CacheEntry>(stream);
                }
            }
        }

        public void RemoveStaleItems()
        {
            lock (this.CacheDirectory)
            {
                foreach (var dir in this.CacheDirectory.GetDirectories())
                {
                    foreach (var file in dir.GetFiles("*.cachefile", SearchOption.TopDirectoryOnly))
                    {
                        if (this.IsStale(file))
                        {
                            file.Delete();
                        }
                    }
                }
            }
        }

        private bool IsStale(FileInfo file)
        {
            // Create a read stream
            using (var stream = new SuperStream(file.OpenRead(), Endianess.Little) { DisposeBaseStream = true })
            {
                // Find out of this entry should be removed
                // by reading a DateTime object (first parameter) 
                // from the file.
                var dt = Misc.Unixtime(stream.ReadInt64());
                return dt < DateTime.Now;
            }
        }
    }
}
