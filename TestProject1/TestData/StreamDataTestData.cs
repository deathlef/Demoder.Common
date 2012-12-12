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
using Demoder.Common.Attributes;
using Demoder.Common;

namespace Demoder.Common.Tests.TestData
{

    public class StreamDataTestData
    {
        public StreamDataTestData()
        {
            this.D = new List<int>();
        }

        [StreamData(0)]
        public int A { get; set; }

        [StreamData(1)]
        [StreamDataString(StringType.Normal)]
        public string B { get; set; }

        [StreamData(2)]
        [StreamDataString(StringType.CString)]
        public string C { get; set; }

        [StreamData(3)]
        [StreamDataCollectionLength(LengthType.UInt32)]
        public List<int> D { get; set; }

        [StreamData(4)]
        public ByteEnum E { get; set; }

        [StreamData(5)]
        public LongEnum F { get; set; }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, this)) { return true; }
            if (Object.ReferenceEquals(obj, null)) { return false; }
            if (obj is StreamDataTestData)
            {
                var obj2 = obj as StreamDataTestData;
                if (obj2.A != this.A) { return false; }
                if (obj2.B != this.B) { return false; }
                if (obj2.C != this.C) { return false; }
                if (!obj2.D.SequenceEqual(this.D)) { return false; }
                return true;
            }
            return base.Equals(obj);
        }
    }

    public enum ByteEnum : byte
    {
        None=0,
        Hello=128,
        Max=255
    }

    public enum LongEnum : long
    {
        None=0,
        Hello=4000000000,
        Max=9223372036854775807
    }
}
