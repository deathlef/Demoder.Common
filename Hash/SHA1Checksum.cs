/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://trac.flw.nu/demoder.common/)

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
	public class SHA1Checksum : ChecksumTemplate
	{
		#region Constructors
		public SHA1Checksum(byte[] Bytes) : base(Bytes) { }
		public SHA1Checksum(string Hex) : base(Hex) { }
		public SHA1Checksum() : base() { }
		#endregion

		#region Static Generate
		/// <summary>
		/// Get SHA1 hash of byte array
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1Checksum Generate(byte[] Input)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(Input);
			return new SHA1Checksum(hash);
		}
		/// <summary>
		/// Get SHA1 hash of Stream input.
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1Checksum Generate(Stream Input)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(Input);
			return new SHA1Checksum(hash);
		}


		/// <summary>
		/// Get SHA1 hash of text
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1Checksum Generate(string Input)
		{
			return Generate(Encoding.Default.GetBytes(Input));
		}

		/// <summary>
		/// Get SHA1 hash of MemoryStream
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1Checksum Generate(MemoryStream Input)
		{
			return Generate(Input.ToArray());
		}

		public static SHA1Checksum Generate(List<byte> Input) { return Generate(Input.ToArray()); }

		/// <summary>
		/// Get SHA1 hash of file
		/// </summary>
		/// <param name="FilePath">path to file</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns></returns>
		public static SHA1Checksum Generate(FileInfo FilePath)
		{
			if (!FilePath.Exists) throw new FileNotFoundException("File does not exist");
			return Generate(File.ReadAllBytes(FilePath.FullName));
		}
		#endregion
	}
}
