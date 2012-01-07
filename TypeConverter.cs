/*
Demoder.Common
Copyright (c) 2010,2011 Demoder <demoder@demoder.me>

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
using System.Globalization;

namespace Demoder.Common
{
    public static class TypeConverter
    {
        public static bool IsSupported(Type type)
        {
            if (type.IsArray)
                return typeof(IConvertible).IsAssignableFrom(type.GetElementType());
            return typeof(IConvertible).IsAssignableFrom(type);
        }
        public static object Convert(Type type, string word)
        {
            if (type == typeof(string)) { return word; }
            if (String.IsNullOrEmpty(word))
                throw new ArgumentException("Parameter lacks value");
            if (!TypeConverter.IsSupported(type))
                throw new ArgumentException("Cannot convert to " + type.ToString());
            if (type.IsEnum)
                return Enum.Parse(type, word, true);
            return ((IConvertible)word).ToType(type, CultureInfo.InvariantCulture);
        }

        public static string FindAcceptedValues(Type t)
        {
            string formatValue = "a whole number between {0} and {1}";
            string formatFloating = "a decimal number between {0} and {1}";
            if (t.IsEnum)
                return String.Join(", ", t.GetEnumNames());
            if (t == typeof(sbyte))
                return String.Format(formatValue, sbyte.MinValue, sbyte.MaxValue);
            if (t == typeof(byte))
                return String.Format(formatValue, byte.MinValue, byte.MaxValue);
            if (t == typeof(short))
                return String.Format(formatValue, short.MinValue, short.MaxValue);
            if (t == typeof(short))
                return String.Format(formatValue, short.MinValue, short.MaxValue);
            if (t == typeof(ushort))
                return String.Format(formatValue, ushort.MinValue, ushort.MaxValue);
            if (t == typeof(int))
                return String.Format(formatValue, int.MinValue, int.MaxValue);
            if (t == typeof(uint))
                return String.Format(formatValue, uint.MinValue, uint.MaxValue);
            if (t == typeof(long))
                return String.Format(formatValue, long.MinValue, long.MaxValue);
            if (t == typeof(ulong))
                return String.Format(formatValue, ulong.MinValue, ulong.MaxValue);

            if (t == typeof(float))
                return String.Format(formatFloating, float.MinValue, float.MaxValue);
            if (t == typeof(double))
                return String.Format(formatFloating, double.MinValue, double.MaxValue);
            if (t == typeof(decimal))
                return String.Format(formatFloating, decimal.MinValue, decimal.MaxValue);

            if (t == typeof(bool))
                return String.Format("{0} or {1}", bool.FalseString, bool.TrueString);
            if (t == typeof(string))
                return "text";

            return t.Name;
        }
    }
}
