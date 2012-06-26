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
using Demoder.Common.Hash;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Demoder.Common.Cache
{
    /// <summary>
    /// A volatile, in-memory cache target
    /// </summary>
    public class MemoryCacheTarget : ICacheTarget
    {
        private ConcurrentDictionary<MD5Checksum, MemoryCacheEntry> entries = new ConcurrentDictionary<MD5Checksum, MemoryCacheEntry>();
        private Timer housekeepingTimer;

        /// <summary>
        /// Current cache size, in bytes
        /// </summary>
        public long Size { get; private set; }
        /// <summary>
        /// Maximum amount of memory to consume for cached items.
        /// </summary>
        public long MaxSize { get; private set; }
        /// <summary>
        /// Amount of spare memory.
        /// </summary>
        public long Available { get { return this.MaxSize - this.Size; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maximumSize">Maximum cache size, in bytes.</param>
        /// <param name="periodicClean">Periodically remove stale items</param>
        public MemoryCacheTarget(long maximumSize = 8388608, bool periodicClean=true)
        {
            this.MaxSize = maximumSize;
            if (periodicClean)
            {
                this.housekeepingTimer = new Timer(this.Housekeeping, null, 1000 * 60 * 2, 1000 * 60 * 2);
            }
        }

        #region ICacheTarget Members

        public void Store(CacheEntry cacheEntry, params object[] identifiers)
        {
            if (cacheEntry.Data == null) { return; }

            MemoryCacheEntry newEntry = new MemoryCacheEntry(cacheEntry);
            var size = newEntry.Data.Count + 24;
            if (size > this.MaxSize)
            {
                // Data is larger than our maximum size.
                return;
            }

            var md5 = CacheBase.GetChecksum(identifiers);

            if (this.Available < 0)
            {
                this.RemoveStaleItems();
                this.Trim(size);
            }

            if (this.entries.ContainsKey(md5))
            {
                MemoryCacheEntry ce;
                this.entries.TryRemove(md5, out ce);
                lock (this)
                {
                    this.Size -= ce.Data.Count + 24;
                }
            }
            this.entries.TryAdd(md5, newEntry);
            lock (this)
            {
                this.Size += size;
            }
        }

        public CacheEntry Retrieve(params object[] identifiers)
        {
            var md5 = CacheBase.GetChecksum(identifiers);
            MemoryCacheEntry ce;
            if (!this.entries.TryGetValue(md5, out ce))
            {
                return CacheEntry.Empty;
            }
            ce.LastAccess = DateTime.Now;
            return ce;
        }

        public void RemoveStaleItems()
        {
            lock (this)
            {
                foreach (var kvp in this.entries.Where(kvp=>kvp.Value.IsExpired).ToArray())
                {
                    MemoryCacheEntry tmp;
                    this.entries.TryRemove(kvp.Key, out tmp);
                }
            }
        }

        /// <summary>
        /// Removes least-frequently used entries until cache size fits limit.
        /// </summary>
        public void Trim(long targetFreeSpace = 0)
        {
            if (targetFreeSpace > this.MaxSize)
            {
                throw new ArgumentOutOfRangeException("targetFreeSpace", "Must be equal or less than maximum size.");
            }

            lock (this)
            {
                if (this.Available >= targetFreeSpace)
                {
                    return;
                }
                foreach (var kvp in this.entries.OrderBy(kvp => kvp.Value.LastAccess).ToArray())
                {
                    if (this.Available >= targetFreeSpace)
                    {
                        return;
                    }
                    MemoryCacheEntry tmp;
                    this.entries.TryRemove(kvp.Key, out tmp);
                }
            }
        }

        #endregion

        private void Housekeeping(object obj)
        {
            this.RemoveStaleItems();
        }
        
    }


    public class MemoryCacheEntry : CacheEntry
    {
        public MemoryCacheEntry()
        {
            this.LastAccess = DateTime.Now;
        }

        public MemoryCacheEntry(CacheEntry ce)
            : this()
        {
            this.Data = ce.Data;
            this.Expirity = ce.Expirity;
        }

        public DateTime LastAccess { get; set; }
    }
}
