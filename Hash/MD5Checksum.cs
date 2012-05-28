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
using System.Security.Cryptography;
using System.IO;

namespace Demoder.Common.Hash
{
    /// <summary>
    /// 128-bit hash
    /// </summary>
    public class MD5Checksum : Checksum
    {
        public MD5Checksum(byte[] checksum)
        {
            this.Bytes = checksum;
        }

        public MD5Checksum(string checksumHex)
            : base(checksumHex)
        {
        }

        #region Static stuff
        public static MD5Checksum Generate(byte[] input)
        {
            var md5 = new MD5CryptoServiceProvider();
            return new MD5Checksum(md5.ComputeHash(input));
        }

        public static MD5Checksum Generate(string input)
        {
            return Generate(Encoding.Default.GetBytes(input));
        }

        public static MD5Checksum Generate(Stream input)
        {
            var md5 = new MD5CryptoServiceProvider();
            return new MD5Checksum(md5.ComputeHash(input));
        }
        #endregion
    }
}
