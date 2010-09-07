/*
MIT Licence
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: https://sourceforge.net/projects/demoderstools/)

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
using System.Text.RegularExpressions;

namespace Demoder.Common
{
	public class FormatString
	{
		private Dictionary<string, object> dictionary;

        /// <summary>
        /// Initialize without any pre-defined parameters 
        /// </summary>
        public FormatString() 
        {
            this.dictionary = null;
        }

        /// <summary>
        /// Initialize with a pre-defined set of parameters
        /// </summary>
        /// <param name="dict"></param>
        public FormatString(Dictionary<string, object> dict)
		{
			this.dictionary = dict;
		}

        /// <summary>
        /// Format a string using the provided parameters
        /// </summary>
        /// <param name="ToFormat"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public string Format(string ToFormat, Dictionary<string, object> dict)
        {
            lock (this)
            {
                if (dict == null)
                    throw new ArgumentException("Argument cannot be null", "dict");
                this.dictionary = dict;
                return this.Format(ToFormat);
            }
        }

        /// <summary>
        /// Format a string using the previously provided parameters
        /// </summary>
        /// <param name="ToFormat"></param>
        /// <returns>formatted string</returns>
		public string Format(string ToFormat)
		{
            lock (this)
            {
                if (this.dictionary == null)
                    throw new InvalidOperationException("Dictioanry not provided upon instance creation. Therefore, it must be provided upon string formatting.");
                string outstring = ToFormat;
                Regex re = new Regex(@"\{[^}]*\}");
                outstring = re.Replace(outstring, doFormatString);
                return outstring;
            }
		}

		/// <summary>
		/// Formats a string using the provided parameters
		/// </summary>
		/// <param name="ToFormat">String which needs formatting</param>
		/// <param name="param">string[] { tag, value }</param>
		/// <returns>formatted string</returns>
		public string Format(string ToFormat, params string[][] param)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			foreach (string[] s in param)
				if (!dict.ContainsKey(s[0]))
					dict.Add(s[0], s[1]);

			return this.Format(ToFormat, dict);
		}
		
        /// <summary>
        /// Method used by the regex replace
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string doFormatString(Match input)
		{
			string matchkey = input.Value.Substring(1, input.Value.Length - 2).ToLower();
			if (this.dictionary.ContainsKey(matchkey))
				return this.dictionary[matchkey].ToString();
			else
				return string.Empty;
		}
	}
}
