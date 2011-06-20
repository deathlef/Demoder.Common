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
        private const string logDebugTemplate = "@Model.Time [@Model.Level] (@Model.File:@Model.Line/@Model.Method) @Model.Category: @Model.Message";
        private const string logNormalTemplate = "@Model.Time [@Model.Level] @Model.Category: @Model.Message";
        private readonly EventLogLevel minLogLevel;
        private readonly string prefix;

        public Logger(EventLogLevel minLevel = EventLogLevel.Debug, string prefix="")
        {
            this.minLogLevel = minLevel;
            this.prefix = String.Format("{0,10}",prefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="skipFrames">Only relevant for level.Debug|level.Error. How many StackTrace frames to skip? If set to less than 0, will not do stack trace</param>
        public void Log(EventLogLevel level, string category, string message, int skipFrames=1)
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
                doStackTrace=true;
            }
            dynamic log;
            if (doStackTrace)
            {
                // StackTrace output, skip first frame as that's irrelevant
                StackTrace trace = new StackTrace(skipFrames, true);
                StackFrame frame = trace.GetFrame(0);
                log = new
                {
                    Time = DateTime.Now.ToLongTimeString(),
                    Level = level.ToString(),
                    File = new FileInfo(frame.GetFileName()).Name,
                    Line = frame.GetFileLineNumber().ToString(),
                    Method = frame.GetMethod().Name,
                    Category = category,
                    Message = message
                };
            }
            else
            {
                log = new
                {
                    Time = DateTime.Now.ToLongTimeString(),
                    Level = level.ToString(),
                    Category = category,
                    Message = message
                };
            }
            Console.WriteLine(this.prefix + " "+this.razorParse(log, doStackTrace));
        }

        private string razorParse(dynamic model, bool isDebug) {
            if (isDebug)
            {
                return RazorEngine.Razor.Parse(logDebugTemplate, model);
            }
            return RazorEngine.Razor.Parse(logNormalTemplate, model);
        }
    }
}
