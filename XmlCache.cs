/*
* Vha.Common
* Copyright (C) 2005-2010 Remco van Oosterhout
* See Credits.txt for all aknowledgements.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

/*
MIT Licence
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: https://sourceforge.net/projects/demoderstools/)

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

// This class has been adapted to work with Demoder.Common. 

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Demoder.Common
{
	[Flags]
	public enum XMLCacheFlags
	{
		/// <summary>
		/// Retrieve the object from recent cache
		/// </summary>
		ReadCache = 0x01,
		/// <summary>
		/// Write the object to recent cache if pulled from live data source
		/// </summary>
		WriteCache = 0x02,
		/// <summary>
		/// Retrieve the object from live data source
		/// </summary>
		ReadLive = 0x04,
		/// <summary>
		/// Retrieve the object from cache as last resort, regardless of age
		/// </summary>
		ReadExpired = 0x08,
		/// <summary>
		/// The default flags
		/// </summary>
		Default = ReadCache | WriteCache | ReadLive
	}

	public class XMLCache<T>
		where T : class
	{
		public string Path { get { return this._path; } }
		public int Duration { get { return this._duration; } }
		public int Timeout { get { return this._timeout; } }

		/// <summary>
		/// Initializes a new XMLCache object
		/// </summary>
		/// <param name="path">The absolute or relative path to the directory to store the cache files in</param>
		/// <param name="duration">The duration of the cache should hold objects for in minutes</param>
		/// <param name="timeout">The timeout of connecting to the web in miliseconds</param>
		public XMLCache(string path, int duration, int timeout)
		{
			this._path = path;
			this._duration = duration;
			this._timeout = timeout;
		}

		public T Request(string url, params string[] args)
		{ return this.Request(XMLCacheFlags.Default, url, args); }

		public T Request(XMLCacheFlags source, string url, params string[] args)
		{ return this.Request(source, new Uri[] { new Uri(url) }, args); }

		public T Request(XMLCacheFlags source, string[] urls, params string[] args)
		{ 
			List<Uri> uris = new List<Uri>();
			foreach (string s in urls)
				uris.Add(new Uri(s));

			return this.Request(source, uris.ToArray(), args); 
		}

		public T Request(XMLCacheFlags source, Uri[] urls, params string[] args)
		{
			if (args.Length == 0)
				throw new ArgumentException("expecting at least 1 argument");
			// Construct path and filename
			string path = this.GetPath(args);
			string file = path + this.GetFile(args);
			// Check if a cached entry exists
			T obj = null;
			if ((source & XMLCacheFlags.ReadCache) != 0 && IsCached(true, file))
			{
				lock (this)
					obj = Xml.Deserialize<T>(new FileInfo(file), false);
				if (obj != null)
					return obj;
			}
			// Fetch fresh
			if ((source & XMLCacheFlags.ReadLive) != 0)
			{
				//Perform parameter replacing on URIs, if any such syntax is specified in the URIs.
				List<Uri> uris = new List<Uri>();
				foreach (Uri uri in urls)
					uris.Add(new Uri(string.Format(uri.ToString(), args)));
				urls = uris.ToArray();

				obj = Xml.Deserialize<T>(urls);
				if (obj != null && (source & XMLCacheFlags.WriteCache) != 0)
				{
					// Write cache
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					lock (this)
						Xml.Serialize<T>(new FileInfo(file), obj, false);
				}
			}
			// Retreive old copy from cache
			if (obj == null && (source & XMLCacheFlags.ReadExpired) != 0)
			{
				if (IsCached(false, file))
					lock (this)
						obj = Xml.Deserialize<T>(new FileInfo(file), false);
			}
			// Finally, return the object
			return obj;
		}

		public bool Cache(T obj, params string[] args)
		{
			if (obj == null) return false;
			string path = this.GetPath(args);
			string file = path + this.GetFile(args);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			lock (this)
				return Xml.Serialize<T>(new FileInfo(file), obj, false);
		}

		public bool IsCached(params string[] args) { return this.IsCached(true, args); }
		public bool IsCached(bool checkDuration, params string[] args) { return this.IsCached(checkDuration, this.GetFilePath(args)); }
		private bool IsCached(bool checkDuration, string file)
		{
			lock (this)
			{
				if (File.Exists(file))
				{
					if (!checkDuration) return true;
					// Check file age
					TimeSpan time = DateTime.Now - File.GetLastWriteTime(file);
					if (time.TotalMinutes <= this.Duration)
						return true;
				}
			}
			return false;
		}

		public string GetPath(params string[] args)
		{
			StringBuilder path = new StringBuilder();
			if (string.IsNullOrEmpty(this._path))
				path.Append(".");
			else
				path.Append(this._path);
			path.Append(System.IO.Path.DirectorySeparatorChar);
			path.Append(string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), args, 0, args.Length - 1));
			if (args.Length > 1)
				path.Append(System.IO.Path.DirectorySeparatorChar);
			return path.ToString();
		}

		public string GetFile(params string[] args)
		{
			return args[args.Length - 1] + ".xml";
		}

		public string GetFilePath(params string[] args)
		{
			return this.GetPath(args) + this.GetFile(args);
		}

		private string _path;
		private int _duration;
		private int _timeout;
	}
}
