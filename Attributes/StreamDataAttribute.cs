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

namespace Demoder.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class StreamDataAttribute : Attribute
    {
        public uint Order { get; private set; }
        public uint Entries { get; set; }
        public Type ReadType { get; set; }
        
        /// <summary>
        /// Tag a propery as being part of the stream data
        /// </summary>
        /// <param name="order">Specifies when to populate this property. 0 is populated first, uint.MaxValue is populated last.</param>
        /// <example>Simple parsing of a single item hash and one integer representing item quality.
        /// <code lang="C#">
        /// [StreamData(0)]
        /// public Hash SpawnItem { get; set; }
        /// 
        /// [StreamData(1)]
        /// public int Quality { get; set; }
        /// </code>
        /// </example>
        /// <example>Simple parsing of a spell which contain a string, and 24 bytes of unknown data.
        /// <code lang="C#">
        /// [StreamData(0)]
        /// public string SomeString { get; set; }
        /// 
        /// [StreamData(1, Entries=24)]
        /// public byte[] UnknownData1 { get; set; }
        /// </code>
        /// </example>
        /// <example>
        /// The specified order does not have to be sequental. <br />
        /// The following code will still end up with the properties being populated in the order of Age, TTL and Options.
        /// <code lang="C#">
        /// [StreamData(1000)]
        /// public uint Age { get; set; }
        /// 
        /// [StreamData(2000)]
        /// public uint TTL { get; set; }
        /// 
        /// [StreamData(3000)]
        /// public Flags Options { get; set; }
        /// </code>
        /// </example>
        /// <example lang="C#">
        /// You can also read one data type and cast it to another.<br/>
        /// In this example, a byte will be read then cast to int.
        /// <code lang="C#">
        /// [StreamData(0, ReadType=typeof(byte))]
        /// public int SomeNumber {get; set;}
        /// </code>
        /// </example>
        /// <example>
        /// You may also associate arbitary attributes with a property to modify the parsers behaviour, if it supports so.
        /// <code lang="C#">
        /// [StreamData(0)]
        /// [StreamDataString(StringType.CString)]
        /// public string SomeString {get; set; }
        /// </code> 
        /// </example>
        public StreamDataAttribute(uint order)
        {
            this.Order = order;
            this.Entries = 0;
        }
    }
}
