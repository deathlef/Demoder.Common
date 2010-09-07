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
using System.ComponentModel;
using System.Text;

namespace Demoder.Common
{
	/// <summary>
	/// Provides a full-featured eventlog
	/// </summary>
	public class EventLog
	{
		#region members
		private List<EventLogEntry> _events = new List<EventLogEntry>();
		private EventLogRead _defaultLimitInclude = EventLogRead.Last;
		#endregion

		#region constructors
		public EventLog()
		{

		}
		#endregion

		#region Methods
		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="LogEntry"></param>
		public void Log(EventLogEntry LogEntry) 
		{
			lock (this._events)
				this._events.Add(LogEntry);
		}
		/// <summary>
		/// Read all log entries.
		/// </summary>
		/// <returns></returns>
		public EventLogEntry[] ReadLog()
		{
			return this.ReadLog(0, this._defaultLimitInclude);
		}
		/// <summary>
		/// Read a limited number of log entries
		/// </summary>
		/// <param name="NumEntries">Number of log entrie to read</param>
		/// <returns></returns>
		public EventLogEntry[] ReadLog(int NumEntries)
		{
			return this.ReadLog(NumEntries, this._defaultLimitInclude);
		}

		/// <summary>
		/// Read the [first|last] log entries.
		/// </summary>
		/// <param name="NumEntries"></param>
		/// <param name="LimitInclude">Should we read the first or the last log entries?</param>
		/// <returns></returns>
		public EventLogEntry[] ReadLog(int NumEntries, EventLogRead LimitInclude)
		{
			if (NumEntries < 1)
				return this._events.ToArray();

			EventLogEntry[] returnVal = new EventLogEntry[NumEntries];
			
			lock (this._events)
			{
				//Ensure we don't read beyond the range of the list
				if (NumEntries > this._events.Count)
					NumEntries = this._events.Count;
				
				switch (LimitInclude)
				{
					case EventLogRead.First:
						//Get the # first entries
						this._events.CopyTo(0, returnVal, 0, NumEntries);
						break;
					case EventLogRead.Last:
						//Get the # last entries
						int startIndex = this._events.Count - NumEntries - 1;
						this._events.CopyTo(startIndex, returnVal, 0, NumEntries);
						break;
				}
			}
			return returnVal;
		}
		#endregion

	}

	/// <summary>
	/// Represents an EventLog entry
	/// </summary>
	public class EventLogEntry
	{
		#region members
		private readonly EventLogLevel _logLevel;
		private readonly DateTime _time;
		private readonly object _source;
		private readonly string _message;
		#endregion
		#region Constructors
		/// <summary>
		/// Create a log entry
		/// </summary>
		/// <param name="LogLevel"></param>
		/// <param name="Source"></param>
		/// <param name="Message"></param>
		public EventLogEntry(EventLogLevel LogLevel, object Source, string Message) : 
			this(LogLevel, Source, Message, DateTime.Now) { }


		/// <summary>
		/// Create a log entry
		/// </summary>
		/// <param name="e"></param>
		public EventLogEntry(EventLogLevel LogLevel, object Source, ProgressChangedEventArgs e) {
			this._logLevel = LogLevel;
			this._source = Source;
			this._time = DateTime.Now;
			
			string message = e.UserState.ToString();
			if (e.ProgressPercentage > 0) 
				message += " (" + e.ProgressPercentage.ToString() + "%)";
			this._message = message;
		}

		/// <summary>
		/// Create a log entry with custom timestamp
		/// </summary>
		/// <param name="LogLevel"></param>
		/// <param name="Source"></param>
		/// <param name="Message"></param>
		/// <param name="Time"></param>
		public EventLogEntry(EventLogLevel LogLevel, object Source, string Message, DateTime Time)
		{
			this._logLevel = LogLevel;
			this._time = Time;
			this._source = Source;
			this._message = Message;
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
		public string Message { get { return this._message; } }
		#endregion

		#region Overrides
		public override string ToString()
		{
			return String.Format("[{0} {1}] [{2}] [{3}] {4}",
				this._time.ToShortDateString(),
				this._time.ToLongTimeString(),
				this._logLevel.ToString(),
				this._source.ToString(),
				this._message);
		}
		#endregion
	}

	public enum EventLogLevel
	{
		Critical = 0x01,
		Serious = 0x02,
		Error = 0x04,
		Warning = 0x08,
		Notice = 0x10,
		Debug = 0x20
	}

	/// <summary>
	/// Which log entries to read?
	/// </summary>
	public enum EventLogRead {
		First,
		Last
	}
}
