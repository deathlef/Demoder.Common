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
using System.Text;
using System.Text.RegularExpressions;

namespace Demoder.Common
{
    //Processes commandline arguments, and make a unix-like flag/argument system out of it.
    public class CommandLineParameters
    {
        private Dictionary<string, string> arguments = new Dictionary<string, string>();
        private Dictionary<string, uint> flags = new Dictionary<string, uint>();
        private Dictionary<string, uint> longflags = new Dictionary<string, uint>();
        /// <summary>
        /// Process provided commandline arguments.
        /// </summary>
        /// <param name="args"></param>
        public CommandLineParameters(string[] Args)
        {
            string args = " " + string.Join(" ", Args) + " ";
            //OK
            Regex rx_longflags = new Regex("[\\s]+[-]{2}([^-][^=\\s]+)[\\s]+"); //--setting
            Regex rx_flags = new Regex("[\\s]+[-]{1}([^-][\\S]+)[\\s]+"); // -flag_to_set
            Regex rx_args = new Regex("[\\s]+[-]{2}([\\S^=]+)=\"([^\"]+)\""); //--setting="value"


            Match mc;
            //Look for arguments
            mc = rx_args.Match(args);
            do
            {
                string arg = mc.Groups[1].Value;
                if (String.IsNullOrEmpty(arg))
                    continue;
                string val = mc.Groups[2].Value;
                if (!this.arguments.ContainsKey(arg))
                    this.arguments.Add(arg, val);
                mc = mc.NextMatch();
            } while (mc.Success);

            //Look for flags
            mc = rx_flags.Match(args);
            do
            {
                string flag = mc.Groups[1].Value;
                if (flag.Length > 0)
                {
                    if (!this.flags.ContainsKey(flag))
                        this.flags.Add(flag, 1);
                    else
                        this.flags[flag]++;
                }
                mc = mc.NextMatch();
            } while (mc.Success);

            //Look for longflags
            mc = rx_longflags.Match(args);
            do
            {
                string flag = mc.Groups[1].Value;
                if (!this.longflags.ContainsKey(flag))
                    this.longflags.Add(flag, 1);
                else
                    this.longflags[flag]++;
                mc = mc.NextMatch();
            } while (mc.Success);
        }
        /// <summary>
        /// Check if Flag is set. (-flag)
        /// </summary>
        /// <param name="Flag">CMD: -v   flag: v</param>
        /// <returns></returns>
        public uint Flag(string FlagName)
        {
            if (this.flags.ContainsKey(FlagName))
                return this.flags[FlagName];
            else
                return 0;
        }

        /// <summary>
        /// Check if LongFlag is set. --flag
        /// </summary>
        /// <param name="LongFlag">CMD: --v   longflag: v</param>
        /// <returns></returns>
        public uint LongFlag(string LongFlagName)
        {
            if (this.longflags.ContainsKey(LongFlagName))
                return this.longflags[LongFlagName];
            else
                return 0;
        }

        /// <summary>
        /// Retrieve the value of ArgumentName. Returns null if no value was provided. (--key=&quot;value&quot;)
        /// </summary>
        /// <param name="ArgumentName"></param>
        /// <returns></returns>
        public string Argument(string ArgumentName)
        {
            if (this.arguments.ContainsKey(ArgumentName))
                return this.arguments[ArgumentName];
            else
                return null;
        }
    }
}
