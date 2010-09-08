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
using System.Net;
using System.Xml.Serialization;
using Demoder.Common;

namespace Demoder.Common.Serialization
{
	public static class Xml
	{
		#region Serialization
		/// <summary>
		/// Serializes an object into an already opened stream
		/// </summary>
		/// <typeparam name="T">Class type to serialize class as</typeparam>
		/// <param name="Stream">Stream to serialize into</param>
		/// <param name="Obj">Class to serialize</param>
		public static bool Serialize<T>(Stream Stream, T Obj, bool CloseStream) where T : class
		{
			if (Stream == null) throw new ArgumentNullException("Stream");
			if (Obj == null) throw new ArgumentNullException("Obj");
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(Stream, Obj);
				if (CloseStream) Stream.Close();
				return true;
			}
			catch (Exception ex)
			{
				if (CloseStream && Stream != null) Stream.Close();
				return false;
			}
		}
		/// <summary>
		/// Serialize a class to a file
		/// </summary>
		/// <typeparam name="T">Class type to serialize</typeparam>
		/// <param name="Path"></param>
		/// <param name="Obj"></param>
		/// <param name="GZip">Whether or not the saved file should be GZipped</param>
		/// <returns></returns>
		public static bool Serialize<T>(FileInfo Path, T Obj, bool GZip) where T : class
		{
			if (Path == null) throw new ArgumentNullException("Path");
			if (Obj == null) throw new ArgumentNullException("Obj");
			if (GZip)
			{
				using (FileStream fs = Path.Create())
				{
					using (System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress, true))
					{
						Serialize<T>(gzs, Obj, true);
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
					serializer.Serialize(ms, Obj); //Serialize into memory

					fs = Path.Create();
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
		/// <summary>
		/// Deserialize a stream
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Stream"></param>
		/// <param name="CloseStream"></param>
		/// <returns></returns>
		public static T Deserialize<T>(Stream Stream, bool CloseStream) where T : class
		{
			object obj = Compat.Deserialize(typeof(T), Stream, CloseStream);
			if (obj == null)
				return default(T);
			else
				return (T)obj;
		}

		/// <summary>
		/// Deserialize a file
		/// </summary>
		/// <typeparam name="T">What class type to parse file as</typeparam>
		/// <param name="Path">Path to the file</param>
		/// <param name="GZip">Whether or not the file is gzip-compressed</param>
		/// <returns></returns>
		public static T Deserialize<T>(FileInfo Path, bool GZip) where T : class
		{
			if (Path == null) throw new ArgumentNullException("Path");

			if (GZip)
			{
				using (FileStream fs = Path.OpenRead())
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
					stream = Path.OpenRead();
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
		/// <param name="Path">Path to fetch</param>
		/// <returns></returns>
		public static T Deserialize<T>(UriBuilder Path) where T : class
		{
			return Deserialize<T>(Path.Uri);
		}

		/// <summary>
		/// Deserialize content of Uris.
		/// </summary>
		/// <typeparam name="T">Class type to parse as</typeparam>
		/// <param name="Uris">List of mirrors to try</param>
		/// <returns></returns>
		public static T Deserialize<T>(Uri[] Uris) where T : class
		{
			T ret = default(T);
			foreach (Uri uri in Uris)
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
		/// <param name="Path">URI to deserialize</param>
		/// <returns></returns>
		public static T Deserialize<T>(Uri Path) where T : class
		{
			if (Path == null) throw new ArgumentNullException("Path");
			try
			{
				MemoryStream stream = new MemoryStream(Net.DownloadManager.GetBinaryData(Path));
				T obj = Deserialize<T>(stream, true);
				return obj;
			}
			catch { return default(T); }
		}
		#endregion


		#region compat
		public static class Compat
		{
			#region Deserialize
			/// <summary>
			/// Deserialize a stream
			/// </summary>
			/// <param name="T"></param>
			/// <param name="Stream"></param>
			/// <param name="CloseStream"></param>
			/// <returns></returns>
			public static object Deserialize(Type T, Stream Stream, bool CloseStream)
			{
				if (Stream == null) throw new ArgumentNullException("Stream");
				try
				{
					XmlSerializer serializer = new XmlSerializer(T);
					object obj = serializer.Deserialize(Stream);
					if (Stream != null && CloseStream) Stream.Close();
					return obj;
				}
				catch (Exception ex)
				{
					if (Stream != null && CloseStream) 
						Stream.Close();
					return null;
				}
			}

			#endregion deserialize
		}
		#endregion
	}
}