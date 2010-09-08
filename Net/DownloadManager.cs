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
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace Demoder.Common.Net
{
	public class DownloadManager
	{
		#region events
		/// <summary>
		/// This event is signaled when the queue is empty, and something has been downloaded.
		/// </summary>
		public event EventHandler EventQueueEmpty;
		#endregion
		#region Members
		/// <summary>
		/// Files that we have downloaded
		/// </summary>
		private List<KeyValuePair<object, byte[]>> _downloadedFiles;
		/// <summary>
		/// Retrieves the downloaded files, and remove them from the instance.
		/// </summary>
		public Dictionary<object, byte[]> RetrieveDownloadedFiles
		{
			get
			{
				lock (this._downloadedFiles)
				{
					Dictionary<object, byte[]> d = new Dictionary<object, byte[]>();
					foreach (KeyValuePair<object, byte[]> kvp in this._downloadedFiles)
						d.Add(kvp.Key, kvp.Value);
					this._downloadedFiles.Clear();
					return d;
				}
			}
		}

		public int NumDownloadedFiles { get { return this._downloadedFiles.Count; } }
		
		private bool _running = false;
        private bool _active = false;
        public bool Active { get { return this._active; } }
		
		public int QueueLength { get { return this._downloadQueue.Count; } }
		private Queue<KeyValuePair<object, Uri>> _downloadQueue;

		#region connection tracking
		private Dictionary<string, DateTime> _hostLastConnectionTime;
		private Dictionary<string, Queue<WebClient>> _webRequests;
		private Dictionary<string, int> _activeconnections = new Dictionary<string, int>();
		
		private int _maxConnections = 12;
		private int _maxConnectionsPerServer = 4;
		private int _activeWebClients = 0;
		private object _activeWebClientsLock = new object();
		#endregion

		#region Error tracking
		private List<KeyValuePair<object,Uri>> _protocolErrors = new List<KeyValuePair<object,Uri>>();
		public int NumProtocolErrors { get { return this._protocolErrors.Count; } }
		public KeyValuePair<object, Uri>[] ProtocolErrors
		{
			get
			{
				lock (this._protocolErrors)
				{
					KeyValuePair<object, Uri>[] ar = new KeyValuePair<object, Uri>[this._protocolErrors.Count];
					this._protocolErrors.CopyTo(ar, 0);
					this._protocolErrors.Clear();
					return ar;
				}
			}
		}
		#endregion

		#region Threading
		/// <summary>
		/// Download queue. Key is identifier placed into the DownloadedFiles dictionary.
		/// </summary>
		private ManualResetEvent _queueMRE = new ManualResetEvent(false);
		private ManualResetEvent _workerMRE = new ManualResetEvent(false);
        #endregion threading
		#endregion members

        #region Constructors
		/// <summary>
		/// Create an instance of the DownloadManager class
		/// </summary>
		/// <param name="MaxConnectionsPerServer">Max number of simultanious connections to each unique hostname:port</param>
        public DownloadManager(int MaxConnectionsPerServer, int MaxConnections)
		{
			this._downloadedFiles = new List<KeyValuePair<object, byte[]>>();
			this._downloadQueue = new Queue<KeyValuePair<object, Uri>>();
			this._activeWebClients = 0;
			this._maxConnections = MaxConnections;
			this._maxConnectionsPerServer = MaxConnectionsPerServer;
			this._hostLastConnectionTime = new Dictionary<string, DateTime>();
            this._webRequests = new Dictionary<string, Queue<WebClient>>();
		}
		#endregion

		#region Queue management
        /// <summary>
        /// Add an address to the queue
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="uri"></param>
		public void Enqueue(object Tag, Uri Uri)
		{
            lock (this._downloadQueue)
			    this._downloadQueue.Enqueue(new KeyValuePair<object, Uri>(Tag, Uri));
			this._queueMRE.Set();
		}

        /// <summary>
        /// Add several items to the download queue
        /// </summary>
        /// <param name="Items"></param>
        public void Enqueue(Dictionary<object, Uri> Items)
        {
            lock (this._downloadQueue)
                foreach (KeyValuePair<object, Uri> kvp in Items)
                    if (!this._downloadQueue.Contains(kvp)) this._downloadQueue.Enqueue(kvp);
            this._queueMRE.Set();
		}

		#region static methods
		/// <summary>
		/// Download data from uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static byte[] GetBinaryData(Uri Uri)
		{
			WebClient wc = new WebClient();
			wc.Headers.Set(HttpRequestHeader.UserAgent, "Demoder.Common.DownloadManager/0.8");
			return wc.DownloadData(Uri);			
		}
		/// <summary>
		/// Open a readable stream to the provided uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static Stream GetReadStream(Uri Uri)
		{
			WebClient wc = new WebClient();
			wc.Headers.Set(HttpRequestHeader.UserAgent, "Demoder.Common.DownloadManager/0.8");
			Stream stream = wc.OpenRead(Uri);
			return stream;
		}
		#endregion

		/// <summary>
        /// Start a queue manager to process the queue
        /// </summary>
        public void Start()
        {
            if (!this._running && !this._active)
            { //If we ain't running, and ain't waiting for a thread to finish...
                this._active = true;
                this._running = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.queueManager));
            }
            else if (!this._running && this._active)
            { //If we ain't running, but are waiting for a thread to finish....
                while (this._active)
                {
                    Thread.Sleep(250);
                }
                this.Start();
            }
        }

        /// <summary>
        /// Tell the queue manager to stop processing the queue
        /// </summary>
        public void Stop()
        {
            this._running = false;
        }

		

		private void queueManager(object Obj)
		{
            this._active = true;
			while (this._running)
            {
				//make sure we don't exceed max number of connections
				#region don't exceed max number of connections
				while (this._maxConnections <= this._activeWebClients)
				{
					this._workerMRE.Reset();
					this._workerMRE.WaitOne();
				}
                //Make sure we have something to toss at the worker
                while (this._downloadQueue.Count == 0)
                {
                    this._queueMRE.Reset();
                    this._queueMRE.WaitOne();
				}
				#endregion
				this._queueMRE.Reset();
                this._workerMRE.Reset();
				KeyValuePair<object, Uri> WorkEntry = new KeyValuePair<object,Uri>();
				lock (this._downloadQueue)
					 WorkEntry = this._downloadQueue.Dequeue();
				if (WorkEntry.Value == null)
					continue;

				string wrhost = WorkEntry.Value.Host + ":" + WorkEntry.Value.Port.ToString();
				#region Track active connections
				if (this._activeconnections.ContainsKey(wrhost))
				{
					if (this._activeconnections[wrhost] >= this._maxConnections)
					{
						lock (this._downloadQueue)
						{
							this._downloadQueue.Enqueue(WorkEntry);
						}
						Thread.Sleep(5); //Lazy anti-100%-load measure..
						continue;
					}
				}
				else
				{
					lock (this._activeconnections)
						this._activeconnections.Add(wrhost, 0);
				}
				if (!this._webRequests.ContainsKey(wrhost))
                    this._webRequests.Add(wrhost, new Queue<WebClient>());
				lock (this._activeWebClientsLock)
					this._activeWebClients++;
				lock (this._activeconnections)
					this._activeconnections[wrhost]++;
				#endregion 
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.DownloadBinaryData), WorkEntry);
				//ThreadPool.QueueUserWorkItem(new WaitCallback(this.DownloadBinaryData_UsingWebRequest), WorkEntry);
                //Check if there's more to do
                if (this._downloadQueue.Count == 0) 
                    this._running = false;
			}
            //Wait for active fetchers to finish
            while (this._activeWebClients > 0)
                this._workerMRE.WaitOne();
            this._active = false;
		}

        private void DownloadBinaryData(object Obj)
        {
            KeyValuePair<object, Uri> kvp = (KeyValuePair<object, Uri>)Obj;
			string wrhost = kvp.Value.Host + ":" + kvp.Value.Port.ToString();
            bool sleep = true;
            bool newConnection = false;
            WebClient wr = null;
            lock (this._webRequests)
            {
                if (this._webRequests[wrhost].Count == 0)
                {
                    wr = new WebClient();
                    wr.Headers.Set(HttpRequestHeader.UserAgent, "Demoder.Common.DownloadManager/0.8");
                    wr.Headers.Set(HttpRequestHeader.KeepAlive, "30");
                    newConnection = true;
                }
                else
                {
                    wr = this._webRequests[wrhost].Dequeue();
                    newConnection = false;
                }
            }
            if (newConnection)
            {
                #region anti-DoS
                while (sleep)
                {
                    int sleeptime = 0;
                    lock (this._hostLastConnectionTime)
                    {
                        if (this._hostLastConnectionTime.ContainsKey(kvp.Value.DnsSafeHost))
                        {
                            TimeSpan ts = DateTime.Now - this._hostLastConnectionTime[kvp.Value.DnsSafeHost];
                            int timespan = 600 - ts.Milliseconds;
                            if (timespan >= 600)  //No need to wait
                                sleep = false;
                            else if (timespan > 100 && this._downloadQueue.Count >= 1)
                            { //Performance-wise smarter to toss things back to queue and fetch the next.
                                lock (this._downloadQueue)
                                    this._downloadQueue.Enqueue(kvp);
                                this.dbdDone(wrhost);
                                return;
                            }
                            else //Performance-wise smarter to wait for it
                                sleeptime = timespan;
                        }
                    }
                    if (sleeptime > 0) //Should we wait?
                        Thread.Sleep(sleeptime);
                    else
                        sleep = false;
                }
                //Update last connection time info.
                lock (this._hostLastConnectionTime)
                {
                    if (this._hostLastConnectionTime.ContainsKey(kvp.Value.Host))
                        this._hostLastConnectionTime[kvp.Value.Host] = DateTime.Now;
                    else
                        this._hostLastConnectionTime.Add(kvp.Value.Host, DateTime.Now);
                }
                #endregion
            }
            #region fetch data
            byte[] response = null;
            try
            {
                response = wr.DownloadData(kvp.Value);
            }
            catch (WebException ex)
			{
				switch (ex.Status)
				{
					case WebExceptionStatus.KeepAliveFailure:
					case WebExceptionStatus.ConnectionClosed:
					case WebExceptionStatus.ConnectFailure:
					case WebExceptionStatus.ReceiveFailure:
					case WebExceptionStatus.RequestCanceled:
					case WebExceptionStatus.SendFailure:
					case WebExceptionStatus.Timeout:
					case WebExceptionStatus.UnknownError:
						lock (this._downloadQueue)
							this._downloadQueue.Enqueue(kvp);
						this.dbdDone(wrhost);
						return;
					case WebExceptionStatus.Success:
						//Why did we get an exception then? Strange... Continue as if it didn't happen!
						break;
					case WebExceptionStatus.ProtocolError: //protocol errors such as 404 not found. Drop the request.
						lock (this._protocolErrors)
							this._protocolErrors.Add(kvp);
						this.dbdDone(wrhost);
						return;
				}
				Console.WriteLine(ex);
			}

            if (response != null)
                lock (this._downloadedFiles)
					this._downloadedFiles.Add(new KeyValuePair<object, byte[]>(kvp.Key, response));
            #endregion
            //Toss the Webclient back to the queue, for other threads to use.
            lock (this._webRequests)
                this._webRequests[wrhost].Enqueue(wr);
            this.dbdDone(wrhost);
        }

        /// <summary>
        /// Method run when the DownloadBinaryData method is done, for one reason or another.
        /// </summary>
        private void dbdDone(string wrhost)
        {
			lock (this._activeWebClientsLock)
				this._activeWebClients--;
			lock (this._activeconnections)
				this._activeconnections[wrhost]--;
            this._workerMRE.Set();
        }
		#endregion
	}
}