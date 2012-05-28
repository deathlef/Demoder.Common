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
using System.Linq;
using System.Text;
using System.Reflection;
using Demoder.Common.Attributes;
using System.Collections;
using Demoder.Common.SimpleLogger;

namespace Demoder.Common.Serialization
{
    /// <summary>
    /// Used to extract information from a stream, into properly tagged objects.
    /// </summary>
    public static class StreamData
    {
        private static object lockObject = new Object();

        public static Logger Log { get; set; }

        #region Private members
        /// <summary>
        /// Registered stream data parsers
        /// </summary>
        private static Dictionary<Type, IStreamDataParser> streamDataParsers = new Dictionary<Type, IStreamDataParser>();
        /// <summary>
        /// This will be used if there's no available stream data parser
        /// </summary>
        private static StreamDataDefaultParser defaultDataParser = new StreamDataDefaultParser();

        /// <summary>
        /// Property information cache per type
        /// </summary>
        private static Dictionary<Type, StreamDataInfo[]> cachedProperties = new Dictionary<Type, StreamDataInfo[]>();

        #endregion

        static StreamData()
        {
            // Register all parsers in this assembly.
            RegisterStreamDataParsers(Assembly.GetAssembly(typeof(StreamData)));
        }

        public static StreamDataInfo[] GetProperties(Type type)
        {
            lock (cachedProperties)
            {
                if (!cachedProperties.ContainsKey(type))
                {
                    cachedProperties[type] =
                        (from pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                         let attr = (StreamDataAttribute)pi.GetCustomAttributes(typeof(StreamDataAttribute), true).FirstOrDefault()
                         // Only consider SpellData properties
                         where attr != null
                         orderby attr.Order ascending
                         select StreamDataInfo.Create(pi, attr)).ToArray();
                }
                return cachedProperties[type];
            }
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
            var properties = GetProperties(obj.GetType());
            dynamic value;
            // Parse spell arguments
            foreach (var pi in properties)
            {
                StreamDataParserTask task;
                if (pi.IsArray)
                {
                    var arr = new ArrayList();
                    for (int i = 0; i < pi.Entries; i++)
                    {
                        task = new StreamDataParserTask(ms, pi.ReadType, pi.DataType, pi.Attributes);
                        
                        if (!GetParserData(task, out value))
                        {
                            throw new Exception();
                        }
                        arr.Add(value);
                    }

                    var arr2 = arr.ToArray(pi.ReadType);
                    pi.PropertyInfo.SetValue(obj, arr2, null);
                    continue;
                }
                task = new StreamDataParserTask(ms, pi.ReadType, pi.DataType, pi.Attributes);
                if (!GetParserData(task, out value))
                {
                    throw new Exception();
                }
                pi.PropertyInfo.SetValue(obj, value, null);
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
            var properties = GetProperties(obj.GetType());
            // Parse spell arguments
            foreach (var pi in properties)
            {
                dynamic value=pi.PropertyInfo.GetValue(obj, null);
                StreamDataParserTask task = new StreamDataParserTask(ms, pi.ReadType, pi.DataType, pi.Attributes);
                if (pi.IsArray)
                { 
                    for (int i = 0; i < pi.Entries; i++)
                    {
                        
                        if (!WriteParserData(task, value[i]))
                        {
                            throw new Exception();
                        }
                    }
                    continue;
                }

                if (!WriteParserData(task, value))
                {
                    throw new Exception();
                }
            }
        }

        public static string[] GetDebugInfo(object obj)
        {
            var values = new List<string>();
            foreach (var p in GetProperties(obj.GetType()))
            {
                // Add each property tagged with SpellDataAttribute in the topmost class
                if (p.IsArray)
                {
                    dynamic values2 = p.PropertyInfo.GetValue(obj, null);
                    values.Add(String.Format("{0}={1}", p.PropertyInfo.Name,
                        "{ " + String.Join(", ", values2) + " }"));
                }
                else
                {
                    values.Add(String.Format("{0}={1}", p.PropertyInfo.Name, p.PropertyInfo.GetValue(obj, null)));
                }
            }
            return values.ToArray();
        }

        #region Stream dataparser registration
        public static void RegisterStreamDataParsers(Assembly assembly)
        {
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
        }

        private static void RegisterStreamDataParser(Type parser)
        {
            var instance = (IStreamDataParser)Activator.CreateInstance(parser);
            if (instance.SupportedTypes.Length == 0) { return; }

            lock (streamDataParsers)
            {
                foreach (var type in instance.SupportedTypes)
                {
                    if (streamDataParsers.ContainsKey(type))
                    {
                        if (Log != null)
                        {
                            Log.Warning(String.Format("\tStreamDataParser type {0}: Replacing \"{1}\" with \"{2}\"",
                                type.Name,
                                streamDataParsers[type].GetType().Name,
                                instance.GetType().Name));
                        }
                        streamDataParsers[type] = instance;
                        continue;
                    }
                    streamDataParsers[type] = instance;
                    if (Log != null)
                    {
                        Log.Debug(String.Format("\tStreamDataParser type {0}: Using \"{1}\"",
                                type.Name,
                                instance.GetType().Name));
                    }
                    continue;
                }
            }
        }
        #endregion


        #region Helper methods
        private static bool GetParserData(StreamDataParserTask task, out dynamic value)
        {
            IStreamDataParser parser = null;

            dynamic tmpVal;
            bool result;
            // Get the right parser
            lock (streamDataParsers)
            {
                if (streamDataParsers.ContainsKey(task.StreamType))
                {
                    parser = streamDataParsers[task.StreamType];
                }
            }

            // Retrieve value from the designated parser.
            if (parser == null)
            {
                result = defaultDataParser.GetObject(task, out tmpVal);
            }
            else
            {
                result = parser.GetObject(task, out tmpVal);
            }

            // Cast value, if necessary.
            if (task.StreamType != task.DataType)
            {
                value = Convert.ChangeType(tmpVal, task.DataType);
            }
            else
            {
                value = tmpVal;
            }

            return result;
        }



        private static bool WriteParserData(StreamDataParserTask task, object value)
        {
            IStreamDataParser parser = null;
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

            bool result;
            // Get the right parser
            lock (streamDataParsers)
            {
                if (streamDataParsers.ContainsKey(task.StreamType))
                {
                    parser = streamDataParsers[task.StreamType];
                }
            }

            // Retrieve value from the designated parser.
            if (parser == null)
            {
                result = defaultDataParser.WriteObject(task, tmpVal);
            }
            else
            {
                result = parser.WriteObject(task, tmpVal);
            }            

            return result;
        }
        #endregion
    }
}
