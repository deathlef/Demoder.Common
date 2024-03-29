﻿/*
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
    /// <summary>
    /// Indicates how to determine the length of data, such as a string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StreamDataLengthAttribute : Attribute
    {
        public LengthType Type { get; private set; }
        public StreamDataLengthAttribute(LengthType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        internal ulong ReadContentLength(SuperStream stream)
        {
            switch (this.Type)
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

        internal void WriteContentLength(SuperStream stream, ulong length)
        {
            switch (this.Type)
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
        }
    }
}
