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
        

        public static VersionInfo GetInfo(string tool, Version currentVersion)
        {
            var response = Xml.Deserialize<VersionInfo>(new Uri(String.Format("{0}/{1}/xml/version.xml", BaseUri, tool)));
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
