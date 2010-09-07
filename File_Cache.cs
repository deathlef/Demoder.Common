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
using System.Xml.Serialization;

namespace Demoder.Common
{
	public class File_Cache
	{
		#region members
		/// <summary>
		/// This is an index of all the cached data
		/// </summary>
		private Dictionary<string, CacheInfo> _cacheIndex = new Dictionary<string, CacheInfo>();
		/// <summary>
		/// Monitors the cache directory for changes
		/// </summary>
		private FileSystemWatcher _fsWatcher;

		private DirectoryInfo _cacheRootDirectory;
		private DirectoryInfo _cacheIndexDirectory;
		private DirectoryInfo _cacheDataDirectory;
		#endregion
		#region Constructors
		/// <summary>
		/// Initializes the URL cache
		/// </summary>
		/// <param name="RootDirectory">Directory used for storage</param>
		public File_Cache(DirectoryInfo RootDirectory)
		{
			//Define cache directories
			this._cacheRootDirectory = RootDirectory;
			this._cacheIndexDirectory = new DirectoryInfo(RootDirectory.FullName + Path.DirectorySeparatorChar + "Index");
			this._cacheDataDirectory = new DirectoryInfo(RootDirectory.FullName + Path.DirectorySeparatorChar + "Data");
			//Check if cache directories exist
			if (!this._cacheRootDirectory.Exists)
				RootDirectory.Create();
			if (!this._cacheIndexDirectory.Exists)
				this._cacheIndexDirectory.Create();
			if (!this._cacheDataDirectory.Exists)
				this._cacheDataDirectory.Create();
			
			//initialize the fsWatcher
			this._fsWatcher = new FileSystemWatcher(RootDirectory.FullName);
		}
		#endregion
		#region fsWatcher implementation

		#endregion

		#region Methods
		public void FileCache(string Key, byte[] Data)
		{
			lock (this._cacheIndex)
			{
				string md5 = GenerateHash.md5(Data);
				//Is this data the same as the old?
				if (this._cacheIndex.ContainsKey(Key))
					if (this._cacheIndex[Key].Hash == md5)
						return;

				//If we made it here, it's a change to the old value.
				CacheInfo ci = new CacheInfo(Key, md5);
				FileInfo dataFile = new FileInfo(this._getDataFileName(Key));
				FileInfo indexFile = new FileInfo(this._getIndexFileName(Key));
				try
				{
					//Update the cache.
					File.WriteAllBytes(dataFile.FullName, Data); //Write the data
					Xml.Serialize<CacheInfo>(indexFile, ci, false); //Write the index
					this._cacheIndex[Key] = ci; //Store index in memory
				}
				catch
				{
					//If something fails with the caching, remove the cache entry.
					if (this._cacheIndex.ContainsKey(Key))
					{
						this._cacheIndex.Remove(Key);
					}
					try
					{
						indexFile.Delete();
						dataFile.Delete();
					}
					catch { return; }
					return;
				}
			}
		}

		/// <summary>
		/// Reads a file from cache.
		/// </summary>
		/// <param name="Key">ID of file to read</param>
		/// <returns></returns>
		public byte[] FileRead(string Key)
		{
			lock (this._cacheIndex)
			{
				if (!this._cacheIndex.ContainsKey(Key))
					return null;
				else
				{
					byte[] bytes = File.ReadAllBytes(this._getDataFileName(Key));
					//Ensure the index is up to date.
					string md5 = GenerateHash.md5(bytes);
					if (md5 != this._cacheIndex[Key].Hash)
						this.FileCache(Key, bytes);
					return bytes;
				}
			}
		}

		public DateTime FileTime(string Key)
		{
			lock (this._cacheIndex)
			{
				if (this._cacheIndex.ContainsKey(Key))
					return new FileInfo(this._getDataFileName(Key)).LastWriteTime;
				else
					return default(DateTime);
			}
		}

		private string _getIndexFileName(string Key) 
		{
			return this._cacheIndexDirectory.FullName + Path.DirectorySeparatorChar + GenerateHash.md5(Key) + ".xml";
		}
		private string _getDataFileName(string Key)
		{
			return this._cacheDataDirectory.FullName + Path.DirectorySeparatorChar + GenerateHash.md5(Key) + ".data";
		}
		#endregion


		#region data classes
		public class CacheInfo
		{
			#region members
			private string _key = default(string);
			private string _hash = default(string);
			#endregion
			#region constructors
			public CacheInfo()
			{
			}
			public CacheInfo(string Key, string MD5Hash)
			{
				this._key = Key;
				this._hash = MD5Hash;
			}
			#endregion
			#region accessors
			/// <summary>
			/// Unique key of the data
			/// </summary>
			public string Key
			{
				get { return this._key; }
				set
				{
					if (this._key == default(string))
						this._key = value;
					else
						throw new InvalidOperationException("Key may not be changed after initialization of object.");
				}
			}

			/// <summary>
			/// Hash value of the data
			/// </summary>
			public string Hash
			{
				get { return this._hash; }
				set { this._hash = value; }
			}
			#endregion
		}
		#endregion
	}
}
