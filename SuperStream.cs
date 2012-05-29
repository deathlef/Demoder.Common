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
    /// <summary>
    /// An endian-aware stream wrapper
    /// </summary>
    public class SuperStream : Stream
    {
        public Endianess Endianess { get; private set; }
        public Stream BaseStream { get; private set; }
        private SuperStream alternateEndian { get; set; }
        
        #region Constructors
        /// <summary>
        /// Creates a new instance using the provided stream.
        /// </summary>
        /// <param name="stream">Endianess of data in the stream</param>
        /// <param name="endianess"></param>
        public SuperStream(Stream stream, Endianess endianess)
        {
            this.BaseStream = stream;
            this.Endianess = endianess;
        }
        /// <summary>
        /// Creates new instance using a empty, writeable MemoryStream.
        /// </summary>
        /// <param name="endianess">Endianess of data in the stream</param>
        public SuperStream(Endianess endianess)
        {
            this.BaseStream = new MemoryStream();
            this.Endianess = endianess;
        }

        #endregion

        /// <summary>
        /// Returns this instance if endianess matches, or a wrapper instance with the specified endianess.<br/>
        /// Stream position is synced between streams!
        /// 
        /// </summary>
        /// <param name="endianess"></param>
        /// <returns></returns>
        public SuperStream AsEndian(Endianess endianess)
        {
            if (this.Endianess == endianess) { return this; }
            if (this.alternateEndian == null)
            {
                this.alternateEndian = new SuperStream(this.BaseStream, endianess);
            }
            return this.alternateEndian;
        }

        #region BinaryWriter
        public void WriteBytes(byte[] bytes)
        {
            this.Write(bytes, 0, bytes.Length);
        }
        #endregion

        #region Write signed integers
        public void WriteInt16(Int16 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }

        public void WriteInt32(Int32 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }

        public void WriteInt64(Int64 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }
        #endregion

        #region Write unsigned integers
        public void WriteUInt16(UInt16 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }

        public void WriteUInt32(UInt32 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }

        public void WriteUInt64(UInt64 value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }
        #endregion

        #region Write floating point numbers
        public void WriteSingle(Single value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }

        public void WriteDouble(Double value)
        {
            var bytes = this.CorrectEndianess(BitConverter.GetBytes(value));
            this.WriteBytes(bytes);
        }
        #endregion


        public void WriteString(string value)
        {
            var bytes = ASCIIEncoding.ASCII.GetBytes(value);
            this.WriteUInt32((uint)bytes.Length);
            this.WriteBytes(bytes);
        }

        public void WriteCString(string value)
        {
            var bytes = ASCIIEncoding.ASCII.GetBytes(value);
            if (bytes.Contains((byte)0)) 
            { 
                // String contains a null byte. 
                // This is not allowed, as the string is terminated by a null byte.
                throw new ArgumentException("Provided string contains a null byte, which is not allowed because the string is terminated by a null byte.", "value"); 
            }
            this.WriteBytes(bytes);
            this.WriteByte(0);
        }


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
        /// <returns><seealso cref="float"/></returns>
        public float ReadSingle()
        {
            var bytes = CorrectEndianess(this.ReadBytes(4));
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// 64bit (8bytes)
        /// </summary>
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

        /// <summary>
        /// Reads a string of specified length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ReadString(uint length)
        {
            char nb = Convert.ToChar(0);
            var bytes = this.ReadBytes(length);
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
            var sb = new StringBuilder();
            char nb = (Char)0;
            
            while (!this.EOF)
            {
                var chr = (Char)this.ReadByte();
                if (chr == nb)
                {
                    // Break on NullByte.
                    break;
                }
                sb.Append(chr);
            }

            return sb.ToString();
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

            var ret = (Int32)Math.Floor(lolRes);
            additionalData = (int)Math.Round((lolRes - ret) * lolModifier);
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


        public void WriteLolTeger32(int value, int additionalData, double lolModifier = 1009)
        {
            if (additionalData >= lolModifier) { throw new ArgumentOutOfRangeException("Additional data must have a value less than lolModifier.", "additionalData"); }

            int lolTeger = (int)((value + 1) * lolModifier) + additionalData;
            this.WriteInt32(lolTeger);            
        }

        public void WriteLolTeger32(int value, double lolModifier = 1009)
        {
            this.WriteLolTeger32(value, 1, lolModifier);
        }
        #endregion
        #endregion



        #region Stream implementation
        public override bool CanRead { get { return this.BaseStream.CanRead; } }

        public override bool CanSeek { get { return this.BaseStream.CanSeek; } }

        public override bool CanWrite { get { return this.BaseStream.CanWrite; } }

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


        public override void SetLength(long value)
        {
            this.BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.BaseStream.Write(buffer, offset, count);
        }
        #endregion

        
    }
}
