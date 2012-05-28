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

using Demoder.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Demoder.Common.Tests
{
    /// <summary>
    ///This is a test class for SuperStreamTest and is intended
    ///to contain all SuperStreamTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SuperStreamTest
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
        ///A test for ReadBytes
        ///</summary>
        [TestMethod()]
        public void BytesTest()
        {
            var expected = new byte[] { 0, 15, 35, 255, 46, 80, 0 };
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteBytes(expected);
            target.Position=0;
            var actual = target.ReadBytes((uint)expected.Length);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        ///A test for ReadCString
        ///</summary>
        [TestMethod()]
        public void CStringTestA()
        {
            var expected = "Hello there! This is a test.";
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteCString(expected);
            target.Position = 0;
            var actual = target.ReadCString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadCString
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void CStringTestB()
        {
            var expected = "Hello there! This is a\0 test.";
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteCString(expected);
            target.Position = 0;
            var actual = target.ReadCString();

            Assert.Equals(expected, actual);
        }

        /// <summary>
        ///A test for ReadDouble
        ///</summary>
        [TestMethod()]
        public void DoubleTest()
        {
            Double expected = Single.MaxValue - 64.64;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteDouble(expected);
            target.Position = 0;
            var actual = target.ReadDouble();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadInt16
        ///</summary>
        [TestMethod()]
        public void Int16Test()
        {
            Int16 expected = Int16.MaxValue-16;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteInt16(expected);
            target.Position = 0;
            var actual = target.ReadInt16();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadInt32
        ///</summary>
        [TestMethod()]
        public void Int32Test()
        {
            Int32 expected = Int32.MaxValue-32;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteInt32(expected);
            target.Position = 0;
            var actual = target.ReadInt32();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadInt64
        ///</summary>
        [TestMethod()]
        public void Int64Test()
        {
            Int64 expected = Int64.MaxValue-64;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteInt64(expected);
            target.Position = 0;
            var actual = target.ReadInt64();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LolTeger32
        ///</summary>
        [TestMethod()]
        public void LolTeger32TestA()
        {
            Int32 expected = 30;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteLolTeger32(expected);
            target.Position = 0;
            var actual = target.ReadLolTeger32();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LolTeger32, using all iterations of additional data.
        ///</summary>
        [TestMethod()]
        public void LolTeger32TestB()
        {
            for (int expectedAdd=0; expectedAdd<1009; expectedAdd++)
            {
                Int32 expected = 30;

                SuperStream target = new SuperStream(Endianess.Little);
                target.WriteLolTeger32(
                    expected,
                    additionalData: expectedAdd);
                target.Position = 0;

                int actualAdd;
                var actual = target.ReadLolTeger32(out actualAdd);

                Assert.AreEqual(expected, actual, "LolTeger value; i=" + expectedAdd.ToString());
                Assert.AreEqual(expectedAdd, actualAdd, "LolTeger additional value; i=" + expectedAdd.ToString());
            }
        }

        /// <summary>
        ///A test for ReadSingle
        ///</summary>
        [TestMethod()]
        public void SingleTest()
        {
            Single expected = Single.MaxValue - 32.32f;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteSingle(expected);
            target.Position = 0;
            var actual = target.ReadSingle();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadString
        ///</summary>
        [TestMethod()]
        public void StringTest()
        {
            string expected = "Hello there!\0Yo!";
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteString(expected);
            target.Position = 0;
            var actual = target.ReadString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadUInt16
        ///</summary>
        [TestMethod()]
        public void ReadUInt16Test()
        {
            UInt16 expected = UInt16.MaxValue - 16;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteSingle(expected);
            target.Position = 0;
            var actual = target.ReadSingle();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadUInt32
        ///</summary>
        [TestMethod()]
        public void ReadUInt32Test()
        {
            UInt32 expected = UInt32.MaxValue-32;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteSingle(expected);
            target.Position = 0;
            var actual = target.ReadSingle();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadUInt64
        ///</summary>
        [TestMethod()]
        public void ReadUInt64Test()
        {
            UInt64 expected = UInt64.MaxValue - 64;
            SuperStream target = new SuperStream(Endianess.Little);
            target.WriteSingle(expected);
            target.Position = 0;
            var actual = target.ReadSingle();

            Assert.AreEqual(expected, actual);
        }
    }
}
