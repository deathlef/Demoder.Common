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

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for MathsTest and is intended
    ///to contain all MathsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MathsTest
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
        ///A test for Percent
        ///</summary>
        [TestMethod()]
        public void PercentTestA()
        {
            int MaxValue = 100;
            int CurrentValue = 25;
            int expected = 25;
            int actual;
            actual = Maths.Percent(MaxValue, CurrentValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Percent
        ///</summary>
        [TestMethod()]
        public void PercentTestB()
        {
            int MaxValue = 200; 
            int CurrentValue = 50;
            int expected = 25;
            int actual;
            actual = Maths.Percent(MaxValue, CurrentValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DePercent
        ///</summary>
        [TestMethod()]
        public void DePercentTestA()
        {
            int MaxValue = 100;
            int Percent = 25;
            int expected = 25;
            int actual;
            actual = Maths.DePercent(MaxValue, Percent);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DePercent
        ///</summary>
        [TestMethod()]
        public void DePercentTestB()
        {
            int MaxValue = 200;
            int Percent = 25;
            int expected = 50;
            int actual;
            actual = Maths.DePercent(MaxValue, Percent);
            Assert.AreEqual(expected, actual);
        }
    }
}
