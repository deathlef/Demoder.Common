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

namespace Demoder.Common
{
    public enum StringType
    {
        /// <summary>
        /// A string terminated by a 0-byte
        /// </summary>
        CString,
        /// <summary>
        /// First read int length, then read string of specified length.
        /// </summary>
        Normal
    }

    public enum Endianess
    {
        Little,
        Big
    }

    /// <summary>
    /// Default enumerator for storing flag values.
    /// </summary>
    [Flags]
    public enum BitFlag : uint
    {
        None = 0,
        Bit0 = 1,
        Bit1 = 2,
        Bit2 = 4,
        Bit3 = 8,
        Bit4 = 16,
        Bit5 = 32,
        Bit6 = 64,
        Bit7 = 128,
        Bit8 = 256,
        Bit9 = 512,
        Bit10 = 1024,
        Bit11 = 2048,
        Bit12 = 4096,
        Bit13 = 8192,
        Bit14 = 16384,
        Bit15 = 32768,
        Bit16 = 65536,
        Bit17 = 131072,
        Bit18 = 262144,
        Bit19 = 524288,
        Bit20 = 1048576,
        Bit21 = 2097152,
        Bit22 = 4194304,
        Bit23 = 8388608,
        Bit24 = 16777216,
        Bit25 = 33554432,
        Bit26 = 67108864,
        Bit27 = 134217728,
        Bit28 = 268435456,
        Bit29 = 536870912,
        Bit30 = 1073741824,
        Bit31 = 2147483648,
    }
}
