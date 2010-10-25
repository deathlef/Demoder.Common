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
	public class SerializableDictionary<TKey, TValue> : ICollection<SerializableKeyValuePair<TKey, TValue>>, IEnumerable<SerializableKeyValuePair<TKey, TValue>>, IEnumerable
    {
        //private List<SerializableKeyValuePair<TKey, TValue>> _items = new List<SerializableKeyValuePair<TKey,TValue>>();
		private Dictionary<int, List<SerializableKeyValuePair<TKey, TValue>>> _hashedList = new Dictionary<int,List<SerializableKeyValuePair<TKey,TValue>>>();
		//private Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
		private int _count = 0;
        //adding value: add to Keys and Values.
        //Retrieving value: Find key in Keys list, fetch same index number from Values list
        //Constructor: Ensure that T1 and T2 are serializable

        public SerializableDictionary()
        {

			Dictionary<string, string> dict = new Dictionary<string, string>();
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
		
		private void hashCreate(int Hash)
		{
			if (!this._hashedList.ContainsKey(Hash))
				this._hashedList.Add(Hash, new List<SerializableKeyValuePair<TKey, TValue>>());
		}

		public SerializableDictionary(int InitialSize)
		{
			//this._items = new List<SerializableKeyValuePair<TKey, TValue>>(InitialSize);
			this._hashedList = new Dictionary<int, List<SerializableKeyValuePair<TKey, TValue>>>();
		}

         /// <summary>
        /// Retrieve Keys associated value.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public TValue Get(TKey Key)
        {
			int hash = Key.GetHashCode();
			this.hashCreate(hash);
			foreach (SerializableKeyValuePair<TKey, TValue> skvp in this._hashedList[hash])
				if (skvp.Key.Equals(Key))
					return skvp.Value;
			/*
			foreach (SerializableKeyValuePair<TKey, TValue> skvp in this._items)
				if (skvp.Key.Equals(Key))
					return skvp.Value;
			 */
			throw new ArgumentException("Key doesn't exist");
        }
        public TValue this[TKey Key]
        {
            get
            {
                return this.Get(Key);
            }
            set
            {
				this.Remove(Key);
				this.Add(Key, value);
                
            }
        }
		
		#region IDictionary<TKey,TValue> Members
		public void Add(TKey key, TValue value)
		{
			lock (this)
			{
				if (this.ContainsKey(key))
					throw new ArgumentException("Duplicate key");
				this._hashedList[key.GetHashCode()].Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
				this._count++;
				//this._items.Add(new SerializableKeyValuePair<TKey,TValue>(key, value));
			}
		}
		public bool ContainsKey(TKey key)
		{
			//foreach (SerializableKeyValuePair<TKey, TValue> skvp in this._items)
			int hash = key.GetHashCode();
			this.hashCreate(hash);
			foreach (SerializableKeyValuePair<TKey, TValue> skvp in this._hashedList[hash])
				if (skvp.Key.Equals(key))
					return true;
			return false;
		}
		[XmlIgnore]
		ICollection<TKey> Keys
		{
			get
			{
				List<TKey> keys = new List<TKey>();
				foreach (KeyValuePair<int, List<SerializableKeyValuePair<TKey, TValue>>> kvp in this._hashedList)
					foreach (SerializableKeyValuePair<TKey, TValue> skvp in kvp.Value)
						keys.Add(skvp.Key);
				return keys.ToArray();
			}
		}
		[XmlIgnore]
		public ICollection<TValue> Values
		{
			get
			{
				List<TValue> values = new List<TValue>();
				foreach (KeyValuePair<int, List<SerializableKeyValuePair<TKey, TValue>>> kvp in this._hashedList)
					foreach (SerializableKeyValuePair<TKey, TValue> skvp in kvp.Value)
						values.Add(skvp.Value);
				return values.ToArray();
			}
		}
		public bool Remove(TKey key)
		{
			lock (this)
			{
				int hash = key.GetHashCode();
				this.hashCreate(hash);
				foreach (SerializableKeyValuePair<TKey, TValue> kvp in this._hashedList[hash])
					if (kvp.Equals(key))
					{
						this._hashedList[key.GetHashCode()].Remove(kvp);
						return true;
					}
			}
			return false;
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
				//this._items = new List<SerializableKeyValuePair<TKey, TValue>>();
				this._hashedList = new Dictionary<int, List<SerializableKeyValuePair<TKey, TValue>>>();
			}
		}
		public bool Contains(SerializableKeyValuePair<TKey, TValue> item)
		{
			lock (this)
			{
				TValue val;
				if (!this.TryGetValue(item.Key, out val))
					return false;
				else if (val.Equals(item.Value))
					return true;
				else
					return false;
			}
		}
		public int Count
		{
			get {
				return this._count;
				//return this._items.Count; 
			}
		}
		public bool IsReadOnly
		{
			get { return false; }
		}
		public bool Remove(SerializableKeyValuePair<TKey, TValue> item)
		{
			lock (this)
			{
				//foreach (SerializableKeyValuePair<TKey, TValue> kvp in this._items)
				int hash = item.Key.GetHashCode();
				this.hashCreate(hash);
				foreach (SerializableKeyValuePair<TKey, TValue> kvp in this._hashedList[hash])
					if (kvp.Key.Equals(item.Key) && kvp.Value.Equals(item.Value))
					{
						//this._items.Remove(kvp);
						this._hashedList[hash].Remove(kvp);
						this._count--;
						return true;
					}
				return false;
			}
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			int pos=arrayIndex;
			foreach (SerializableKeyValuePair<TKey, TValue> kvp in this.SerializableKeyValuePairs)
			{
				KeyValuePair<TKey, TValue> skvp = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
				array[pos] = skvp;
				pos++;
			}
		}
		public void CopyTo(SerializableKeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			int pos = arrayIndex;
			foreach (SerializableKeyValuePair<TKey, TValue> kvp in this.SerializableKeyValuePairs)
			{
				array[pos] = kvp;
				pos++;
			}
		}
		#endregion

		#region IEnumerable<SerializableKeyValuePair<TKey,TValue>> Members
		public IEnumerator<SerializableKeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.SerializableKeyValuePairs.GetEnumerator();
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
				foreach (KeyValuePair<int, List<SerializableKeyValuePair<TKey, TValue>>> kvp in this._hashedList)
					foreach (SerializableKeyValuePair<TKey, TValue> skvp in kvp.Value)
						skvps.Add(skvp);
				return skvps;
			}
			set
			{
				foreach (SerializableKeyValuePair<TKey, TValue> skvp in value)
					this.Add(skvp);
			}
		}
		#endregion
	}
}
