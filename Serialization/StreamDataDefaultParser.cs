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
            return GetValue(task.StreamType, task.Stream, out value);
        }

        #region IStreamDataParser Members


        public bool WriteObject(StreamDataParserTask task, object value)
        {
            return WriteValue(task.StreamType, task.Stream, value);

        }

        private bool WriteValue(Type dataType, SuperStream stream, object value)
        {
            if (dataType == typeof(Byte))
            {
                stream.WriteByte((byte)value);
                return true;
            }

            if (dataType == typeof(Int64))
            {
                stream.WriteInt64((Int64)value);
                return true;
            }
            if (dataType == typeof(UInt64))
            {
                stream.WriteUInt64((UInt64)value);
                return true;
            }

            if (dataType == typeof(Int32))
            {
                stream.WriteInt32((Int32)value);
                return true;
            }
            if (dataType == typeof(UInt32))
            {
                stream.WriteUInt32((UInt32)value);
                return true;
            }

            if (dataType == typeof(Int16))
            {
                stream.WriteInt16((Int16)value);
                return true;
            }
            if (dataType == typeof(UInt16))
            {
                stream.WriteUInt16((UInt16)value);
                return true;
            }

            if (dataType == typeof(Single))
            {
                stream.WriteSingle((Single)value);
                return true;
            }

            if (dataType == typeof(Double))
            {
                stream.WriteDouble((Double)value);
                return true;
            }

            if (dataType == typeof(bool))
            {
                // If bool is true, write 1. Otherwise, write 0.
                int val = ((bool)value) ? 1 : 0;
                stream.WriteInt32(val);
            }

            if (dataType.IsEnum)
            {
                dynamic storeValue = Convert.ChangeType(value, Enum.GetUnderlyingType(dataType));
                return this.WriteValue(storeValue.GetType(), stream, storeValue);
            }

            // Test if type has StreamDataAttribute on properties.
            // This allows nesting of StreamData-aware task.DataTypes
            var props = StreamData.GetProperties(dataType);
            if (props.Length == 0)
            {
                return false;
            }

            StreamData.Serialize(value, stream);
            // Need to add error condition here.
            return true;
        }

        #endregion

        #region Helper methods
        private bool GetValue(Type dataType, SuperStream stream, out object value)
        {
            if (dataType == typeof(Byte))
            {
                value = (byte)stream.ReadByte();
                return true;
            }

            if (dataType == typeof(Int64))
            {
                value = stream.ReadInt64();
                return true;
            }
            if (dataType == typeof(UInt64))
            {
                value = stream.ReadUInt64();
                return true;
            }

            if (dataType == typeof(Int32))
            {
                value = stream.ReadInt32();
                return true;
            }
            if (dataType == typeof(UInt32))
            {
                value = stream.ReadUInt32();
                return true;
            }

            if (dataType == typeof(Int16))
            {
                value = stream.ReadInt16();
                return true;
            }
            if (dataType == typeof(UInt16))
            {
                value = stream.ReadUInt16();
                return true;
            }

            if (dataType == typeof(Single))
            {
                value = stream.ReadSingle();
                return true;
            }

            if (dataType == typeof(Double))
            {
                value = stream.ReadDouble();
                return true;
            }

            if (dataType == typeof(bool))
            {
                var val = stream.ReadUInt32();
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

            if (dataType.IsEnum)
            {
                object readValue;
                // Read the enums underlying type
                if (!this.GetValue(Enum.GetUnderlyingType(dataType), stream, out readValue))
                {
                    value = null;
                    return false;
                }
                // Parse enum using the read value.
                value = Enum.ToObject(dataType, readValue);
                return true;
            }

            // Test if type has StreamDataAttribute on properties.
            // This allows nesting of StreamData-aware task.DataTypes
            var props = StreamData.GetProperties(dataType);
            if (props.Length == 0)
            {
                value = null;
                return false;
            }

            value = StreamData.Create(dataType, stream);
            if (value == null)
            {
                return false;
            }
            return true;
        }


        #endregion
    }
}
