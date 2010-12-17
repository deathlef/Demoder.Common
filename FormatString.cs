/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://redmine.flw.nu/projects/demoder-common/)

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
		#region members
		private Dictionary<string, object> _dictionary;
		#endregion

		#region constructors
		/// <summary>
        /// Initialize without any pre-defined parameters 
        /// </summary>
        public FormatString() 
        {
            this._dictionary = null;
        }

        /// <summary>
        /// Initialize with a pre-defined set of parameters
        /// </summary>
        /// <param name="dict"></param>
        public FormatString(Dictionary<string, object> Dictionary)
		{
			this._dictionary = Dictionary;
		}
		#endregion

		#region methods
		/// <summary>
        /// Format a string using the provided parameters
        /// </summary>
        /// <param name="ToFormat"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public string Format(string ToFormat, Dictionary<string, object> Dictionary)
        {
            lock (this)
            {
                if (Dictionary == null)
                    throw new ArgumentException("Argument cannot be null", "dict");
                this._dictionary = Dictionary;
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
                if (this._dictionary == null)
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
		public string Format(string ToFormat, params KeyValuePair<string, object>[] Parameters)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> kvp in Parameters)
				if (!dict.ContainsKey(kvp.Key))
					dict.Add(kvp.Key, kvp.Value);
			return this.Format(ToFormat, dict);
		}
		
        /// <summary>
        /// Method used by the regex replace
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string doFormatString(Match Input)
		{
			string matchkey = Input.Value.Substring(1, Input.Value.Length - 2).ToLower();
			if (this._dictionary.ContainsKey(matchkey))
				return this._dictionary[matchkey].ToString();
			else
				return string.Empty;
		}
		#endregion
	}
}
