/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://redmine.flw.nu/projects/demoder-common/)

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
	public interface IDownloadItem
	{
		#region Accessors
		/// <summary>
		/// Is the downloaded data as expected?
		/// </summary>
		bool IntegrityOK { get; }
		/// <summary>
		/// Retrieves the datas actual MD5 checksum
		/// </summary>
		MD5Checksum MD5 { get; }
		/// <summary>
		/// Retrieves the wanted MD5 checksum
		/// </summary>
		MD5Checksum WantedMD5 { get; }
		/// <summary>
		/// Get the downloaded bytes
		/// </summary>
		byte[] Data { get; }
		/// <summary>
		/// Userdefined tag for this item
		/// </summary>
		object Tag { get; }
		/// <summary>
		/// Peek at the top mirror.
		/// </summary>
		Uri Mirror { get; }
		/// <summary>
		/// Tags for the current mirror. Will be wiped on mirror cycle.
		/// </summary>
		List<object> MirrorTags { get; }
		/// <summary>
		/// Retrieves scheme://host:port representing the top mirror.
		/// </summary>
		string MirrorConnectionString { get; }
		/// <summary>
		/// File to save the downloaded data to.
		/// </summary>
		FileInfo SaveAs { get; }
		#endregion

		#region Methods
		/// <summary><![CDATA[Pass successfully downloaded data to this method.
		/// It will determine if the data is as expected.
		/// Returns: true if integrity is ok, otherwise false.]]>
		/// </summary>
		/// <param name="Bytes"></param>
		/// <returns><![CDATA[If integrity of data is ok: true
		/// If integrity isn't OK: false]]></returns>
		bool SuccessfullDownload(byte[] Bytes);
		/// <summary>
		/// Download failed. Move the mirror to the failed queue. If there are no more mirrors, call the DownloadFailed delegate and return false.
		/// </summary>
		/// <param name="CycleMirror">Should the current mirror be marked as failed?</param>
		/// <returns>true if we have more mirrors in queue, false otherwise</returns>
		bool FailedDownload(bool FailMirror);
		/// <summary>
		/// Wait for the download to finish
		/// </summary>
		void Wait();
		/// <summary>
		/// Wait for the download to finish
		/// </summary>
		/// <param name="timeout">Timeout in milliseconds.</param>
		void Wait(int timeout);
		#endregion
	}
}
