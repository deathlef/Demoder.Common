/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://trac.flw.nu/demoder.common/)

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
	public class EventLogEntry<LogItemType>
	{
		#region members
		private readonly EventLogLevel _logLevel;
		private readonly DateTime _time;
		private readonly object _source;
		private readonly LogItemType _logitem;
		#endregion
		#region Constructors
		/// <summary>
		/// Create a log entry
		/// </summary>
		/// <param name="LogLevel"></param>
		/// <param name="Source"></param>
		/// <param name="Message"></param>
		public EventLogEntry(EventLogLevel LogLevel, object Source, LogItemType LogItem) :
			this(LogLevel, Source, LogItem, DateTime.Now) { }

		/// <summary>
		/// Create a log entry with custom timestamp
		/// </summary>
		/// <param name="LogLevel"></param>
		/// <param name="Source"></param>
		/// <param name="Message"></param>
		/// <param name="Time"></param>
		public EventLogEntry(EventLogLevel LogLevel, object Source, LogItemType LogItem, DateTime Time)
		{
			this._logLevel = LogLevel;
			this._time = Time;
			this._source = Source;
			this._logitem = LogItem;
		}

		#endregion

		#region Public accessors
		public EventLogLevel LogLevel { get { return this._logLevel; } }
		/// <summary>
		/// When did the event happen?
		/// </summary>
		public DateTime Time { get { return this._time; } }
		/// <summary>
		/// How long ago did the event happen?
		/// </summary>
		public TimeSpan TimeSpan
		{
			get
			{
				return (DateTime.Now - this._time);
			}
		}
		/// <summary>
		/// When did the event happen? (UNIX timestamp)
		/// </summary>
		public long TimeStamp { get { return Misc.Unixtime(this._time); } }
		/// <summary>
		/// The logged message
		/// </summary>
		public LogItemType LogItem { get { return this._logitem; } }
		#endregion

		#region Overrides
		public override string ToString()
		{
			return String.Format("[{0} {1}] [{2}] [{3}] {4}",
				this._time.ToShortDateString(),
				this._time.ToLongTimeString(),
				this._logLevel.ToString(),
				this._source.ToString(),
				this._logitem);
		}
		#endregion
	}
}
