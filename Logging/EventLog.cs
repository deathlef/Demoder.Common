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
using System.ComponentModel;
using System.Text;

namespace Demoder.Common.Logging
{
	/// <summary>
	/// Provides a full-featured eventlog
	/// </summary>
	public class EventLog<LogEntryType>
	{
		#region members
		private List<LogEntryType> _events = new List<LogEntryType>();
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
		public void Log(LogEntryType LogEntry) 
		{
			lock (this._events)
				this._events.Add(LogEntry);
		}
		#region ReadLog
		/// <summary>
		/// Read all log entries.
		/// </summary>
		/// <returns></returns>
		public LogEntryType[] ReadLog()
		{
			return this.ReadLog(0, this._defaultLimitInclude);
		}
		/// <summary>
		/// Read a limited number of log entries
		/// </summary>
		/// <param name="NumEntries">Number of log entrie to read</param>
		/// <returns></returns>
		public LogEntryType[] ReadLog(int NumEntries)
		{
			return this.ReadLog(NumEntries, this._defaultLimitInclude);
		}

		/// <summary>
		/// Read the [first|last] log entries.
		/// </summary>
		/// <param name="NumEntries"></param>
		/// <param name="LimitInclude">Should we read the first or the last log entries?</param>
		/// <returns></returns>
		public LogEntryType[] ReadLog(int NumEntries, EventLogRead LimitInclude)
		{
			if (NumEntries < 1)
				return this._events.ToArray();

			LogEntryType[] returnVal = new LogEntryType[NumEntries];
			
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
		#endregion

	}
}
