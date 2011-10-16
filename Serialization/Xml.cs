/*
Demoder.Common
Copyright (c) 2010,2011 Demoder <demoder@demoder.me>

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
using Demoder.Common.Net;

namespace Demoder.Common.Serialization
{
    public static class Xml
    {
        /// <summary>
        /// Last exception thrown by this XML library on this thread.
        /// </summary>
        [ThreadStatic]
        public static Exception LastException = null;

        private static void ReportException(Exception ex=null)
        {
            LastException = ex;
        }


        #region Serialization
        /// <summary>
        /// Serializes an object into an already opened stream
        /// </summary>
        /// <typeparam name="T">Class type to serialize class as</typeparam>
        /// <param name="stream">Stream to serialize into</param>
        /// <param name="obj">Class to serialize</param>
        public static bool Serialize<T>(Stream stream, object obj, bool closeStream) where T : class
        {
            return Compat.Serialize(typeof(T), stream, obj, closeStream);
        }
        /// <summary>
        /// Serialize a class to a file
        /// </summary>
        /// <typeparam name="T">Class type to serialize</typeparam>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="gzip">Whether or not the saved file should be GZipped</param>
        /// <returns></returns>
        public static bool Serialize<T>(FileInfo path, T obj, bool gzip) where T : class
        {
            return Compat.Serialize(typeof(T), path, obj, gzip);
        }
        #endregion

        #region deserialization
        /// <summary>
        /// Deserialize a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="closeStream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream, bool closeStream) where T : class
        {
            object obj = Compat.Deserialize(typeof(T), stream, closeStream);
            if (obj == null)
                return default(T);
            else
                return (T)obj;
        }

        /// <summary>
        /// Deserialize a file
        /// </summary>
        /// <typeparam name="T">What class type to parse file as</typeparam>
        /// <param name="path">Path to the file</param>
        /// <param name="gzip">Whether or not the file is gzip-compressed</param>
        /// <returns></returns>
        public static T Deserialize<T>(FileInfo path, bool gzip) where T : class
        {
            object obj = Compat.Deserialize(typeof(T), path, gzip);
            if (obj == null)
                return default(T);
            else
                return (T)obj;
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
        /// <param name="uris">List of mirrors to try</param>
        /// <returns></returns>
        public static T Deserialize<T>(IEnumerable<Uri> uris) where T : class
        {
            object obj = Compat.Deserialize(typeof(T), uris);
            if (obj == null)
                return default(T);
            else
                return (T)obj;
        }

        /// <summary>
        /// Deserialize content of URI
        /// </summary>
        /// <typeparam name="T">Class type to parse as</typeparam>
        /// <param name="path">URI to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(Uri path) where T : class
        {
            object obj = Compat.Deserialize(typeof(T), path);
            if (obj == null)
                return default(T);
            else
                return (T)obj;
        }
        #endregion


        public static class Compat
        {
            #region Serialize
            /// <summary>
            /// Serialize object to stream
            /// </summary>
            /// <param name="t"></param>
            /// <param name="stream"></param>
            /// <param name="obj"></param>
            /// <param name="closeStream"></param>
            /// <returns></returns>
            public static bool Serialize(Type t, Stream stream, object obj, bool closeStream)
            {
                if (stream == null) { throw new ArgumentNullException("Stream"); }
                if (obj == null) { throw new ArgumentNullException("Obj"); }
                ReportException(null);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(t);
                    serializer.Serialize(stream, obj);
                    return true;
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return false;
                }
                finally
                {
                    if (closeStream && stream != null)
                    {
                        stream.Close();
                    }
                }
            }

            /// <summary>
            /// Serialize object to file
            /// </summary>
            /// <param name="t"></param>
            /// <param name="path"></param>
            /// <param name="obj"></param>
            /// <param name="gzip"></param>
            /// <returns></returns>
            public static bool Serialize(Type t, FileInfo path, object obj, bool gzip)
            {
                if (path == null) throw new ArgumentNullException("Path");
                if (obj == null) throw new ArgumentNullException("Obj");
                ReportException(null);
                if (gzip)
                {
                    using (FileStream fs = path.Create())
                    {
                        using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                        {
                            Serialize(t, gzs, obj, true);
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
                        XmlSerializer serializer = new XmlSerializer(t);
                        serializer.Serialize(ms, obj); //Serialize into memory

                        fs = path.Create();
                        ms.WriteTo(fs);
                        if (fs != null) fs.Close();
                        if (ms != null) ms.Close();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                        if (fs != null) fs.Close();
                        if (ms != null) ms.Close();
                        return false;
                    }
                }
            }
            #endregion
            #region Deserialize
            /// <summary>
            /// Deserialize a stream
            /// </summary>
            /// <param name="t"></param>
            /// <param name="stream"></param>
            /// <param name="closeStream"></param>
            /// <returns></returns>
            public static object Deserialize(Type t, Stream stream, bool closeStream)
            {
                if (stream == null) throw new ArgumentNullException("Stream");
                ReportException(null);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(t);
                    object obj = serializer.Deserialize(stream);
                    if (stream != null && closeStream) stream.Close();
                    return obj;
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return null;
                }
                finally
                {
                    if (stream != null && closeStream)
                    {
                        stream.Close();
                    }
                }
            }

            /// <summary>
            /// Deserialize a file
            /// </summary>
            /// <param name="t"></param>
            /// <param name="path"></param>
            /// <param name="gzip"></param>
            /// <returns></returns>
            public static object Deserialize(Type t, FileInfo path, bool gzip)
            {
                if (path == null) throw new ArgumentNullException("Path");
                ReportException(null);

                if (gzip)
                {
                    using (FileStream fs = path.OpenRead())
                    {
                        using (GZipStream gzs = new System.IO.Compression.GZipStream(fs, CompressionMode.Decompress, true))
                        {
                            return Deserialize(t, gzs, true);
                        }
                    }
                }
                else
                {
                    FileStream stream = null;
                    try
                    {
                        stream = path.OpenRead();
                        XmlSerializer serializer = new XmlSerializer(t);
                        Object obj = serializer.Deserialize(stream);
                        if (stream != null) stream.Close();
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                        if (stream != null) stream.Close();
                        return null;
                    }
                }
            }

            /// <summary>
            /// Deserialize an URI
            /// </summary>
            /// <param name="t"></param>
            /// <param name="path"></param>
            /// <returns></returns>
            public static object Deserialize(Type t, UriBuilder path)
            {
                return Deserialize(t, path.Uri);
            }

            /// <summary>
            /// Deserialize an URI
            /// </summary>
            /// <param name="t"></param>
            /// <param name="uris"></param>
            /// <returns></returns>
            public static object Deserialize(Type t, IEnumerable<Uri> uris)
            {
                try
                {
                    DownloadItem di = new DownloadItem(null, uris, null, null);
                    return Deserialize(t, new MemoryStream(DownloadManager.GetBinaryData(di, int.MaxValue)), true);
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return null;
                }
            }

            /// <summary>
            /// Deserialize an URI
            /// </summary>
            /// <param name="t"></param>
            /// <param name="path"></param>
            /// <returns></returns>
            public static object Deserialize(Type t, Uri path)
            {
                if (path == null) throw new ArgumentNullException("Path");
                try
                {
                    return Deserialize(t, new List<Uri>(new Uri[] { path }));
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return null;
                }
            }
            #endregion deserialize
        }
    }
}