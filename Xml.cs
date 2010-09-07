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
using System.Net;
using System.Xml.Serialization;

namespace Demoder.Common
{
	public static class Xml
	{
		#region Serialization
		/// <summary>
		/// Serializes an object into an already opened stream
		/// </summary>
		/// <typeparam name="T">Class type to serialize class as</typeparam>
		/// <param name="stream">Stream to serialize into</param>
		/// <param name="obj">Class to serialize</param>
		public static bool Serialize<T>(Stream stream, T obj, bool closestream) where T : class
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (obj == null) throw new ArgumentNullException("obj");
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(stream, obj);
				if (closestream) stream.Close();
				return true;
			}
			catch (Exception ex)
			{
				if (closestream && stream != null) stream.Close();
				return false;
			}
		}
		/// <summary>
		/// Serialize a class to a file
		/// </summary>
		/// <typeparam name="T">Class type to serialize</typeparam>
		/// <param name="path"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool Serialize<T>(FileInfo path, T obj, bool GZip) where T : class
		{
			if (path == null) throw new ArgumentNullException("path");
			if (obj == null) throw new ArgumentNullException("obj");
			if (GZip)
			{
				using (FileStream fs = path.Create())
				{
					using (System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress, true))
					{
						Serialize<T>(gzs, obj, true);
					}
				}
				return true;
			}
			else
			{ //don't gzip the output
				MemoryStream ms = new MemoryStream();
				FileStream fs = null;
				try
				{
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					serializer.Serialize(ms, obj); //Serialize into memory

					fs = path.Create();
					ms.WriteTo(fs);
					if (fs != null) fs.Close();
					if (ms != null) ms.Close();
					return true;
				}
				catch (Exception ex)
				{
					if (fs != null) fs.Close();
					if (ms != null) ms.Close();
					return false;
				}
			}
		}
		#endregion

		#region deserialization
		public static T Deserialize<T>(Stream stream, bool closestream) where T : class
		{
			if (stream == null) throw new ArgumentNullException("stream");
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				T obj = (T)serializer.Deserialize(stream);
				if (stream != null && closestream) stream.Close();
				return obj;
			}
			catch (Exception ex)
			{
				if (stream != null && closestream) stream.Close();
				return default(T);
			}
		}

		/// <summary>
		/// Deserialize a file
		/// </summary>
		/// <typeparam name="T">What class type to parse file as</typeparam>
		/// <param name="path">Path to the file</param>
		/// <param name="GZip">Whether or not the file is gzip-compressed</param>
		/// <returns></returns>
		public static T Deserialize<T>(FileInfo path, bool GZip) where T : class
		{
			if (path == null) throw new ArgumentNullException("path");

			if (GZip)
			{
				using (FileStream fs = path.OpenRead())
				{
					using (System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, true))
					{
						return Deserialize<T>(gzs, true);
					}
				}
			}
			else
			{
				FileStream stream = null;
				try
				{
					stream = path.OpenRead();
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					T obj = (T)serializer.Deserialize(stream);
					if (stream != null) stream.Close();
					return obj;
				}
				catch (Exception ex)
				{
					if (stream != null) stream.Close();
					return default(T);
				}
			}
		}

		/// <summary>
		/// Deserialize content of UriBuilder
		/// </summary>
		/// <typeparam name="T">What class to parse file as</typeparam>
		/// <param name="path">Path to fetch</param>
		/// <returns></returns>
		public static T Deserialize<T>(UriBuilder path) where T : class
		{
			return Deserialize<T>(path.Uri);
		}

		/// <summary>
		/// Deserialize content of Uris.
		/// </summary>
		/// <typeparam name="T">Class type to parse as</typeparam>
		/// <param name="urls">List of mirrors to try</param>
		/// <returns></returns>
		public static T Deserialize<T>(Uri[] urls) where T : class
		{
			T ret = default(T);
			foreach (Uri uri in urls)
			{
				ret = Deserialize<T>(uri);
				if (ret != default(T))  //If we got proper data
					break;
			}
			return ret;
		}

		/// <summary>
		/// Deserialize content of URI
		/// </summary>
		/// <typeparam name="T">Class type to parse as</typeparam>
		/// <param name="path">URI to deserialize</param>
		/// <returns></returns>
		public static T Deserialize<T>(Uri path) where T : class
		{
			if (path == null) throw new ArgumentNullException("path");
			try
			{
				Stream stream = DownloadManager.GetReadStream(path);
				T obj = Deserialize<T>(stream, true);
				return obj;
			}
			catch { return default(T); }
		}
		#endregion
	}
}