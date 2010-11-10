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
		private readonly object _tag;
		private readonly MD5Checksum _expectedMD5;
		private Queue<Uri> _mirrors;
		private List<object> _mirrorTags = new List<object>();

		private List<Uri> _failedMirrors;
		private ManualResetEvent _downloadMre = new ManualResetEvent(false);

		//Describing the download data
		private byte[] _bytes = null;
		private object _httpStatusCode = null;
		private MD5Checksum _downloadedMD5 = null;
		private FileInfo _saveAs = null;
		//Delegates
		/// <summary>
		/// Signaled when the download succeeds.
		/// </summary>
		private readonly DownloadItemEventHandler _downloadSuccessDelegate;
		/// <summary>
		/// Signalted when the download fails.
		/// </summary>
		private readonly DownloadItemEventHandler _downloadFailureDelegate;
		#endregion

		#region Constructors
		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Userdefined tag of this download</param>
		/// <param name="Uri">Uri to download from</param>
		public DownloadItem(object Tag,
			Uri Uri,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate) :
			this(Tag, new List<Uri>(new Uri[] { Uri }), DownloadSuccessDelegate, DownloadFailureDelegate) { }

		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Uerdefined tag for this download</param>
		/// <param name="Mirrors">URIs to download from</param>
		public DownloadItem(
			object Tag,
			IEnumerable<Uri> Mirrors,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate) :
			this(Tag, Mirrors, DownloadSuccessDelegate, DownloadFailureDelegate, null) { }


		public DownloadItem(object Tag,
			IEnumerable<Uri> Mirrors,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate,
			MD5Checksum ExcpectedMD5,
			FileInfo SaveAs)
			: this(Tag, Mirrors, DownloadSuccessDelegate, DownloadFailureDelegate, ExcpectedMD5)
		{
			this._saveAs = SaveAs;
		}

		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Userdefined tag for this download</param>
		/// <param name="Mirrors">URIs to download from</param>
		/// <param name="ExpectedMD5">The file should have this MD5 hash to be considered a successfull download</param>
		public DownloadItem(
			object Tag,
			IEnumerable<Uri> Mirrors,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate,
			MD5Checksum ExpectedMD5)
		{
			this._tag = Tag;
			this._mirrors = new Queue<Uri>(Mirrors);
			this._failedMirrors = new List<Uri>(this._mirrors.Count);
			this._downloadSuccessDelegate = DownloadSuccessDelegate;
			this._downloadFailureDelegate = DownloadFailureDelegate;
			this._expectedMD5 = ExpectedMD5;
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
				if (this._bytes == null)
					return false;
				lock (this._downloadedMD5)
				{
					if (this._expectedMD5==null) //Since we have data, and no expected MD5, assume the download manager verified the server-reported MD5.
						return true;
					if (this._expectedMD5 == this._downloadedMD5) //Integrity ok
						return true;
				}
				return false; //All other scenarios, download integrity is not ok.
			}
		}

		/// <summary>
		/// Retrieves the datas actual MD5 checksum
		/// </summary>
		public MD5Checksum MD5 { get { return this._downloadedMD5; } }
		/// <summary>
		/// Retrieves the wanted MD5 checksum
		/// </summary>
		public MD5Checksum WantedMD5 { get { return this._expectedMD5; } }

		/// <summary>
		/// The downloaded data
		/// </summary>
		public byte[] Data
		{
			get
			{
				if (this._bytes == null && this._saveAs != null)
				{
					//Get the binary data from file, cache it in this object, then return it.
					try
					{
						this.storeBytes(File.ReadAllBytes(this._saveAs.FullName));
					}
					catch { }
					return this._bytes;
				}
				return this._bytes;
			}
		}
		public FileInfo SaveAs { get { return this._saveAs; } }

		/// <summary>
		/// Userdefined tag
		/// </summary>
		public object Tag { get { return this._tag; } }
		/// <summary>
		/// Retrieve a list of failed mirrors.
		/// </summary>
		public Uri[] FailedMirrors { get { return this._failedMirrors.ToArray(); } }

		/// <summary>
		/// Retrieve the top mirror from the queue.
		/// </summary>
		public Uri Mirror
		{
			get
			{
				lock (this)
				{
					if (this._mirrors.Count == 0)
					{
						return null;
					}
					else
						return this._mirrors.Peek();
				}
			}
		}

		/// <summary>
		/// A list of tags for this mirror. Will be reset when cycling mirrors.
		/// </summary>
		public List<object> MirrorTags { get { return this._mirrorTags; } }

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
		private void storeBytes(byte[] Bytes)
		{
			this._bytes = Bytes;
			this._downloadedMD5 = MD5Checksum.Generate(Bytes);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// This method will call the DownloadSuccessDelegate.
		/// </summary>
		public bool SuccessfullDownload(byte[] Bytes)
		{
			lock (this)
			{
				this.storeBytes(Bytes);
				if (this.IntegrityOK)
				{
					if (this._downloadSuccessDelegate != null)
						lock (this._downloadSuccessDelegate)
							this._downloadSuccessDelegate(this);
					this._downloadMre.Set();
				}
				return (this.IntegrityOK);
			}
		}

		/// <summary>
		/// Download failed. Move the mirror to the failed queue. If there are no more mirrors, call the DownloadFailed delegate and return false.
		/// </summary>
		/// <param name="FailMirror">Should we mark the mirror as failed</param>
		/// <returns>true if we have more mirrors in queue, false otherwise.</returns>
		public bool FailedDownload(bool FailMirror)
		{
			lock (this)
			{
				if (FailMirror)
				{
					Uri uri = this._mirrors.Dequeue();
					this._failedMirrors.Add(uri);
					//Clear mirror tags for previous mirror.
					this._mirrorTags.Clear();
				}
				if (this._mirrors.Count == 0)
				{
					DownloadItemEventHandler dieh = null;
					if (this._downloadFailureDelegate != null)
						lock (this._downloadFailureDelegate)
							dieh = this._downloadFailureDelegate;
					if (dieh != null)
						dieh(this);
					this._downloadMre.Set();
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
			this._downloadMre.WaitOne();
		}
		/// <summary>
		/// Wait for download to finish.
		/// </summary>
		/// <param name="Timeout">Timeout in milliseconds</param>
		public void Wait(int Timeout)
		{
			this._downloadMre.WaitOne(Timeout);
		}
		#endregion Public Methods

		#region Overrides
		public override string ToString()
		{
			int bytecount = 0;
			if (this._bytes != null)
				bytecount = this._bytes.Length;

			List<string> mirrors = new List<string>();
			foreach (Uri uri in this._mirrors)
				mirrors.Add(uri.ToString());
			foreach (Uri uri in this._failedMirrors)
				mirrors.Add(uri.ToString());
			return String.Format("bytes: {0}, md5: {1}, mirrors: {2}",
				bytecount,
				this._downloadedMD5,
				string.Join(", ", mirrors.ToArray()));
		}
		#endregion
	}
}
