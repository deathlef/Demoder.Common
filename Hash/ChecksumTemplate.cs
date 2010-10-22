﻿/*
Demoder.Common
Copyright (c) 2010 Demoder <demoder@flw.nu> (project: http://trac.flw.nu/demoder.common/)

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
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Demoder.Common.Hash
{
	/// <summary>
	/// Template used for storing byte/hexadecimal checksums.
	/// This class cannot be serialized into an attribute.
	///	Workaround: Add [XmlIgnore] to the member of this class, and add a public accessor to access the member of this class which you want to use for serialization.
	/// </summary>
	public class ChecksumTemplate : IFormattable
	{
		#region Members
		private byte[] _bytes = new byte[0];
		#endregion
		#region Constructors
		/// <summary>
		/// Initializes an instance using a byte representation of a checksum
		/// </summary>
		/// <param name="Bytes"></param>
		public ChecksumTemplate(byte[] Bytes)
		{
			this._bytes = Bytes;
		}

		/// <summary>
		/// Initializes an instance using a string representation of a checksum
		/// </summary>
		/// <param name="Hex"></param>
		public ChecksumTemplate(string Hex)
		{
			this.String = Hex;
		}

		/// <summary>
		/// Constructor used by the XML serializer
		/// </summary>
		public ChecksumTemplate()
		{

		}

		#endregion

		#region Overrides
		public override bool Equals(object obj)
		{
			//If it's a string.
			if (obj.GetType() == typeof(string))
			{
				if ((string)obj == this.String)
					return true;
				else
					return false;
			}

			try
			{
				ChecksumTemplate template = (ChecksumTemplate)obj;
				if (template.String == this.String)
					return true;
				else
					return false;
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region Private methods
		private string generateString()
		{
			//Generate a hexadecimal string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < this._bytes.Length; i++)
			{
				sb.Append(this._bytes[i].ToString("X2"));
			}
			return sb.ToString();
		}

		private byte[] generateBytes(string Hex)
		{
			int numberChars = Hex.Length;
			byte[] bytes = new byte[numberChars / 2];
			for (int i = 0; i < numberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(Hex.Substring(i, 2), 16);
			return bytes;
		}
		#endregion

		#region Public Accessors
		/// <summary>
		/// Byte array representing the checksum
		/// </summary>
		[XmlIgnore]
		public byte[] Bytes
		{
			set
			{
				this._bytes = value;
			}
			get
			{
				return this._bytes;
			}
		}
		/// <summary>
		/// String representing the checksum
		/// </summary>
		[XmlAttribute("value")] 
		public string String
		{
			get { return this.generateString(); }
			set
			{
				string val = value;
				//See if it starts with 0x. If it does, remove the hexadecimal prefix.
				if (val.StartsWith("0x"))
					val = val.Substring(2);
				try
				{
					this._bytes = this.generateBytes(val);
				}
				catch 
				{
					throw new ArgumentException("Provided string is not a hexadecimal string");
				}
				
			}
		}
		#endregion

		#region Operators
		public static bool operator ==(ChecksumTemplate template1, ChecksumTemplate template2)
		{
			if (template1.String == template2.String)
				return true;
			else
				return false;
		}

		public static bool operator !=(ChecksumTemplate template1, ChecksumTemplate template2)
		{
			if (template1 == template2)
				return false;
			else
				return true;
		}
		#endregion





		#region IFormattable Members
		public override string ToString()
		{
			return this.ToString("", null);
		}

		public string ToString(string Format, IFormatProvider Provider)
		{
			if (Provider == null)
				Provider = System.Globalization.CultureInfo.CurrentCulture;

			return this.generateString().ToString(Provider);
		}

		#endregion
	}
}
