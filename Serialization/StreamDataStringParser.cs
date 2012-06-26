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
using Demoder.Common.Serialization;
using Demoder.Common;
using Demoder.Common.Attributes;

namespace Demoder.Common.Serialization
{
    public class StreamDataStringParser : IStreamDataParser
    {
        #region IDataParser Members
        public Type[] SupportedTypes { get { return new Type[] { typeof(string) }; } }

        public bool GetObject(StreamDataParserTask task, out dynamic value)
        {
            var strType = this.GetStringType(task.Attributes);
            var lengthType = LengthType.UInt32;
            var lengthAttr = task.Attributes.FirstOrDefault(a => a is StreamDataLengthAttribute) as StreamDataLengthAttribute;
            if (lengthAttr != null)
            {
                lengthType = lengthAttr.Type;
            }

            switch (strType)
            {
                case StringType.CString:
                    value = task.Stream.ReadCString(this.GetStringEncoding(task.Attributes));
                    return true;
                default:
                    value = task.Stream.ReadString(lengthType, this.GetStringEncoding(task.Attributes));
                    return true;
            }
        }

        public bool WriteObject(StreamDataParserTask task, object value)
        {
            var strType = this.GetStringType(task.Attributes);

            var lengthType = LengthType.UInt32;
            var lengthAttr = task.Attributes.FirstOrDefault(a => a is StreamDataLengthAttribute) as StreamDataLengthAttribute;
            if (lengthAttr != null)
            {
                lengthType = lengthAttr.Type;
            }

            switch (strType)
            {
                case StringType.CString:
                    task.Stream.WriteCString((string)value, this.GetStringEncoding(task.Attributes));
                    return true;
                default:
                    task.Stream.WriteString(
                        (string)value,  
                        this.GetStringEncoding(task.Attributes),
                        lengthType);
                    return true;
            }
        }

        #endregion

        private StringType GetStringType(Attribute[] attributes)
        {
            StringType strType = StringType.Normal;
            var attr = attributes.FirstOrDefault(a => a is StreamDataStringAttribute) as StreamDataStringAttribute;
            if (attr != null)
            {
                strType = attr.Type;
            }
            return strType;
        }

        private Encoding GetStringEncoding(Attribute[] attributes)
        {
            StringEncoding encoding = StringEncoding.ASCII;
            var attr = attributes.FirstOrDefault(a => a is StreamDataStringAttribute) as StreamDataStringAttribute;
            if (attr != null)
            {
                encoding = attr.Encoding;
            }


            switch (encoding)
            {
                default:
                case StringEncoding.ASCII:
                    return Encoding.ASCII;
                case StringEncoding.Unicode:
                    return Encoding.Unicode;
                case StringEncoding.UTF8:
                    return Encoding.UTF8;
            }
        }
    }
}
