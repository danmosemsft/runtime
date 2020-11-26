// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsCompiler
{
	internal struct CfgScriptRef
	{
		internal int _idRule;

		internal int _idMethod;

		internal RuleMethodScript _method;
	}
}
