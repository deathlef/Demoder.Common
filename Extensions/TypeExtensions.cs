﻿/*
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
    public static class TypeExtensions
    {
        public static object CreateInstance(this Type t)
        {
            return Activator.CreateInstance(t);
        }
        public static object CreateInstance(this Type t, params object[] args)
        {
            return Activator.CreateInstance(t, args);
        }

        public static object GetStaticFieldValue(this Type t, string fieldName)
        {
            var field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            var value = field.GetValue(null);
            return value;
        }


        public static T GetAttribute<T>(this Type t, bool inherit = false)
        {
            return t.GetAttributes<T>(inherit).FirstOrDefault();
        }
        public static T[] GetAttributes<T>(this Type t, bool inherit=false)
        {
            return t.GetCustomAttributes(typeof(T), inherit) as T[];
        }

        /// <summary>
        /// Retrieve properties which have specified attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesWithAttribute<T>(this Type type)
            where T : Attribute
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(typeof(T), true).Length != 0).ToArray();
        }
    }

}
