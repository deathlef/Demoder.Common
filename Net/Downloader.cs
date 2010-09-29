﻿/*
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
	public class Downloader
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

		private Queue<DownloadItem> _downloadQueue = new Queue<DownloadItem>(8);
		
		//Threading
		private Thread _queueManager;
		private ManualResetEvent _queueManagerMRE = new ManualResetEvent(false);
		private volatile bool _running;

		/// <summary>
		/// Set this to something else than null to enable slave mode.
		/// In slave mode, the Downloader will be 'dumb', and pass parameters to a delegate method to deal with the specifics surrounding the download.
		/// Use this when you want to implement more advanced handling of downloads than this class provides.
		/// </summary>
		public DownloaderSlaveEventHandler SlaveModeDelegate = null;

		/// <summary>
		/// Have this instance been cancelled?
		/// </summary>
		public bool HaveCancelled
		{
			get
			{
				if (this._running)
					return false;
				else
					return true;
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
			//this._queueManager.SetApartmentState(ApartmentState.STA);
			this._running = true;
			
		}
		#endregion

		#region methods
		public void DownloadData(DownloadItem DownloadItem)
		{
			if (DownloadItem.NextMirror.Host.ToLower() != this._hostName)
				throw new ArgumentException("This downloader may only retrieve data from the hostname " + this._hostName, "URI");
			//insert code to download here...
			this._downloadQueue.Enqueue(DownloadItem);
			this._queueManagerMRE.Set();
#warning Trigger a MRE here, to signal the download thread.
		}

		/// <summary>
		/// Tells the downloader to stop when it's done with the current download, and returns the remaining queue if any.
		/// </summary>
		/// <returns></returns>
		public DownloadItem[] Stop()
		{
			this._running = false;
			lock (this._downloadQueue)
			{
				DownloadItem[] ldi = this._downloadQueue.ToArray();
				this._downloadQueue.Clear();
				return ldi;
			}
			
		}

		private WebClient createWebClient()
		{
			WebClient wc = new WebClient();
			wc.Proxy = new WebProxy(this._ipEndPoint.Address.ToString(), this._ipEndPoint.Port); //Workaround: Enable connecting to a specified mirror
			wc.Headers.Add(HttpRequestHeader.Host, this._hostName);
			wc.Headers.Add(HttpRequestHeader.KeepAlive, "15");
			wc.Headers.Add(HttpRequestHeader.UserAgent, this._userAgent);
			return wc;
		}
		#endregion

		#region Queue handler
		private void queueHandler()
		{
			while (this._running)
			{
				//Get item.
				DownloadItem di;
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
					// Wait a maximum of 100ms in case we've been told to stop while waiting.
					this._queueManagerMRE.WaitOne(100);
					continue; //Restart the loop in case we have been told to stop.
				}
				this.queueProcessEntry(di);
			}
		}

		/// <summary>
		/// Processes a DownloadItem queue entry.
		/// </summary>
		/// <param name="DownloadItem"></param>
		private void queueProcessEntry(DownloadItem DownloadItem)
		{
			try
			{
				DownloadItem.Data = this._webClient.DownloadData(DownloadItem.NextMirror);
			}
			catch { }
			if (this.SlaveModeDelegate != null) //Slave mode.
				this.SlaveModeDelegate(this, DownloadItem);
			else
			{	//Standalone (no-slave) mode.
				if (DownloadItem.Data == null)
				{ //Failed download
					DownloadItem.DownloadFailed();
				}
				else
				{ //Successfull download
					DownloadItem.SuccessfullDownload();
				}
			}
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
	}
}