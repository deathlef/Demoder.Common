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
using System.ComponentModel;
using System.Text;

namespace Demoder.Common.Logging
{
    /// <summary>
    /// Provides a full-featured eventlog
    /// </summary>
    public class EventLog
    {
        #region members
        private List<IEventLogEntry> events = new List<IEventLogEntry>();
        private EventLogRead defaultLimitInclude = EventLogRead.Last;
        private ILogWriter logWriter = null;
        /// <summary>
        /// Store log entries in memory
        /// </summary>
        private bool storeInMemory = true;
        #endregion

        #region constructors
        /// <summary>
        /// Keep log entries in memory, don't write them anywhere.
        /// </summary>
        public EventLog()
        {
            this.logWriter = null;
            this.storeInMemory = true;
        }

        /// <summary>
        /// Pass log entries to the provided LogWriter, don't keep entries in memory
        /// </summary>
        /// <param name="logWriter"></param>
        public EventLog(ILogWriter logWriter) : this(logWriter, false) { }

        /// <summary>
        /// Pass log entries to the provided LogWriter.
        /// </summary>
        /// <param name="logWriter"></param>
        /// <param name="storeInMemory">Should log entries be stored in memory?</param>
        public EventLog(ILogWriter logWriter, bool storeInMemory)
        {
            this.logWriter = logWriter;
            this.storeInMemory = storeInMemory;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="logEntry"></param>
        public void Log(IEventLogEntry logEntry)
        {
            //Store to memory
            if (this.storeInMemory)
                lock (this.events)
                    this.events.Add(logEntry);
            //Pass to writer
            if (this.logWriter != null)
                lock (this.logWriter)
                    this.logWriter.Write(logEntry);
        }

        #region ReadLog
        /// <summary>
        /// Read all log entries.
        /// </summary>
        /// <returns></returns>
        public IEventLogEntry[] ReadLog()
        {
            return this.ReadLog(0, this.defaultLimitInclude);
        }
        /// <summary>
        /// Read a limited number of log entries
        /// </summary>
        /// <param name="numEntries">Number of log entrie to read</param>
        /// <returns></returns>
        public IEventLogEntry[] ReadLog(int numEntries)
        {
            return this.ReadLog(numEntries, this.defaultLimitInclude);
        }

        /// <summary>
        /// Read the [first|last] log entries.
        /// </summary>
        /// <param name="numEntries"></param>
        /// <param name="limitInclude">Should we read the first or the last log entries?</param>
        /// <returns></returns>
        public IEventLogEntry[] ReadLog(int numEntries, EventLogRead limitInclude)
        {
            if (numEntries < 1)
                return this.events.ToArray();

            IEventLogEntry[] returnVal = new IEventLogEntry[numEntries];

            lock (this.events)
            {
                //Ensure we don't read beyond the range of the list
                if (numEntries > this.events.Count)
                    numEntries = this.events.Count;

                switch (limitInclude)
                {
                    case EventLogRead.First:
                        //Get the # first entries
                        this.events.CopyTo(0, returnVal, 0, numEntries);
                        break;
                    case EventLogRead.Last:
                        //Get the # last entries
                        int startIndex = this.events.Count - numEntries - 1;
                        this.events.CopyTo(startIndex, returnVal, 0, numEntries);
                        break;
                }
            }
            return returnVal;
        }
        #endregion ReadLog
        #endregion Methods

        #region Static methods
        /// <summary>
        /// Creates a new log line. Appends \\r\\n to end of message.
        /// </summary>
        /// <param name="time">Time the event occured</param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string CreateLogString(DateTime time, EventLogLevel logLevel, string message)
        {
            return CreateLogString(time, logLevel, message, "\r\n");
        }

        /// <summary>
        /// Creates a new log line.
        /// </summary>
        /// <param name="time">Time the event occured</param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="lineEnd">String to append to the end of line, or null</param>
        /// <returns></returns>
        public static string CreateLogString(DateTime time, EventLogLevel logLevel, string message, string lineEnd)
        {
            if (lineEnd == null)
                lineEnd = string.Empty;

            return String.Format("[{0} {1}] [{2}]: {3}{4}",
                time.ToShortDateString(),
                time.ToShortTimeString(),
                logLevel.ToString(),
                message,
                lineEnd);
        }
        #endregion
    }
}
