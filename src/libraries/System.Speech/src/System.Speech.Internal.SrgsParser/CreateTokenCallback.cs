// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SrgsParser
{
	internal delegate IToken CreateTokenCallback(IElement parent, string content, string pronumciation, string display, float reqConfidence);
}
