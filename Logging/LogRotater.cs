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
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;

namespace Demoder.Common.Logging
{
    /// <summary>
    /// Handles rotating of a single logfile using provided parameters
    /// </summary>
    public class LogRotater : IDisposable
    {
        #region Members
        /// <summary>
        /// Directory to store log files in
        /// </summary>
        private DirectoryInfo logDirectory;
        /// <summary>
        /// This logs name
        /// </summary>
        private string logName = null;

        //Logfile restrictions
        /// <summary>
        /// Logfile size in bytes
        /// </summary>
        private long logMaxSize = 0;
        /// <summary>
        /// Logfile age in seconds
        /// </summary>
        private TimeSpan logMaxAge = new TimeSpan(30, 0, 0, 0);
        /// <summary>
        /// Number of uncompressed log iterations to keep. (logname.X)
        /// </summary>
        private byte logIterationsUncompressed = 1;
        /// <summary>
        /// Number of compressed iterations to keep. (logname.X.gz)
        /// </summary>
        private byte logIterationsCompressed = 3;

        //Misc log settings
        /// <summary>
        /// Are we disposed?
        /// </summary>
        private bool disposed = false;
        #endregion

        #region Public accessors
        /// <summary>
        /// Logfiles are in this directory
        /// </summary>
        public DirectoryInfo LogDir
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("");
                return this.logDirectory;
            }
        }
        /// <summary>
        /// Name of the log we're rotating
        /// </summary>
        public string LogName
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("");
                return this.logName;
            }
        }
        /// <summary>
        /// Maximum size of log in bytes
        /// </summary>
        public long LogMaxSize
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("");
                return this.logMaxSize;
            }
        }
        /// <summary>
        /// Maximum size of log in bytes
        /// </summary>
        public TimeSpan LogMaxAge
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("");
                return this.logMaxAge;
            }
        }

        /// <summary>
        /// Represents the logfile this instance is set up to rotate.
        /// </summary>
        public FileInfo LogFile
        {
            get
            {
                if (this.disposed)
                    throw new ObjectDisposedException("");
                return new FileInfo(Path.Combine(this.logDirectory.FullName, this.logName));
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <![CDATA[Handles rotating of a single logfile using provided parameters
        /// Initializes with default limits.
        /// MaxSize: 10KiB
        /// MagAge: 7 days
        /// UncompressedIterations: 1
        /// CompressedIterations: 3]]>
        /// </summary>
        /// <param name="logDir">Directory to store logs in</param>
        /// <param name="logName">Name of this log file</param>
        public LogRotater(DirectoryInfo logDir, string logName) : this(logDir, logName, 10240, new TimeSpan(0), 1, 3) { }
        /// <summary>
        /// <![CDATA[Handles rotating of a single logfile using provided parameters
        /// Initializes with default limits. No rotation by age.
        /// MaxLogSize: 10KiB
        /// UncompressedIterations: 1
        /// CompressedIterations: 3]]>
        /// </summary>
        /// <param name="logDir">Directory to store logs in</param>
        /// <param name="logName">Name of this log file</param>
        /// <param name="maxSize">Custom maxmimum log size in bytes</param>
        /// <param name="maxAge">Custom maximum log age</param>
        public LogRotater(DirectoryInfo logDir, string logName, long maxSize, TimeSpan maxAge) : this(logDir, logName, maxSize, maxAge, 1, 3) { }
        /// <summary>
        /// <![CDATA[Handles rotating of a single logfile using provided parameters
        /// Initialized with default limits. No rotation by age.
        /// UncompressedIterations: 1
        /// CompressedIterations: 3]]>
        /// </summary>
        /// <param name="logDir">Directory to store logs in</param>
        /// <param name="logName">Name of this log file</param>
        /// <param name="MaxSize"></param>
        public LogRotater(DirectoryInfo logDir, string logName, long MaxSize) : this(logDir, logName, MaxSize, new TimeSpan(0), 1, 3) { }
        /// <summary>
        /// Handles rotating of a single logfile using provided parameters
        /// </summary>
        /// <param name="logDir">Directory to store logs in</param>
        /// <param name="logName">Name of this log file</param>
        /// <param name="maxSize">Maximum size of logfile in bytes before it's rotated. Set to 0 to disable</param>
        /// <param name="maxAge">Maximum age of logfile before it's rotated. Set to 0 ticks to disable</param>
        /// <param name="uncompressedIterations">Maximum number of uncompressed rotations of the logfile to keep</param>
        /// <param name="compressedIterations">Maximum number of compressed rotations of the logfile to keep</param>
        public LogRotater(DirectoryInfo logDir, string logName, long maxSize, TimeSpan maxAge, byte uncompressedIterations, byte compressedIterations)
        {
            this.logDirectory = logDir;
            this.logName = logName;
            this.logMaxSize = maxSize;
            this.logMaxAge = maxAge;

            this.logIterationsUncompressed = uncompressedIterations;
            this.logIterationsCompressed = compressedIterations;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Rotates the predefined log if necessary. Use LogStream as reference to current log.
        /// </summary>
        /// <param name="logStream">Referenec to current log</param>
        public void Rotate(ref FileStream logStream)
        {
            if (this.disposed)
                throw new ObjectDisposedException("");
            lock (this)
            {
                bool doRotate = false;
                if (logStream == null)
                    doRotate = false;
                else if (this.logMaxSize != 0 && logStream.Position > this.logMaxSize)
                    doRotate = true;
                else if ((this.logMaxAge.TotalSeconds != 0) && ((DateTime.UtcNow - this.LogFile.CreationTimeUtc) >= this.logMaxAge))
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
                        logStream.Dispose();
                        logStream = this.rotateLog(doRotate);
                        break;
                    case false:
                        //If FileStream isn't writeable, dispose & set to null, so it will be recreated later.
                        if (logStream != null && !logStream.CanWrite)
                        {
                            logStream.Dispose();
                            logStream = null;
                        }
                        //We shouldn't rotate. Only reopen FileStream if it's null.
                        if (logStream == null)
                            logStream = this.rotateLog(doRotate);
                        break;
                }
            }
        }
        #endregion

        #region Private methods

        /// <summary>
        /// If Rotate is true, will rotate the log.
        /// Will always return a FileStream handle to the new log.
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        private FileStream rotateLog(bool rotate)
        {
            if (rotate)
            {
                //Do stuff to rotate
                this.rotateLog(this.LogFile, 0);
            }

            //Return a handle to the new log file
            FileStream fs = null;
            try
            {
                fs = File.Open(this.LogFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
                fs.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (rotate && fs != null)
            {
                //Add notice to the new file that we rotated the file
                byte[] bytes = ASCIIEncoding.ASCII.GetBytes(EventLog.CreateLogString(DateTime.Now, EventLogLevel.Notice, "LogRotateWriter: Log was rotated."));
                fs.Write(bytes, 0, bytes.Length);
            }
            return fs;
        }

        private void rotateLog(FileInfo logFile, uint iteration)
        {
            FileInfo newLogFile = null;
            FileInfo curLogFile = null;
            bool compress = false;

            //If iteration is 0, current logfile has no suffix.
            if (iteration == 0)
                curLogFile = new FileInfo(logFile.FullName);

            if ((iteration < this.logIterationsUncompressed))
            {
                if (iteration != 0)
                    curLogFile = new FileInfo(String.Format("{0}.{1}", logFile.FullName, iteration));
                newLogFile = new FileInfo(String.Format("{0}.{1}", logFile.FullName, iteration + 1));
                compress = false; //We need to move, but not compress, since we haven't hit the cap of uncompressed files yet.
            }
            else if ((this.logIterationsCompressed > 0) && (iteration == this.logIterationsUncompressed))
            {
                //We're rotating a log which isn't compressed, but will be.
                if (iteration != 0)
                    curLogFile = new FileInfo(String.Format("{0}.{1}", logFile.FullName, iteration));
                newLogFile = new FileInfo(String.Format("{0}.{1}.gz", logFile.FullName, iteration + 1));
                compress = true; //We need to move & compress.
            }
            else if ((this.logIterationsCompressed > 0) && ((iteration - this.logIterationsUncompressed) <= this.logIterationsCompressed))
            {
                //We're iterating an already compressed log
                if (iteration != 0)
                    curLogFile = new FileInfo(String.Format("{0}.{1}.gz", logFile.FullName, iteration));
                newLogFile = new FileInfo(String.Format("{0}.{1}.gz", logFile.FullName, iteration + 1));
                compress = false; //Already compressed
            }
            else
                return;
            //Check if the new logfile already exist.
            if (newLogFile != null && newLogFile.Exists)
                this.rotateLog(logFile, iteration + 1);

            //The current iteration > than max iterations. Don't rotate file, but delete it.
            if (iteration >= (this.logIterationsCompressed + this.logIterationsUncompressed))
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
        #endregion

        #region Interfaces
        #region IDisposable Members
        void IDisposable.Dispose()
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.logName = null;
                    this.logDirectory = null;
                }
            }
        }
        #endregion
        #endregion
    }
}
