// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Speech.Internal.SrgsParser;

namespace System.Speech.Internal.SrgsCompiler
{
    internal abstract class ParseElement : IElement
    {
        internal int _confidence;

        internal Rule _rule;

        internal ParseElement(Rule rule)
        {
            _rule = rule;
        }

        void IElement.PostParse(IElement parent)
        {
        }
    }
}
