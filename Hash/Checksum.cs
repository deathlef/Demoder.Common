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
using System.IO;

namespace Demoder.Common.Hash
{
    public class Checksum
    {
        protected byte[] Bytes { get; set; }
        protected uint BitLength { get { return (uint)(this.Bytes.Length * 4); } }
        
        #region Constructors
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        protected Checksum()
        {
            this.Bytes = new byte[0];
        }

        /// <summary>
        /// Creates an instance using specified hexadecimal string representation of hash bytes
        /// </summary>
        /// <param name="csHex">Hexadecimal string representation of checksum.</param>
        /// <example>
        /// You may specify hex with 0x prefix
        /// <code language="C#">
        /// var checksum = new Checksum("0xAFB36300C4")
        /// </code>
        /// </example>
        protected Checksum(string csHex)
        {
            if (csHex.StartsWith("0x")) { csHex = csHex.Substring(2); }
            if (csHex.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hexadecimal string; Length is not an even number.");
            }

            this.Bytes = new byte[csHex.Length / 2];
            for (int i = 0; i < csHex.Length; i += 2)
            {
                this.Bytes[i / 2] = Convert.ToByte(csHex.Substring(i, 2));
            }
        }

        /// <summary>
        /// Creates an instance using specified hash bytes
        /// </summary>
        /// <param name="checksumBytes"></param>
        protected Checksum(byte[] checksumBytes)
        {
            this.Bytes = checksumBytes;
        }
        #endregion


        #region Overrides
        public override string ToString()
        {
            var sb = new StringBuilder(this.Bytes.Length);
            foreach (var b in this.Bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            // Handle special cases
            if (this.Bytes == null) { return 0; }
            if (this.Bytes.Length == 0) { return 0; }
            if (this.Bytes.Length == 1) { return this.Bytes[0]; }
            if (this.Bytes.Length == 2) { return BitConverter.ToInt16(this.Bytes, 0); }
            if (this.Bytes.Length == 3) { return BitConverter.ToInt16(this.Bytes, 0) + this.Bytes[2]; }
            if (this.Bytes.Length == 4) { return BitConverter.ToInt32(this.Bytes, 0); }

            // Handle normal cases.
            int[] integers = new int[this.Bytes.Length / 4];
            var ms = new SuperStream(new MemoryStream(this.Bytes), BitConverter.IsLittleEndian ? Endianess.Little : Endianess.Big);
            for (int i = 0; i<integers.Length; i++)
            {
                integers[i] = ms.ReadInt32();
            }
            int hash = integers[0];
            for (int i = 1; i < integers.Length; i++)
            {
                hash ^= integers[i];
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is Checksum)
            {
                return ((Checksum)obj) == this;
            }
            return base.Equals(obj);
        }
        #endregion


        #region Static operators
        public static bool operator == (Checksum a, Checksum b)
        {
            if (Object.ReferenceEquals(a, b)) { return true; }
            if (Object.ReferenceEquals(a, null)) { return false; }
            if (Object.ReferenceEquals(b, null)) { return false; }
            if (!a.GetType().IsAssignableFrom(b.GetType()) &&
                !b.GetType().IsAssignableFrom(a.GetType()))
            {
                return false;
            }

            return a.Bytes.SequenceEqual(b.Bytes);
        }

        public static bool operator !=(Checksum a, Checksum b)
        {
            return !(a == b);
        }

        public static Checksum operator ^(Checksum a, Checksum b)
        {
            var bytes = new byte[Math.Max(a.Bytes.Length, b.Bytes.Length)];
            for (int i = 0; i < bytes.Length; i++)
            {
                byte aVal = 0;
                byte bVal = 0;
                if (a.Bytes.Length >= i) { aVal = a.Bytes[i]; }
                if (b.Bytes.Length >= i) { bVal = b.Bytes[i]; }

                bytes[i] = (byte)(aVal ^ bVal);
            }
            return new Checksum(bytes);
        }
        #endregion

        public byte[] ToArray()
        {
            return this.Bytes.Clone() as byte[];
        }
    }
}
