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
    public class RotateLogWriter : ILogWriter, IDisposable
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
        private string _logName = null;
        /// <summary>
        /// Length of logfile.
        /// </summary>
        private FileStream _logStream = null;

        private LogRotater _logRotate;
        #endregion
        private ManualResetEvent _writeMRE = new ManualResetEvent(false);

        #region All members within here are only accessed after locking _messageQueue
        /// <summary>
        /// Queue containing the to-be-written text
        /// </summary>
        private Queue<IEventLogEntry> _messageQueue = new Queue<IEventLogEntry>(64);

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
        /// <summary>
        /// Abort the thread?
        /// </summary>
        private bool _abort = false;
        private bool _disposed = false;
        #endregion Members

        #region Constructors
        public RotateLogWriter(LogRotater LogRotater)
        {
            this._logRotate = LogRotater;

            this._writeTimer = new Timer(new TimerCallback(this.timerTriggerWriterThread), true, Timeout.Infinite, Timeout.Infinite);
            this._writerThread = new Thread(new ThreadStart(this.writeLog));
            this._writerThread.IsBackground = true;
            this._writerThread.Name = "LogRotateWriter: " + LogRotater.LogDir.FullName + "\\" + LogRotater.LogName;
            this._writerThread.Start();
        }

        #endregion

        #region Threaded methods
        /// <summary>
        /// Threaded method to handle writes to the log.
        /// </summary>
        private void writeLog()
        {
            while (!this._abort)
            {
                this._logRotate.Rotate(ref this._logStream);
                this._writeMRE.WaitOne();
                lock (this._messageQueue)
                {
                    //Cycle until we have emptied the queue.
                    while (this._messageQueue.Count > 0)
                    {
                        byte writtenEntries = 0;
                        string message = string.Empty;
                        long maxSize;
                        if (this._logRotate.LogMaxSize > 0)
                            maxSize = this._logRotate.LogMaxSize - this._logStream.Position;
                        else
                            maxSize = long.MaxValue;
                        if (maxSize <= 0)
                            this._logRotate.Rotate(ref this._logStream);
                        //Fetch up to byte.MaxValue logentries & make one string for one big write.
                        while (this._messageQueue.Count > 0 && writtenEntries < byte.MaxValue && message.Length <= maxSize)
                        {
                            IEventLogEntry iele = this._messageQueue.Dequeue();
                            message += EventLog.CreateLogString(iele.TimeStamp, iele.LogLevel, iele.Message);
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
        bool ILogWriter.Write(IEventLogEntry LogEntry)
        {
            if (this._disposed)
                throw new ObjectDisposedException("");
            lock (this._messageQueue)
            {
                this._messageQueue.Enqueue(LogEntry);
                bool setTimer = true;
                //Is this the first bump since last write?
                if (!this._bumped)
                {
                    setTimer = true;
                    this._firstBump = DateTime.Now;
                    this._bumped = true;
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
                    this._abort = true; //Tell writerthread to stop on next loop
                    //Add "we're disposing" to logfile, and trigger the thread immediately.
                    lock (this._messageQueue)
                    {
                        EventLogEntry<string> el = new EventLogEntry<string>(EventLogLevel.Notice, String.Format("{0} {1}: LogRotateWriter: Disposing.", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
                        this._messageQueue.Enqueue(el);
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
                }
            }
        }
        #endregion
        #endregion Interfaces
    }
}
