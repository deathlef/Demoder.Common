﻿/*
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
using System.Text;

namespace Demoder.Common.Logging
{
	public interface IEventLogEntry
	{
		/// <summary>
		/// The time the logged event happened
		/// </summary>
		/// <returns></returns>
		DateTime TimeStamp { get; }
		/// <summary>
		/// String representation of event
		/// </summary>
		/// <returns></returns>
		string Message { get; }
		/// <summary>
		/// An object representing the logged event
		/// </summary>
		/// <returns></returns>
		object LoggedObject { get; }
		EventLogLevel LogLevel { get; }
	}
}
