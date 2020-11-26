// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Speech.Internal.GrammarBuilding
{
	internal class IdentifierCollection
	{
		protected List<string> _identifiers;

		internal IdentifierCollection()
		{
			_identifiers = new List<string>();
			CreateNewIdentifier("_");
		}

		internal string CreateNewIdentifier(string id)
		{
			if (!_identifiers.Contains(id))
			{
				_identifiers.Add(id);
				return id;
			}
			int num = 1;
			string text;
			do
			{
				text = id + num;
				num++;
			}
			while (_identifiers.Contains(text));
			_identifiers.Add(text);
			return text;
		}
	}
}
