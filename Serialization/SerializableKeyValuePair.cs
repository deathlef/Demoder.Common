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

namespace Demoder.Common.Serialization
{
	public class SerializableKeyValuePair<TKey, TValue>
	{
		#region Members
		private TKey _key;
		private TValue _value;
		#endregion
		#region Public accessors
		public TKey Key
		{
			get { return this._key; }
			set
			{
				lock (this)
					this._key = value;
			}
		}
		public TValue Value { 
			get { return this._value; }
			set {
				lock (this)
					this._value = value;
			}
		}
		#endregion
		public SerializableKeyValuePair() 
		{
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
		public SerializableKeyValuePair(TKey Key, TValue Value) : this()
		{ 
			KeyValuePair<string,string>test = new KeyValuePair<string,string>();
			this._key = Key; 
			this._value = Value; 
		}
		public override string ToString()
		{
			return String.Format("[{0}, {1}]", this._key, this._value);
		}

		#region Operators
		public static bool operator ==(SerializableKeyValuePair<TKey, TValue> SKVP1, SerializableKeyValuePair<TKey,TValue> SKVP2)
		{
			if (!SKVP1.Key.Equals(SKVP2.Key))
				return false;
			if (!SKVP1.Value.Equals(SKVP2.Value))
				return false;
			return true;
		}

		public static bool operator !=(SerializableKeyValuePair<TKey, TValue> SKVP1, SerializableKeyValuePair<TKey, TValue> SKVP2)
		{
			if (SKVP1.Key.Equals(SKVP2.Key))
				return false;
			if (SKVP1.Value.Equals(SKVP2.Value))
				return false;
			return true;
		}
		#endregion
		public bool Equals(SerializableKeyValuePair<TKey, TValue> obj)
		{
			if (!this._key.Equals(obj.Key))
				return false;
			if (!this._value.Equals(obj.Value))
				return false;
			return true;
		}
		public bool Equals(KeyValuePair<TKey, TValue> obj)
		{
			if (!this._key.Equals(obj.Key))
				return false;
			if (!this._value.Equals(obj.Value))
				return false;
			return true;
		}
	}
}
