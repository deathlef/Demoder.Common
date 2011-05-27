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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Demoder.Common.Hash;

namespace Demoder.Common.Net
{
    /// <summary>
    /// This class represents a single download.
    /// </summary>
    public class DownloadItem : IDownloadItem
    {
        #region Members
        //Describing the download task
        private readonly object tag;
        private readonly MD5Checksum expectedMD5;
        private Queue<Uri> mirrors;
        private List<object> mirrorTags = new List<object>();

        private List<Uri> failedMirrors;
        private ManualResetEvent downloadMre = new ManualResetEvent(false);

        //Describing the download data
        private byte[] bytes = null;
        private object httpStatusCode = null;
        private MD5Checksum downloadedMD5 = null;
        private FileInfo saveAs = null;
        //Delegates
        /// <summary>
        /// Signaled when the download succeeds.
        /// </summary>
        private readonly DownloadItemEventHandler downloadSuccessDelegate;
        /// <summary>
        /// Signalted when the download fails.
        /// </summary>
        private readonly DownloadItemEventHandler downloadFailureDelegate;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize a DownloadItem
        /// </summary>
        /// <param name="tag">Userdefined tag of this download</param>
        /// <param name="uri">Uri to download from</param>
        public DownloadItem(object tag,
            Uri uri,
            DownloadItemEventHandler downloadSuccessDelegate,
            DownloadItemEventHandler downloadFailureDelegate) :
            this(tag, new List<Uri>(new Uri[] { uri }), downloadSuccessDelegate, downloadFailureDelegate) { }

        /// <summary>
        /// Initialize a DownloadItem
        /// </summary>
        /// <param name="tag">Uerdefined tag for this download</param>
        /// <param name="mirrors">URIs to download from</param>
        public DownloadItem(
            object tag,
            IEnumerable<Uri> mirrors,
            DownloadItemEventHandler downloadSuccessDelegate,
            DownloadItemEventHandler downloadFailureDelegate) :
            this(tag, mirrors, downloadSuccessDelegate, downloadFailureDelegate, null) { }


        public DownloadItem(object tag,
            IEnumerable<Uri> mirrors,
            DownloadItemEventHandler downloadSuccessDelegate,
            DownloadItemEventHandler downloadFailureDelegate,
            MD5Checksum ExcpectedMD5,
            FileInfo SaveAs)
            : this(tag, mirrors, downloadSuccessDelegate, downloadFailureDelegate, ExcpectedMD5)
        {
            this.saveAs = SaveAs;
        }

        /// <summary>
        /// Initialize a DownloadItem
        /// </summary>
        /// <param name="Tag">Userdefined tag for this download</param>
        /// <param name="mirrors">URIs to download from</param>
        /// <param name="ExpectedMD5">The file should have this MD5 hash to be considered a successfull download</param>
        public DownloadItem(
            object tag,
            IEnumerable<Uri> mirrors,
            DownloadItemEventHandler downloadSuccessDelegate,
            DownloadItemEventHandler downloadFailureDelegate,
            MD5Checksum ExpectedMD5)
        {
            this.tag = Tag;
            this.mirrors = new Queue<Uri>(mirrors);
            this.failedMirrors = new List<Uri>(this.mirrors.Count);
            this.downloadSuccessDelegate = downloadSuccessDelegate;
            this.downloadFailureDelegate = downloadFailureDelegate;
            this.expectedMD5 = ExpectedMD5;
        }
        #endregion

        #region Public accessors
        /// <summary>
        /// Is the downloaded datas integrity OK?
        /// </summary>
        public bool IntegrityOK
        {
            get
            {
                if (this.bytes == null)
                    return false;
                lock (this.downloadedMD5)
                {
                    if (this.expectedMD5 == null) //Since we have data, and no expected MD5, assume the download manager verified the server-reported MD5.
                        return true;
                    if (this.expectedMD5 == this.downloadedMD5) //Integrity ok
                        return true;
                }
                return false; //All other scenarios, download integrity is not ok.
            }
        }

        /// <summary>
        /// Retrieves the datas actual MD5 checksum
        /// </summary>
        public MD5Checksum MD5 { get { return this.downloadedMD5; } }
        /// <summary>
        /// Retrieves the wanted MD5 checksum
        /// </summary>
        public MD5Checksum WantedMD5 { get { return this.expectedMD5; } }

        /// <summary>
        /// The downloaded data
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (this.bytes == null && this.saveAs != null)
                {
                    //Get the binary data from file, cache it in this object, then return it.
                    try
                    {
                        this.storeBytes(File.ReadAllBytes(this.saveAs.FullName));
                    }
                    catch { }
                    return this.bytes;
                }
                return this.bytes;
            }
        }
        public FileInfo SaveAs { get { return this.saveAs; } }

        /// <summary>
        /// Userdefined tag
        /// </summary>
        public object Tag { get { return this.tag; } }
        /// <summary>
        /// Retrieve a list of failed mirrors.
        /// </summary>
        public Uri[] FailedMirrors { get { return this.failedMirrors.ToArray(); } }

        /// <summary>
        /// Retrieve the top mirror from the queue.
        /// </summary>
        public Uri Mirror
        {
            get
            {
                lock (this)
                {
                    if (this.mirrors.Count == 0)
                    {
                        return null;
                    }
                    else
                        return this.mirrors.Peek();
                }
            }
        }

        /// <summary>
        /// A list of tags for this mirror. Will be reset when cycling mirrors.
        /// </summary>
        public List<object> MirrorTags { get { return this.mirrorTags; } }

        /// <summary>
        /// Retrieves scheme://host:port representing the top mirror.
        /// </summary>
        public string MirrorConnectionString
        {
            get
            {
                Uri nextMirror = this.Mirror;
                if (nextMirror != null)
                {
                    return String.Format("{0}://{1}:{2}",
                        nextMirror.Scheme,
                        nextMirror.Host,
                        nextMirror.Port);
                }
                else
                {
                    return "Null";
                }
            }
        }
        #endregion

        #region Private Methods
        private void storeBytes(byte[] bytes)
        {
            this.bytes = bytes;
            this.downloadedMD5 = MD5Checksum.Generate(bytes);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// This method will call the DownloadSuccessDelegate.
        /// </summary>
        public bool SuccessfullDownload(byte[] bytes)
        {
            lock (this)
            {
                this.storeBytes(bytes);
                if (this.IntegrityOK)
                {
                    if (this.downloadSuccessDelegate != null)
                        lock (this.downloadSuccessDelegate)
                            this.downloadSuccessDelegate(this);
                    this.downloadMre.Set();
                }
                return (this.IntegrityOK);
            }
        }

        /// <summary>
        /// Download failed. Move the mirror to the failed queue. If there are no more mirrors, call the DownloadFailed delegate and return false.
        /// </summary>
        /// <param name="failMirror">Should we mark the mirror as failed</param>
        /// <returns>true if we have more mirrors in queue, false otherwise.</returns>
        public bool FailedDownload(bool failMirror)
        {
            lock (this)
            {
                if (failMirror)
                {
                    Uri uri = this.mirrors.Dequeue();
                    this.failedMirrors.Add(uri);
                    //Clear mirror tags for previous mirror.
                    this.mirrorTags.Clear();
                }
                if (this.mirrors.Count == 0)
                {
                    DownloadItemEventHandler dieh = null;
                    if (this.downloadFailureDelegate != null)
                        lock (this.downloadFailureDelegate)
                            dieh = this.downloadFailureDelegate;
                    if (dieh != null)
                        dieh(this);
                    this.downloadMre.Set();
                    return false;
                }
            }
            return true;

        }
        /// <summary>
        /// Wait for the download to finish
        /// </summary>
        public void Wait()
        {
            this.downloadMre.WaitOne();
        }
        /// <summary>
        /// Wait for download to finish.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        public void Wait(int timeout)
        {
            this.downloadMre.WaitOne(timeout);
        }
        #endregion Public Methods

        #region Overrides
        public override string ToString()
        {
            int bytecount = 0;
            if (this.bytes != null)
                bytecount = this.bytes.Length;

            List<string> mirrors = new List<string>();
            foreach (Uri uri in this.mirrors)
                mirrors.Add(uri.ToString());
            foreach (Uri uri in this.failedMirrors)
                mirrors.Add(uri.ToString());
            return String.Format("bytes: {0}, md5: {1} / {2}, mirrors: {3}",
                bytecount,
                this.downloadedMD5,
                this.expectedMD5,
                string.Join(", ", mirrors.ToArray()));
        }
        #endregion
    }
}
