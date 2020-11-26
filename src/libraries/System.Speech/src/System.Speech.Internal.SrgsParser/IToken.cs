// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SrgsParser
{
    internal interface IToken : IElement
    {
        string Text
        {
            set;
        }

        string Display
        {
            set;
        }

        string Pronunciation
        {
            set;
        }
    }
}
