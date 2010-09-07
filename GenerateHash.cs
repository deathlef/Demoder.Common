/*
MIT Licence
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: https://sourceforge.net/projects/demoderstools/)

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

namespace Demoder.Common
{
	#region MD5
	public static class GenerateHash
	{
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="input">byte[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static string md5(byte[] input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(input);
			//Generate a hexadecimal string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			_md5.Clear();
			return sb.ToString();
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="input">stream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static string md5(Stream input)
		{
			MD5 _md5 = System.Security.Cryptography.MD5.Create();
			byte[] hash = _md5.ComputeHash(input);
			//Generate a hexadecimal string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			_md5.Clear();
			return sb.ToString();
		}
		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="ms">MemoryStream input</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static string md5(MemoryStream ms)
		{
			return md5(ms.ToArray());
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data.
		/// </summary>
		/// <param name="input">char[] array representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static string md5(char[] input)
		{
			//Convert the char array to a byte array
			byte[] b = new byte[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				b[i] = byte.Parse(input[i].ToString());
			}
			return md5(b);
		}

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the provided data
		/// </summary>
		/// <param name="input">string input representing data</param>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided data</returns>
		public static string md5(string input) { return md5(Encoding.Default.GetBytes(input)); }

		public static string md5(List<byte> input) { return md5(input.ToArray()); }

		/// <summary>
		/// Generates a hexadecimal string representing the MD5 hash of the file located at path
		/// </summary>
		/// <param name="path">Full path to the file we should generate a MD5 hash of</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns>a hexadecimal string of 32 characters representing the MD5 hash of the provided file</returns>
		public static string md5_file(string path) {
			if (!File.Exists(path)) throw new FileNotFoundException("File does not exist");
			return md5(File.ReadAllBytes(path));
		}

		#endregion
		#region SHA1
		/// <summary>
		/// Get SHA1 hash of byte array
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string sha1(byte[] data)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(data);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			_sha1.Clear();
			return sb.ToString();
		}
		/// <summary>
		/// Get SHA1 hash of Stream input.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string sha1(Stream data)
		{
			SHA1 _sha1 = new SHA1CryptoServiceProvider();
			byte[] hash = _sha1.ComputeHash(data);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			_sha1.Clear();
			return sb.ToString();
		}


		/// <summary>
		/// Get SHA1 hash of text
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string sha1(string text)
		{
			return sha1(Encoding.Default.GetBytes(text));
		}

		/// <summary>
		/// Get SHA1 hash of MemoryStream
		/// </summary>
		/// <param name="ms"></param>
		/// <returns></returns>
		public static string sha1(MemoryStream ms)
		{
			return sha1(ms.ToArray());
		}

		public static string sha1(List<byte> input) { return sha1(input.ToArray()); }

		/// <summary>
		/// Get SHA1 hash of file
		/// </summary>
		/// <param name="path">path to file</param>
		/// <exception cref="FileNotFoundException">File does not exist</exception>
		/// <returns></returns>
		public static string sha1_file(string path)
		{
			if (!File.Exists(path)) throw new FileNotFoundException("File does not exist");
			return sha1(File.ReadAllBytes(path));
		}

		

		#endregion
	}
}