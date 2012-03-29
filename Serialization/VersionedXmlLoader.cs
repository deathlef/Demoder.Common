/*
* Vha.Common
* Copyright (C) 2005-2010 Remco van Oosterhout
* See Credits.txt for all aknowledgements.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

/* This class originates from Vha.Common.Data.Base.cs (r242)
 * Modifications by Demoder
 */

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
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Net;

namespace Demoder.Common.Serialization
{
    [XmlRoot("Root"), Serializable]
    public abstract class VersionedXmlLoader
    {
        #region Members
        [XmlAttribute]
        public string Name { get; private set; }
        [XmlAttribute]
        public int Version { get; private set; }
        [XmlIgnore]
        public bool CanUpgrade { get; private set; }
        [XmlIgnore]
        public Type Type { get; private set; }

        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        #endregion Members

        public abstract VersionedXmlLoader Upgrade();

        #region Static methods
        public static void Register(string name, int version, Type type)
        {
            lock (types)
            {
                string id = name + "_" + version.ToString();
                if (types.ContainsKey(id))
                    types[id] = type;
                else
                    types.Add(id, type);
            }
        }

        public static void Unregister(string name, int version)
        {
            lock (types)
            {
                string id = name + "_" + version.ToString();
                if (types.ContainsKey(id))
                    types.Remove(id);
            }
        }

        public static bool IsRegistered(string name, int version)
        {
            lock (types)
            {
                string id = name + "_" + version.ToString();
                return types.ContainsKey(id);
            }
        }

        #region Load
        public static VersionedXmlLoader Load(FileInfo File)
        {
            FileStream fs = null;
            VersionedXmlLoader vxl = null;
            try
            {
                fs = File.OpenRead();
                vxl = Load(fs);
            }
            catch
            {
                if (fs != null)
                    fs.Close();
            }
            return vxl;
        }

        public static VersionedXmlLoader Load(Uri uri)
        {
            var webClient = new WebClient();
            var data = webClient.DownloadData(uri.ToString());
            Stream stream = new MemoryStream(data);
            return Load(stream);
        }

        public static VersionedXmlLoader Load(Stream stream)
        {
            // Check stream
            if (stream == null)
                throw new ArgumentNullException();
            if (!stream.CanRead)
                throw new ArgumentException("Stream can not be read");
            XmlReader reader = null;
            try
            {
                // Start reading XML
                reader = XmlReader.Create(stream);
                if (reader.ReadToFollowing("Root") == false)
                    return null;
                // Determine the type of the XML file
                string name = reader.GetAttribute("Name");
                string version = reader.GetAttribute("Version");
                // XML file doesn't meet our expected format
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(version))
                    return null;
                string type = name + "_" + version;
                // Load file as known type
                stream.Seek(0, SeekOrigin.Begin);
                Type dataType = null;
                lock (types)
                {
                    if (types.ContainsKey(type))
                        dataType = types[type];
                }
                if (dataType == null)
                    return null;
                // Read data
                VersionedXmlLoader data = (VersionedXmlLoader)Xml.Compat.Deserialize(dataType, stream, false);
                if (data == null)
                    return null;
                // Upgrade data if needed
                while (data.CanUpgrade)
                    data = data.Upgrade();
                return data;
            }
            catch (Exception) { return null; }
            finally
            {
                // Close anything left open
                if (reader != null)
                    reader.Close();
            }
        }
        #endregion Load
        #endregion Static Methods

        #region Internal
        protected VersionedXmlLoader(string name, int version, bool canUpgrade, Type type)
        {
            this.Name = name;
            this.Version = version;
            this.CanUpgrade = canUpgrade;
            this.Type = type;
        }
        #endregion
    }
}
