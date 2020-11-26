// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.Synthesis
{
	internal class SsmlXmlAttribute
	{
		internal string _prefix;

		internal string _name;

		internal string _value;

		internal string _ns;

		internal SsmlXmlAttribute(string prefix, string name, string value, string ns)
		{
			_prefix = prefix;
			_name = name;
			_value = value;
			_ns = ns;
		}
	}
}
