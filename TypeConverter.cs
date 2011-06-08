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
            if (String.IsNullOrEmpty(word))
                throw new ArgumentException("Parameter lacks value");
            if (!TypeConverter.IsSupported(type))
                throw new ArgumentException("Cannot convert to " + type.ToString());
            if (type.IsEnum)
                return Enum.Parse(type, word);
            return ((IConvertible)word).ToType(type, CultureInfo.InvariantCulture);
        }
    }
}
