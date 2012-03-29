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
using System.Xml;
using System.Xml.Serialization;

namespace Demoder.Common.Hash
{
    /// <summary>
    /// Represents a single MD5 Checksum
    /// </summary>
    public class MD5Checksum : ICheckSum
    {
        #region Members
        private ICheckSum checkSumStore;
        #endregion
        #region Constructors
        public MD5Checksum(byte[] bytes) : this() { this.checkSumStore = new ChecksumHexStore(bytes); }
        public MD5Checksum(string hex) : this() { this.checkSumStore = new ChecksumHexStore(hex); }
        public MD5Checksum() { this.checkSumStore = null; }
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
                    return String.Empty;
                return this.checkSumStore.String;
            }
            set
            {
                if (this.checkSumStore == null)
                    this.checkSumStore = new ChecksumHexStore(value);
                else
                    this.checkSumStore.String = value;
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
            catch { return false; }
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
            if (this.Bytes.Length < 4) { return int.MaxValue-1; }
            return BitConverter.ToInt32(this.Bytes, 0);
        }
        #endregion
        #endregion Interfaces

        public override string ToString()
        {
            return this.String;
        }

        #region static operators
        public static bool operator ==(MD5Checksum cs1, MD5Checksum cs2)
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
        public static bool operator !=(MD5Checksum cs1, MD5Checksum cs2)
        {
            return !(cs1 == cs2);
        }
        #endregion

        #region Static Generate
        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the provided data.
        /// </summary>
        /// <param name="input">byte[] array representing data</param>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
        public static MD5Checksum Generate(byte[] input)
        {
            MD5 _md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = _md5.ComputeHash(input);
            return new MD5Checksum(hash);
        }

        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the provided data.
        /// </summary>
        /// <param name="input">stream input</param>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
        public static MD5Checksum Generate(Stream input)
        {
            MD5 _md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = _md5.ComputeHash(input);
            return new MD5Checksum(hash);
        }
        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the provided data
        /// </summary>
        /// <param name="input">MemoryStream input</param>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
        public static MD5Checksum Generate(MemoryStream input)
        {
            return Generate(input.ToArray());
        }

        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the provided data.
        /// </summary>
        /// <param name="input">char[] array representing data</param>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
        public static MD5Checksum Generate(char[] input)
        {
            //Convert the char array to a byte array
            byte[] b = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                b[i] = byte.Parse(input[i].ToString());
            }
            return Generate(b);
        }

        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the provided data
        /// </summary>
        /// <param name="input">string input representing data</param>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
        public static MD5Checksum Generate(string input) { return Generate(Encoding.Default.GetBytes(input)); }

        public static MD5Checksum Generate(List<byte> input) { return Generate(input.ToArray()); }

        /// <summary>
        /// Generates a hexadecimal string representing the MD5 hash of the file located at path
        /// </summary>
        /// <param name="filePath">Full path to the file we should generate a MD5 hash of</param>
        /// <exception cref="FileNotFoundException">File does not exist</exception>
        /// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided file</returns>
        public static MD5Checksum Generate(FileInfo filePath)
        {
            if (!filePath.Exists) 
                throw new FileNotFoundException("File does not exist");
            return Generate(File.ReadAllBytes(filePath.FullName));
        }
        #endregion
    }
}
