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

    public enum LengthType
    {
        Byte = -2,
        UInt16 = -1,
        UInt32 = 0,
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


    /// <summary>
    /// Default enumerator for storing flag values.
    /// </summary>
    [Flags]
    public enum BitFlag64 : ulong
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
        Bit32 = 4294967296,
        Bit33 = 8589934592,
        Bit34 = 17179869184,
        Bit35 = 34359738368,
        Bit36 = 68719476736,
        Bit37 = 137438953472,
        Bit38 = 274877906944,
        Bit39 = 549755813888,

        Bit40 = 1099511627776,
        Bit41 = 2199023255552,
        Bit42 = 4398046511104,
        Bit43 = 8796093022208,
        Bit44 = 17592186044416,
        Bit45 = 35184372088832,
        Bit46 = 70368744177664,
        Bit47 = 140737488355328,
        Bit48 = 281474976710656,
        Bit49 = 562949953421312,

        Bit50 = 1125899906842624,
        Bit51 = 2251799813685248,
        Bit52 = 4503599627370496,
        Bit53 = 9007199254740992,
        Bit54 = 18014398509481984,
        Bit55 = 36028797018963968,
        Bit56 = 72057594037927936,
        Bit57 = 144115188075855872,
        Bit58 = 288230376151711744,
        Bit59 = 576460752303423488,

        Bit60 = 1152921504606846976,
        Bit61 = 2305843009213693952,
        Bit62 = 4611686018427387904,
        Bit63 = 18446744073709551615,
    }

    [Flags]
    public enum CacheFlags
    {
        /// <summary>
        /// Retrieve the object from recent cache
        /// </summary>
        ReadCache = 0x01,
        /// <summary>
        /// Write the object to recent cache if pulled from live data source
        /// </summary>
        WriteCache = 0x02,
        /// <summary>
        /// Retrieve the object from live data source
        /// </summary>
        ReadLive = 0x04,
        /// <summary>
        /// Retrieve the object from cache as last resort, regardless of age
        /// </summary>
        ReadExpired = 0x08,
        /// <summary>
        /// The default flags
        /// </summary>
        Default = ReadCache | WriteCache | ReadLive
    }

    public enum StringEncoding
    {
        ASCII,
        UTF8,
        Unicode
    }
}
