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
    [Flags]
    public enum XMLCacheFlags
    {
        /// <summary>
        /// Retrieve the object from recent cache
        /// </summary>
        ReadCache = 0x01,
        /// <summary>
        /// Write the object to recent cache if pulled from live data source
        /// </summary>
        WriteCache = 0x02,
        /// <summary>
        /// Retrieve the object from live data source
        /// </summary>
        ReadLive = 0x04,
        /// <summary>
        /// Retrieve the object from cache as last resort, regardless of age
        /// </summary>
        ReadExpired = 0x08,
        /// <summary>
        /// The default flags
        /// </summary>
        Default = ReadCache | WriteCache | ReadLive
    }

    public class XMLCache<T>
        where T : class
    {
        private ICacheTarget cache;

        public TimeSpan Duration { get; private set; }

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
            this.cache = new FileCache(new DirectoryInfo(Path.Combine(path, MD5Checksum.Generate(typeof(T).FullName).ToString())));
            this.Duration = new TimeSpan(0, duration, 0);
        }

        /// <summary>
        /// Initializes a new FileCache-backed instance
        /// </summary>
        /// <param name="path"></param>
        /// <param name="duration">Cache items for this duration</param>
        public XMLCache(DirectoryInfo cacheDirectory, TimeSpan duration)
        {
            this.cache = new FileCache(cacheDirectory);
            this.Duration = duration;
        }

        /// <summary>
        /// Creates an instance backed by specified cache target
        /// </summary>
        /// <param name="cache">Used to store data</param>
        public XMLCache(ICacheTarget cache)
        {
            this.cache = cache;
        }

        #endregion

        public T Request(string url, params string[] args)
        { 
            return this.Request(
                XMLCacheFlags.Default, 
                url, 
                args);
        }

        public T Request(XMLCacheFlags source, string url, params string[] args)
        { 
            return this.Request(
                source, 
                new Uri(url), 
                args); 
        }

        public T Request(XMLCacheFlags source, Uri uri, params string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("expecting at least 1 argument");
            }

            
            // Read cache entry, if any.
            var ce = this.cache.Retrieve(args);

            var obj = default(T);
            if ((source.HasFlag(XMLCacheFlags.ReadCache) && !ce.IsExpired && ce.Data!=null))
            {
                // Cache entry appears to be valid.
                obj = Xml.Deserialize<T>(ce.ToStream(), true);
                if (obj != null)
                {
                    return obj;
                }
            }

            if (source.HasFlag(XMLCacheFlags.ReadLive))
            {
                if (args.Length > 0)
                {
                    uri = new Uri(String.Format(uri.ToString(), args));
                }

                obj = Xml.Deserialize<T>(uri);

                if (obj != null)
                {
                    if (source.HasFlag(XMLCacheFlags.WriteCache))
                    {
                        this.Cache(obj, args);
                    }
                    return obj;
                }
            }

            // Retreive old copy from cache
            if (obj == null && (source.HasFlag(XMLCacheFlags.ReadExpired)))
            {
                obj = Xml.Deserialize<T>(ce.ToStream(), true);
                return obj;
            }
            // Finally, return the object
            return obj;
        }

        public bool Cache(T obj, params string[] args)
        {
            if (obj == null)
            {
                return false;
            }
            var ms = new MemoryStream();
            Xml.Serialize<T>(ms, obj, false);

            this.cache.Store(
                new CacheEntry(ms.ToArray()) { Expirity = this.GetExpirity(obj) },
                args);
            return true;
        }

        private DateTime GetExpirity(T obj)
        {
            if (obj is ICacheExpity)
            {
                return (obj as ICacheExpity).CacheUntil;
            }
            return DateTime.Now.Add(this.Duration);
        }
    }
}
