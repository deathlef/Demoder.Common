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
using System.Xml.Serialization;

namespace Demoder.Common
{
	public static class GenerateHash
	{
		#region MD5
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="Input">byte[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5CheckSum MD5(byte[] Input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(Input);
			return new MD5CheckSum(hash);
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="Input">stream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5CheckSum MD5(Stream Input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(Input);
			return new MD5CheckSum(hash);
		}
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="Input">MemoryStream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5CheckSum MD5(MemoryStream Input)
		{
			return MD5(Input.ToArray());
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="input">char[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5CheckSum MD5(char[] Input)
		{
			//Convert the char array to a byte array
			byte[] b = new byte[Input.Length];
			for (int i = 0; i < Input.Length; i++)
			{
				b[i] = byte.Parse(Input[i].ToString());
			}
			return MD5(b);
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="Input">string input representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static MD5CheckSum MD5(string Input) { return MD5(Encoding.Default.GetBytes(Input)); }

		public static MD5CheckSum MD5(List<byte> Input) { return MD5(Input.ToArray()); }

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the file located at path
		/// </summary>
		/// <param name="FilePath">Full path to the file we should generate a MD5 hash of</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided file</returns>
		public static MD5CheckSum MD5(FileInfo FilePath)
		{
			if (!FilePath.Exists) throw new FileNotFoundException("File does not exist");
			return MD5(File.ReadAllBytes(FilePath.FullName));
		}

		#endregion

		#region SHA1
		/// <summary>
		/// Get SHA1 hash of byte array
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1CheckSum SHA1(byte[] Input)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(Input);
			return new SHA1CheckSum(hash);
		}
		/// <summary>
		/// Get SHA1 hash of Stream input.
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1CheckSum SHA1(Stream Input)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(Input);
			return new SHA1CheckSum(hash);
		}


		/// <summary>
		/// Get SHA1 hash of text
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1CheckSum SHA1(string Input)
		{
			return SHA1(Encoding.Default.GetBytes(Input));
		}

		/// <summary>
		/// Get SHA1 hash of MemoryStream
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public static SHA1CheckSum SHA1(MemoryStream Input)
		{
			return SHA1(Input.ToArray());
		}

		public static SHA1CheckSum SHA1(List<byte> Input) { return SHA1(Input.ToArray()); }

		/// <summary>
		/// Get SHA1 hash of file
		/// </summary>
		/// <param name="FilePath">path to file</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns></returns>
		public static SHA1CheckSum SHA1(FileInfo FilePath)
		{
			if (!FilePath.Exists) throw new FileNotFoundException("File does not exist");
			return SHA1(File.ReadAllBytes(FilePath.FullName));
		}
		#endregion

		#region Data classes
		/// <summary>
		/// Represents a single MD5 Checksum
		/// </summary>
		public class MD5CheckSum : CheckSumTemplate
		{
			public MD5CheckSum(byte[] Bytes) : base(Bytes) { }
		}

		/// <summary>
		/// Represents a single SHA1 checksum
		/// </summary>
		public class SHA1CheckSum : CheckSumTemplate
		{
			public SHA1CheckSum(byte[] Bytes) : base(Bytes) { }
		}

		public class CheckSumTemplate
		{
			#region Members
			private byte[] _bytes;
			private string _string;
			#endregion
			#region Constructors
			public CheckSumTemplate(byte[] Bytes)
			{
				this._bytes = Bytes;
				this._string = this.generateString();
			}
			#endregion

			#region Overrides
			public override string ToString()
			{
				return this._string;
			}
			#endregion

			#region Private methods
			private string generateString()
			{
				//Generate a hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < this._bytes.Length; i++)
				{
					sb.Append(this._bytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
			#endregion

			#region Public Accessors
			/// <summary>
			/// Byte array representing the checksum
			/// </summary>
			[XmlAttribute("bytes")]
			public byte[] Bytes
			{
				set
				{
					this._bytes = value;
					this._string = this.generateString();

				}
				get
				{
					return this._bytes;
				}
			}
			/// <summary>
			/// String representing the checksum
			/// </summary>
			public string String
			{
				get { return this._string; }
			}
			#endregion
		}
		#endregion
	}
}