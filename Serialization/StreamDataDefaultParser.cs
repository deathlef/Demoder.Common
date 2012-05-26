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


namespace Demoder.Common.Serialization
{
    /// <summary>
    /// Fallback data parser, for handling the most basic types.
    /// </summary>
    public class StreamDataDefaultParser : IStreamDataParser
    {
        public Type[] SupportedTypes
        {
            get
            {
                return new Type[0];
            }
        }

        public bool Parse(StreamDataParserTask task, out dynamic value)
        {
            if (task.ReadType == typeof(Byte))
            {
                value= (byte)task.Stream.ReadByte();
                return true;
            }

            if (task.ReadType == typeof(Int64))
            {
                value= task.Stream.ReadInt64();
                return true;
            }
            if (task.ReadType == typeof(UInt64))
            {
                value= task.Stream.ReadUInt64();
                return true;
            }

            if (task.ReadType == typeof(Int32))
            {
                value =  task.Stream.ReadInt32();
                return true;
            }
            if (task.ReadType == typeof(UInt32))
            {
                value= task.Stream.ReadUInt32();
                return true;
            }

            if (task.ReadType == typeof(Int16)) 
            {
                value = task.Stream.ReadInt16();
                return true;
            }
            if (task.ReadType == typeof(UInt16))
            {
                value = task.Stream.ReadUInt16();
                return true;
            }

            if (task.ReadType == typeof(Single))
            {
                value = task.Stream.ReadSingle();
                return true;
            }

            if (task.ReadType == typeof(Double))
            {
                value = task.Stream.ReadDouble();
                return true;
            }

            if (task.ReadType == typeof(bool))
            {
                var val = task.Stream.ReadUInt32();
                if (val == 0)
                {
                    value = false;
                    return true;
                }
                else if (val == 1)
                {
                    value = true;
                    return true;
                }
                else
                {
                    throw new Exception("Parsing type bool: Expected value to be either 0 or 1, but it was " + val.ToString());
                }
            }

            if (task.ReadType.IsEnum)
            {
                var flags = task.Stream.ReadUInt32();
                value = Enum.ToObject(task.ReadType, flags);
                return true;
            }

            // Test if type has StreamDataAttribute on properties.
            // This allows nesting of StreamData-aware task.DataTypes
            var props = StreamData.GetProperties(task.ReadType);
            if (props.Length == 0)
            {
                value = null;
                return false;
            }

            value = StreamData.Create(task.ReadType, task.Stream);
            // Need to add error condition here.
            return true;
        }
    }
}
