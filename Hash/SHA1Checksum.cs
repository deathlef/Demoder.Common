/*
Demoder.Common
Copyright (c) 2010,2011 Demoder <demoder@demoder.me>

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
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Demoder.Common.Hash
{
    /// <summary>
    /// Represents a single SHA1 checksum
    /// </summary>
    public class SHA1Checksum : ICheckSum
    {
        #region Members
        private ICheckSum checkSumStore;
        #endregion

        #region Constructors
        public SHA1Checksum(byte[] bytes) : this() { this.checkSumStore = new ChecksumHexStore(bytes); }
        public SHA1Checksum(string hex) : this() { this.checkSumStore = new ChecksumHexStore(hex); }
        public SHA1Checksum() { this.checkSumStore = null; }
        #endregion
        #region Interfaces

        #region ICheckSum Members
        /// <summary>
        /// Set or retrieve a byte representation of this class
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                if (this.checkSumStore == null)
                    return null;
                return this.checkSumStore.Bytes;
            }
            set
            {
                if (this.checkSumStore == null)
                    this.checkSumStore = new ChecksumHexStore(value);
                else
                    this.checkSumStore.Bytes = value;
            }
        }
        /// <summary>
        /// Set or retrieve a string representation of this class
        /// </summary>
        public string String
        {
            get
            {
                if (this.checkSumStore == null)
                {
                    return String.Empty;
                }
                return this.checkSumStore.String;
            }
            set
            {
                if (this.checkSumStore == null)
                {
                    this.checkSumStore = new ChecksumHexStore(value);
                }
                else
                {
                    this.checkSumStore.String = value;
                }
            }
        }
        #endregion


        #region IEquatable<ICheckSum> Members
        public override bool Equals(object obj)
        {
            ICheckSum other;
            try
            {
                other = (ICheckSum)obj;
            }
            catch
            {
                return false;
            }

            if (this.String == other.String)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (this.Bytes == null) { return int.MaxValue; }
            if (this.Bytes.Length < 4) { return int.MaxValue; }
            return BitConverter.ToInt32(this.Bytes, 0);
        }
        #endregion


        #endregion Interfaces

        public override string ToString()
        {
            return this.String;
        }


        #region static operators
        public static bool operator ==(SHA1Checksum cs1, MD5Checksum cs2)
        {
            // Check null
            if (System.Object.ReferenceEquals(cs1, cs2)) { return true; }
            if (System.Object.ReferenceEquals(cs1, null)) { return false; }
            if (System.Object.ReferenceEquals(cs2, null)) { return false; }

            if (cs1.String == cs2.String)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool operator !=(SHA1Checksum cs1, MD5Checksum cs2)
        {
            return !(cs1 == cs2);
        }
        #endregion


        #region Static Generate
        /// <summary>
        /// Get SHA1 hash of byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SHA1Checksum Generate(byte[] input)
        {
            SHA1 _sha1 = new SHA1CryptoServiceProvider();
            byte[] hash = _sha1.ComputeHash(input);
            return new SHA1Checksum(hash);
        }
        /// <summary>
        /// Get SHA1 hash of Stream input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SHA1Checksum Generate(Stream input)
        {
            SHA1 _sha1 = new SHA1CryptoServiceProvider();
            byte[] hash = _sha1.ComputeHash(input);
            return new SHA1Checksum(hash);
        }


        /// <summary>
        /// Get SHA1 hash of text
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SHA1Checksum Generate(string input)
        {
            return Generate(Encoding.Default.GetBytes(input));
        }

        /// <summary>
        /// Get SHA1 hash of MemoryStream
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SHA1Checksum Generate(MemoryStream input)
        {
            return Generate(input.ToArray());
        }

        public static SHA1Checksum Generate(List<byte> Input) { return Generate(Input.ToArray()); }

        /// <summary>
        /// Get SHA1 hash of file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <exception cref="FileNotFoundException">File does not exist</exception>
        /// <returns></returns>
        public static SHA1Checksum Generate(FileInfo filePath)
        {
            if (!filePath.Exists) throw new FileNotFoundException("File does not exist");
            return Generate(File.ReadAllBytes(filePath.FullName));
        }
        #endregion
    }
}
