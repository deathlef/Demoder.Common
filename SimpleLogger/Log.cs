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
using System.IO;

namespace Demoder.Common.SimpleLogger
{
    public class Log
    {
        private readonly EventLogLevel minLogLevel;
        private readonly string prefix;
        private readonly ConsoleColor tagColor;
        private readonly TextWriter logWriter;
        private readonly int prefixReservedColumns;

        private Dictionary<string, Logger> loggers = new Dictionary<string, Logger>(StringComparer.InvariantCultureIgnoreCase);

        public Log(EventLogLevel minLevel = EventLogLevel.Debug, string prefix="", ConsoleColor tagColor = ConsoleColor.White, int prefixReservedColumns=20, TextWriter logWriter=null)
        {
            this.minLogLevel = minLevel;
            this.prefix = String.Format("{0,10}",prefix);
            this.tagColor = tagColor;
            this.prefixReservedColumns = prefixReservedColumns; 
            this.logWriter = logWriter;
            
        }

        public Logger this[string category]
        {
            get
            {
                lock (this.loggers)
                {
                    if (!this.loggers.ContainsKey(category))
                    {
                        this.loggers.Add(category, 
                            new Logger(
                                category, 
                                this.minLogLevel, 
                                this.prefix, 
                                this.tagColor, 
                                this.prefixReservedColumns,
                                this.logWriter));
                    }
                    return this.loggers[category];
                }
            }
        }
    }
}
