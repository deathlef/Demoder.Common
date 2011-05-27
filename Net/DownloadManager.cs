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
using System.Net;
using System.Threading;
using System.Text;

namespace Demoder.Common.Net
{
    public class DownloadManager : IDisposable
    {
        #region Static fields
        internal static DownloadManager StaticDLM = new DownloadManager();
        #endregion
        #region Members
        /// <summary>
        /// Have we been told to abort/dispose?
        /// </summary>
        private bool disposed = false;
        /// <summary>
        /// Key: Hostname.  Value: [Key: IP Endpoint. Value: Downloader]
        /// </summary>
        private Dictionary<string, List<Downloader>> connections;

        //Connection limits.
        /// <summary>
        /// Max connections per IP
        /// </summary>
        private int clMaxPerIp = 6;

        /// <summary>
        /// UserAgent reported to remote web server.
        /// </summary>
        public string UserAgent = "Demoder.Common DownloadManager";

        private WebProxy proxySettings;

        /// <summary>
        /// Queue with successfull downloads to fire
        /// </summary>
        private Queue<IDownloadItem> successfullDownloads = new Queue<IDownloadItem>();
        /// <summary>
        /// Queue with failed downloads to fire
        /// </summary>
        private Queue<IDownloadItem> failedDownloads = new Queue<IDownloadItem>();
        #endregion
        #region Threads
        private Thread eventQueueProcesserThread;
        private ManualResetEvent eventQueueProcesserMRE = new ManualResetEvent(false);
        #endregion

        #region Events
        /// <summary>
        /// This event is signaled when an item is successfully downloaded.
        /// </summary>
        public event DownloadItemEventHandler DownloadSuccess = null;
        /// <summary>
        /// This event is signaled when the download manager gives up downloading an item.
        /// </summary>
        public event DownloadItemEventHandler DownloadFailure = null;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes the DownloadManager, not using a proxy
        /// </summary>
        public DownloadManager() : this(null) { }

        /// <summary>
        /// Initializes the DownloadManager, telling it to use a proxy.
        /// </summary>
        /// <param name="proxySettings"></param>
        public DownloadManager(WebProxy proxySettings)
        {
            this.proxySettings = proxySettings;
            this.connections = new Dictionary<string, List<Downloader>>(32);
            this.eventQueueProcesserThread = new Thread(new ThreadStart(this.eventQueueProcesser));
            this.eventQueueProcesserThread.IsBackground = true;
            this.eventQueueProcesserThread.Name = "DownloadManager Event Firer";
            this.eventQueueProcesserThread.Start();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a single item to the download queue
        /// </summary>
        /// <param name="downloadItem"></param>
        public void Download(IDownloadItem downloadItem)
        {
            this.addItemToQueue(downloadItem);
        }
        /// <summary>
        /// Add multiple items to the download queue
        /// </summary>
        /// <param name="downloadItems"></param>
        public void Download(IEnumerable<IDownloadItem> downloadItems)
        {
            foreach (IDownloadItem idi in downloadItems)
                this.addItemToQueue(idi);
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Download a single Uri
        /// </summary>
        /// <param name="uri">Uri to download</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns></returns>
        public static byte[] GetBinaryData(Uri uri, int timeout)
        {
            DownloadItem di = new DownloadItem(null, uri, null, null);
            return GetBinaryData(di, timeout);
        }
        /// <summary>
        /// Download data from the provided DownloadItem
        /// </summary>
        /// <param name="downloadItem">Time to wait in milliseconds. int.MaxValue for infinite.</param>
        /// <returns></returns>
        public static byte[] GetBinaryData(IDownloadItem downloadItem, int timeout)
        {
            DownloadManager.StaticDLM.Download(downloadItem);
            if (timeout == int.MaxValue)
                downloadItem.Wait();
            else
                downloadItem.Wait(timeout);
            return downloadItem.Data;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds a download item to queue.
        /// </summary>
        /// <param name="downloadItem"></param>
        private void addItemToQueue(IDownloadItem downloadItem)
        {
            //Initial check if the next mirror is null.
            if (downloadItem.Mirror == null)
            {
                //No more mirrors to try.
                this.onDownloadFailure(downloadItem);
                return;
            }

            string connectionKey = downloadItem.MirrorConnectionString;
            lock (this.connections)
            {
                if (!this.connections.ContainsKey(connectionKey))
                    this.createDownloaders(downloadItem.Mirror, connectionKey);
                //Check if we have any available downloaders.
                if (this.connections[connectionKey].Count == 0)
                {
                    //All mirrors for that host/port have been marked as bad.
                    downloadItem.FailedDownload(true);
                }
                if (downloadItem.Mirror == null)
                {
                    //No more mirrors to try.
                    this.onDownloadFailure(downloadItem);
                    return;
                }
                int queuelength = int.MaxValue;
                Downloader lowestEntry = null;

                //Find the one with the shortest queue.
                foreach (Downloader dl in this.connections[connectionKey])
                {
                    if (dl.QueueCount < queuelength)
                    {
                        //If we haven't tried downloading from this IP endpoint yet
                        if (!downloadItem.MirrorTags.Contains(dl.IPEndPoint))
                        {
                            queuelength = dl.QueueCount;
                            lowestEntry = dl;
                        }
                    }
                }

                //Actually add it to the queue.
                if (lowestEntry == null)
                {
                    //Cycle mirror.
                    downloadItem.FailedDownload(true);
                    //If there's a mirror
                    if (downloadItem.Mirror != null)
                        this.addItemToQueue(downloadItem);
                    else //Add the item to the failure queue
                        this.onDownloadFailure(downloadItem);
                }
                else
                {
                    lowestEntry.DownloadData(downloadItem);
                }
            }
        }

        /// <summary>
        /// Create the downloaders used for a given URI
        /// </summary>
        /// <param name="Uri"></param>
        /// <param name="Key"></param>
        private void createDownloaders(Uri uri, string Key)
        {

            IPAddress[] ips;
            // DNS lookup
            if (this.proxySettings == null || this.proxySettings.IsBypassed(uri))
                ips = Dns.GetHostAddresses(uri.Host); //Not using proxy
            else
                ips = Dns.GetHostAddresses(this.proxySettings.Address.Host); //Using proxy

            // Spawn 3 downloaders per IP
            List<Downloader> downloaders = new List<Downloader>((int)Math.Round(((double)this.clMaxPerIp * (double)ips.Length), 0));
            // Mix the destination IPs throughout the list for better load balancing.
            for (int i = 0; i < this.clMaxPerIp; i++)
            {
                foreach (IPAddress ip in ips)
                {
                    Downloader downloader = new Downloader(new IPEndPoint(ip, uri.Port), uri.Host, this.UserAgent);
                    downloader.MasterDelegate = new DownloaderSlaveEventHandler(this.dseHandler);
                    downloaders.Add(downloader);
                }
            }
            lock (this.connections)
                if (!this.connections.ContainsKey(Key))
                    this.connections.Add(Key, downloaders);
        }

        /// <summary>
        /// Signaled when a download is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="downloadItem"></param>
        private void dseHandler(Downloader sender, IDownloadItem downloadItem)
        {
            // Check if it was successful or not.
            if (downloadItem.Data == null)
            {
                // Download failed
                this.dseDownloadFailed(sender, downloadItem);
            }
            else
            {
                if (downloadItem.IntegrityOK)
                {
                    //Toss it to our event.
                    this.onDownloadSuccess(downloadItem);
                    return;
                }
                else
                    this.dseDownloadFailed(sender, downloadItem);
            }
        }
        /// <summary>
        /// Signaled when a download failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="downloadItem"></param>
        private void dseDownloadFailed(Downloader sender, IDownloadItem downloadItem)
        {
            // Check if we should remove this downloader from the list or not.
            // If we do, we should redistribute its queue as well.
            bool shouldRedistributeQueue = false;

            downloadItem.MirrorTags.Add(sender.IPEndPoint); //Add the IPEndpoint to our "internal" fail list.

            if (sender.HaveCancelled)
                shouldRedistributeQueue = false;
            else if (sender.FailedDownloads > 20 && ((double)sender.FailedDownloads / (double)sender.SuccessfullDownloads > 1.2))
                shouldRedistributeQueue = true;
            List<IDownloadItem> downloadItems = new List<IDownloadItem>();

            //Should stop this particular mirror from being used.
            if (shouldRedistributeQueue)
            {
                lock (this.connections)
                {
                    while (true)
                    {
                        if (!this.connections.ContainsKey(downloadItem.MirrorConnectionString))
                            break;
                        List<Downloader> connections = this.connections[downloadItem.MirrorConnectionString];
                        //Walk through each downloader and check it has already been used by this DownloadItem.
                        foreach (Downloader dloader in connections)
                        {
                            if (dloader.IPEndPoint == sender.IPEndPoint)
                            {
                                this.connections[downloadItem.MirrorConnectionString.ToLower()].Remove(dloader);
                                downloadItems.AddRange(dloader.Stop()); //Add each and every ones download lists to our list.
                                continue;
                            }
                        }
                        break;
                    }
                }
            }
            //Check if there are more mirrors for this host. If not, move to the next host.
            if (this.connections.ContainsKey(downloadItem.MirrorConnectionString))
            {
                if (this.connections[downloadItem.MirrorConnectionString].Count == 0)
                {
                    downloadItem.FailedDownload(true);
                }
            }
            downloadItems.Add(downloadItem);

            //Add each and every item to the queue.
            foreach (IDownloadItem di in downloadItems)
                this.Download(di);
        }


        private void eventQueueProcesser()
        {
            while (!this.disposed)
            {
                this.eventQueueProcesserMRE.WaitOne();
                //Always wait 1s, to allow sending multiple items.
                Thread.Sleep(1000);
                //Reset the event once we start working.
                this.eventQueueProcesserMRE.Reset();
                //Fetch failed downloads
                List<IDownloadItem> failedDownloads = new List<IDownloadItem>();
                lock (this.failedDownloads)
                    while (this.failedDownloads.Count > 0)
                        failedDownloads.Add(this.failedDownloads.Dequeue());

                //Fetch successfull downloads
                List<IDownloadItem> successDownloads = new List<IDownloadItem>();
                lock (this.successfullDownloads)
                    while (this.successfullDownloads.Count > 0)
                        successDownloads.Add(this.successfullDownloads.Dequeue());


                //Trigger failure event handler.
                if (failedDownloads.Count > 0)
                {
                    DownloadItemEventHandler diehFail = this.DownloadFailure;
                    if (diehFail != null)
                        foreach (IDownloadItem idi in failedDownloads)
                            lock (diehFail)
                                diehFail(idi);
                }

                //Trigger failure event handler.
                if (successDownloads.Count > 0)
                {
                    DownloadItemEventHandler diehSuccess = this.DownloadSuccess;
                    if (diehSuccess != null)
                        foreach (IDownloadItem idi in successDownloads)
                            lock (diehSuccess)
                                diehSuccess(idi);
                }

            }
        }
        #endregion




        #region Event firers
        private void onDownloadFailure(IDownloadItem downloadItem)
        {
            lock (this.failedDownloads)
            {
                this.failedDownloads.Enqueue(downloadItem);
            }
            DownloadItemEventHandler df = null;
            if (this.DownloadFailure != null)
                lock (this.DownloadFailure)
                    df = this.DownloadFailure;

            if (df != null)
                df(downloadItem);
        }

        private void onDownloadSuccess(IDownloadItem downloadItem)
        {
            DownloadItemEventHandler ds = this.DownloadSuccess;
            if (ds != null)
                lock (ds)
                    ds(downloadItem);
        }
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                foreach (List<Downloader> ldl in this.connections.Values)
                    foreach (Downloader dl in ldl)
                        ((IDisposable)dl).Dispose();
                this.connections = null;
                this.eventQueueProcesserMRE = null;
                this.eventQueueProcesserThread = null;
                this.failedDownloads = null;
                this.successfullDownloads = null;
                this.DownloadFailure = null;
                this.DownloadSuccess = null;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        public override string ToString()
        {
            return "DownloadManager";
        }
    }
}