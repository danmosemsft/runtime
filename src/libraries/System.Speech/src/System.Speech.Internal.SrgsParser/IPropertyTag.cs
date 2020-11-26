// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Speech.Internal.SrgsParser
{
    internal interface IPropertyTag : IElement
    {
        void NameValue(IElement parent, string name, object value);
    }
}
