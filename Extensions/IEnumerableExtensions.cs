using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demoder.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns a random entry (or default if empty) from the IEnumerable instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Random<T>(this IEnumerable<T> obj)
        {
            if (obj.Count() == 0) { return default(T); }
            int maxPos = obj.Count() -1;
            int pos = Misc.NewRandom().Next(0, maxPos);
            return obj.ElementAt(pos);
        }
    }
}
