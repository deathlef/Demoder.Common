/*
Demoder.Common
Copyright (c) 2010-2012 Demoder <demoder@demoder.me>

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Demoder.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Type of key to be stored</typeparam>
    /// <typeparam name="TValue">Type of value to be stored</typeparam>
    public class ConcurrentBidirectionalMap<TKey, TValue> : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Contains a TKey to TValue relation
        /// </summary>
        private ConcurrentDictionary<TKey, TValue> keyValueMap;
        /// <summary>
        /// Contains a TValue to TKey relation
        /// </summary>
        private ConcurrentDictionary<TValue, TKey> valueKeyMap;

        private ReaderWriterLockSlim locker;

        /// <summary>
        /// Used to perform comparisons on TKey
        /// </summary>
        private IEqualityComparer<TKey> keyComparer;
        /// <summary>
        /// Used to perform comparisons on TValue
        /// </summary>
        private IEqualityComparer<TValue> valueComparer;

        public int Count
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.keyValueMap.Count;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }


        /// <summary>
        /// Initializes a new ConcurrentBidirectionalMap that is empty
        /// </summary>
        /// <param name="keyComparer">IEqualityComparer to compare keys, or null for default.</param>
        /// <param name="valueComparer">IEqualityComparer to compare values, or null for default.</param>
        public ConcurrentBidirectionalMap(IEqualityComparer<TKey> keyComparer=null, IEqualityComparer<TValue> valueComparer=null)
        {
            this.locker = new ReaderWriterLockSlim();

            if (keyComparer == null)
            {
                this.keyComparer = EqualityComparer<TKey>.Default;
            }
            else
            {
                this.keyComparer = keyComparer;
            }

            if (valueComparer == null)
            {
                this.valueComparer = EqualityComparer<TValue>.Default;
            }
            else
            {
                this.valueComparer = valueComparer;
            }


            this.keyValueMap = new ConcurrentDictionary<TKey, TValue>(this.keyComparer);
            this.valueKeyMap = new ConcurrentDictionary<TValue, TKey>(this.valueComparer);
        }

        /// <summary>
        /// Determines whether the <paramref name="key"/> exists
        /// </summary>
        /// <param name="key">The key to locate</param>
        /// <returns>true if <paramref name="key"/> is found, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">Key is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool ContainsKey(TKey key)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.locker.EnterReadLock();
            try
            {
                return this.keyValueMap.ContainsKey(key);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="value"/> exists
        /// </summary>
        /// <param name="value">The value to locate</param>
        /// <returns>true if <paramref name="value"/> is found, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">value is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool ContainsValue(TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.locker.EnterReadLock();
            try
            {
                return this.valueKeyMap.ContainsKey(value);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="key" /> and <paramref name="value"/> exists, and if they are associated.
        /// </summary>
        /// <param name="key">The key to locate</param>
        /// <param name="value">The value to locate</param>
        /// <returns>true if key and value exist and are associated with eachother. Otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">key or value is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool Contains(TKey key, TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.locker.EnterReadLock();
            try
            {
                if (!this.keyValueMap.ContainsKey(key))
                {
                    return false;
                }
                if (!this.valueComparer.Equals(this.keyValueMap[key], value))
                {
                    return false;
                }
                return true;
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Attempts to get the value associated with the specified <paramref name="key"/>
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">When this method returns, value contains the object with the specified value or the default value of <typeparamref name="TValue"/>, if the operation failed.</param>
        /// <returns>True if the key was found, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">Key is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.locker.EnterReadLock();
            try
            {
                return this.keyValueMap.TryGetValue(key, out value);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Attempts to get the key associated with the specified <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value of the key to get</param>
        /// <param name="key">When this method returns, key contains the object with the specified key or the default value of <typeparamref name="TKey"/>, if the operation failed.</param>
        /// <returns>True if the value was found, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">Key is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryGetKey(TValue value, out TKey key)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (value == null) 
            { 
                throw new ArgumentNullException("value"); 
            }

            this.locker.EnterReadLock();
            try
            {
                return this.valueKeyMap.TryGetValue(value, out key);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Stores a key/value combination, removing other key/value combination if necessary.
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value to add</param>
        /// <returns>An enumerator describing which alterations were made, if any.</returns>
        /// <exception cref="System.ArgumentNullException">key or value is a null reference.</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public MapAlteration Store(TKey key, TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.locker.EnterWriteLock();
            MapAlteration returnValue = MapAlteration.Unmodified;
            try
            { 
                bool haveKey = this.keyValueMap.ContainsKey(key);
                if (haveKey)
                {
                    if (this.valueComparer.Equals(this.keyValueMap[key], value))
                    {
                        // Value is the same. No changes necessary.
                        return returnValue;
                    }

                    // Value is different.
                    // Get old value.
                    TValue oldValue = this.keyValueMap[key];
                   
                    // Remove old value and ignore old values key, because it's the current key.
                    TKey oldValueKey;
                    this.valueKeyMap.TryRemove(oldValue, out oldValueKey);
                    returnValue |= MapAlteration.RemovedValue;

                    // Update key.
                    this.keyValueMap[key] = value;
                    returnValue |= MapAlteration.ModifiedKey;
                }

                bool haveValue = this.valueKeyMap.ContainsKey(value);
                if (haveValue)
                {
                    // Find values old key.
                    TKey oldKey = this.valueKeyMap[value];
                    // Remove old key. Ignore value, as we know that's our current value.
                    TValue oldKeyValue;
                    this.keyValueMap.TryRemove(oldKey, out oldKeyValue);
                    returnValue |= MapAlteration.RemovedKey;
                    // Remove old value map.
                    this.valueKeyMap.TryRemove(value, out oldKey);
                    returnValue |= MapAlteration.ModifiedValue;
                }
                else
                {
                    returnValue |= MapAlteration.AddedValue;
                }

                if (!haveKey)
                {
                    this.keyValueMap[key] = value;
                    returnValue |= MapAlteration.AddedKey;
                }
                this.valueKeyMap[value] = key;
                return returnValue;
            }

            finally
            {
                this.locker.ExitWriteLock();
            }
        }


        /// <summary>
        /// Attempts to add a specified key and value to the map
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value to add</param>
        /// <returns>true if the key/value was successfully added. false if key or value already exist.</returns>
        /// <exception cref="System.ArgumentNullException">key or value is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryStore(TKey key, TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.locker.EnterWriteLock();
            try
            {
                if (this.keyValueMap.ContainsKey(key))
                {
                    return false;
                }
                if (this.valueKeyMap.ContainsKey(value))
                {
                    return false;
                }
                this.keyValueMap[key] = value;
                this.valueKeyMap[value] = key;
                return true;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Attempts to remove the specified key and associated value, returning the value.
        /// </summary>
        /// <param name="key">The key of the element to remove and return</param>
        /// <param name="value">Value associated with key</param>
        /// <returns>True if a mapping was removed, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryRemoveKey(TKey key, out TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.locker.EnterWriteLock();
            try
            {
                if (!this.keyValueMap.TryRemove(key, out value))
                {
                    return false;
                }
                TKey tmp;
                this.valueKeyMap.TryRemove(value, out tmp);
                return true;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Attempts to remove the specified value and associated key, returning the key.
        /// </summary>
        /// <param name="value">The value of the element to remove and return</param>
        /// <param name="key">Key associated with value</param>
        /// <returns>True if a mapping was removed, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">value is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryRemoveValue(TValue value, out TKey key)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.locker.EnterWriteLock();
            try
            {
                if (!this.valueKeyMap.TryRemove(value, out key))
                {
                    return false;
                }
                TValue tmp;
                this.keyValueMap.TryRemove(key, out tmp);
                return true;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a key=value combination, but only if key and value exist, and they're associated with eachother.
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="value">Value to remove</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">key or value is a null reference</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public bool TryRemove(TKey key, TValue value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.locker.EnterWriteLock();
            try
            {
                if (!this.keyValueMap.ContainsKey(key))
                {
                    // Don't have key
                    return false;
                }
                if (!this.valueComparer.Equals(this.keyValueMap[key], value))
                {
                    // Value isn't what we want to remove
                    return false;
                }
                // Remove key and value
                TKey outKey;
                TValue outValue;
                if (!this.keyValueMap.TryRemove(key, out outValue))
                {
                    throw new InvalidDataException("Have key but can't remove it. Key: " + key.ToString());
                }
                if (!this.valueKeyMap.TryRemove(value, out outKey))
                {
                    throw new InvalidDataException("Have value but can't remove it. Value: " + value.ToString());
                }
                return true;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Tests the integrity of this instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method is only used for unit tests during development of this class, and will be removed in a future version.")]
        public bool TestIntegrity()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(String.Format("ConcurrentBidirectionalMap<{0},{1}>", typeof(TKey), typeof(TValue)));
            }

            this.locker.EnterReadLock();
            try
            {
                foreach (var kvp in this.keyValueMap)
                {
                    if (!this.valueKeyMap.ContainsKey(kvp.Value))
                    {
                        return false;
                    }
                    if (!this.keyComparer.Equals(this.valueKeyMap[kvp.Value], kvp.Key))
                    {
                        return false;
                    }
                }

                foreach (var kvp in this.valueKeyMap)
                {
                    if (!this.keyValueMap.ContainsKey(kvp.Value))
                    {
                        return false;
                    }
                    if (!this.valueComparer.Equals(this.keyValueMap[kvp.Value], kvp.Key))
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {

                this.locker.ExitReadLock();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool includeManaged)
        {
            if (!includeManaged) { return; }
            this.disposed = true;
            this.locker.Dispose();
            this.keyValueMap.Clear();
            this.valueKeyMap.Clear();
        }
    }

    [Flags]
    public enum MapAlteration : uint
    {
        /// <summary>
        /// No changes were made
        /// </summary>
        Unmodified = BitFlag.None,

        /// <summary>
        /// An existing value received a new key.
        /// </summary>
        /// <remarks>
        /// This is often paired with <see cref="MapAlteration.RemoveKey" /> as the old key is no longer associated with a value.
        /// </remarks>
        ModifiedValue = BitFlag.Bit0,
        
        /// <summary>
        /// A value was removed
        /// </summary>
        RemovedValue = BitFlag.Bit1,

        /// <summary>
        /// A value was added
        /// </summary>
        AddedValue = BitFlag.Bit2,
        


        /// <summary>
        /// An existing key received a new value.
        /// </summary>
        /// <remarks>
        /// This is often paired with <see cref="MapAlteration.RemovedValue"/> as the old value is no longer associated with a key.
        /// </remarks>
        ModifiedKey=BitFlag.Bit3,
        
        /// <summary>
        /// An existing key was removed.
        /// </summary>
        /// <remarks>
        /// This is often paired with <see cref="MapAlteration.RemovedValue"/> or <see cref="MapAlteration.ModifiedValue"/>
        /// </remarks>
        RemovedKey = BitFlag.Bit4,

        /// <summary>
        /// A key was added
        /// </summary>
        AddedKey = BitFlag.Bit5,
    }
}
