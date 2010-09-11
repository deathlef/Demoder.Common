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
using System.Linq;
using System.Text;

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
		private readonly string _expectedMD5;
		private Queue<Uri> _mirrors;

		private List<Uri> _failedMirrors = null;

		//Describing the download data
		private byte[] _bytes = null;
		private object _httpStatusCode = null;
		private string _downloadedMD5 = string.Empty;
		#endregion

		#region Constructors
		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Userdefined tag of this download</param>
		/// <param name="Uri">Uri to download from</param>
		public DownloadItem(object Tag, Uri Uri) :
			this(Tag, new List<Uri>(new Uri[] { Uri })) { }
		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Uerdefined tag for this download</param>
		/// <param name="Mirrors">URIs to download from</param>
		public DownloadItem(object Tag, List<Uri> Mirrors) :
			this(Tag, Mirrors, string.Empty) { }
		/// <summary>
		/// Initialize a DownloadItem
		/// </summary>
		/// <param name="Tag">Userdefined tag for this download</param>
		/// <param name="Mirrors">URIs to download from</param>
		/// <param name="ExpectedMD5">The file should have this MD5 hash to be considered a successfull download</param>
		public DownloadItem(object Tag, List<Uri> Mirrors, string ExpectedMD5)
		{
			if (ExpectedMD5 == null)
				throw new ArgumentNullException("ExpectedMD5", "Parameter cannot be null. Use string.Empty instead.");

			this._tag = Tag;
			this._mirrors = new Queue<Uri>(Mirrors);
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
					if (this._expectedMD5 == string.Empty) //Since we have data, and no expected MD5, assume the download manager verified the server-reported MD5.
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
				return this._bytes;
			}
			set
			{
				lock (this._downloadedMD5)
				{
					this._bytes = value;
					if (value == null)
						this._downloadedMD5 = string.Empty;
					else
						this._downloadedMD5 = GenerateHash.MD5(value);
				}
			}
		}
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
						return this._mirrors.Dequeue();
				}
			}
		}
		#endregion

		#region Members
		public void FailMirror(Uri Uri)
		{
			lock (this._failedMirrors)
				this._failedMirrors.Add(Uri);
		}
		#endregion
	}
}
