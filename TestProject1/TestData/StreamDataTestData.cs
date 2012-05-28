using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demoder.Common.Attributes;
using Demoder.Common;

namespace Demoder.Common.Tests.TestData
{

    public class StreamDataTestData
    {
        [StreamData(0)]
        public int A { get; set; }

        [StreamData(1)]
        [StreamDataString(StringType.Normal)]
        public string B { get; set; }

        [StreamData(2)]
        [StreamDataString(StringType.CString)]
        public string C { get; set; }


        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, this)) { return true; }
            if (Object.ReferenceEquals(obj, null)) { return false; }
            if (obj is StreamDataTestData)
            {
                var obj2 = obj as StreamDataTestData;
                if (obj2.A != this.A) { return false; }
                if (obj2.B != this.B) { return false; }
                if (obj2.C != this.C) { return false; }
                return true;
            }
            return base.Equals(obj);
        }
    }
}
