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

		WebClient _webClient;
		#endregion

		#region Constructor
		public Downloader(IPEndPoint IPEndPoint, string HostName, string UserAgent)
		{
			this._ipEndPoint = IPEndPoint;
			this._hostName = HostName.ToLower();
			this._userAgent = UserAgent;
			createWebClient();
		}
		#endregion

		#region methods
		public byte[] DownloadData(Uri URI)
		{
			if (URI.Host.ToLower() != this._hostName)
				throw new ArgumentException("This downloader may only retrieve data from the hostname " + this._hostName, "URI");
			//insert code to download here...
			return this._webClient.DownloadData(URI.PathAndQuery);
		}

		private void createWebClient()
		{
			WebClient wc = new WebClient();
			wc.Proxy = new WebProxy(this._ipEndPoint.Address.ToString(), this._ipEndPoint.Port); //Workaround: Enable connecting to a specified mirror
			wc.Headers.Add(HttpRequestHeader.Host, this._hostName);
			wc.Headers.Add(HttpRequestHeader.KeepAlive, "15");
			wc.Headers.Add(HttpRequestHeader.UserAgent, this._userAgent);
			this._webClient = wc;
		}
		#endregion


		#region Overrides
		public override string ToString()
		{
			return String.Format("{0}: Connected to IP {1} port {2}. (Host: {3})",
				this.GetType().ToString(),
				this._ipEndPoint.Address,
				this._ipEndPoint.Port,
				this._hostName);
		}
		#endregion
	}
}
