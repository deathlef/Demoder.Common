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
using System.Threading;
using System.Web;
using System.Net;
using System.Text;

namespace Demoder.Common.Net
{
    /// <summary>
    /// Establishes a connection to a webserver, and maintains it for a period. Uses keep-alive to allow download of multiple items using the same connection.
    /// </summary>
    public class Downloader : IDisposable
    {
        #region members
        /// <summary>
        /// IP Endpoint to connect to
        /// </summary>
        private IPEndPoint ipEndPoint;
        /// <summary>
        /// Hostname to provide to the foreign webserver
        /// </summary>
        private string hostName;
        /// <summary>
        /// Our user-agent.
        /// </summary>
        private string userAgent;

        private WebClient webClient;

        private volatile int failedDownloads = 0;
        private volatile int successfullDownloads = 0;

        /*
         * Need to think out a smart way of dealing with this queue.
         */

        private Queue<IDownloadItem> downloadQueue = new Queue<IDownloadItem>(8);

        //Threading
        private Thread queueManager;
        private ManualResetEvent queueManagerMRE = new ManualResetEvent(false);
        private volatile bool disposed;

        /// <summary>
        /// Set this to something else than null to enable slave mode.
        /// In slave mode, the Downloader will be 'dumb', and pass parameters to a delegate method to deal with the specifics surrounding the download.
        /// Use this when you want to implement more advanced handling of downloads than this class provides.
        /// </summary>
        public DownloaderSlaveEventHandler MasterDelegate = null;

        /// <summary>
        /// Have this instance been cancelled?
        /// </summary>
        public bool HaveCancelled
        {
            get
            {
                return this.disposed;
            }
        }

        #endregion
        #region Public accessors
        public bool IsBusy { get { return this.webClient.IsBusy; } }
        /// <summary>
        /// Number of failed downloads
        /// </summary>
        public int FailedDownloads { get { return this.failedDownloads; } }
        /// <summary>
        /// Number of successfull downloads
        /// </summary>
        public int SuccessfullDownloads { get { return this.successfullDownloads; } }

        public int QueueCount
        {
            get { return this.downloadQueue.Count; }
        }

        public IPEndPoint IPEndPoint { get { return this.ipEndPoint; } }
        #endregion

        #region Constructor
        public Downloader(IPEndPoint ipEndPoint, string hostName, string userAgent)
        {
            this.ipEndPoint = ipEndPoint;
            this.hostName = hostName.ToLower();
            this.userAgent = userAgent;
            this.webClient = createWebClient();

            this.queueManager = new Thread(new ThreadStart(this.queueHandler));
            this.queueManager.IsBackground = true;
            this.queueManager.Name = "Queue Manager: " + this.ToString();
            this.queueManager.Priority = ThreadPriority.Lowest;
            this.queueManager.Start();
            this.disposed = false;

        }
        #endregion

        #region methods
        public void DownloadData(IDownloadItem downloadItem)
        {
            if (downloadItem.Mirror.Host.ToLower() != this.hostName)
                throw new ArgumentException("This downloader may only retrieve data from the hostname " + this.hostName, "URI");
            //insert code to download here...
            lock (this.downloadQueue)
                this.downloadQueue.Enqueue(downloadItem);
            this.queueManagerMRE.Set();
        }

        /// <summary>
        /// Tells the downloader to stop when it's done with the current download, and returns the remaining queue if any.
        /// </summary>
        /// <returns></returns>
        public IDownloadItem[] Stop()
        {
            this.disposed = true;
            IDownloadItem[] ldi;
            lock (this.downloadQueue)
            {
                ldi = this.downloadQueue.ToArray();
                this.downloadQueue.Clear();
                this.queueManagerMRE.Set();
            }
            ((IDisposable)this).Dispose();
            return ldi;

        }

        private WebClient createWebClient()
        {
            WebClient wc = new WebClient();
            wc.Proxy = new WebProxy(this.ipEndPoint.Address.ToString(), this.ipEndPoint.Port); //Workaround: Enable connecting to a specified mirror
            //wc.Headers.Add(HttpRequestHeader.Host, this._hostName);
            wc.Headers.Add(HttpRequestHeader.KeepAlive, "15");
            wc.Headers.Add(HttpRequestHeader.UserAgent, this.userAgent);
            return wc;
        }
        #endregion

        #region Queue handler
        private void queueHandler()
        {
            while (!this.disposed)
            {
                //Get item.
                IDownloadItem di;
                lock (this.downloadQueue)
                {
                    if (this.downloadQueue.Count == 0)
                        di = null;
                    else
                        di = this.downloadQueue.Dequeue();
                }

                if (di == null) //Empty queue
                {
                    // We should wait for the MRE. 
                    // The MRE will be set when we're cancelled, so no point with a timeout.
                    this.queueManagerMRE.WaitOne();
                    continue; //Restart the loop in case we have been told to stop.
                }
                this.queueProcessEntry(di);
            }
        }

        /// <summary>
        /// Processes a DownloadItem queue entry.
        /// </summary>
        /// <param name="downloadItem"></param>
        private void queueProcessEntry(IDownloadItem downloadItem)
        {
            try
            {
                if (downloadItem.SaveAs != null)
                {
                    this.webClient.DownloadFile(downloadItem.Mirror, downloadItem.SaveAs.FullName);
                    downloadItem.SuccessfullDownload(File.ReadAllBytes(downloadItem.SaveAs.FullName));
                }
                else
                {
                    byte[] bytes = this.webClient.DownloadData(downloadItem.Mirror);
                    downloadItem.SuccessfullDownload(bytes);
                }
            }
            catch (Exception ex)
            {
            }
            //Signal the DownloadItems faileddelegate
            if (!downloadItem.IntegrityOK)
                downloadItem.FailedDownload((this.MasterDelegate == null));

            //Signal our master, if any
            if (this.MasterDelegate != null) //Slave mode.
                this.MasterDelegate(this, downloadItem);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return String.Format("{0}: IP {1} port {2}. (Host: {3})",
                this.GetType().ToString(),
                this.ipEndPoint.Address,
                this.ipEndPoint.Port,
                this.hostName);
        }
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.queueManager.Join();
                this.queueManager = null;
                this.downloadQueue = null;
                this.failedDownloads = 0;
                this.hostName = null;
                this.ipEndPoint = null;
                this.queueManagerMRE = null;
                this.userAgent = null;
                this.webClient.Dispose();
                this.webClient = null;
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
