// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SapiInterop
{
    internal struct SPTEXTSELECTIONINFO
    {
        internal uint ulStartActiveOffset;

        internal uint cchActiveChars;

        internal uint ulStartSelection;

        internal uint cchSelection;

        internal SPTEXTSELECTIONINFO(uint ulStartActiveOffset, uint cchActiveChars, uint ulStartSelection, uint cchSelection)
        {
            this.ulStartActiveOffset = ulStartActiveOffset;
            this.cchActiveChars = cchActiveChars;
            this.ulStartSelection = ulStartSelection;
            this.cchSelection = cchSelection;
        }
    }
}
