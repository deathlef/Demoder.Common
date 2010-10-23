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
		/// <summary>
		/// Length of logfile.
		/// </summary>
		private FileStream _logStream = null;

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
		private ManualResetEvent _writeMRE = new ManualResetEvent(false);

		#region All members within here are only accessed after locking _messageQueue
		/// <summary>
		/// Queue containing the to-be-written text
		/// </summary>
		private Queue<string> _messageQueue = new Queue<string>(64);
		
		/// <summary>
		/// Timer which should trigger the _writeMRE to make the threaded writer start writing.
		/// </summary>
		private Timer _writeTimer = null;
		/// <summary>
		/// How many times have the timer been bumped?
		/// </summary>
		private DateTime _firstBump = DateTime.Now;
		private bool _bumped = false;
		#endregion
		
		private Thread _writerThread;
		private bool _stopThread = false;
		private bool _disposed = false;
		#endregion Members

		#region Constructors
		/// <summary>
		/// Initializes with default limits.
		/// MaxSize: 10KiB
		/// MagAge: 7 days
		/// UncompressedIterations: 1
		/// CompressedIterations: 3
		/// </summary>
		/// <param name="LogDir">Directory to store logs in</param>
		/// <param name="LogName">Name of this log file</param>
		public LogRotateWriter(DirectoryInfo LogDir, string LogName) : this(LogDir, LogName,10240, new TimeSpan(0), 1, 3) { }
		/// <summary>
		/// Initializes with default limits. No rotation by age.
		/// MaxLogSize: 10KiB
		/// UncompressedIterations: 1
		/// CompressedIterations: 3
		/// </summary>
		/// <param name="LogDir">Directory to store logs in</param>
		/// <param name="LogName">Name of this log file</param>
		/// <param name="MaxSize">Custom maxmimum log size in bytes</param>
		/// <param name="MaxAge">Custom maximum log age</param>
		public LogRotateWriter(DirectoryInfo LogDir, string LogName, long MaxSize, TimeSpan MaxAge) : this(LogDir, LogName, MaxSize, MaxAge, 1, 3) { }
		/// <summary>
		/// Initialized with default limits. No rotation by age.
		/// UncompressedIterations: 1
		/// CompressedIterations: 3
		/// </summary>
		/// <param name="LogDir">Directory to store logs in</param>
		/// <param name="LogName">Name of this log file</param>
		/// <param name="MaxSize"></param>
		public LogRotateWriter(DirectoryInfo LogDir, string LogName, long MaxSize) : this(LogDir, LogName, MaxSize, new TimeSpan(0), 1, 3) { }

		
		/// <summary>
		/// Initializes with provided limits
		/// </summary>
		/// <param name="LogDir">Directory to store logs in</param>
		/// <param name="LogName">Name of this log file</param>
		/// <param name="MaxSize">Maximum size of logfile in bytes before it's rotated. Set to 0 to disable</param>
		/// <param name="MaxAge">Maximum age of logfile before it's rotated. Set to 0 ticks to disable</param>
		/// <param name="UncompressedIterations">Maximum number of uncompressed rotations of the logfile to keep</param>
		/// <param name="CompressedIterations">Maximum number of compressed rotations of the logfile to keep</param>
		public LogRotateWriter(DirectoryInfo LogDir, string LogName, long MaxSize, TimeSpan MaxAge, byte UncompressedIterations, byte CompressedIterations )
		{
			this._logDirectory = LogDir;
			this._logName = LogName;
			this._logMaxSize = MaxSize;
			this._logMaxAge = MaxAge;

			this._logIterationsUncompressed = UncompressedIterations;
			this._logIterationsCompressed = CompressedIterations;

			this._writeTimer = new Timer(new TimerCallback(this.timerTriggerWriterThread), true, Timeout.Infinite, Timeout.Infinite);
			this._writerThread = new Thread(new ThreadStart(this.writeLog));
			this._writerThread.IsBackground = true;
			this._writerThread.Name = "LogRotateWriter: " + LogDir.FullName + "\\"+LogName;
			this._writerThread.Start();
		}

		#endregion
		
		#region Threaded methods
		/// <summary>
		/// Threaded method to handle writes to the log.
		/// </summary>
		private void writeLog()
		{
			while (!this._stopThread)
			{
				this.rotateLog();
				this._writeMRE.WaitOne();
				lock (this._messageQueue)
				{
					//Cycle until we have emptied the queue.
					while (this._messageQueue.Count > 0)
					{
						byte writtenEntries = 0;
						string message = string.Empty;
						long maxSize;
						if (this._logMaxSize > 0)
							maxSize = this._logMaxSize - this._logStream.Position;
						else
							maxSize = long.MaxValue;
						if (maxSize <= 0)
							this.rotateLog();
						//Fetch up to byte.MaxValue logentries & make one string for one big write.
						while (this._messageQueue.Count > 0 && writtenEntries < byte.MaxValue && message.Length <= maxSize)
						{
							message += this._messageQueue.Dequeue() + this._lineEnd;
							writtenEntries++;
						}
						//Write all the entries to the logfile.
						byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
						this._logStream.Write(bytes, 0, bytes.Length);
					}
					//Reset the timer
					this._bumped = false;
					this._writeMRE.Reset();
					this._writeTimer.Change(Timeout.Infinite, Timeout.Infinite);
				}
			}
		}
		#endregion
		
		#region rotateLog
		/// <summary>
		/// Check if we should rotate the log.
		/// If FileStream==null, a new FileStream() will be created for the logfile in question.
		/// If the logfile is rotated, the current FileStream will be closed, and a new one will be opened.
		/// </summary>
		/// <param name="FileStream">Filestream used to access the current log</param>
		private void rotateLog()
		{
			bool doRotate=false;
			if (this._logStream == null)
				doRotate = false;
			else if (this._logMaxSize != 0 && this._logStream.Position > this._logMaxSize)
				doRotate = true;
			else if ((this._logMaxAge.TotalSeconds != 0) && ((DateTime.UtcNow - this._logFile.CreationTimeUtc) >= this._logMaxAge))
				//If MaxAge is enabled
				doRotate = true;
			else
			{
				doRotate = false;
			}

			switch (doRotate)
			{
				case true:
					//We should rotate.
					this._logStream.Dispose();
					this._logStream = this.rotateLog(doRotate);
					break;
				case false:
					//If FileStream isn't writeable, dispose & set to null, so it will be recreated later.
					if (this._logStream != null && !this._logStream.CanWrite)
					{
						this._logStream.Dispose();
						this._logStream = null;
					}
					//We shouldn't rotate. Only reopen FileStream if it's null.
					if (this._logStream == null)
						this._logStream = this.rotateLog(doRotate);
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
			FileStream fs = null;
			try
			{
				fs = File.Open(this._logFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
				fs.Seek(0, SeekOrigin.End);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			
			if (Rotate && fs!=null)
			{
				//Add notice to the new file that we rotated the file
				byte[] bytes=ASCIIEncoding.ASCII.GetBytes(String.Format("{0} {1}: LogRotateWriter: Log was rotated.{2}",
					DateTime.Now.ToShortDateString(),
					DateTime.Now.ToShortTimeString(),
					this._lineEnd));
				fs.Write(bytes,0, bytes.Length);
			}
			return fs;
		}

		private void rotateLog(FileInfo LogFile, uint Iteration)
		{
			FileInfo newLogFile = null;
			FileInfo curLogFile = null;
			bool compress = false;

			//If iteration is 0, current logfile has no suffix.
			if (Iteration==0)
				curLogFile = new FileInfo(LogFile.FullName);
			
			if ((Iteration < this._logIterationsUncompressed))
			{
				if (Iteration != 0)
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
			else if ((this._logIterationsCompressed > 0) && ((Iteration - this._logIterationsUncompressed) <= this._logIterationsCompressed))
			{
				//We're iterating an already compressed log
				if (Iteration != 0)
					curLogFile = new FileInfo(String.Format("{0}.{1}.gz", LogFile.FullName, Iteration));
				newLogFile = new FileInfo(String.Format("{0}.{1}.gz", LogFile.FullName, Iteration + 1));
				compress = false; //Already compressed
			}
			else 
				return;
			//Check if the new logfile already exist.
			if (newLogFile!=null && newLogFile.Exists)
				this.rotateLog(LogFile, Iteration + 1);

			//The current iteration > than max iterations. Don't rotate file, but delete it.
			if (Iteration >= (this._logIterationsCompressed + this._logIterationsUncompressed))
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

		/// <summary>
		/// Timed delegate for setting the writer MRE.
		/// </summary>
		/// <param name="Obj"></param>
		private void timerTriggerWriterThread(object Obj)
		{
			this._writeMRE.Set();
		}

		#region Interfaces
		#region ILogWriter Members
		bool ILogWriter.WriteLogEntry(string Message)
		{
			if (this._disposed)
				throw new ObjectDisposedException("This instance has either already been disposed, or is in the progress of disposing.");
			lock (this._messageQueue)
			{
				this._messageQueue.Enqueue(Message);
				bool setTimer = true;
				//Is this the first bump since last write?
				if (!this._bumped)
				{
					setTimer = true;
					this._firstBump = DateTime.Now;
					this._bumped=true;
				}
				#warning This hardcoded value should be configurable
				//If we haven't written anything for 20s, don't postpone timer any more
				else if ((DateTime.Now - this._firstBump).TotalSeconds >= 20)
					setTimer = false;
				//Otherwise, postpone timer.
				else
					setTimer = true;
				if (setTimer)
				{
					#warning This hardcoded value should be configurable
					int timerTime;
					if (this._messageQueue.Count >= 64)
						timerTime = 0;
					else
						timerTime = 2000;
					this._writeTimer.Change(timerTime, Timeout.Infinite);
				}
			}
			return true;
		}
		#endregion
	
		#region IDisposable Member
		void IDisposable.Dispose()
		{
			lock (this)
			{
				if (!this._disposed)
				{
					this._disposed = true; //Set disposed flag
					this._stopThread = true; //Tell writerthread to stop on next loop
					//Add "we're disposing" to logfile, and trigger the thread immediately.
					lock (this._messageQueue)
					{
						this._messageQueue.Enqueue(String.Format("{0} {1}: LogRotateWriter: Disposing.", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
						this._writeTimer.Change(0, Timeout.Infinite);
					}
					this._writerThread.Join(2500); //Wait max 2.5s for thread to exit.
					if (this._writerThread.IsAlive)
					{
						throw new Exception("Failed to terminate writer thread within the defined timeframe");
					}
					this._writeTimer = null;
					this._writerThread = null;
					this._writeMRE = null;
					this._messageQueue = null;
					this._logName = null;
					this._logDirectory = null;
					this._lineEnd = null;
				}
			}
		}
		#endregion
		#endregion Interfaces
	}
}
