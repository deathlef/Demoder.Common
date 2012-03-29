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
    [XmlRoot("TowerSites")]
    [Serializable]
    public class TowerSites
    {
        [XmlElement("TowerSite")]
        public List<TowerSite> Sites = new List<TowerSite>();
    }

    [Serializable]
    public class TowerSite 
    {
        [XmlAttribute("zone")]
        public string Zone;
        [XmlAttribute("id")]
        public int ID;
        [XmlAttribute("faction")]
        public Faction Faction;
        [XmlAttribute("guild")]
        public string Guild;
        [XmlAttribute("minlevel")]
        public int MinLevel;
        [XmlAttribute("maxlevel")]
        public int MaxLevel;

        public override string ToString()
        {
            return String.Format("{0} x{1} - {2}",
                this.Zone,
                this.ID,
                this.Faction);
        }
    }
}
