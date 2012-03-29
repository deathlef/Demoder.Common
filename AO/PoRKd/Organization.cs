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
using Demoder.Common.Web;
using System.Net;
using System.IO;

namespace Demoder.Common.AO.PoRKd
{
    public class Organization
    {
        const string apiURI="http://porkd.botsharp.net/";


        public static int GetID(string name, string botTag, Dimension dimension)
        {
            return GetID(name, botTag, (int)dimension);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Organization name to search for</param>
        /// <param name="botTag">Identify as this bot</param>
        /// <returns></returns>
        public static int GetID(string name, string botTag, int dimension)
        {
            var qb = new UriQueryBuilder();
            qb.Set("type", "orgid");
            qb.Set("d", dimension);
            qb.Set("name", name);
            qb.Set("bot", botTag);
            qb.Set("output", "plain");
            var uri = qb.ToUri(new Uri(apiURI));
            
            var webClient = new WebClient();
            var result = webClient.DownloadData(uri.ToString());
            if (result==null || result.Length==0) 
            {
                throw new Exception("Returned data was empty.");
            }

            var result2 = ASCIIEncoding.ASCII.GetString(result);
            int value;
            if (!int.TryParse(result2, out value))
            {
                throw new Exception("Returned data was not a valid integer. Returned data: " + result2);
            }

            return value;
        }
    }
}
