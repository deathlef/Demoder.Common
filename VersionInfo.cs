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
using Demoder.Common.Serialization;
using Demoder.Common.Web;
using System.Xml.Serialization;

namespace Demoder.Common
{
    [XmlRoot("VersionInfo")]
    public class VersionInfo
    {
        /// <summary>
        /// Base path for version checking. Example: http://tools.demoder.me
        /// </summary>
        public static string BaseUri = "http://tools.demoder.me/";

        [XmlAttribute("productName")]
        public string ProductName { get; set; }
        [XmlAttribute("productVersion")]
        public string ProductVersion { get; set; }
        [XmlAttribute("downloadUri")]
        public string DownloadUri { get; set; }
        [XmlAttribute("changelogUri")]
        public string ChangelogUri { get; set; }
        
        /// <summary>
        /// Checks for updates for a given tool, located at the defined VersionInfo.BaseURI file structure.
        /// </summary>
        /// <param name="tool">Which tool to check for updates</param>
        /// <returns></returns>
        public static VersionInfo GetInfo(string tool)
        {
            return GetInfo(new Uri(String.Format("{0}/{1}/xml/version.xml", BaseUri, tool)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">Uri to update information</param>
        /// <returns></returns>
        public static VersionInfo GetInfo(Uri uri)
        {
            var response = Xml.Deserialize<VersionInfo>(uri);
            return response;
        }

        public bool UpdateAvailable(Version currentVersion)
        {
            var thisVersion = new Version(this.ProductVersion);
            if (thisVersion > currentVersion)
            {
                return true;
            }
            return false;
        }

        public enum ErrorCodes : int
        {
            InvalidUri = 0x1,
            Security=0x2
        }
    }
}
