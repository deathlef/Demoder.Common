/*
Demoder.Common
Copyright (c) 2010,2011 Demoder <demoder@demoder.me>

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
    public class SerializableKeyValuePair<T1, T2>
    {
        #region Members
        private T1 key;
        private T2 value;
        #endregion

        #region Public accessors
        public T1 Key
        {
            get { return this.key; }
            set
            {
                lock (this)
                    this.key = value;
            }
        }
        public T2 Value
        {
            get { return this.value; }
            set
            {
                lock (this)
                    this.value = value;
            }
        }
        #endregion

        public SerializableKeyValuePair()
        {
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
        public SerializableKeyValuePair(T1 key, T2 value)
            : this()
        {
            KeyValuePair<string, string> test = new KeyValuePair<string, string>();
            this.key = key;
            this.value = value;
        }
        public override string ToString()
        {
            return String.Format("[{0}, {1}]", this.key, this.value);
        }

        #region Operators
        public static bool operator ==(SerializableKeyValuePair<T1, T2> obj1, SerializableKeyValuePair<T1, T2> obj2)
        {
            if (!obj1.Key.Equals(obj2.Key))
                return false;
            if (!obj1.Value.Equals(obj2.Value))
                return false;
            return true;
        }

        public static bool operator !=(SerializableKeyValuePair<T1, T2> obj1, SerializableKeyValuePair<T1, T2> obj2)
        {
            if (obj1.Key.Equals(obj2.Key))
                return false;
            if (obj1.Value.Equals(obj2.Value))
                return false;
            return true;
        }
        #endregion
        public bool Equals(SerializableKeyValuePair<T1, T2> obj)
        {
            if (!this.key.Equals(obj.Key))
                return false;
            if (!this.value.Equals(obj.Value))
                return false;
            return true;
        }
        public bool Equals(KeyValuePair<T1, T2> obj)
        {
            if (!this.key.Equals(obj.Key))
                return false;
            if (!this.value.Equals(obj.Value))
                return false;
            return true;
        }
    }
}