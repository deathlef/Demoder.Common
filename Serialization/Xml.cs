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
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Xml.Serialization;
using Demoder.Common;

namespace Demoder.Common.Serialization
{
    public static class Xml
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="stream">Write to this stream</param>
        /// <returns>true on success, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If obj or stream is null</exception>
        public static bool TrySerialize<T>(T obj, Stream stream)
        {
            return TrySerialize(typeof(T), obj, stream);
        }

        public static void Serialize<T>(T obj, Stream stream)
        {
            Serialize(typeof(T), obj, stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="file">Write to this file</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">If obj or stream is null</exception>
        public static bool TrySerialize<T>(T obj, FileInfo file)
        {
            return TrySerialize(typeof(T), obj, file);
        }

        public static void Serialize<T>(T obj, FileInfo file)
        {
            Serialize(typeof(T), obj, file);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="stream">Stream to read from</param>
        /// <param name="obj">Result is stored here</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">if stream is null</exception>
        public static bool TryDeserialize<T>(Stream stream, out T obj)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            try
            {
                obj = (T)Deserialize(typeof(T), stream);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public static T Deserialize<T>(Stream stream)
        {
            return (T)Deserialize(typeof(T), stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="file">Read from this file</param>
        /// <param name="obj">Result is stored here</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">if file is null</exception>
        public static bool TryDeserialize<T>(FileInfo file, out T obj)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            try
            {
                obj = (T)Deserialize(typeof(T), file);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public static T Deserialize<T>(FileInfo file)
        {
            return (T)Deserialize(typeof(T), file);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="uri">Read from this URI</param>
        /// <param name="obj">Result is stored here</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">if uri is null</exception>
        public static bool TryDeserialize<T>(UriBuilder uri, out T obj)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            try
            {
                obj = (T)Deserialize(typeof(T), uri);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public static T Deserialize<T>(UriBuilder uri)
        {
            return (T)Deserialize(typeof(T), uri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="uri">Read from this URI</param>
        /// <param name="obj">Result is stored here</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">if uri is null</exception>
        public static bool TryDeserialize<T>(Uri uri, out T obj)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            try
            {
                obj = (T)Deserialize(typeof(T), uri);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public static T Deserialize<T>(Uri uri)
        {
            return (T)Deserialize(typeof(T), uri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="xml">XML to deserialize</param>
        /// <param name="encoding">Which encoding to use when reading the XML</param>
        /// <param name="obj">Result is stored here</param>
        /// <returns>true on success, otherwise false</returns>
        /// <exception cref="ArgumentNullException">if xml or encoding is null</exception>
        public static bool TryDeserialize<T>(string xml, Encoding encoding, out T obj)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            try
            {
                obj = (T)Deserialize(typeof(T), xml, encoding);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        public static T Deserialize<T>(string xml, Encoding encoding)
        {
            return (T)Deserialize(typeof(T), xml, encoding);
        }




        private static XmlSerializerFactory factory = new XmlSerializerFactory();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <param name="stream">Stream to write to</param>
        /// <param name="type">Which type to serialize object as. If null, will use objects real type.</param>
        /// <exception cref="ArgumentNullException">type, obj or stream is null</exception>
        /// <exception cref="IOException">stream isn't writeable</exception>
        public static void Serialize(Type type, object obj, Stream stream)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanWrite)
            {
                throw new IOException("Cannot write to stream", new ArgumentException("stream"));
            }

            XmlSerializer serializer = factory.CreateSerializer(type);
            serializer.Serialize(stream, obj);
        }

        public static bool TrySerialize(Type type, object obj, Stream stream)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            try
            {
                Serialize(type, obj, stream);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <param name="file">File to write to</param>
        /// <param name="type">Which type to serialize object as.</param>
        /// <exception cref="ArgumentNullException">type, obj or file is null</exception>
        public static void Serialize(Type type, object obj, FileInfo file)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            using (FileStream stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096 * 8))
            {
                Serialize(type, obj, stream);
            }
        }

        public static bool TrySerialize(Type type, object obj, FileInfo file)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            try
            {
                Serialize(type, obj, file);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type to deserialize as</param>
        /// <param name="stream">Stream to read from</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="ArgumentNullException">If type or stream is null</exception>
        /// <exception cref="IOException">If stream is not readable</exception>
        public static object Deserialize(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!stream.CanRead)
            {
                throw new IOException("Cannot read from stream.", new ArgumentException("stream"));
            }

            XmlSerializer serializer = factory.CreateSerializer(type);
            return serializer.Deserialize(stream);
        }

        public static bool TryDeserialize(Type type, Stream stream, out object obj)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            try
            {
                obj = Deserialize(type, stream);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type to deserialize</param>
        /// <param name="file">File to read from</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="ArgumentNullException">If type or file is null</exception>
        /// <exception cref="FileNotFountException">If file does not exist</exception>
        public static object Deserialize(Type type, FileInfo file)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("File was not found", file.FullName, new ArgumentException("file"));
            }

            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                return Deserialize(type, stream);
            }
        }

        public static bool TryDeserialize(Type type, FileInfo file, out object obj)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            try
            {
                obj = Deserialize(type, file);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type to deserialize as</param>
        /// <param name="uri">URI to download for deserialization</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="ArgumentNullException">If type or uri is null</exception>
        public static object Deserialize(Type type, UriBuilder uri)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            return Deserialize(type, uri.Uri);
        }

        public static bool TryDeserialize(Type type, UriBuilder uri, out object obj)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            try
            {
                obj = Deserialize(type, uri);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type to deserialize as</param>
        /// <param name="uri">URI to download for deserialization</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="ArgumentNullException">If type or uri is null</exception>
        /// <exception cref="ArgumentException">If uri is empty</exception>
        public static object Deserialize(Type type, Uri uri)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (String.IsNullOrWhiteSpace(uri.ToString()))
            {
                throw new ArgumentException("URI is empty", "uri");
            }
            using (WebClient web = new WebClient())
            {
                byte[] data = web.DownloadData(uri);
                using (MemoryStream stream = new MemoryStream(data))
                {
                    return Deserialize(type, stream);
                }
            }
        }

        public static object TryDeserialize(Type type, Uri uri, out object obj)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            try
            {
                obj = Deserialize(type, uri);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type to deserialize as</param>
        /// <param name="xml">XML to deserialize</param>
        /// <param name="encoding">Encoding to use when deserializing</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="ArgumentNullException">If type, xml or encoding is null</exception>
        public static object Deserialize(Type type, string xml, Encoding encoding)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            using (MemoryStream stream = new MemoryStream(encoding.GetBytes(xml)))
            {
                return Deserialize(type, stream);
            }
        }

        public static bool TryDeserialize(Type type, string xml, Encoding encoding, out object obj)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            try
            {
                obj = Deserialize(type, xml, encoding);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }
    }
}