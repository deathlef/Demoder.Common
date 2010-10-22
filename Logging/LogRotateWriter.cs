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

/*  Todo
 * WriteLogEntry will add to the write queue.
 * ThreadWriter will write queue to file. Will rotate files according to settings.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Demoder.Common.Logging 
{
	public class LogRotateWriter : ILogWriter, IDisposable
	{
		#region Members
		#region LogFile information
		/// <summary>
		/// Directory to store log files in
		/// </summary>
		private DirectoryInfo _logDirectory;
		/// <summary>
		/// This logs name
		/// </summary>
		private string _logName=null;
		/// <summary>
		/// Characters printed at end of a written log line
		/// </summary>
		private string _lineEnd = "\r\n";

		private FileInfo _logFile
		{
			get
			{
				return new FileInfo(String.Format("{1}{0}{2}",
					Path.DirectorySeparatorChar,
					this._logDirectory,
					this._logName));
			}
		}

		private long _logFileStartSize = 0;

		//Logfile restrictions
		/// <summary>
		/// Logfile size in bytes
		/// </summary>
		private int _logSize = 0;
		/// <summary>
		/// Logfile age in seconds
		/// </summary>
		private int _logAge = 0;
		/// <summary>
		/// Number of uncompressed log iterations to keep. (logname.X)
		/// </summary>
		private int _logIterations = 1;
		/// <summary>
		/// Number of compressed iterations to keep. (logname.X.gz)
		/// </summary>
		private int _logCompressedIterations = 3;
		#endregion
		/// <summary>
		/// Queue containing the to-be-written 
		/// </summary>
		private Queue<string> _messageQueue = new Queue<string>(16);
		private ManualResetEvent _writeMRE = new ManualResetEvent(false);
		
		#endregion Members

		#region Constructors
		public LogRotateWriter()
		{
		}

		#endregion

		/// <summary>
		/// Threaded method to handle writes to the log.
		/// </summary>
		private void writeLog()
		{
			long startSize = this._logFile.Length;
			FileStream fs = this.rotateLog();
				
			while (true)
			{
				this._writeMRE.WaitOne();
				int writtenEntries = 0;
				lock (this._messageQueue)
				{
					//Fetch all log entries & make one string for one big write.
					string message = string.Empty;
					while (this._messageQueue.Count > 0)
					{
						message += this._messageQueue.Dequeue() + this._lineEnd;
						writtenEntries += 1;
					}
					//Write all the entries to the logfile.
					byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
					fs.Write(bytes, 0, bytes.Length);
					this._writeMRE.Reset();
				}
				this.rotateLog();
			}
		}

		/// <summary>
		/// Check if we should rotate the log.
		/// </summary>
		private FileStream rotateLog()
		{
			if (this._logSize != 0 && this._logFileStartSize > this._logSize)
				return this.rotateLog(true);
#warning todo: Implement checking for # of log entries.
			else
			{
				return this.rotateLog(false);
			}
			
		}
		private FileStream rotateLog(bool Rotate)
		{
			if (Rotate)
			{
				//Do stuff to rotate


				
				
			}
			//Return a handle to the new log file
			FileStream fs = File.Open(this._logFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
			fs.Seek(0, SeekOrigin.End);
			return fs;
		}

		#region Interfaces
		#region ILogWriter Members
		public bool WriteLogEntry(string Message)
		{
			lock (this._messageQueue)
				this._messageQueue.Enqueue(Message);
			this._writeMRE.Set();
			return true;
		}
		#endregion
	
		#region IDisposable Member
		public void  Dispose()
		{
 			throw new NotImplementedException();
		}
		#endregion
		#endregion Interfaces
	}
}
