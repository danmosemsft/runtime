// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Speech.Internal.SapiInterop;

namespace System.Speech.Internal.ObjectTokens
{
    internal class ObjectTokenCategory : RegistryDataKey, IEnumerable<ObjectToken>, IEnumerable
    {
        protected ObjectTokenCategory(string keyId, RegistryDataKey key)
            : base(keyId, key)
        {
        }

        internal static ObjectTokenCategory Create(string sCategoryId)
        {
            RegistryDataKey key = RegistryDataKey.Open(sCategoryId, fCreateIfNotExist: true);
            return new ObjectTokenCategory(sCategoryId, key);
        }

        internal ObjectToken OpenToken(string keyName)
        {
            string text = keyName;
            if (!string.IsNullOrEmpty(text) && text.IndexOf("HKEY_", StringComparison.Ordinal) != 0)
            {
                text = string.Format(CultureInfo.InvariantCulture, "{0}\\Tokens\\{1}", new object[2]
                {
                    base.Id,
                    text
                });
            }
            return ObjectToken.Open(null, text, fCreateIfNotExist: false);
        }

        internal IList<ObjectToken> FindMatchingTokens(string requiredAttributes, string optionalAttributes)
        {
            IList<ObjectToken> list = new List<ObjectToken>();
            ISpObjectTokenCategory spObjectTokenCategory = null;
            IEnumSpObjectTokens ppEnum = null;
            try
            {
                spObjectTokenCategory = (ISpObjectTokenCategory)new SpObjectTokenCategory();
                spObjectTokenCategory.SetId(_sKeyId, fCreateIfNotExist: false);
                spObjectTokenCategory.EnumTokens(requiredAttributes, optionalAttributes, out ppEnum);
                ppEnum.GetCount(out uint pCount);
                for (uint num = 0u; num < pCount; num++)
                {
                    ISpObjectToken ppToken = null;
                    ppEnum.Item(num, out ppToken);
                    ObjectToken item = ObjectToken.Open(ppToken);
                    list.Add(item);
                }
                return list;
            }
            finally
            {
                if (ppEnum != null)
                {
                    Marshal.ReleaseComObject(ppEnum);
                }
                if (spObjectTokenCategory != null)
                {
                    Marshal.ReleaseComObject(spObjectTokenCategory);
                }
            }
        }

        IEnumerator<ObjectToken> IEnumerable<ObjectToken>.GetEnumerator()
        {
            IList<ObjectToken> list = FindMatchingTokens(null, null);
            foreach (ObjectToken item in list)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ObjectToken>)this).GetEnumerator();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
