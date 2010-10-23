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
using System.IO.Compression;
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
		
		//Logfile restrictions
		/// <summary>
		/// Logfile size in bytes
		/// </summary>
		private long _logMaxSize = 0;
		/// <summary>
		/// Logfile age in seconds
		/// </summary>
		private TimeSpan _logMaxAge = new TimeSpan(30, 0, 0, 0);
		/// <summary>
		/// Number of uncompressed log iterations to keep. (logname.X)
		/// </summary>
		private byte _logIterationsUncompressed = 1;
		/// <summary>
		/// Number of compressed iterations to keep. (logname.X.gz)
		/// </summary>
		private byte _logIterationsCompressed = 3;
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
			FileStream fs = null;
			this.rotateLog(ref fs);
				
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
				this.rotateLog(ref fs);
			}
		}


		#region rotateLog
		/// <summary>
		/// Check if we should rotate the log.
		/// If FileStream==null, a new FileStream() will be created for the logfile in question.
		/// If the logfile is rotated, the current FileStream will be closed, and a new one will be opened.
		/// </summary>
		/// <param name="FileStream">Filestream used to access the current log</param>
		private void rotateLog(ref FileStream FileStream)
		{
			bool doRotate=false;
			if (this._logMaxSize != 0 && this._logFile.Length > this._logMaxSize)
				doRotate = true;
			else if ((DateTime.UtcNow - this._logFile.CreationTimeUtc) >= this._logMaxAge)
				doRotate = true;
			else
			{
				doRotate = false;
			}

			switch (doRotate)
			{
				case true:
					//We should rotate.
					FileStream = this.rotateLog(doRotate);
					break;
				case false:
					//If FileStream isn't writeable, dispose & set to null, so it will be recreated later.
					if (FileStream != null && !FileStream.CanWrite)
					{
						FileStream.Dispose();
						FileStream = null;
					}
					//We shouldn't rotate. Only reopen FileStream if it's null.
					if (FileStream == null)
						FileStream = this.rotateLog(doRotate);
					break;
			}
		}
		/// <summary>
		/// If Rotate is true, will rotate the log.
		/// Will always return a FileStream handle to the new log.
		/// </summary>
		/// <param name="Rotate"></param>
		/// <returns></returns>
		private FileStream rotateLog(bool Rotate)
		{
			if (Rotate)
			{
				//Do stuff to rotate
				this.rotateLog(this._logFile, 0);
			}
			
			//Return a handle to the new log file
			FileStream fs = File.Open(this._logFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
			fs.Seek(0, SeekOrigin.End);
			return fs;
		}

		private void rotateLog(FileInfo LogFile, uint Iteration)
		{
			FileInfo newLogFile = null;
			FileInfo curLogFile = null;
			//If iteration is 0, current logfile has no suffix.
			if (Iteration==0)
				curLogFile = new FileInfo(LogFile.FullName);
			bool compress = false;
			if ((Iteration < this._logIterationsUncompressed))
			{
				if (Iteration!=0)
					curLogFile = new FileInfo(String.Format("{0}.{1}", LogFile.FullName, Iteration));
				newLogFile = new FileInfo(String.Format("{0}.{1}", LogFile.FullName, Iteration + 1));
				compress = false; //We need to move, but not compress, since we haven't hit the cap of uncompressed files yet.
			}
			else if ((this._logIterationsCompressed > 0) && (Iteration == this._logIterationsUncompressed))
			{
				//We're rotating a log which isn't compressed, but will be.
				if (Iteration != 0)
					curLogFile = new FileInfo(String.Format("{0}.{1}", LogFile.FullName, Iteration));
				newLogFile = new FileInfo(String.Format("{0}.{1}.gz", LogFile.FullName, Iteration + 1));
				compress = true; //We need to move & compress.
			}
			else if ((this._logIterationsCompressed > 0) && ((Iteration - this._logIterationsUncompressed) < this._logIterationsCompressed))
			{
				//We're iterating an already compressed log
				if (Iteration != 0)
					curLogFile = new FileInfo(String.Format("{0}.{1}.gz", LogFile.FullName, Iteration));
				newLogFile = new FileInfo(String.Format("{0}.{1}.gz", LogFile.FullName, Iteration + 1));
				compress = false; //Already compressed
			}
			//Check if the new logfile already exist.
			if (newLogFile.Exists)
				this.rotateLog(LogFile, Iteration + 1);

			//The current iteration > than max iterations. Don't rotate file, but delete it.
			if (Iteration > (this._logIterationsCompressed + this._logIterationsUncompressed))
			{
				curLogFile.Delete();
			}
			else
			{
				if (!compress)
				{
					//We are not compression. Only move file.
					curLogFile.MoveTo(newLogFile.FullName);
				}
				else
				{
					//Write new log file.
					using (GZipStream gzstream = new GZipStream(newLogFile.Create(), CompressionMode.Compress))
					{
						byte[] bytes = File.ReadAllBytes(curLogFile.FullName);
						gzstream.Write(bytes, 0, bytes.Length);
					}
					//Delete current log file.
					curLogFile.Delete();
				}
			}
		}
		#endregion rotateLog

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
