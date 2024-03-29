﻿/*
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

using Demoder.Common.Attributes;
using Demoder.Common.Extensions;
using Demoder.Common.SimpleLogger;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Demoder.Common.Serialization
{
    /// <summary>
    /// Used to extract information from a stream, into properly tagged objects.
    /// </summary>
    public static class StreamData
    {
        public static Logger Log { get; set; }

        #region Private members
        /// <summary>
        /// Registered stream data parsers
        /// </summary>
        private static ConcurrentDictionary<Type, IStreamDataParser> streamDataParsers = new ConcurrentDictionary<Type, IStreamDataParser>();
        /// <summary>
        /// This will be used if there's no available stream data parser
        /// </summary>
        private static StreamDataDefaultParser defaultDataParser = new StreamDataDefaultParser();

        

        #endregion

        static StreamData()
        {
            // Register all parsers in this assembly.
            RegisterStreamDataParsers(Assembly.GetAssembly(typeof(StreamData)));
        }

        /// <summary>
        /// Retrieve all StreamData properties contained within type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete("Use StreamDataInfo.GetProperties(Type type) instead.")]
        public static StreamDataInfo[] GetProperties(Type type)
        {
            return StreamDataInfo.GetProperties(type);
        }

        /// <summary>
        /// Creates a new object and populates it with data from the provided stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static T Create<T>(SuperStream ms)
        {
            
            var obj = Activator.CreateInstance<T>();
            return (T)Populate(obj, ms);
        }

        /// <summary>
        /// Creates a new object and populates it with data from the provided stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static dynamic Create(Type t, SuperStream ms)
        {
            var obj = Activator.CreateInstance(t);
            return Populate(obj, ms);
        }

        /// <summary>
        /// Populates an existing object with data from stream.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static object Populate(object obj, SuperStream ms)
        {
            var properties = StreamDataInfo.GetProperties(obj.GetType());
            bool gracefulStopAtEOF = obj.GetType().GetAttribute<StreamDataGracefulEofAttribute>() != null;
            dynamic value;
            // Parse spell arguments
            foreach (var pi in properties)
            {
                if (gracefulStopAtEOF && ms.EOF)
                {
                    break;
                }
                StreamDataParserTask task = new StreamDataParserTask(ms, pi.ReadType, pi.DataType, pi.Attributes);

                if (pi.IsCollection)
                {
                    var entries = pi.ReadContentLength(ms);

                    if (pi.IsArray)
                    {
                        var arr = new ArrayList((int)Math.Min(int.MaxValue, entries));
                        for (ulong i = 0; i < entries; i++)
                        {
                            if (!GetParserData(task, out value))
                            {
                                throw new Exception();
                            }
                            arr.Add(value);
                        }

                        var arr2 = arr.ToArray(pi.ReadType);
                        pi.PropertyInfo.SafeSetValue(obj, arr2, null);
                        continue;
                    }
                    else if (pi.IsList)
                    {
                        dynamic list = Activator.CreateInstance(
                            typeof(List<>).MakeGenericType(pi.DataType), 
                            (int)Math.Min(int.MaxValue, entries));

                        for (ulong i = 0; i < entries; i++)
                        {
                            if (!GetParserData(task, out value))
                            {
                                throw new Exception();
                            }
                            list.Add(value);
                        }
                        pi.PropertyInfo.SafeSetValue(obj, (object)list, null);
                    }
                }
                else
                {
                    if (!GetParserData(task, out value))
                    {
                        throw new Exception();
                    }
                    pi.PropertyInfo.SafeSetValue(obj, (object)value, null);
                    continue;
                }
            }
            if (typeof(IStreamDataFinalizer).IsAssignableFrom(obj.GetType()))
            {
                IStreamDataFinalizer fin = (IStreamDataFinalizer)obj;
                fin.OnDeserialize();
            }
            return obj;
        }

        /// <summary>
        /// Write a given object to stream
        /// </summary>
        /// <param name="obj">Object to write</param>
        /// <param name="ms">Stream to write to</param>
        public static void Serialize(object obj, SuperStream ms)
        {
            if (typeof(IStreamDataFinalizer).IsAssignableFrom(obj.GetType()))
            {
                IStreamDataFinalizer fin = (IStreamDataFinalizer)obj;
                fin.OnSerialize();
            }
            var properties = StreamDataInfo.GetProperties(obj.GetType());
            // Parse spell arguments
            foreach (var pi in properties)
            {
                dynamic value = pi.PropertyInfo.SafeGetValue(obj, null);
                StreamDataParserTask task = new StreamDataParserTask(ms, pi.ReadType, pi.DataType, pi.Attributes);

                if (pi.IsCollection)
                {
                    // Find actual length of collection.
                    ulong length;
                    if (pi.IsArray)
                    {
                        length = (ulong)value.Length;
                    }
                    else if (pi.IsList)
                    {
                        length = (ulong)value.Count;
                    }
                    else
                    {
                        throw new Exception("Property is collection, but not array nor list.");
                    }

                    // Write length, and return written length. (Entries= will override length of collection if set)
                    length = pi.WriteContentLength(ms, length);

                    dynamic enumerable = pi.PropertyInfo.SafeGetValue(obj, null);
                    ulong count = 0;
                    foreach (var entry in enumerable)
                    {
                        // Make sure we do not write more entries than we've declared
                        count++;
                        if (count > length)
                        {
                            throw new Exception("Collection contains more items than ");
                        }

                        // Write enry.
                        if (!WriteParserData(task, entry))
                        {
                            throw new Exception();
                        }
                    }
                    continue;

                }
                else
                {
                    if (!WriteParserData(task, value))
                    {
                        throw new Exception();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve human-readable display of PropertyName=Value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GetDebugInfo(object obj)
        {
            var values = new List<string>();
            foreach (var p in StreamDataInfo.GetProperties(obj.GetType()))
            {
                // Add each property tagged with SpellDataAttribute in the topmost class
                if (p.IsArray)
                {
                    dynamic values2 = p.PropertyInfo.SafeGetValue(obj, null);
                    values.Add(String.Format("{0}={1}", p.PropertyInfo.Name,
                        "{ " + String.Join(", ", values2) + " }"));
                }
                else
                {
                    values.Add(String.Format("{0}={1}", p.PropertyInfo.Name, p.PropertyInfo.SafeGetValue(obj, null)));
                }
            }
            return values.ToArray();
        }

        public static object[] GetPropertyValues(object obj)
        {
            var values = new List<object>();
            foreach (var p in StreamDataInfo.GetProperties(obj.GetType()))
            {
                values.Add(p.PropertyInfo.SafeGetValue(obj, null));
            }
            return values.ToArray();
        }

        #region Stream dataparser registration

        private static List<Assembly> registeredAssemblies = new List<Assembly>();

        /// <summary>
        /// Find and utilize all StreamDataParsers within assembly
        /// </summary>
        /// <param name="assembly"></param>
        public static void RegisterStreamDataParsers(Assembly assembly)
        {
            if (registeredAssemblies.Contains(assembly))
            {
                return;
            }
            if (Log != null)
            {
                Log.Debug("Registering stream data parsers from assembly " + assembly.GetName().Name);
            }

            var candidates = from t in assembly.GetTypes()
                             where typeof(IStreamDataParser).IsAssignableFrom(t) && t != typeof(IStreamDataParser)
                             select t;

            foreach (var t in candidates)
            {
                RegisterStreamDataParser(t);
            }

            registeredAssemblies.Add(assembly);
        }

        /// <summary>
        /// Register a specific StreamDataParser.
        /// </summary>
        /// <param name="parser"></param>
        private static void RegisterStreamDataParser(Type parser)
        {
            var instance = (IStreamDataParser)Activator.CreateInstance(parser);
            if (instance.SupportedTypes.Length == 0) { return; }

            foreach (var type in instance.SupportedTypes)
            {
                if (!streamDataParsers.TryAdd(type, instance))
                {

                    IStreamDataParser tmp;
                    if (!streamDataParsers.TryRemove(type, out tmp))
                    {
                        if (Log != null)
                        {
                            Log.Warning(String.Format("\tStreamDataParser type {0}: Tried to replacing \"{1}\" with \"{2}\" but failed",
                                 type.Name,
                                 streamDataParsers[type].GetType().Name,
                                 instance.GetType().Name));
                        }
                        continue;
                    }
                    if (!streamDataParsers.TryUpdate(type, instance, tmp))
                    {
                        if (Log != null)
                        {
                            Log.Warning(String.Format("\tStreamDataParser type {0}: Tried to replacing \"{1}\" with \"{2}\" but failed",
                                 type.Name,
                                 streamDataParsers[type].GetType().Name,
                                 instance.GetType().Name));
                        }
                        continue;
                    }
                    if (Log != null)
                    {
                        Log.Warning(String.Format("\tStreamDataParser type {0}: Replacing \"{1}\" with \"{2}\"",
                                type.Name,
                                streamDataParsers[type].GetType().Name,
                                instance.GetType().Name));
                    }
                    continue;
                }
                
                if (Log != null)
                {
                    Log.Debug(String.Format("\tStreamDataParser type {0}: Using \"{1}\"",
                            type.Name,
                            instance.GetType().Name));
                }
                continue;
            }
        }
        #endregion


        #region Helper methods

        private static IStreamDataParser GetParser(Type dataType)
        {
            IStreamDataParser parser;
            if (streamDataParsers.TryGetValue(dataType, out parser))
            {
                return parser;
            }
            return defaultDataParser;            
        }

        /// <summary>
        /// Retrieve data as described by task
        /// </summary>
        /// <param name="task">How to retrieve value</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private static bool GetParserData(StreamDataParserTask task, out dynamic value)
        {
            IStreamDataParser parser = GetParser(task.StreamType);

            dynamic tmpVal;
            bool result;
            result = parser.GetObject(task, out tmpVal);
            
            // Cast value, if necessary.
            if (task.StreamType != task.DataType)
            {
                value = Convert.ChangeType(tmpVal, task.DataType);
            }
            else
            {
                value = tmpVal;
            }

            if (typeof(IStreamDataFinalizer).IsAssignableFrom(task.DataType))
            {
                var finalizer = (IStreamDataFinalizer)value;
                finalizer.OnDeserialize();
            }

            return result;
        }


        /// <summary>
        /// Write data as described by task
        /// </summary>
        /// <param name="task">How to serialize object</param>
        /// <param name="value">Object to serialize</param>
        /// <returns></returns>
        private static bool WriteParserData(StreamDataParserTask task, object value)
        {
            IStreamDataParser parser = GetParser(task.StreamType);
            if (typeof(IStreamDataFinalizer).IsAssignableFrom(task.DataType))
            {
                var finalizer = (IStreamDataFinalizer)value;
                finalizer.OnSerialize();
            }

            dynamic tmpVal;
            // Cast value, if necessary.
            if (task.StreamType != task.DataType)
            {
                tmpVal = Convert.ChangeType(value, task.StreamType);
            }
            else
            {
                tmpVal = value;
            }

            var result = parser.WriteObject(task, tmpVal);      
            return result;
        }
        #endregion
    }
}
