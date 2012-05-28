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

        public bool GetObject(StreamDataParserTask task, out object value)
        {
            if (task.StreamType == typeof(Byte))
            {
                value= (byte)task.Stream.ReadByte();
                return true;
            }

            if (task.StreamType == typeof(Int64))
            {
                value= task.Stream.ReadInt64();
                return true;
            }
            if (task.StreamType == typeof(UInt64))
            {
                value= task.Stream.ReadUInt64();
                return true;
            }

            if (task.StreamType == typeof(Int32))
            {
                value =  task.Stream.ReadInt32();
                return true;
            }
            if (task.StreamType == typeof(UInt32))
            {
                value= task.Stream.ReadUInt32();
                return true;
            }

            if (task.StreamType == typeof(Int16)) 
            {
                value = task.Stream.ReadInt16();
                return true;
            }
            if (task.StreamType == typeof(UInt16))
            {
                value = task.Stream.ReadUInt16();
                return true;
            }

            if (task.StreamType == typeof(Single))
            {
                value = task.Stream.ReadSingle();
                return true;
            }

            if (task.StreamType == typeof(Double))
            {
                value = task.Stream.ReadDouble();
                return true;
            }

            if (task.StreamType == typeof(bool))
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

            if (task.StreamType.IsEnum)
            {
                var flags = task.Stream.ReadUInt32();
                value = Enum.ToObject(task.StreamType, flags);
                return true;
            }

            // Test if type has StreamDataAttribute on properties.
            // This allows nesting of StreamData-aware task.DataTypes
            var props = StreamData.GetProperties(task.StreamType);
            if (props.Length == 0)
            {
                value = null;
                return false;
            }

            value = StreamData.Create(task.StreamType, task.Stream);
            // Need to add error condition here.
            return true;
        }

        #region IStreamDataParser Members


        public bool WriteObject(StreamDataParserTask task, object value)
        {
            if (task.StreamType == typeof(Byte))
            {
                task.Stream.WriteByte((byte)value);
                return true;
            }

            if (task.StreamType == typeof(Int64))
            {
                task.Stream.WriteInt64((Int64)value);
                return true;
            }
            if (task.StreamType == typeof(UInt64))
            {
                task.Stream.WriteUInt64((UInt64)value);
                return true;
            }

            if (task.StreamType == typeof(Int32))
            {
                task.Stream.WriteInt32((Int32)value);
                return true;
            }
            if (task.StreamType == typeof(UInt32))
            {
                task.Stream.WriteUInt32((UInt32)value);
                return true;
            }

            if (task.StreamType == typeof(Int16))
            {
                task.Stream.WriteInt16((Int16)value);
                return true;
            }
            if (task.StreamType == typeof(UInt16))
            {
                task.Stream.WriteUInt16((UInt16)value);
                return true;
            }

            if (task.StreamType == typeof(Single))
            {
                task.Stream.WriteSingle((Single)value);
                return true;
            }

            if (task.StreamType == typeof(Double))
            {
                task.Stream.WriteDouble((Double)value);
                return true;
            }

            if (task.StreamType == typeof(bool))
            {
                // If bool is true, write 1. Otherwise, write 0.
                int val = ((bool)value) ? 1 : 0;
                task.Stream.WriteInt32(val);
            }

            if (task.StreamType.IsEnum)
            {
                var flags = task.Stream.ReadUInt32();
                value = Enum.ToObject(task.StreamType, flags);
                return true;
            }

            // Test if type has StreamDataAttribute on properties.
            // This allows nesting of StreamData-aware task.DataTypes
            var props = StreamData.GetProperties(task.StreamType);
            if (props.Length == 0)
            {
                value = null;
                return false;
            }

            value = StreamData.Create(task.StreamType, task.Stream);
            // Need to add error condition here.
            return true;

        }

        #endregion
    }
}
