/*
Demoder.Common
Copyright (c) 2012 Demoder <demoder@demoder.me>

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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Demoder.Common.Serialization
{
    public static class XmlCompat
    {
        private static XmlSerializerFactory factory = new XmlSerializerFactory();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <param name="stream">Stream to write to</param>
        /// <param name="type">Which type to serialize object as. If null, will use objects real type.</param>
        /// <exception cref="ArgumentNullException">obj or stream is null</exception>
        /// <exception cref="IOException">stream isn't writeable</exception>
        public static void Serialize(object obj, Stream stream, Type type = null)
        {
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

            if (type == null)
            {
                type = obj.GetType();
            }

            XmlSerializer serializer = factory.CreateSerializer(type);
            serializer.Serialize(stream, obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <param name="file">File to write to</param>
        /// <param name="type">Which type to serialize object as. If null, will use objects real type.</param>
        /// <exception cref="ArgumentNullException">obj or file is null</exception>
        public static void Serialize(object obj, FileInfo file, Type type = null)
        {
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
                XmlCompat.Serialize(obj, stream, type);
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
                return XmlCompat.Deserialize(type, stream);
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
            return XmlCompat.Deserialize(type, uri.Uri);
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
                    return XmlCompat.Deserialize(type, stream);
                }
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
                return XmlCompat.Deserialize(type, stream);
            }
        }
    }
}