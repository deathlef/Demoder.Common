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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Demoder.Common.Extensions
{
    public static class PropertyExtensions
    {
        public static T GetAttribute<T>(this PropertyInfo pi, bool inherit = false)
        {
            return (T)pi.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static T[] GetAttributes<T>(this PropertyInfo pi, bool inherit = false)
        {
            return pi.GetCustomAttributes(typeof(T), inherit) as T[];
        }

        public static void SafeSetValue(this PropertyInfo pi, object obj, object value, object[] index)
        {
            lock (pi)
            {
                pi.SetValue(obj, value, index);
            }
        }

        public static object SafeGetValue(this PropertyInfo pi, object obj, object[] index)
        {
            lock (pi)
            {
                return pi.GetValue(obj, index);
            }
        }
    }
}
