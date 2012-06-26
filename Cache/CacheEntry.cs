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
using Demoder.Common.Attributes;
using System.IO;

namespace Demoder.Common.Cache
{
    public class CacheEntry
    {
        public CacheEntry(){}
        public CacheEntry(byte[] data)
        {
            this.Data = new List<byte>(data);
        }

        [StreamData(100)]
        public DateTime Expirity { get; set; }

        [StreamData(200)]
        public IList<byte> Data { get; set; }

        /// <summary>
        /// Has this entry expired?
        /// </summary>
        public bool IsExpired { get { return this.Expirity.ToUniversalTime() < DateTime.UtcNow; } }

        public Stream ToStream()
        {
            return new MemoryStream(Data.ToArray());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CacheEntry))
            {
                return base.Equals(obj);
            }
            var o = obj as CacheEntry;
            if (!o.Expirity.Equals(this.Expirity)) { return false; }
            if (o.Data == null && this.Data != null) { return false; }
            if (o.Data != null && this.Data == null) { return false; }
            if (!o.Data.SequenceEqual(this.Data)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            if (this.Data == null) { return 0; }
            if (this.Data.Count == 1) { return this.Data[0]; }

            var hash = this.Data[0];
            for (int i = 1; i < this.Data.Count; i++)
            {
                hash ^= this.Data[i];
            }
            return hash;
        }

        public static CacheEntry Empty { get { return new CacheEntry { Data = null, Expirity = DateTime.MinValue }; } }
    }
}
