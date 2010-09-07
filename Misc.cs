/*
MIT Licence
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: https://sourceforge.net/projects/demoderstools/)

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
using System.IO;

namespace Demoder.Common
{
	public static class Misc
	{
		#region FindPos
		/// <summary>
		/// Try to find Needle in HayStack
		/// </summary>
		/// <param name="HayStack"></param>
		/// <param name="Needle"></param>
		/// <returns></returns>
		public static int FindPos(List<byte> HayStack, List<byte> Needle)
		{
			return FindPos(HayStack, Needle, 0, HayStack.Count);
		}
		/// <summary>
		/// Try to find Needle in HayStack
		/// </summary>
		/// <param name="HayStack"></param>
		/// <param name="Needle"></param>
		/// <param name="Offset"></param>
		/// <returns></returns>
		public static int FindPos(List<byte> HayStack, List<byte> Needle, int Offset)
		{
			return FindPos(HayStack, Needle, Offset, HayStack.Count -1);
		}

		/// <summary>
		/// Tries to find Needle in Haystack
		/// </summary>
		/// <param name="HayStack"></param>
		/// <param name="Needle"></param>
		/// <param name="Offset"></param>
		/// <param name="StopAt"></param>
		/// <returns></returns>
		public static int FindPos(List<byte> HayStack, List<byte> Needle, int Offset, int StopAt)
		{
			#region exceptions
			if (HayStack == null) throw new ArgumentNullException("HayStack");
			if (Needle == null) throw new ArgumentNullException("Needle");

			if (StopAt >= HayStack.Count) throw new ArgumentOutOfRangeException("StopAt", StopAt, "StopAt must be less than the final byte pos.");
			if (StopAt < 0) throw new ArgumentOutOfRangeException("StopAt", StopAt, "StopAt must be >=0.");
			if (StopAt <= Offset + Needle.Count) throw new ArgumentOutOfRangeException("StopAt", StopAt, "StopAt must be >= bytepos of Offset+Needle length.");

			if (Offset >= (HayStack.Count - Needle.Count)) throw new ArgumentOutOfRangeException("Offset", Offset, "Offset must be less than the final byte pos minus Needle length.");
			if (Offset < 0) throw new ArgumentOutOfRangeException("Offset", Offset, "Offset must be >=0.");
			#endregion

			//Bytepos that Needle starts at.
			int needleBytePos = 0;
			int needleMatchIndex = 0;
			for (int curBytePos = Offset; curBytePos < StopAt; curBytePos++)
			{
				if (Needle[needleMatchIndex] == HayStack[curBytePos])
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
				if (needleMatchIndex == Needle.Count) return needleBytePos;
				if (curBytePos >= StopAt) throw new Exception("Needle not found in haystack.");
			}
			throw new Exception("Needle not found in haystack.");
		}
		#endregion

		/// <summary>
		/// Get unixtime representing NOW
		/// </summary>
		/// <returns></returns>
		public static Int64 Unixtime()
		{
			DateTime dt = new DateTime(1970, 1, 1);
			return Unixtime(DateTime.UtcNow);
		}

		public static Int64 Unixtime(DateTime DateTime)
		{
			DateTime dt = new DateTime(1970, 1, 1);
			TimeSpan ts = (DateTime.ToUniversalTime() - dt);
			return (Int64)Math.Floor(ts.TotalSeconds);
		}
		/// <summary>
		/// Get a DateTime object representing the local time defined by the provided unixtime
		/// </summary>
		/// <param name="unixtime"></param>
		/// <returns></returns>
		public static DateTime Unixtime(Int64 unixtime)
		{
			DateTime dt = new DateTime(1970, 1, 1);
			return dt.AddSeconds(unixtime).ToLocalTime();
		}

		public static void PadMemoryStream(ref MemoryStream ms, int length, byte PadByte)
		{
			while (ms.Length < length)
				ms.WriteByte(PadByte);
				//If slice will be larger than the padding
			if (ms.Length > length)
				throw new Exception("Padding: MemoryStream is larger than defined static length!");
		}
	}
}