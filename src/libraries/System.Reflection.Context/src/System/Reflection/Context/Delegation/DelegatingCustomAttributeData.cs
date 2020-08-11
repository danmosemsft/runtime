// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Reflection.Context.Delegation
{
    internal class DelegatingCustomAttributeData : CustomAttributeData
    {
        public DelegatingCustomAttributeData(CustomAttributeData attribute)
        {
            Debug.Assert(attribute != null);

            UnderlyingAttribute = attribute;
        }

        public CustomAttributeData UnderlyingAttribute { get; }

        public override ConstructorInfo Constructor
        {
            get { return UnderlyingAttribute.Constructor; }
        }

        public override IList<CustomAttributeTypedArgument> ConstructorArguments
        {
            get { return UnderlyingAttribute.ConstructorArguments; }
        }

        public override IList<CustomAttributeNamedArgument> NamedArguments
        {
            get { return UnderlyingAttribute.NamedArguments; }
        }

        public override string ToString()
        {
            return UnderlyingAttribute.ToString();
        }
    }
}
