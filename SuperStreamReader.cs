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

namespace Demoder.Common
{
    public class SuperStreamReader : Stream
    {
        public Endianess Endianess { get; private set; }
        public Stream BaseStream { get; private set; }
        
        #region Constructors
        public SuperStreamReader(byte[] bytes, Endianess endianess) 
            : this(endianess)
        {
            if (bytes == null) { throw new ArgumentNullException("bytes"); }
            this.BaseStream = new MemoryStream(bytes);
        }

        public SuperStreamReader(Stream stream, Endianess endianess)
            : this(endianess)
        {
            this.BaseStream = stream;
        }

        private SuperStreamReader(Endianess endianess)
        {
            this.Endianess = endianess;
        }

        #endregion

        #region BinaryReader
        public byte[] ReadBytes(uint numBytes)
        {
            byte[] bytes = new byte[numBytes];
            this.Read(bytes, 0, (int)numBytes);
            return bytes;
        }

        #region Read Signed Integers
        public Int16 ReadInt16()
        {
            var bytes = this.CorrectEndianess(this.ReadBytes(2));
            return BitConverter.ToInt16(bytes, 0);
        }

        public Int32 ReadInt32()
        {
            var bytes = this.CorrectEndianess(this.ReadBytes(4));
            return BitConverter.ToInt32(bytes, 0);
        }

        public Int64 ReadInt64()
        {
            var bytes = CorrectEndianess(this.ReadBytes(8));
            return BitConverter.ToInt64(bytes, 0);
        }
        #endregion


        #region Read Unsigned Integers
        public UInt16 ReadUInt16()
        {
            var bytes = this.CorrectEndianess(this.ReadBytes(2));
            return BitConverter.ToUInt16(bytes, 0);
        }

        public UInt32 ReadUInt32()
        {
            var bytes = this.CorrectEndianess(this.ReadBytes(4));
            return BitConverter.ToUInt32(bytes, 0);
        }

        public UInt64 ReadUInt64()
        {
            var bytes = this.CorrectEndianess(this.ReadBytes(8));
            return BitConverter.ToUInt64(bytes, 0);
        }
        #endregion


        #region Read floating point numbers
        /// <summary>
        /// 32bit (4bytes) (float)
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public float ReadSingle()
        {
            var bytes = CorrectEndianess(this.ReadBytes(4));
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// 64bit (8bytes)
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public double ReadDouble()
        {
            var bytes = CorrectEndianess(this.ReadBytes(8));
            return BitConverter.ToDouble(bytes, 0);
        }
        #endregion

        /// <summary>
        /// Reads (Int32 stringLength), then the string.
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            var strLen =this.ReadUInt32();
            return this.ReadString(strLen);
        }

        public string ReadString(uint length)
        {
            char nb = Convert.ToChar(0);
            var bytes = this.CorrectEndianess(this.ReadBytes(length));
            var str = Encoding.ASCII.GetString(bytes);
            return str.Trim(nb);
        }

        // TODO: Check if this needs to account for endianess
        // TODO: Consider using StringBuilder
        /// <summary>
        /// Reads a nil-terminated string.
        /// </summary>
        /// <returns></returns>
        public string ReadCString()
        {
            char nb = (Char)0;
            string outString = String.Empty;
            
            while (!this.EOF)
            {
                var chr = (Char)this.ReadByte();
                if (chr == nb)
                {
                    // Break on NullByte.
                    break;
                }
                outString += chr;
            }
            
            return outString;
        }


        public bool EOF
        {
            get
            {
                return this.Position >= this.Length;
            }
        }

        #region Helper methods
        public byte[] CorrectEndianess(byte[] bytes)
        {
            bool isLittleEndian = this.Endianess == Endianess.Little;
            if (isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        #endregion


        #region LolCode
        /// <summary>
        /// Also known as Read3F1 and CNum in Xyphos' tools
        /// </summary>
        /// <returns></returns>
        public Int32 ReadLolTeger32(out int additionalData, double lolModifier = 1009)
        {
            var lolData = ((double)this.ReadInt32());
            var lolRes = (lolData / lolModifier) - 1;

            var ret = (Int32)Math.Round(lolRes);
            additionalData = (int)((lolRes - ret) * lolModifier);
            return ret;
        }

        /// <summary>
        /// Also known as Read3F1 and CNum in Xyphos' tools
        /// </summary>
        /// <returns></returns>
        public Int32 ReadLolTeger32(double lolModifier = 1009)
        {
            int extra;
            return this.ReadLolTeger32(out extra, lolModifier);
        }
        #endregion
        #endregion

        #region Stream implementation
        public override bool CanRead { get { return this.BaseStream.CanRead; } }

        public override bool CanSeek { get { return this.BaseStream.CanSeek; } }

        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
            this.BaseStream.Flush();
        }

        public override long Length { get { return this.BaseStream.Length; } }
        public override long Position
        {
            get
            {
                return this.BaseStream.Position;
            }
            set
            {
                this.BaseStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion

        
    }
}
