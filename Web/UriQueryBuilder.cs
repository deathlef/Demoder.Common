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
using System.Web;

namespace Demoder.Common.Web
{
    public class UriQueryBuilder
    {
        private Dictionary<string, string> queryItems = new Dictionary<string, string>();

        public void Set(string key, object value) 
        {
            this.queryItems[key] = Uri.EscapeDataString(value.ToString());
        }

        public void Delete(string key)
        {
            this.queryItems.Remove(key);
        }

        public override string ToString()
        {
            List<string> items = new List<string>();
            foreach (KeyValuePair<string,string> kvp in this.queryItems) 
            {
                items.Add(String.Format("{0}={1}",kvp.Key, kvp.Value));
            }
            return String.Join("&", items);
        }

        /// <summary>
        /// Return query as Uri
        /// </summary>
        /// <param name="baseUri">Base URI to add query to. Any existing query information is disregarded.</param>
        /// <returns></returns>
        public string ToString(Uri baseUri)
        {
            UriBuilder ub = new UriBuilder(baseUri);
            ub.Query = this.ToString();
            return ub.ToString();
        }

        /// <summary>
        /// Return query as Uri
        /// </summary>
        /// <param name="baseUri">Base URI to add query to. Any existing query information is disregarded.</param>
        /// <returns></returns>
        public Uri ToUri(Uri baseUri)
        {
            UriBuilder ub = new UriBuilder(baseUri);
            ub.Query = this.ToString();
            return ub.Uri;
        }
    }
}
