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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Demoder.Common
{
	public class XmlCacheWrapper
	{
		#region Members
		private DirectoryInfo _rootDirectory;
		/// <summary>
		/// Minutes to hold items in cache.
		/// </summary>
		private int _cacheTime = 1440;
		/// <summary>
		/// Timeout for HTTP connections in milliseconds
		/// </summary>
		private int _fetchTimeout = 2000;
		private Dictionary<Type, Object> _xmlCache = new Dictionary<Type, Object>();
		#endregion
		#region Constructors
		/// <summary>
		/// Initializes the cache wrapper
		/// </summary>
		/// <param name="CacheRootDirectory">Root directory to store cache in</param>
		public XmlCacheWrapper(DirectoryInfo CacheRootDirectory)
		{
			this._rootDirectory = CacheRootDirectory;
		}

		/// <summary>
		/// Get an instance of XMLCache. If it doesn't exist, it will be created.
		/// </summary>
		/// <typeparam name="T">Class the XMLCache object is caching</typeparam>
		/// <returns></returns>
		public XMLCache<T> Get<T>() where T : class
		{
			if (!this._xmlCache.ContainsKey(typeof(T)))
				return this.Create<T>(this._cacheTime, this._fetchTimeout);
			else
				return (XMLCache<T>)this._xmlCache[typeof(T)];
		}

		/// <summary>
		/// Creates an instance of XMLCache for this type.
		/// </summary>
		/// <typeparam name="T">Class the XMLCache object is caching</typeparam>
		/// <param name="CacheTime">Time in minutes to keep items in cache</param>
		/// <param name="FetchTimeout">Timeout for HTTP connections, in milliseconds</param>
		/// <returns></returns>
		public XMLCache<T> Create<T>(int CacheTime, int FetchTimeout) where T : class
		{
			if (this._xmlCache.ContainsKey(typeof(T)))
				return this.Get<T>();
			lock (this._xmlCache)
			{
				XMLCache<T> ret = new XMLCache<T>(string.Format("{0}{1}{2}",
					this._rootDirectory.FullName,
					Path.DirectorySeparatorChar,
					typeof(T).GUID), CacheTime, FetchTimeout);
				//If typeof(t).GUID doesn't work as expected, use: GenerateHash.md5(typeof(T).Assembly.FullName + typeof(T).FullName)

				this._xmlCache.Add(typeof(T), ret); //Add to internal list.
				return ret;
			}
		}
		#endregion
	}
}
