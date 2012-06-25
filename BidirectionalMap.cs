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

namespace Demoder.Common
{
    public class BidirectionalMap<T1, T2>
    {
        private Dictionary<T1, T2> t1T2;
        private Dictionary<T2, T1> t2T1;

        public BidirectionalMap()
        {

        }

        public BidirectionalMap(IEqualityComparer<T1> t1Comparer, IEqualityComparer<T2> t2Comparer)
        {
            if (t1Comparer != null)
            {
                this.t1T2 = new Dictionary<T1, T2>(t1Comparer);
            }
            if (t2Comparer != null)
            {
                this.t2T1 = new Dictionary<T2, T1>(t2Comparer);
            }

            if (typeof(T1) == typeof(T2)) { throw new ArgumentException("T1 and T2 cannot be the same type."); }
        }

        public T2 this[T1 key]
        {
            get
            {
                lock (this)
                {
                    if (!this.t1T2.ContainsKey(key))
                    {
                        return default(T2);
                    }
                    return this.t1T2[key];
                }
            }
            set
            {
                this.SetValue(key, value);
            }
        }

        public T1 this[T2 key]
        {
            get
            {
                lock (this)
                {
                    if (!this.t2T1.ContainsKey(key))
                    {
                        return default(T1);
                    }
                    return this.t2T1[key];
                }
            }
            set
            {
                this.SetValue(value, key);
            }
        }

        private void SetValue(T1 t1, T2 t2)
        {
            lock (this)
            {
                // Set new entries
                this.t1T2[t1] = t2;
                this.t2T1[t2] = t1;
            }
        }

        public int Count { get { return this.t1T2.Count; } }
    }
}
