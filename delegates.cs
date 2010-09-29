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
using System.Text;
using System.Net;
using Demoder.Common.Net;

namespace Demoder.Common
{
	/// <summary>
	/// Used to pass DownloadItem between methods.
	/// </summary>
	/// <param name="DownloadItem"></param>
	public delegate void DownloadItemEventHandler(DownloadItem DownloadItem);

	/// <summary>
	/// Used to pass data between Downloader in slave mode and the calling application.
	/// </summary>
	/// <param name="DownloaderInstance"></param>
	/// <param name="DownloadItem"></param>
	public delegate void DownloaderSlaveEventHandler(Downloader DownloaderInstance, DownloadItem DownloadItem);
}