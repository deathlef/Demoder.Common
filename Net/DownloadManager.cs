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
		#region Members
		/// <summary>
		/// Key: Hostname.  Value: [Key: IP Endpoint. Value: Downloader]
		/// </summary>
		private Dictionary<string, List<Downloader>> _connections;

		//Connection limits.
		/// <summary>
		/// Max connections per IP
		/// </summary>
		private int _clMaxPerIp = 1;
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
		public DownloadManager()
		{
			this._connections = new Dictionary<string, List<Downloader>>(32);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Download a single URI
		/// </summary>
		/// <param name="DownloadItem"></param>
		public void Download(DownloadItem DownloadItem)
		{
				this.addItemToQueue(DownloadItem);
		}
		#endregion

		#region Public static methods
		//These need to be knit into the DLM as a whole, somehow.
		public static byte[] GetBinaryData(Uri Uri)
		{
			WebClient wc = new WebClient();
			return wc.DownloadData(Uri);
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Adds a download item to queue.
		/// </summary>
		/// <param name="DownloadItem"></param>
		private void addItemToQueue(DownloadItem DownloadItem)
		{
			//Initial check if the next mirror is null.
			if (DownloadItem.NextMirror == null)
			{
				//No more mirrors to try.
				this.onDownloadFailure(DownloadItem);
				return;
			}

			string connectionKey = DownloadItem.NextMirrorConnectionString;
			lock (this._connections)
			{
				if (!this._connections.ContainsKey(connectionKey))
					this.createDownloaders(DownloadItem.NextMirror, connectionKey);
				//Check if we have any available downloaders.
				if (this._connections[connectionKey].Count == 0)
				{
					//All mirrors for that host/port have been marked as bad.
					DownloadItem.DownloadFailed();
				}
				if (DownloadItem.NextMirror == null)
				{
					//No more mirrors to try.
					this.onDownloadFailure(DownloadItem);
					return;
				}
				int queuelength = int.MaxValue;
				Downloader lowestEntry = null;

				//Find the one with the shortest queue.
				foreach (Downloader dl in this._connections[connectionKey])
				{
					if (dl.QueueCount < queuelength)
					{
						if (!DownloadItem.InfoTags.Contains(dl.IPEndPoint))
						{
							queuelength = dl.QueueCount;
							lowestEntry = dl;
						}
					}
				}

				//Actually add it to the queue.
				if (lowestEntry == null)
				{
					DownloadItem.DownloadFailed();
					this.addItemToQueue(DownloadItem);
				}
				else
				{
					lowestEntry.DownloadData(DownloadItem);
				}
			}
		}

		/// <summary>
		/// Create the downloaders used for a given URI
		/// </summary>
		/// <param name="Uri"></param>
		/// <param name="Key"></param>
		private void createDownloaders(Uri Uri, string Key)
		{
			// DNS lookup
			IPAddress[] ips = Dns.GetHostAddresses(Uri.Host);
			// Spawn 3 downloaders per IP
			List<Downloader> downloaders = new List<Downloader>(this._clMaxPerIp);
			// Mix the destination IPs throughout the list for better load balancing.
			for (int i = 0; i < this._clMaxPerIp; i++)
			{
				foreach (IPAddress ip in ips)
				{
					Downloader downloader = new Downloader(new IPEndPoint(ip, Uri.Port), Uri.Host, "Demoder.Common DownloadManager");
					downloader.SlaveModeDelegate = new DownloaderSlaveEventHandler(this.dseHandler);
					downloaders.Add(downloader);
				}
			}

			lock (this._connections)
				if (!this._connections.ContainsKey(Key))
					this._connections.Add(Key, downloaders);
		}

		/// <summary>
		/// Signaled when a download is complete.
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="DownloadItem"></param>
		private void dseHandler(Downloader Sender, DownloadItem DownloadItem)
		{
			// Check if it was successful or not.
			if (DownloadItem.Data == null)
			{
				// Download failed
				this.dseDownloadFailed(Sender, DownloadItem);
			}
			else
			{
				if (DownloadItem.IntegrityOK)
				{
					//Toss it to our event.
					this.onDownloadSuccess(DownloadItem);
					return;
				}
				else
					this.dseDownloadFailed(Sender, DownloadItem);
			}
		}
		/// <summary>
		/// Signaled when a download failed
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="DownloadItem"></param>
		private void dseDownloadFailed(Downloader Sender, DownloadItem DownloadItem)
		{
			// Check if we should remove this downloader from the list or not.
			// If we do, we should redistribute its queue as well.
			bool shouldRedistributeQueue = false;
			
			DownloadItem.InfoTags.Add(Sender.IPEndPoint); //Add the IPEndpoint to our "internal" fail list.

			if (Sender.HaveCancelled)
				shouldRedistributeQueue = false;
			else if (Sender.FailedDownloads > 5 && ((double)Sender.FailedDownloads / (double)Sender.SuccessfullDownloads > 1))
				shouldRedistributeQueue = true;
			List<DownloadItem> downloadItems = new List<DownloadItem>();

			//Should stop this particular mirror from being used.
			if (shouldRedistributeQueue)
			{
				lock (this._connections)
				{
					while (true) {
						if (!this._connections.ContainsKey(DownloadItem.NextMirrorConnectionString))
							break;
						List<Downloader> connections = this._connections[DownloadItem.NextMirrorConnectionString];
						//Walk through each downloader and check its
						foreach (Downloader dloader in connections)
						{
							if (dloader.IPEndPoint == Sender.IPEndPoint)
							{
								this._connections[DownloadItem.NextMirrorConnectionString.ToLower()].Remove(dloader);
								downloadItems.AddRange(dloader.Stop()); //Add each and every ones download lists to our list.
								continue;
							}
						}
						break;
					}
				}
			}
			//Check if there are more mirrors for this host. If not, move to the next host.
			if (this._connections.ContainsKey(DownloadItem.NextMirrorConnectionString))
			{
				if (this._connections[DownloadItem.NextMirrorConnectionString].Count == 0)
					DownloadItem.DownloadFailed();
			}

			downloadItems.Add(DownloadItem);

			foreach (DownloadItem di in downloadItems)
				this.Download(di);
		}
		#endregion

		#region Event firers
		private void onDownloadFailure(DownloadItem DownloadItem) 
		{
			DownloadItemEventHandler df = null;
			if (this.DownloadFailure != null)
				lock (this.DownloadFailure)
					df = this.DownloadFailure;
					
			if (df != null)
				df(DownloadItem);
		}

		private void onDownloadSuccess(DownloadItem DownloadItem)
		{
			DownloadItemEventHandler ds = null;
			if (this.DownloadSuccess != null)
				lock (this.DownloadSuccess)
					ds = this.DownloadSuccess;

			if (ds != null)
				ds(DownloadItem);
		}
		#endregion
	}
}