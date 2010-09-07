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
	//Processes commandline arguments, and make a unix-like flag/argument system out of it.
	public class cmdParams
	{
		private Dictionary<string, string> _arguments = new Dictionary<string, string>();
		private Dictionary<string, uint> _flags = new Dictionary<string, uint>();
        private Dictionary<string, uint> _longflags = new Dictionary<string, uint>(); 
		/// <summary>
		/// Process provided commandline arguments.
		/// </summary>
		/// <param name="args"></param>
		public cmdParams(string[] args)
		{
			Regex rx_flags = new Regex("[^-]-([\\w]*)"); // -flag_to_set
			Regex rx_args = new Regex("--([\\w]*)=\"([^\"]+)\""); //--setting="value"
            Regex rx_longflags = new Regex("--([\\w]*)[^=]"); //--setting

			
			Match mc = rx_flags.Match(" "+string.Join(" ", args));
			do
			{
				string flag = mc.Groups[1].Value;
				if (flag.Length > 0)
				{
                    if (!this._flags.ContainsKey(flag))
                        this._flags.Add(flag, 1);
                    else
                        this._flags[flag]++;
				}
				mc = mc.NextMatch();
			} while (mc.Success);

			mc = rx_args.Match(string.Join(" ", args));
			do
			{
				string arg = mc.Groups[1].Value;
				string val = mc.Groups[2].Value;
				if (!this._arguments.ContainsKey(arg))
					this._arguments.Add(arg, val);
				mc = mc.NextMatch();
			} while (mc.Success);

            mc = rx_longflags.Match(string.Join(" ", args));
            do
            {
                string flag = mc.Groups[1].Value;
                string val = mc.Groups[2].Value;
                if (!this._longflags.ContainsKey(flag))
                    this._longflags.Add(flag, 1);
                else
                    this._longflags[flag]++;
                mc = mc.NextMatch();
            } while (mc.Success);
		}
		/// <summary>
		/// Check if Flag is set.
		/// </summary>
		/// <param name="Flag">CMD: -v   flag: v</param>
		/// <returns></returns>
		public uint Flag(string Flag)
		{
            if (this._flags.ContainsKey(Flag))
                return this._flags[Flag];
            else
                return 0;
		}

        /// <summary>
        /// Check if LongFlag is set.
        /// </summary>
        /// <param name="LongFlag">CMD: --v   longflag: v</param>
        /// <returns></returns>
        public uint LongFlag(string LongFlag)
        {
            if (this._longflags.ContainsKey(LongFlag))
                return this._longflags[LongFlag];
            else
                return 0;
        }

		//Retrieve the value of ArgumentName. Returns null if no value was provided.
		public string Argument(string ArgumentName)
		{
			if (this._arguments.ContainsKey(ArgumentName))
				return this._arguments[ArgumentName];
			else
				return null;
		}
	}
}
