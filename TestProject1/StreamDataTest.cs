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

using Demoder.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Demoder.Common;
using Demoder.Common.Tests.TestData;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Demoder.Common.Tests
{
    
    
    /// <summary>
    ///This is a test class for StreamDataTest and is intended
    ///to contain all StreamDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StreamDataTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CreateTest()
        {
            Type t = typeof(int);
            SuperStream ms = new SuperStream(Endianess.Little);
            var expected = new StreamDataTestData
            {
                A = -15,
                B = "Test one!",
                C = "Test two!",
                D = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })
            };
            ms.WriteInt32(expected.A);
            ms.WriteString(expected.B);
            ms.WriteCString(expected.C);
            ms.WriteUInt32((uint)expected.D.Count);
            foreach (var i in expected.D)
            {
                ms.WriteInt32(i);
            }
            
            ms.Position = 0;
            
            StreamDataTestData actual;
            actual = StreamData.Create<StreamDataTestData>(ms);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Serialize
        ///</summary>
        [TestMethod()]
        public void SerializeTest()
        {

            SuperStream expectedStream = new SuperStream(Endianess.Little);
            var expectedObject = new StreamDataTestData
            {
                A = -15,
                B = "Test one!",
                C = "Test two!",
                D = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })
            };
            expectedStream.WriteInt32(expectedObject.A);
            expectedStream.WriteString(expectedObject.B);
            expectedStream.WriteCString(expectedObject.C);
            expectedStream.WriteUInt32((uint)expectedObject.D.Count);
            foreach (var i in expectedObject.D)
            {
                expectedStream.WriteInt32(i);
            }
            var expected = ((MemoryStream)expectedStream.BaseStream).ToArray();

            var actualStream = new SuperStream(Endianess.Little);
            StreamData.Serialize(expectedObject, actualStream);

            var actual = ((MemoryStream)actualStream.BaseStream).ToArray();

            Assert.IsTrue(actual.SequenceEqual(expected));
        }
    }
}
