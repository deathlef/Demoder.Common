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
using System.Linq;
using System.Text;
using System.Threading;

namespace Demoder.Common
{
    public class ConcurrentBidirectionalMap<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> keyValueMap;
        private ConcurrentDictionary<TValue, TKey> valueKeyMap;
        private ReaderWriterLockSlim locker;

        IEqualityComparer<TKey> keyComparer;
        IEqualityComparer<TValue> valueComparer;

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

        public bool TryGetValue(TKey key, out TValue value)
        {
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

        public bool TryGetKey(TValue value, out TKey key)
        {
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


        public BidirectionalMapStoreResult TryStore(TKey key, TValue value)
        {
            this.locker.EnterWriteLock();
            BidirectionalMapStoreResult returnValue = BidirectionalMapStoreResult.Unmodified;
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
                    returnValue |= BidirectionalMapStoreResult.RemovedValue;

                    // Update key.
                    this.keyValueMap[key] = value;
                    returnValue |= BidirectionalMapStoreResult.ModifiedValue;
                }

                bool haveValue = this.valueKeyMap.ContainsKey(value);
                if (haveValue)
                {
                    // Find values old key.
                    TKey oldKey = this.valueKeyMap[value];
                    // Remove old key. Ignore value, as we know that's our current value.
                    TValue oldKeyValue;
                    this.keyValueMap.TryRemove(oldKey, out oldKeyValue);
                    returnValue |= BidirectionalMapStoreResult.RemovedKey;
                    // Remove old value map.
                    this.valueKeyMap.TryRemove(value, out oldKey);
                    returnValue |= BidirectionalMapStoreResult.ModifiedKey;
                }
                else
                {
                    returnValue |= BidirectionalMapStoreResult.AddedValue;
                }

                if (!haveKey)
                {
                    this.keyValueMap[key] = value;
                    returnValue |= BidirectionalMapStoreResult.AddedKey;
                }
                this.valueKeyMap[value] = key;
                return returnValue;
            }

            finally
            {
                this.locker.ExitWriteLock();
            }
        }
    }

    [Flags]
    public enum BidirectionalMapStoreResult : uint
    {
        /// <summary>
        /// No changes were made
        /// </summary>
        Unmodified = BitFlag.None,
        /// <summary>
        /// A value had its key association updated
        /// </summary>
        ModifiedValue = BitFlag.Bit0,
        /// <summary>
        /// A value was removed
        /// </summary>
        RemovedValue = BitFlag.Bit1,
        /// <summary>
        /// A key had its value association modified
        /// </summary>
        ModifiedKey=BitFlag.Bit2,
        /// <summary>
        /// A key was removed
        /// </summary>
        RemovedKey = BitFlag.Bit3,

        /// <summary>
        /// A value was added
        /// </summary>
        AddedValue = BitFlag.Bit4,
        /// <summary>
        /// A key was added
        /// </summary>
        AddedKey = BitFlag.Bit5,
    }
}
