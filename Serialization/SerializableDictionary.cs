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
using System.Text;
using System.Xml.Serialization;
using System.Collections;

namespace Demoder.Common.Serialization
{
    public class SerializableDictionary<T1, T2> : ICollection<KeyValuePair<T1, T2>>, IEnumerable<KeyValuePair<T1, T2>>, IEnumerable
    {
        //private List<SerializableKeyValuePair<TKey, TValue>> _items = new List<SerializableKeyValuePair<TKey,TValue>>();
        //private Dictionary<int, List<SerializableKeyValuePair<TKey, TValue>>> _hashedList = new Dictionary<int,List<SerializableKeyValuePair<TKey,TValue>>>();
        private Dictionary<T1, T2> items = new Dictionary<T1, T2>();
        private int count = 0;
        //adding value: add to Keys and Values.
        //Retrieving value: Find key in Keys list, fetch same index number from Values list
        //Constructor: Ensure that T1 and T2 are serializable

        public SerializableDictionary()
        {

            this.items = new Dictionary<T1, T2>();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T1));
            }
            catch
            {
                throw new ArgumentException("Key type is not serializable");
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T2));
            }
            catch
            {
                throw new ArgumentException("Value type is not serializable");
            }
        }

        public SerializableDictionary(int initialSize)
        {
            this.items = new Dictionary<T1, T2>(initialSize);
        }

        /// <summary>
        /// Retrieve Keys associated value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T2 Get(T1 key)
        {
            return this.items[key];
        }
        public T2 this[T1 key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                this.items[key] = value;
            }
        }

        #region IDictionary<TKey,TValue> Members
        public void Add(T1 key, T2 value)
        {
            lock (this)
            {
                this.items.Add(key, value);
            }
        }
        public bool ContainsKey(T1 key)
        {
            return this.items.ContainsKey(key);
        }
        [XmlIgnore]
        ICollection<T1> Keys
        {
            get
            {
                return this.items.Keys;
            }
        }
        [XmlIgnore]
        public ICollection<T2> Values
        {
            get
            {
                return this.items.Values;
            }
        }
        public bool Remove(T1 key)
        {
            lock (this)
            {
                return this.items.Remove(key);
            }
        }
        public bool TryGetValue(T1 key, out T2 value)
        {
            lock (this)
            {
                if (!this.ContainsKey(key))
                {
                    value = default(T2);
                    return false;
                }
                else
                {
                    value = this[key];
                    return true;
                }
            }
        }
        #endregion

        #region ICollection<SerializableKeyValuePair<TKey,TValue>> Members
        public void Add(KeyValuePair<T1, T2> item)
        {
            this.Add(item.Key, item.Value);
        }
        public void Add(SerializableKeyValuePair<T1, T2> item)
        {
            this.Add(item.Key, item.Value);
        }
        public void Clear()
        {
            lock (this)
            {
                this.items = new Dictionary<T1, T2>();

            }
        }
        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(KeyValuePair<T1, T2> item)
        {
            lock (this)
            {
                return this.items.Remove(item.Key);
            }
        }
        #endregion

        #region IEnumerable<SerializableKeyValuePair<TKey,TValue>> Members
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        #endregion

        #region Serialization
        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<SerializableKeyValuePair<T1, T2>> SerializableKeyValuePairs
        {
            get
            {
                List<SerializableKeyValuePair<T1, T2>> skvps = new List<SerializableKeyValuePair<T1, T2>>(this.count);
                foreach (KeyValuePair<T1, T2> kvp in this.items)
                {

                    skvps.Add(new SerializableKeyValuePair<T1, T2>(kvp.Key, kvp.Value));
                }
                return skvps;
            }
            set
            {
                foreach (SerializableKeyValuePair<T1, T2> skvp in value)
                    this.Add(skvp);
            }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members


        public bool Contains(KeyValuePair<T1, T2> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
