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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Demoder.Common.Extensions
{
    public static class RegexExtensions
    {
        /*
         * Tips on how to use Regex: http://msdn.microsoft.com/en-us/library/az24scfc.aspx 
         * 
         */

        /// <summary>
        /// Retrieves an associative dictionary with SubPatternName = Value.
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetMatchCollection(this Regex regex, string inputText)
        {
            var ret = new List<Dictionary<string, string>>();
            foreach (Match match in regex.Matches(inputText))
            {
                var dict = new Dictionary<string, string>();
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    Group group = match.Groups[i];
                    dict[regex.GroupNameFromNumber(i)] = group.Value;
                }
                ret.Add(dict);
            }
            return ret;
        }
    }
}
