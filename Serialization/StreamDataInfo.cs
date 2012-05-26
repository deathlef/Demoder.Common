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
using System.Reflection;
using Demoder.Common.Attributes;

namespace Demoder.Common.Serialization
{
    public class StreamDataInfo
    {
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsArray { get; private set; }
        public uint Entries { get; private set; }
        public Type ReadType { get; private set; }
        public Type DataType { get; private set; }
        public Attribute[] Attributes { get; set; }

        private StreamDataInfo() { }

        public static StreamDataInfo Create(PropertyInfo pi, StreamDataAttribute attr)
        {
            var sdi = new StreamDataInfo
            {
                PropertyInfo = pi,
                Entries = attr.Entries,
                IsArray = pi.PropertyType.IsArray,
                ReadType = attr.ReadType,
                Attributes = (from a in pi.GetCustomAttributes(true)
                              where a.GetType() != typeof(StreamDataAttribute)
                              where a is Attribute
                              select a as Attribute).ToArray()
            };

            #region Validate input and assign proper DataType
            if (sdi.ReadType != null && sdi.ReadType.IsArray)
            {
                throw new ArgumentException(String.Format("{0}->{1}: [StreamDataAttribute] specified ReadType=typeof({2}), but {2} is an array. Specify its member type {3} instead.",
                    pi.DeclaringType.FullName, pi.Name, sdi.ReadType.Name, sdi.ReadType.MemberType));
            }

            if (sdi.Entries == 0)
            {
                // Entries==0 is always invalid.
                throw new ArgumentException(String.Format("{0}->{1}: [StreamDataAttribute] specified Entries={2}.",
                    pi.DeclaringType.FullName, pi.Name, sdi.Entries));
            }
            else if (sdi.IsArray)
            {
                sdi.DataType = pi.PropertyType.GetElementType();
            }
            else if (sdi.Entries != 1)
            {
                // If it's not an array and entries are not 1, there's something wrong and we should shout out about it.
                throw new ArgumentException(String.Format("{0}->{1}: [StreamDataAttribute] specified Entries={2}, but property is not an array type.",
                    pi.DeclaringType.FullName, pi.Name, sdi.Entries));
            }
            else
            {
                sdi.DataType = pi.PropertyType;
            }

            if (sdi.ReadType == null)
            {
                sdi.ReadType = sdi.DataType;
            }
            #endregion

            return sdi;
        }
    }
}
