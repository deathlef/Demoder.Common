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
using System.Diagnostics;
using System.IO;

namespace Demoder.Common.SimpleLogger
{
    public class Logger
    {
        private readonly string category;
        private readonly EventLogLevel minLogLevel;
        private readonly string prefix;
        private readonly ConsoleColor tagColor;
        private readonly ConsoleColor defaultFgColor;
        private readonly int prefixReservedColumns;


        public Logger(string category, EventLogLevel minLevel = EventLogLevel.Debug, string prefix = "", ConsoleColor tagColor = ConsoleColor.White, int prefixReservedColumns=20)
        {
            this.category = category;
            this.minLogLevel = minLevel;
            this.prefix = String.Format("{0,10}", prefix);
            this.tagColor = tagColor;
            this.defaultFgColor = System.Console.ForegroundColor;
            this.prefixReservedColumns = prefixReservedColumns;
        }

        public void Console(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Console, message, skipFrames + 2, textColor);
        }

        public void Critical(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Critical, message, skipFrames + 2, textColor);
        }

        public void Serious(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Serious, message, skipFrames + 2, textColor);
        }

        public void Error(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Error, message, skipFrames + 2, textColor);
        }

        public void Warning(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Warning, message, skipFrames + 2, textColor);
        }

        public void Notice(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Notice, message, skipFrames + 2, textColor);
        }

        public void Debug(string message, int skipFrames = 0, ConsoleColor textColor = ConsoleColor.White)
        {
            this.Log(EventLogLevel.Debug, message, skipFrames + 2, textColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="skipFrames">Only relevant for level.Debug|level.Error. How many StackTrace frames to skip? If set to less than 0, will not do stack trace</param>
        private void Log(EventLogLevel level, string message, int skipFrames = 1, ConsoleColor textColor = ConsoleColor.White)
        {
            if (level < this.minLogLevel)
            {
                return;
            }
            bool doStackTrace = false;
            if (skipFrames < 0)
            {
                doStackTrace = false;
            }
            else if (level == EventLogLevel.Debug || level == EventLogLevel.Error)
            {
                doStackTrace = true;
            }

            if (doStackTrace)
            {
                // StackTrace output, skip first frame as that's irrelevant
                StackTrace trace = new StackTrace(skipFrames, true);
                StackFrame frame = trace.GetFrame(0);
                string filename = frame.GetFileName();

                if (filename != null)
                {
                    filename = new FileInfo(filename).Name;
                }
                else
                {
                    filename = "";
                }
                lock (System.Console.InputEncoding)
                {
                    string traceInfo = String.Format("({0}:{1}/{2})",
                        filename,
                        frame.GetFileLineNumber(),
                        frame.GetMethod().Name);
                    this.WriteToConsole(String.Format(" {0} [{1}] {2,50} {3,10}: {4}",
                        DateTime.Now.ToLongTimeString(),
                        level,
                        traceInfo,
                        this.category,
                        message), textColor);
                }
            }
            else
            {
                    this.WriteToConsole(String.Format(" {0} [{1}] {2,10}: {3}",
                        DateTime.Now.ToLongTimeString(),
                        level,
                        this.category,
                        message
                        ), textColor);
                }
        }

        private void WriteToConsole(string message, ConsoleColor textColor = ConsoleColor.White)
        {
            lock (System.Console.InputEncoding)
            {
                System.Console.ForegroundColor = this.tagColor;
                System.Console.Write("{0,"+this.prefixReservedColumns.ToString()+"}",this.prefix);
                System.Console.ForegroundColor = textColor;

                System.Console.WriteLine(message);

                System.Console.ForegroundColor = this.defaultFgColor;
            }
        }
    }
}
