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
	/// Represents a single MD5 Checksum
	/// </summary>
	public class MD5Checksum : ChecksumTemplate
	{
		#region Constructors
		public MD5Checksum(byte[] Bytes) : base(Bytes) { }
		public MD5Checksum(string Hex) : base(Hex) { }
		public MD5Checksum() : base() { }
		#endregion

		#region Static Generate
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="Input">byte[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5Checksum Generate(byte[] Input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(Input);
			return new MD5Checksum(hash);
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="Input">stream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5Checksum Generate(Stream Input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(Input);
			return new MD5Checksum(hash);
		}
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="Input">MemoryStream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5Checksum Generate(MemoryStream Input)
		{
			return Generate(Input.ToArray());
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="input">char[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5Checksum Generate(char[] Input)
		{
			//Convert the char array to a byte array
			byte[] b = new byte[Input.Length];
			for (int i = 0; i < Input.Length; i++)
			{
				b[i] = byte.Parse(Input[i].ToString());
			}
			return Generate(b);
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="Input">string input representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5Checksum Generate(string Input) { return Generate(Encoding.Default.GetBytes(Input)); }

		public static MD5Checksum Generate(List<byte> Input) { return Generate(Input.ToArray()); }

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the file located at path
		/// </summary>
		/// <param name="FilePath">Full path to the file we should generate a MD5 hash of</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided file</returns>
		public static MD5Checksum Generate(FileInfo FilePath)
		{
			if (!FilePath.Exists) throw new FileNotFoundException("File does not exist");
			return Generate(File.ReadAllBytes(FilePath.FullName));
		}

		#endregion
	}
}
