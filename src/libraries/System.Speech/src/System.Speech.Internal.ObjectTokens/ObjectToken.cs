// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Speech.Internal.SapiInterop;

namespace System.Speech.Internal.ObjectTokens
{
    internal class ObjectToken : RegistryDataKey, ISpObjectToken, ISpDataKey
    {
        [ComImport]
        [Guid("5B559F40-E952-11D2-BB91-00C04F8EE6C0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISpObjectWithToken
        {
            [PreserveSig]
            int SetObjectToken(ISpObjectToken pToken);

            [PreserveSig]
            int GetObjectToken(IntPtr ppToken);
        }

        private ISpObjectToken _sapiObjectToken;

        private bool _disposeSapiObjectToken;

        private RegistryDataKey _attributes;

        internal RegistryDataKey Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    return _attributes = OpenKey("Attributes");
                }
                return _attributes;
            }
        }

        internal ISpObjectToken SAPIToken => _sapiObjectToken;

        internal string Age
        {
            get
            {
                if (Attributes == null || !Attributes.TryGetString("Age", out string value))
                {
                    return string.Empty;
                }
                return value;
            }
        }

        internal string Gender
        {
            get
            {
                if (Attributes == null || !Attributes.TryGetString("Gender", out string value))
                {
                    return string.Empty;
                }
                return value;
            }
        }

        internal CultureInfo Culture
        {
            get
            {
                CultureInfo result = null;
                if (Attributes.TryGetString("Language", out string value))
                {
                    result = SapiAttributeParser.GetCultureInfoFromLanguageString(value);
                }
                return result;
            }
        }

        internal string Description
        {
            get
            {
                string value = string.Empty;
                string valueName = string.Format(CultureInfo.InvariantCulture, "{0:x}", new object[1]
                {
                    CultureInfo.CurrentUICulture.LCID
                });
                if (!TryGetString(valueName, out value))
                {
                    TryGetString(null, out value);
                }
                return value;
            }
        }

        protected ObjectToken(ISpObjectToken sapiObjectToken, bool disposeSapiToken)
            : base(sapiObjectToken)
        {
            if (sapiObjectToken == null)
            {
                throw new ArgumentNullException(nameof(sapiObjectToken));
            }
            _sapiObjectToken = sapiObjectToken;
            _disposeSapiObjectToken = disposeSapiToken;
        }

        internal static ObjectToken Open(ISpObjectToken sapiObjectToken)
        {
            return new ObjectToken(sapiObjectToken, disposeSapiToken: false);
        }

        internal static ObjectToken Open(string sCategoryId, string sTokenId, bool fCreateIfNotExist)
        {
            ISpObjectToken spObjectToken = (ISpObjectToken)new SpObjectToken();
            try
            {
                spObjectToken.SetId(sCategoryId, sTokenId, fCreateIfNotExist);
            }
            catch (Exception)
            {
                Marshal.ReleaseComObject(spObjectToken);
                return null;
            }
            return new ObjectToken(spObjectToken, disposeSapiToken: true);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_disposeSapiObjectToken && _sapiObjectToken != null)
                    {
                        Marshal.ReleaseComObject(_sapiObjectToken);
                        _sapiObjectToken = null;
                    }
                    if (_attributes != null)
                    {
                        _attributes.Dispose();
                        _attributes = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override bool Equals(object obj)
        {
            ObjectToken objectToken = obj as ObjectToken;
            if (objectToken != null)
            {
                return string.Compare(base.Id, objectToken.Id, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.Id.GetHashCode();
        }

        internal string TokenName()
        {
            string value = string.Empty;
            if (Attributes != null)
            {
                Attributes.TryGetString("Name", out value);
                if (string.IsNullOrEmpty(value))
                {
                    TryGetString(null, out value);
                }
            }
            return value;
        }

        public void SetId([MarshalAs(UnmanagedType.LPWStr)] string pszCategoryId, [MarshalAs(UnmanagedType.LPWStr)] string pszTokenId, [MarshalAs(UnmanagedType.Bool)] bool fCreateIfNotExist)
        {
            throw new NotImplementedException();
        }

        public void GetId([MarshalAs(UnmanagedType.LPWStr)] out IntPtr ppszCoMemTokenId)
        {
            ppszCoMemTokenId = Marshal.StringToCoTaskMemUni(base.Id);
        }

        public void Slot15()
        {
            throw new NotImplementedException();
        }

        public void Slot16()
        {
            throw new NotImplementedException();
        }

        public void Slot17()
        {
            throw new NotImplementedException();
        }

        public void Slot18()
        {
            throw new NotImplementedException();
        }

        public void Slot19()
        {
            throw new NotImplementedException();
        }

        public void Slot20()
        {
            throw new NotImplementedException();
        }

        public void Slot21()
        {
            throw new NotImplementedException();
        }

        public void MatchesAttributes([MarshalAs(UnmanagedType.LPWStr)] string pszAttributes, [MarshalAs(UnmanagedType.Bool)] out bool pfMatches)
        {
            throw new NotImplementedException();
        }

        internal bool MatchesAttributes(string[] sAttributes)
        {
            bool flag = true;
            foreach (string valueName in sAttributes)
            {
                flag &= (HasValue(valueName) || (Attributes != null && Attributes.HasValue(valueName)));
                if (!flag)
                {
                    break;
                }
            }
            return flag;
        }

        internal T CreateObjectFromToken<T>(string name)
        {
            T val = default(T);
            if (!TryGetString(name, out string value))
            {
                throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
            }
            try
            {
                Type typeFromCLSID = Type.GetTypeFromCLSID(new Guid(value));
                val = (T)Activator.CreateInstance(typeFromCLSID);
                ISpObjectWithToken spObjectWithToken = val as ISpObjectWithToken;
                if (spObjectWithToken != null)
                {
                    int num = spObjectWithToken.SetObjectToken(this);
                    if (num < 0)
                    {
                        throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
                    }
                    return val;
                }
                return val;
            }
            catch (Exception ex)
            {
                if (ex is MissingMethodException || ex is TypeLoadException || ex is FileLoadException || ex is FileNotFoundException || ex is MethodAccessException || ex is MemberAccessException || ex is TargetInvocationException || ex is InvalidComObjectException || ex is NotSupportedException || ex is FormatException)
                {
                    throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
                }
                throw;
            }
        }
    }
}
