/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://trac.flw.nu/demoder.common/)

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
	public class SerializableDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        //private List<SerializableKeyValuePair<TKey, TValue>> _items = new List<SerializableKeyValuePair<TKey,TValue>>();
		//private Dictionary<int, List<SerializableKeyValuePair<TKey, TValue>>> _hashedList = new Dictionary<int,List<SerializableKeyValuePair<TKey,TValue>>>();
		private Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
		private int _count = 0;
        //adding value: add to Keys and Values.
        //Retrieving value: Find key in Keys list, fetch same index number from Values list
        //Constructor: Ensure that T1 and T2 are serializable

        public SerializableDictionary()
        {

			this._items = new Dictionary<TKey, TValue>();
			try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TKey));
            }
            catch
            {
                throw new ArgumentException("Key type is not serializable");
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TValue));
            }
            catch
            {
                throw new ArgumentException("Value type is not serializable");
            }
		}
		
		public SerializableDictionary(int InitialSize)
		{
			this._items = new Dictionary<TKey, TValue>(InitialSize);
		}

         /// <summary>
        /// Retrieve Keys associated value.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public TValue Get(TKey Key)
        {
			return this._items[Key];
        }
        public TValue this[TKey Key]
        {
            get
            {
                return this.Get(Key);
            }
            set
            {
				this._items[Key] = value;
            }
        }
		
		#region IDictionary<TKey,TValue> Members
		public void Add(TKey key, TValue value)
		{
			lock (this)
			{
				this._items.Add(key, value);
			}
		}
		public bool ContainsKey(TKey key)
		{
			return this._items.ContainsKey(key);
		}
		[XmlIgnore]
		ICollection<TKey> Keys
		{
			get
			{
				return this._items.Keys;
			}
		}
		[XmlIgnore]
		public ICollection<TValue> Values
		{
			get
			{
				return this._items.Values;
			}
		}
		public bool Remove(TKey key)
		{
			lock (this)
			{
				return this._items.Remove(key);
			}
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (this)
			{
				if (!this.ContainsKey(key))
				{
					value = default(TValue);
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
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}
		public void Add(SerializableKeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}
		public void Clear()
		{
			lock (this)
			{
				this._items = new Dictionary<TKey, TValue>();
				
			}
		}
		public int Count
		{
			get {
				return this._items.Count;
			}
		}
		public bool IsReadOnly
		{
			get { return false; }
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			lock (this)
			{
				return this._items.Remove(item.Key);
			}
		}
		#endregion

		#region IEnumerable<SerializableKeyValuePair<TKey,TValue>> Members
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this._items.GetEnumerator();
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
		public List<SerializableKeyValuePair<TKey, TValue>> SerializableKeyValuePairs
		{
			get
			{
				List<SerializableKeyValuePair<TKey, TValue>> skvps = new List<SerializableKeyValuePair<TKey,TValue>>(this._count);
				foreach (KeyValuePair<TKey, TValue> kvp in this._items)
				{

					skvps.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
				}
				return skvps;
			}
			set
			{
				foreach (SerializableKeyValuePair<TKey, TValue> skvp in value)
					this.Add(skvp);
			}
		}
		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members


		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
