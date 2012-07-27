using Demoder.Common.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for MemoryCacheTargetTest and is intended
    ///to contain all MemoryCacheTargetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MemoryCacheTargetTest
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


        private static MemoryCacheTarget cache;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            cache = new MemoryCacheTarget();
        }


        /// <summary>
        ///A test for Retrieve
        ///</summary>
        [TestMethod()]
        public void TestA()
        {
            CacheEntry expected = new CacheEntry { Data = new byte[] { 25, 32, 67, 120, 80 }, Expirity = DateTime.Now.AddHours(1) };

            MemoryCacheTarget target = cache;
            target.Store(expected, "lala");
            
            CacheEntry actual;
            actual = target.Retrieve("lala");
            Assert.AreEqual(expected, actual);
            
        }
    }
}
