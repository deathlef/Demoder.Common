/*
MIT Licence
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: https://sourceforge.net/projects/demoderstools/)

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

namespace Demoder.Common
{
    public class SerializableDictionary<T1, T2>
    {
        public List<T1> Keys = new List<T1>();
        public List<T2> Values = new List<T2>();
        //adding value: add to Keys and Values.
        //Retrieving value: Find key in Keys list, fetch same index number from Values list
        //Constructor: Ensure that T1 and T2 are serializable

        public SerializableDictionary()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T1));
            }
            catch
            {
                throw new Exception("Key type is not serializable");
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T2));
            }
            catch
            {
                throw new Exception("Value type is not serializable");
            }
        }


        /// <summary>
        /// Add a key=>value pair
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns>Index of key</returns>
        public int Add(T1 Key, T2 Value)
        {
            lock (this)
            {
                int index = this.FindKeyIndex(Key);
                if (index == -1)
                {
                    index = this.Keys.Count;
                    this.Keys.Add(Key);
                    this.Values.Add(Value);
                }
                else
                    throw new ArgumentException("Key already exists.");
                return index;
            }
        }

        public int Set(T1 Key, T2 Value)
        {
            lock (this)
            {
                int index = this.FindKeyIndex(Key);
                if (index == -1)
                {
                    index = this.Keys.Count;
                    this.Keys.Add(Key);
                    this.Values.Add(Value);
                }
                else if (index >= 0)
                    this.Values[index] = Value;
                return index;
            }
        }


        /// <summary>
        /// Find the index of Key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns>Index of key, -1 if not found</returns>
        public int FindKeyIndex(T1 Key)
        {
            lock (this)
            {
                int index = -1;
                for (int i = 0; i < this.Keys.Count; i++)
                {
                    foreach (T1 key in this.Keys)
                    {
                        if (key.Equals(Key))
                        {
                            index = i;
                        }
                    }
                }
                return index;
            }
        }

        public void Remove(T1 Key)
        {
            lock (this)
            {
                int index = this.FindKeyIndex(Key);
                if (index >= 0)
                {
                    this.Keys.RemoveAt(index);
                    this.Values.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Retrieve Keys associated value.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public T2 Get(T1 Key)
        {
            int index = this.FindKeyIndex(Key);
            if (index == -1) return default(T2);
            else
                return this.Values[index];
        }

        public T2 this[T1 Key]
        {
            get
            {
                return this.Get(Key);
            }
            set
            {
                if (Values != null && Values.Count > 0)
                {
                    foreach (T2 Value in Values)
                    {
                        this.Set(Key, Value);
                    }
                }
                else
                {
                    this.Set(Key, value);
                }
            }
        }
    }
}
