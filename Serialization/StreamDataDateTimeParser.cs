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
using Demoder.Common.Hash;
using Demoder.Common.Attributes;

namespace Demoder.Common.Serialization
{
    public class StreamDataDateTimeParser : IStreamDataParser
    {
        public Type[] SupportedTypes { get { return new Type[] { typeof(DateTime) }; } }

        public bool GetObject(StreamDataParserTask task, out object value)
        {
            if (task.StreamType == typeof(DateTime))
            {
                var val = task.Stream.ReadInt64();
                value = Misc.Unixtime(val);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool WriteObject(StreamDataParserTask task, object value)
        {
            if (task.StreamType == typeof(DateTime))
            {
                var val = Misc.Unixtime((DateTime)value);
                task.Stream.WriteInt64(val);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
