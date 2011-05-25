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
using System.Text;

namespace Demoder.Common.Logging
{
    /// <summary>
    /// Represents an EventLog entry
    /// </summary>
    public class EventLogEntry<LogItemType> : IEventLogEntry
    {
        #region members
        private readonly EventLogLevel logLevel;
        private readonly DateTime time;
        private readonly LogItemType logitem;
        #endregion
        #region Constructors
        /// <summary>
        /// Create a log entry
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="Source"></param>
        /// <param name="Message"></param>
        public EventLogEntry(EventLogLevel logLevel, LogItemType logItem) :
            this(logLevel, logItem, DateTime.Now) { }

        /// <summary>
        /// Create a log entry with custom timestamp
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="Source"></param>
        /// <param name="Message"></param>
        /// <param name="time"></param>
        public EventLogEntry(EventLogLevel logLevel, LogItemType logItem, DateTime time)
        {
            this.logLevel = logLevel;
            this.time = time;
            this.logitem = logItem;
        }

        #endregion

        #region Public accessors
        /// <summary>
        /// How long ago did the event happen?
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return (DateTime.Now - this.time);
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return String.Format("[{0} {1}] [{2}] {3}",
                this.time.ToShortDateString(),
                this.time.ToLongTimeString(),
                this.logLevel.ToString(),
                this.logitem);
        }
        #endregion

        #region IEventLogEntry Members

        DateTime IEventLogEntry.TimeStamp
        {
            get { return this.time; }
        }

        string IEventLogEntry.Message
        {
            get { return this.logitem.ToString(); }
        }
        EventLogLevel IEventLogEntry.LogLevel
        {
            get { return this.logLevel; }
        }
        object IEventLogEntry.LoggedObject
        {
            get { return this.logitem; }
        }
        #endregion
    }
}
