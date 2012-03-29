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
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Demoder.Common
{
    public static class Misc
    {
        #region FindPos
        /// <summary>
        /// Try to find Needle in HayStack
        /// </summary>
        /// <param name="hayStack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static int FindPos(List<byte> hayStack, List<byte> needle)
        {
            return FindPos(hayStack, needle, 0, hayStack.Count);
        }
        /// <summary>
        /// Try to find Needle in HayStack
        /// </summary>
        /// <param name="hayStack"></param>
        /// <param name="needle"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int FindPos(List<byte> hayStack, List<byte> needle, int offset)
        {
            return FindPos(hayStack, needle, offset, hayStack.Count - 1);
        }

        /// <summary>
        /// Tries to find Needle in Haystack
        /// </summary>
        /// <param name="hayStack"></param>
        /// <param name="needle"></param>
        /// <param name="offset"></param>
        /// <param name="stopAt"></param>
        /// <returns></returns>
        public static int FindPos(List<byte> hayStack, List<byte> needle, int offset, int stopAt)
        {
            #region exceptions
            if (hayStack == null) throw new ArgumentNullException("HayStack");
            if (needle == null) throw new ArgumentNullException("Needle");

            if (stopAt >= hayStack.Count) throw new ArgumentOutOfRangeException("StopAt", stopAt, "StopAt must be less than the final byte pos.");
            if (stopAt < 0) throw new ArgumentOutOfRangeException("StopAt", stopAt, "StopAt must be >=0.");
            if (stopAt <= offset + needle.Count) throw new ArgumentOutOfRangeException("StopAt", stopAt, "StopAt must be >= bytepos of Offset+Needle length.");

            if (offset >= (hayStack.Count - needle.Count)) throw new ArgumentOutOfRangeException("Offset", offset, "Offset must be less than the final byte pos minus Needle length.");
            if (offset < 0) throw new ArgumentOutOfRangeException("Offset", offset, "Offset must be >=0.");
            #endregion

            //Bytepos that Needle starts at.
            int needleBytePos = 0;
            int needleMatchIndex = 0;
            for (int curBytePos = offset; curBytePos < stopAt; curBytePos++)
            {
                if (needle[needleMatchIndex] == hayStack[curBytePos])
                {
                    if (needleMatchIndex == 0) needleBytePos = curBytePos;
                    needleMatchIndex++;
                }
                else if (needleMatchIndex != 0)
                {
                    needleMatchIndex = 0;
                    curBytePos = needleBytePos; //Start 1byte in front of where we started looking last time. The for loop will add 1 to this at the next loop.
                }
                else { /*Nothing to do*/ }
                if (needleMatchIndex == needle.Count) return needleBytePos;
                if (curBytePos >= stopAt) throw new Exception("Needle not found in haystack.");
            }
            throw new Exception("Needle not found in haystack.");
        }
        #endregion

        public static bool IsUserAdministrator()
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        /// <summary>
        /// Get unixtime representing NOW
        /// </summary>
        /// <returns></returns>
        public static Int64 Unixtime()
        {
            DateTime dt = new DateTime(1970, 1, 1);
            return Unixtime(DateTime.UtcNow);
        }

        public static Int64 Unixtime(DateTime dateTime)
        {
            DateTime dt = new DateTime(1970, 1, 1);
            TimeSpan ts = (dateTime.ToUniversalTime() - dt);
            return (Int64)Math.Floor(ts.TotalSeconds);
        }
        /// <summary>
        /// Get a DateTime object representing the local time defined by the provided unixtime
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static DateTime Unixtime(Int64 unixTime)
        {
            DateTime dt = new DateTime(1970, 1, 1);
            return dt.AddSeconds(unixTime).ToLocalTime();
        }

        public static void PadMemoryStream(ref MemoryStream memoryStream, int length, byte padByte)
        {
            while (memoryStream.Length < length)
                memoryStream.WriteByte(padByte);
            //If slice will be larger than the padding
            if (memoryStream.Length > length)
                throw new Exception("Padding: MemoryStream is larger than defined static length!");
        }

        public static Random NewRandom()
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] blah = new byte[4];
            random.GetNonZeroBytes(blah);
            int seed = BitConverter.ToInt32(blah, 0);
            return new Random(seed);
        }
    }
}