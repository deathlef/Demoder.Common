/*
Demoder.Common
Copyright (c) 2010,2011,2012 Demoder <demoder@demoder.me>

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
using Demoder.Common.Attributes;
using Demoder.Common.Extensions;

namespace Demoder.Common
{
    public class SettingInfo
    {
        private SettingInfo()
        {

        }

        /// <summary>
        /// Type containing this property
        /// </summary>
        public Type Type { get; private set; }
        
        /// <summary>
        /// Described property
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Default setting value
        /// </summary>
        public SettingAttribute Setting { get; private set; }

        public SettingOptionAttribute[] Options { get; private set; }

        public string Value { get; set; }

        public static SettingInfo Create(Type type, PropertyInfo property)
        {
            var info = new SettingInfo();
            info.Type = type;
            info.Property = property;

            // Check for setting
            var settingProperty = property.GetAttribute<SettingAttribute>();
            if (settingProperty == null)
            {
                return null;
            }

            info.Setting = settingProperty;
            info.Value = settingProperty.DefaultValue.ToString();

            info.Options = property.GetAttributes<SettingOptionAttribute>() ?? new SettingOptionAttribute[0];

            return info;
        }
    }
}
