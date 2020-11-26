// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SrgsParser
{
    internal interface IScript : IElement
    {
        IScript Create(string rule, RuleMethodScript onInit);
    }
}
