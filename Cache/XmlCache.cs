/*
* Vha.Common
* Copyright (C) 2005-2010 Remco van Oosterhout
* See Credits.txt for all aknowledgements.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

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

// This class has been adapted to work with Demoder.Common. 

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Demoder.Common.Serialization;
using Demoder.Common.Hash;

namespace Demoder.Common.Cache
{
    public class XMLCache<T> : CacheBase
        where T : class
    {
        #region Constructors
        /// <summary>
        /// Initializes a new XMLCache object
        /// </summary>
        /// <param name="path">The absolute or relative path to the directory to store the cache files in</param>
        /// <param name="duration">The duration of the cache should hold objects for in minutes</param>
        /// <param name="timeout">[Ignored]</param>
        [Obsolete]
        public XMLCache(string path, int duration, int timeout)
        {
            this.CacheTarget = new FileCacheTarget(new DirectoryInfo(Path.Combine(path, MD5Checksum.Generate(typeof(T).FullName).ToString())));
            this.DefaultDuration = new TimeSpan(0, duration, 0);
        }

        /// <summary>
        /// Initializes a new FileCache-backed instance
        /// </summary>
        /// <param name="path"></param>
        /// <param name="duration">Cache items for this duration</param>
        public XMLCache(DirectoryInfo cacheDirectory, TimeSpan duration)
        {
            this.CacheTarget = new FileCacheTarget(cacheDirectory);
            this.DefaultDuration = duration;
        }

        /// <summary>
        /// Creates an instance backed by specified cache target
        /// </summary>
        /// <param name="cache">Used to store data</param>
        public XMLCache(ICacheTarget cache)
        {
            this.CacheTarget = cache;
        }

        #endregion

        public T Request(string url, params string[] args)
        {
            return this.Request(
                CacheFlags.Default,
                url,
                args);
        }

        public T Request(CacheFlags source, string url, params string[] args)
        {
            return this.Request(
                source,
                new Uri(url),
                args);
        }

        public T Request(CacheFlags source, Uri uri, params string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("expecting at least 1 argument");
            }


            // Read cache entry, if any.
            CacheEntry ce = this.CacheTarget.Retrieve(args);

            T obj = default(T);
            if ((source.HasFlag(CacheFlags.ReadCache) && !ce.IsExpired && ce.Data != null))
            {
                // Cache entry appears to be valid.
                using (Stream stream = ce.ToStream())
                {
                    if (Xml.TryDeserialize<T>(stream, out obj))
                    {
                        return obj;
                    }
                }
            }

            if (source.HasFlag(CacheFlags.ReadLive))
            {
                if (args.Length > 0)
                {
                    uri = new Uri(String.Format(uri.ToString(), args));
                }

                if (Xml.TryDeserialize<T>(uri, out obj))
                {
                    if (source.HasFlag(CacheFlags.WriteCache))
                    {
                        this.Cache(obj, args);
                    }
                    return obj;
                }
            }

            // Retreive old copy from cache
            if (source.HasFlag(CacheFlags.ReadExpired))
            {
                using (Stream stream = ce.ToStream())
                {
                    if (Xml.TryDeserialize<T>(stream, out obj))
                    {
                        return obj;
                    }
                }
            }

            // Finally, return the object
            return default(T);
        }

        public bool Cache(T obj, params string[] args)
        {
            if (obj == null)
            {
                return false;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                if (Xml.TrySerialize<T>(obj, stream))
                {
                    this.CacheTarget.Store(
                       new CacheEntry(stream.ToArray()) { Expirity = this.ExpireTime(obj) },
                       args);
                    return true;
                }
                return false;
            }
        }
    }
}
