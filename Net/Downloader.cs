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
		private IPEndPoint _ipEndPoint;
		/// <summary>
		/// Hostname to provide to the foreign webserver
		/// </summary>
		private string _hostName;
		/// <summary>
		/// Our user-agent.
		/// </summary>
		private string _userAgent;

		private WebClient _webClient;

		private volatile int _failedDownloads = 0;
		private volatile int _successfullDownloads = 0;

		/*
		 * Need to think out a smart way of dealing with this queue.
		 */

		private Queue<IDownloadItem> _downloadQueue = new Queue<IDownloadItem>(8);
		
		//Threading
		private Thread _queueManager;
		private ManualResetEvent _queueManagerMRE = new ManualResetEvent(false);
		private volatile bool _disposed;

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
				return this._disposed;
			}
		}

		#endregion
		#region Public accessors
		public bool IsBusy { get { return this._webClient.IsBusy; }	}
		/// <summary>
		/// Number of failed downloads
		/// </summary>
		public int FailedDownloads { get { return this._failedDownloads; } }
		/// <summary>
		/// Number of successfull downloads
		/// </summary>
		public int SuccessfullDownloads { get { return this._successfullDownloads; } }

		public int QueueCount
		{
			get { return this._downloadQueue.Count; }
		}

		public IPEndPoint IPEndPoint { get { return this._ipEndPoint; } }
		#endregion

		#region Constructor
		public Downloader(IPEndPoint IPEndPoint, string HostName, string UserAgent)
		{
			this._ipEndPoint = IPEndPoint;
			this._hostName = HostName.ToLower();
			this._userAgent = UserAgent;
			this._webClient = createWebClient();

			this._queueManager = new Thread(new ThreadStart(this.queueHandler));
			this._queueManager.IsBackground = true;
			this._queueManager.Name = "Queue Manager: "+this.ToString();
			this._queueManager.Priority = ThreadPriority.Lowest;
			this._queueManager.Start();
			this._disposed = false;
			
		}
		#endregion

		#region methods
		public void DownloadData(IDownloadItem DownloadItem)
		{
			if (DownloadItem.Mirror.Host.ToLower() != this._hostName)
				throw new ArgumentException("This downloader may only retrieve data from the hostname " + this._hostName, "URI");
			//insert code to download here...
			lock (this._downloadQueue)
				this._downloadQueue.Enqueue(DownloadItem);
			this._queueManagerMRE.Set();
		}

		/// <summary>
		/// Tells the downloader to stop when it's done with the current download, and returns the remaining queue if any.
		/// </summary>
		/// <returns></returns>
		public IDownloadItem[] Stop()
		{
			this._disposed = true;
			IDownloadItem[] ldi;
			lock (this._downloadQueue)
			{
				ldi = this._downloadQueue.ToArray();
				this._downloadQueue.Clear();
				this._queueManagerMRE.Set();
			}
			((IDisposable)this).Dispose();
			return ldi;
			
		}

		private WebClient createWebClient()
		{
			WebClient wc = new WebClient();
			wc.Proxy = new WebProxy(this._ipEndPoint.Address.ToString(), this._ipEndPoint.Port); //Workaround: Enable connecting to a specified mirror
			//wc.Headers.Add(HttpRequestHeader.Host, this._hostName);
			wc.Headers.Add(HttpRequestHeader.KeepAlive, "15");
			wc.Headers.Add(HttpRequestHeader.UserAgent, this._userAgent);
			return wc;
		}
		#endregion

		#region Queue handler
		private void queueHandler()
		{
			while (!this._disposed)
			{
				//Get item.
				IDownloadItem di;
				lock (this._downloadQueue)
				{
					if (this._downloadQueue.Count == 0)
						di = null;
					else
						di = this._downloadQueue.Dequeue();
				}

				if (di == null) //Empty queue
				{
					// We should wait for the MRE. 
					// The MRE will be set when we're cancelled, so no point with a timeout.
					this._queueManagerMRE.WaitOne();
					continue; //Restart the loop in case we have been told to stop.
				}
				this.queueProcessEntry(di);
			}
		}

		/// <summary>
		/// Processes a DownloadItem queue entry.
		/// </summary>
		/// <param name="DownloadItem"></param>
		private void queueProcessEntry(IDownloadItem DownloadItem)
		{
			try
			{
				if (DownloadItem.SaveAs != null)
				{
					this._webClient.DownloadFile(DownloadItem.Mirror, DownloadItem.SaveAs.FullName);
					DownloadItem.SuccessfullDownload(File.ReadAllBytes(DownloadItem.SaveAs.FullName));
				}
				else
				{
					byte[] bytes = this._webClient.DownloadData(DownloadItem.Mirror);
					DownloadItem.SuccessfullDownload(bytes);
				}
			}
			catch (Exception ex)
			{
			}
			//Signal the DownloadItems faileddelegate
			if (!DownloadItem.IntegrityOK)
				DownloadItem.FailedDownload((this.MasterDelegate == null));

			//Signal our master, if any
			if (this.MasterDelegate != null) //Slave mode.
				this.MasterDelegate(this, DownloadItem);
		}
		#endregion

		#region Overrides
		public override string ToString()
		{
			return String.Format("{0}: IP {1} port {2}. (Host: {3})",
				this.GetType().ToString(),
				this._ipEndPoint.Address,
				this._ipEndPoint.Port,
				this._hostName);
		}
		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (!this._disposed)
			{
				this._disposed = true;
				this._queueManager.Join();
				this._queueManager = null;
				this._downloadQueue = null;
				this._failedDownloads = 0;
				this._hostName = null;
				this._ipEndPoint = null;
				this._queueManagerMRE = null;
				this._userAgent = null;
				this._webClient.Dispose();
				this._webClient = null;
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}
