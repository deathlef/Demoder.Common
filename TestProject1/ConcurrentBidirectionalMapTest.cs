/*
Demoder.Common
Copyright (c) 2010,2011,2012 Demoder <demoder@demoder.me>

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Demoder.Common;

namespace TestProject1
{
    [TestClass]
    public class ConcurrentBidirectionalMapTest
    {
        [TestMethod]
        public void TryStoreNewTestMethod()
        {
            var map = new ConcurrentBidirectionalMap<int, char>();

            MapAlteration expected;
            MapAlteration actual;

            expected = MapAlteration.AddedKey | MapAlteration.AddedValue;
            actual = map.Store(1, 'a');
            Assert.AreEqual(expected, actual, "Add new key/value pair (1=a)");

            expected = MapAlteration.AddedKey | MapAlteration.AddedValue;
            actual = map.Store(2, 'b');
            Assert.AreEqual(expected, actual, "Add new key/value pair (2=b)");

            expected = MapAlteration.AddedKey | MapAlteration.AddedValue;
            actual = map.Store(3, 'c');
            Assert.AreEqual(expected, actual, "Add new key/value pair (3=c)");

            Assert.AreEqual<int>(3, map.Count, "Number of entries");
            Assert.IsTrue(map.TestIntegrity());
        }



        [TestMethod]
        public void TryStoreModifyATestMethod()
        {
            var map = this.CreateMap();
            // Done setting up

            MapAlteration expected;
            MapAlteration actual;
        
            expected = MapAlteration.AddedKey | MapAlteration.RemovedKey | MapAlteration.ModifiedValue;
            actual = map.Store(4, 'a');
            Assert.AreEqual(expected, actual, "Add new key with pre-existing value");
            Assert.AreEqual<int>(3, map.Count);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryStoreModifyBTestMethod()
        {
            var map = this.CreateMap();
            // Done setting up


            MapAlteration expected;
            MapAlteration actual;

            
            // Modifying 3
            expected = MapAlteration.ModifiedKey;
            // Remove 1
            expected |= MapAlteration.RemovedKey; 
            // a now points to 3 instead of 1
            expected|= MapAlteration.ModifiedValue;
            // c is orphaned, and thus, removed.
            expected |= MapAlteration.RemovedValue;

            actual = map.Store(3, 'a');
            Assert.AreEqual(expected, actual, "Update key with value of other key");

            Assert.AreEqual<int>(2, map.Count);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryGetKeyATestMethod()
        {
            var map = this.CreateMap();
            // Done setting up

            int expected = 1;
            int actual;
            map.TryGetKey('a', out actual);

            Assert.AreEqual<int>(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryGetKeyBTestMethod()
        {
            var map = this.CreateMap();
            // Done setting up

            map.Store(3, 'a');

            int expected = 3;
            int actual;
            map.TryGetKey('a', out actual);

            Assert.AreEqual<int>(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }


        [TestMethod]
        public void TryGetValueATestMethod()
        {
            var map = this.CreateMap();
            // Done setting up

            char expected = 'c';
            char actual;
            map.TryGetValue(3, out actual);

            Assert.AreEqual<char>(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryGetValueBTestMethod()
        {
            var map = this.CreateMap();   
            // Done setting up

            map.Store(3, 'a');

            char expected = 'a';
            char actual;
            map.TryGetValue(3, out actual);

            Assert.AreEqual<char>(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }
        #region TryRemove
        [TestMethod]
        public void TryRemoveTestMethodA()
        {
            var map = this.CreateMap();

            bool expected = false;
            bool actual = map.TryRemove(1, 'b');
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryRemoveTestMethodB()
        {
            var map = this.CreateMap();

            bool expected = true;
            bool actual = map.TryRemove(1, 'a');
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryRemoveKeyTestMethodA()
        {
            var map = this.CreateMap();

            bool expectedReturn = true;
            char expectedValue = 'a';
            char actualValue;
            bool actualReturn = map.TryRemoveKey(1, out actualValue);

            Assert.AreEqual(expectedReturn, actualReturn, "return value");
            Assert.AreEqual(expectedValue, actualValue, "associated value");
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryRemoveKeyTestMethodB()
        {
            var map = this.CreateMap();

            bool expectedReturn = false;
            char actualValue;
            bool actualReturn = map.TryRemoveKey(4, out actualValue);

            Assert.AreEqual(expectedReturn, actualReturn, "return value");
            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryRemoveValueTestMethodA()
        {
            var map = this.CreateMap();

            bool expectedReturn = true;
            int expectedKey = 1;
            int actualKey;
            bool actualReturn = map.TryRemoveValue('a', out actualKey);

            Assert.AreEqual(expectedReturn, actualReturn, "return value");
            Assert.AreEqual(expectedKey, actualKey, "associated key");

            Assert.IsTrue(map.TestIntegrity());
        }

        [TestMethod]
        public void TryRemoveValueTestMethodB()
        {
            var map = this.CreateMap();

            bool expectedReturn = false;
            int actualKey;
            bool actualReturn = map.TryRemoveValue('d', out actualKey);

            Assert.AreEqual(expectedReturn, actualReturn, "return value");
            Assert.IsTrue(map.TestIntegrity());
        }

        #endregion

        #region Contains
        [TestMethod]
        public void ContainsKeyATestMethod()
        {
            var map = this.CreateMap();
            bool expected = true;
            bool actual = map.ContainsKey(1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ContainsKeyBTestMethod()
        {
            var map = this.CreateMap();
            bool expected = false;
            bool actual = map.ContainsKey(-5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ContainsValueATestMethod()
        {
            var map = this.CreateMap();
            bool expected = true;
            bool actual = map.ContainsValue('a');
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ContainsValueBTestMethod()
        {
            var map = this.CreateMap();
            bool expected = false;
            bool actual = map.ContainsValue('z');
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ContainsATestMethod()
        {
            var map = this.CreateMap();
            map.Store(4, 'd');
            map.Store(5, 'd');

            map.Store(5, 'a');

            /* Map should look like this now:
             * 2 = b
             * 3 = c
             * 4 = d
             * 5 = a
             */

            bool expected = false;
            bool actual = map.ContainsKey(1);
            Assert.AreEqual(expected, actual, "Key=1");

            expected = true;
            actual = map.ContainsValue('a');
            Assert.AreEqual(expected, actual, "Value=a");
        }

        [TestMethod]
        public void ContainsBTestMethod()
        {
            var map = this.CreateMap();
            map.Store(4, 'd');
            map.Store(5, 'd');

            map.Store(5, 'a');
            map.Store(3, 'd');


            /* Map should look like this now:
             * 2 = b
             * 3 = d
             * 5 = a
             */
            bool expected = false;
            bool actual = map.Contains(2, 'a');
            Assert.AreEqual(expected, actual, "Key=2 Value=a");

            expected = true;
            actual = map.Contains(2, 'b');
            Assert.AreEqual(expected, actual, "Key=2 Value=b");

            expected = false;
            actual = map.Contains(66, 'z');
            Assert.AreEqual(expected, actual, "Key=66 Value=z");
        }

        [TestMethod]
        public void AdvancedATestMethod()
        {
            var map = this.CreateMap();
            map.Store(4, 'd');
            map.Store(5, 'd');

            map.Store(5, 'a');
            map.Store(3, 'd');
            

            /* Map should look like this now:
             * 2 = b
             * 3 = d
             * 5 = a
             */

            // Test keys
            bool expectedReturn = true;
            char expectedValue = 'b';
            char actualValue;
            bool actualReturn = map.TryGetValue(2, out actualValue);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedValue, actualValue);

            expectedReturn = false;
            actualReturn = map.TryGetValue(1, out actualValue);
            Assert.AreEqual(expectedReturn, actualReturn);

            expectedReturn = true;
            expectedValue = 'd';
            actualReturn = map.TryGetValue(3, out actualValue);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedValue, actualValue);

            expectedReturn = true;
            expectedValue = 'a';
            actualReturn = map.TryGetValue(5, out actualValue);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void AdvancedBTestMethod()
        {
            var map = this.CreateMap();
            map.Store(4, 'd');
            map.Store(5, 'd');

            map.Store(5, 'a');
            map.Store(3, 'd');


            /* Map should look like this now:
             * 2 = b
             * 3 = d
             * 5 = a
             */

            // Test keys
            bool expectedReturn = true;
            int expectedKey = 2;
            int actualKey;
            bool actualReturn = map.TryGetKey('b', out actualKey);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedKey, actualKey);

            expectedReturn = false;
            actualReturn = map.TryGetKey('c', out actualKey);
            Assert.AreEqual(expectedReturn, actualReturn);

            expectedReturn = true;
            expectedKey = 3;
            actualReturn = map.TryGetKey('d', out actualKey);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedKey, actualKey);

            expectedReturn = true;
            expectedKey = 5;
            actualReturn = map.TryGetKey('a', out actualKey);
            Assert.AreEqual(expectedReturn, actualReturn);
            Assert.AreEqual(expectedKey, actualKey);
        }
        #endregion


        #region Test for null parameters
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestStoreValueNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.Store("hello", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestStoreKeyNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.Store(null, "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTryStoreValueNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.TryStore("hello", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTryStoreKeyNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.TryStore(null, "hello");
        }


        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsValueNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.ContainsValue(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsKeyNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.ContainsKey(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsNullValue()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.Contains("hello", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsNullKey()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            map.Contains(null, "hello");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryGetKeyNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string tmp;
            map.TryGetKey(null, out tmp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryGetValueNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string tmp;
            map.TryGetValue(null, out tmp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryRemoveKeyNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string key;
            map.TryRemoveKey(null, out key);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryRemoveValueNull()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string key;
            map.TryRemoveValue(null, out key);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryRemoveNullKey()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string key;
            map.TryRemove(null, "lala");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryRemoveNullValue()
        {
            var map = new ConcurrentBidirectionalMap<string, string>();
            string key;
            map.TryRemove("lala", null);
        }

        #endregion


        private ConcurrentBidirectionalMap<int, char> CreateMap()
        {
            var map = new ConcurrentBidirectionalMap<int, char>();
            map.Store(1, 'a');
            map.Store(2, 'b');
            map.Store(3, 'c');
            return map;
        }
    }
}
