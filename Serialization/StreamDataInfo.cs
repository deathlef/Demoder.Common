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
using System.Collections.Concurrent;
using Demoder.Common.Extensions;

namespace Demoder.Common.Serialization
{
    public class StreamDataInfo : ICloneable
    {
        public PropertyInfo PropertyInfo { get; private set; }
        public bool IsArray { get; private set; }
        public bool IsList { get; private set; }
        public bool IsCollection { get { return this.IsArray || this.IsList; } }
        public uint Entries { get; private set; }
        public Type ReadType { get; private set; }
        public Type DataType { get; private set; }
        public Attribute[] Attributes { get; set; }

        private StreamDataInfo() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        internal ulong ReadContentLength(SuperStream stream)
        {
            var countType = this.Attributes.FirstOrDefault(a => a is StreamDataCollectionLengthAttribute) as StreamDataCollectionLengthAttribute;

            if (countType == null)
            {
                return this.Entries;
            }

            switch (countType.Type)
            {
                case LengthType.Byte:
                    return (byte)stream.ReadByte();
                case LengthType.UInt16:
                    return stream.ReadUInt16();
                default:
                case LengthType.UInt32:
                    return stream.ReadUInt32();
            }
        }

        internal ulong WriteContentLength(SuperStream stream, ulong length)
        {
            var countType = this.Attributes.FirstOrDefault(a => a is StreamDataCollectionLengthAttribute) as StreamDataCollectionLengthAttribute;
            if (countType == null) { return this.Entries; }
            switch (countType.Type)
            {
                case LengthType.Byte:
                    stream.WriteByte((byte)length);
                    break;
                case LengthType.UInt16:
                    stream.WriteUInt16((ushort)length);
                    break;
                default:
                case LengthType.UInt32:
                    stream.WriteUInt32((uint)length);
                    break;
            }
            return length;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public StreamDataInfo Clone()
        {
            return new StreamDataInfo
            {
                PropertyInfo = this.PropertyInfo.DeclaringType.GetProperty(this.PropertyInfo.Name),
                Attributes = this.Attributes,
                DataType = this.DataType,
                Entries = this.Entries,
                IsArray = this.IsArray,
                IsList = this.IsList,
                ReadType = this.ReadType
            };
        }

        #region Static stuff
        /// <summary>
        /// Property information cache per type.
        /// Common store shared amongst threads
        /// </summary>
        private static ConcurrentDictionary<Type, StreamDataInfo[]> cachedProperties = new ConcurrentDictionary<Type, StreamDataInfo[]>();
        
        public static StreamDataInfo Create(PropertyInfo pi, StreamDataAttribute attr)
        {
            var sdi = new StreamDataInfo
            {
                PropertyInfo = pi,
                Entries = attr.Entries,
                IsArray = false,
                IsList = false,
                ReadType = attr.ReadType,
                Attributes = (from a in pi.GetCustomAttributes(true)
                              where a.GetType() != typeof(StreamDataAttribute)
                              where a is Attribute
                              select a as Attribute).ToArray()
            };

            sdi.IsArray = pi.PropertyType.IsArray;
            #region Check if it's a IList.
            if (pi.PropertyType.IsGenericType &&
                pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                sdi.IsList = true;
            }


            #endregion

            #region Validate input and assign proper DataType
            if (sdi.ReadType != null && sdi.ReadType.IsArray)
            {
                throw new ArgumentException(String.Format("{0}->{1}: [StreamDataAttribute] specified ReadType=typeof({2}), but {2} is an array. Specify its member type {3} instead.",
                    pi.DeclaringType.FullName, pi.Name, sdi.ReadType.Name, sdi.ReadType.MemberType));
            }

            if (sdi.IsArray)
            {
                sdi.DataType = pi.PropertyType.GetElementType();
            }
            else if (sdi.IsList)
            {
                sdi.DataType = pi.PropertyType.GetGenericArguments().First();
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
            #region Verify that collections have correct parameters
            if (sdi.IsCollection)
            {
                if (sdi.Entries != 0 && sdi.Attributes.FirstOrDefault(a => a is StreamDataCollectionLengthAttribute) != null)
                {
                    throw new ArgumentException(String.Format("{0}->{1}: Cannot specify both [StreamDataCollectionLengthAttribute] and [StreamDataAttribute].Entries>0.",
                    pi.DeclaringType.FullName, pi.Name, sdi.Entries));
                }
                if (sdi.Entries == 0 && sdi.Attributes.FirstOrDefault(a => a is StreamDataCollectionLengthAttribute) == null)
                {
                    throw new ArgumentException(String.Format("{0}->{1}: Collection must have either [StreamDataCollectionLengthAttribute] or [StreamDataAttribute] specifying Entries>0.",
                    pi.DeclaringType.FullName, pi.Name, sdi.Entries));
                }
            }
            #endregion

            return sdi;
        }


        /// <summary>
        /// Retrieve all StreamData properties contained within type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static StreamDataInfo[] GetProperties(Type type)
        {
            StreamDataInfo[] retVal;
            // Try fetching from common cache
            if (cachedProperties.TryGetValue(type, out retVal))
            {
                return retVal.Select(r => r.Clone()).ToArray();
            }

            BindingFlags bind = BindingFlags.Instance | BindingFlags.Public;
            if (type.GetAttribute<StreamDataIncludeBaseAttribute>() == null)
            {
                bind |= BindingFlags.DeclaredOnly;
            }
            retVal = (from pi in type.GetProperties(bind)
                      let attr = (StreamDataAttribute)pi.GetCustomAttributes(typeof(StreamDataAttribute), true).FirstOrDefault()
                      // Only consider SpellData properties
                      where attr != null
                      orderby attr.Order ascending
                      select StreamDataInfo.Create(pi, attr)).ToArray();

            cachedProperties.TryAdd(type, retVal);
            return retVal;
        }
        #endregion
    }
}
