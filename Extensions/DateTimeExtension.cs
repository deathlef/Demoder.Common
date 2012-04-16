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

namespace Demoder.Common.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Returns a representation of this instance in UnixTime (seconds since 1st jan, 1970)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long ToUnixTime(this DateTime obj)
        {
            DateTime dt = new DateTime(1970, 1, 1);
            TimeSpan ts = (obj.ToUniversalTime() - dt);
            return (long)Math.Floor(ts.TotalSeconds);
        }

        /// <summary>
        /// Adds a timespan in format 365d24h60m60s to the DateTime object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime AddFriendlyStringTimeSpan(this DateTime obj, string time)
        {
            int days, hours, minutes, seconds; days = hours = minutes = seconds = 0;
            string val = String.Empty;
            string markers = "dhms";
            int test;
            foreach (Char c in time)
            {
                if (!markers.Contains(c) && (int.TryParse(c.ToString(), out test)))
                    val += c;
                else if (c == 'd')
                {
                    days = int.Parse(val);
                    val = String.Empty;
                }
                else if (c == 'h')
                {
                    hours = int.Parse(val);
                    val = String.Empty;
                }
                else if (c == 'm')
                {
                    minutes = int.Parse(val);
                    val = String.Empty;
                }
                else if (c == 's')
                {
                    seconds = int.Parse(val);
                    val = String.Empty;
                }
                else
                {
                    val = String.Empty;
                }
            }
            return obj.AddSeconds(seconds).AddMinutes(minutes).AddHours(hours).AddDays(days);
        }

        public static string ToIso8601String(this DateTime dt, bool displaySeconds=true)
        {
            if (displaySeconds)
            {
                return String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }

            return String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
        }

        public static string ToFriendlyString(this TimeSpan obj)
        {
            if (obj.TotalSeconds < 1) { return "0s"; }
            string ret = String.Empty;
            if (obj.Days > 0)
            {
                ret += obj.Days.ToString() + "d";
            }
            if (obj.Hours > 0)
            {
                ret += obj.Hours.ToString() + "h";
            }
            if (obj.Minutes > 0)
            {
                ret += obj.Minutes.ToString() + "m";
            }
            if (obj.Seconds > 0)
            {
                ret += obj.Seconds.ToString() + "s";
            }
            return ret;
        }
    }
}
