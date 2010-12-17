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
		private List<IEventLogEntry> _events = new List<IEventLogEntry>();
		private EventLogRead _defaultLimitInclude = EventLogRead.Last;
		private ILogWriter _logWriter = null;
		/// <summary>
		/// Store log entries in memory
		/// </summary>
		private bool _storeInMemory = true;
		#endregion

		#region constructors
		/// <summary>
		/// Keep log entries in memory, don't write them anywhere.
		/// </summary>
		public EventLog()
		{
			this._logWriter = null;
			this._storeInMemory = true;
		}

		/// <summary>
		/// Pass log entries to the provided LogWriter, don't keep entries in memory
		/// </summary>
		/// <param name="LogWriter"></param>
		public EventLog(ILogWriter LogWriter) : this(LogWriter, false) { }

		/// <summary>
		/// Pass log entries to the provided LogWriter.
		/// </summary>
		/// <param name="LogWriter"></param>
		/// <param name="StoreInMemory">Should log entries be stored in memory?</param>
		public EventLog(ILogWriter LogWriter, bool StoreInMemory)
		{
			this._logWriter = LogWriter;
			this._storeInMemory = StoreInMemory;
		}
		#endregion
		#region Methods
		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="LogEntry"></param>
		public void Log(IEventLogEntry LogEntry)
		{
			//Store to memory
			if (this._storeInMemory)
				lock (this._events)
					this._events.Add(LogEntry);
			//Pass to writer
			if (this._logWriter != null)
				lock (this._logWriter)
					this._logWriter.Write(LogEntry);
		}

		#region ReadLog
		/// <summary>
		/// Read all log entries.
		/// </summary>
		/// <returns></returns>
		public IEventLogEntry[] ReadLog()
		{
			return this.ReadLog(0, this._defaultLimitInclude);
		}
		/// <summary>
		/// Read a limited number of log entries
		/// </summary>
		/// <param name="NumEntries">Number of log entrie to read</param>
		/// <returns></returns>
		public IEventLogEntry[] ReadLog(int NumEntries)
		{
			return this.ReadLog(NumEntries, this._defaultLimitInclude);
		}

		/// <summary>
		/// Read the [first|last] log entries.
		/// </summary>
		/// <param name="NumEntries"></param>
		/// <param name="LimitInclude">Should we read the first or the last log entries?</param>
		/// <returns></returns>
		public IEventLogEntry[] ReadLog(int NumEntries, EventLogRead LimitInclude)
		{
			if (NumEntries < 1)
				return this._events.ToArray();

			IEventLogEntry[] returnVal = new IEventLogEntry[NumEntries];
			
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
		#endregion ReadLog
		#endregion Methods

		#region Static methods
		/// <summary>
		/// Creates a new log line. Appends \\r\\n to end of message.
		/// </summary>
		/// <param name="Time">Time the event occured</param>
		/// <param name="LogLevel"></param>
		/// <param name="Message"></param>
		/// <returns></returns>
		public static string CreateLogString(DateTime Time, EventLogLevel LogLevel, string Message)
		{
			return CreateLogString(Time, LogLevel, Message, "\r\n");
		}

		/// <summary>
		/// Creates a new log line.
		/// </summary>
		/// <param name="Time">Time the event occured</param>
		/// <param name="LogLevel"></param>
		/// <param name="Message"></param>
		/// <param name="LineEnd">String to append to the end of line, or null</param>
		/// <returns></returns>
		public static string CreateLogString(DateTime Time, EventLogLevel LogLevel, string Message, string LineEnd)
		{
			if (LineEnd == null)
				LineEnd = string.Empty;

			return String.Format("[{0} {1}] [{2}]: {3}{4}",
				Time.ToShortDateString(),
				Time.ToShortTimeString(),
				LogLevel.ToString(),
				Message,
				LineEnd);
		}
		#endregion
	}
}
