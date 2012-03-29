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
using System.Xml.Serialization;

namespace Demoder.Common.AO.Towerwars.DataClasses
{
    [XmlRoot("Results")]
    [Serializable]
    public class TowerResults
    {
        [XmlElement("Result")]
        public List<TowerResult> Results = new List<TowerResult>();
    }

    [Serializable]
    public class TowerResult
    {
        [XmlAttribute("time")]
        public int Unixtime;
        [XmlIgnore]
        public DateTime Time { get { return Demoder.Common.Misc.Unixtime(this.Unixtime); } }

        [XmlAttribute("zonename")]
        public string Zone;

        [XmlAttribute("zoneshortname")]
        public string ZoneShortName;

        [XmlAttribute("siteid")]
        public int SiteID;

        [XmlAttribute("sitename")]
        public string SiteName;

        [XmlAttribute("siteminlevel")]
        public int SiteMinLevel;

        [XmlAttribute("sitemaxlevel")]
        public int SiteMaxLevel;

        [XmlAttribute("sitecenterx")]
        public int SiteCenterX;
        
        [XmlAttribute("sitecentery")]
        public int SiteCenterY;

        [XmlAttribute("attackerguildid")]
        public int AttackerGuildID;

        [XmlAttribute("attackerguildname")]
        public string AttackerGuildName;

        [XmlAttribute("attackerfactionid")]
        public int AttackerFactionID;

        [XmlAttribute("attackerfaction")]
        public Faction AttackerFaction;

        [XmlAttribute("defenderguildid")]
        public int DefenderGuildID;

        [XmlAttribute("defenderguildname")]
        public string DefenderGuildName;

        [XmlAttribute("defenderfaction")]
        public Faction DefenderFaction;

        public override string ToString()
        {
            return String.Format("The {1} {2} attacked the {3} {4} in {5} x{6}",
                this.AttackerFaction,
                this.AttackerGuildName,

                this.DefenderFaction,
                this.DefenderGuildName,
                this.ZoneShortName,
                this.SiteID);
        }
    }
}
