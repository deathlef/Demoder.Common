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
using System.IO;
using System.Collections.Generic;
using System.Text;

using Demoder.Common.Hash;

namespace Demoder.Common.Net
{
	/// <summary>
	/// This class represents a single download.
	/// </summary>
	public class DownloadItem
	{
		#region Members
		//Describing the download task
		private readonly object _tag;
		private readonly MD5Checksum _expectedMD5;
		private Queue<Uri> _mirrors;

		private List<Uri> _failedMirrors;
		/// <summary>
		/// A list of tags assigned to this object.
		/// </summary>
		public List<object> InfoTags = new List<object>();

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
			List<Uri> Mirrors,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate) :
			this(Tag, Mirrors, DownloadSuccessDelegate, DownloadFailureDelegate, null) { }


		public DownloadItem(object Tag,
			List<Uri> Mirrors,
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
			List<Uri> Mirrors,
			DownloadItemEventHandler DownloadSuccessDelegate,
			DownloadItemEventHandler DownloadFailureDelegate,
			MD5Checksum ExpectedMD5)
		{
			if (ExpectedMD5 == null)
				throw new ArgumentNullException("ExpectedMD5", "Parameter cannot be null. Use string.Empty instead.");

			this._tag = Tag;
			this._mirrors = new Queue<Uri>(Mirrors);
			this._failedMirrors = new List<Uri>(Mirrors.Count);
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
				lock (this._downloadedMD5)
				{
					if (this._bytes == null) //Don't have data. Assume the download failed.
						return false;
					if (this._expectedMD5==null) //Since we have data, and no expected MD5, assume the download manager verified the server-reported MD5.
						return true;
					if (this._expectedMD5 == this._downloadedMD5) //Integrity ok
						return true;
				}
				return false; //All other scenarios, download integrity is not ok.
			}
		}
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
						this.Data = File.ReadAllBytes(this._saveAs.FullName);
					}
					catch { }
					return this._bytes;
				}
				return this._bytes;
			}
			set
			{
				lock (this._downloadedMD5)
				{
					this._bytes = value;
					if (value == null)
						this._downloadedMD5 = null;
					else
						this._downloadedMD5 = Generate.MD5(value);
				}
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
		/// Retrieve the next mirror from the queue.
		/// </summary>
		public Uri NextMirror
		{
			get
			{
				lock (this._mirrors)
				{
					if (this._mirrors.Count == 0)
						return null;
					else
						return this._mirrors.Peek();
				}
			}
		}

		/// <summary>
		/// Retrieves scheme://host:port representing the next mirror.
		/// </summary>
		public string NextMirrorConnectionString
		{
			get
			{
				Uri nextMirror = this.NextMirror;
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

		/// <summary>
		/// Download failed. Move the mirror to the failed queue and call the DownloadFailed delegate.
		/// </summary>
		/// <returns>true if we have more items in queue, false otherwise.</returns>
		public bool DownloadFailed()
		{
			Uri uri;
			lock (this._mirrors)
				uri = this._mirrors.Dequeue();
			lock (this._failedMirrors)
				this._failedMirrors.Add(uri);

			if (this._mirrors.Count == 0)
			{
				DownloadItemEventHandler dieh=null;
				if (this._downloadFailureDelegate!=null)
					lock (this._downloadFailureDelegate)
						dieh = this._downloadFailureDelegate;
				if (dieh != null)
					dieh(this);
				return false;
			}
			return true;
		}
		#endregion

		#region Methods
		/// <summary>
		/// This method will call the DownloadSuccessDelegate.
		/// </summary>
		public void SuccessfullDownload()
		{
			DownloadItemEventHandler dieh;
			lock (this._downloadSuccessDelegate)
				dieh = this._downloadSuccessDelegate;
			if (dieh != null)
				dieh(this);
		}
		#endregion

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
