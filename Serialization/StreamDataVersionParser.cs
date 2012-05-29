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
    /// Allows (de)serialization of Version objects.
    /// </summary>
    public class StreamDataVersionParser : IStreamDataParser
    {
        #region IStreamDataParser Members

        public Type[] SupportedTypes { get { return new Type[] { typeof(Version) }; } }

        public bool GetObject(StreamDataParserTask task, out object value)
        {
            var a = task.Stream.ReadInt32();
            var b = task.Stream.ReadInt32();
            var c = task.Stream.ReadInt32();
            var d = task.Stream.ReadInt32();

            value = new Version(a, b, c, d);
            return true;
        }

        public bool WriteObject(StreamDataParserTask task, object value)
        {
            var ver = value as Version;
            task.Stream.WriteInt32(ver.Major);
            task.Stream.WriteInt32(ver.MajorRevision);
            task.Stream.WriteInt32(ver.Minor);
            task.Stream.WriteInt32(ver.MinorRevision);

            return true;
        }

        #endregion

    }
}
