using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demoder.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> obj)
        {
            int maxPos = obj.Count() -1;
            int pos = Misc.NewRandom().Next(0, maxPos);
            return obj.ElementAt(pos);
        }
    }
}
