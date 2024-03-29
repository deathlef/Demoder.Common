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
using System.Text;
using Demoder.Common.Extensions;

namespace Demoder.Common
{
    public static class Maths
    {
        public static int Percent(int MaxValue, int CurrentValue)
        {
            return (int)System.Math.Round(MaxValue.Percent(CurrentValue), 0);
        }

        /// <summary>
        /// Determines how much of 'full' the provided 'percent' means.
        /// </summary>
        /// <param name="MaxValue">Number of units</param>
        /// <param name="Percent">Percent</param>
        /// <returns></returns>
        public static int DePercent(int MaxValue, int Percent)
        {
            return (int)Math.Round(Percent * ((double)MaxValue / 100), 0);
        }
    }
}
