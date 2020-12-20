//---------------------------------------------------------------------------
//
// <copyright file="ObjectToken.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: 
//		Object Token 
//
// History:
//		7/1/2004	jeanfp		
//---------------------------------------------------------------------------

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using RegistryEntry = System.Collections.Generic.KeyValuePair<string, object>;

#if SERVERTESTDLL
using Microsoft.Speech.Internal.SapiInterop;
#else // SERVERTESTDLL
using System.Speech.Internal.SapiInterop;
#endif // SERVERTESTDLL


#if SERVERTESTDLL
namespace Microsoft.Speech.Internal.ObjectTokens
#else
namespace System.Speech.Internal.ObjectTokens
#endif
{
    /// <summary>
    /// Summary description for ObjectToken.
    /// </summary>
#if VSCOMPILE
    [DebuggerDisplay("{Name}")]
#endif
    internal class ObjectToken : RegistryDataKey, ISpObjectToken
#if SPEECHSERVER
, ISpObjectTokenMss
#endif
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        protected ObjectToken(ISpObjectToken sapiObjectToken, bool disposeSapiToken)
            : base(sapiObjectToken)
        {
            if (sapiObjectToken == null)
            {
                throw new ArgumentNullException("sapiObjectToken");
            }

            _sapiObjectToken = sapiObjectToken;
            _disposeSapiObjectToken = disposeSapiToken;
        }

        /// <summary>
        /// Creates a ObjectToken from an already-existing ISpObjectToken.
        /// Assumes the token was created through enumeration, thus should not be disposed by us.
        /// </summary>
        /// <param name="sapiObjectToken"></param>
        /// <returns>ObjectToken object</returns>
        internal static ObjectToken Open(ISpObjectToken sapiObjectToken)
        {
            return new ObjectToken(sapiObjectToken, false);
        }

        /// <summary>
        /// Creates a new ObjectToken from a category
        /// Unlike the other Open overload, this one creates a new SAPI object, so Dispose must be called if
        /// you are creating ObjectTokens with this function.
        /// </summary>
        /// <param name="sCategoryId"></param>
        /// <param name="sTokenId"></param>
        /// <param name="fCreateIfNotExist"></param>
        /// <returns>ObjectToken object</returns>
        internal static ObjectToken Open(string sCategoryId, string sTokenId, bool fCreateIfNotExist)
        {
            ISpObjectToken sapiObjectToken = (ISpObjectToken)new SpObjectToken();

            try
            {
                sapiObjectToken.SetId(sCategoryId, sTokenId, fCreateIfNotExist);
            }
            catch (Exception)
            {
                Marshal.ReleaseComObject(sapiObjectToken);
                return null;
            }

            return new ObjectToken(sapiObjectToken, true);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_disposeSapiObjectToken == true && _sapiObjectToken != null)
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

        #endregion

        //*******************************************************************
        //
        // Public Methods
        //
        //*******************************************************************

        #region public Methods

        /// <summary>
        /// Tests whether two AutomationIdentifier objects are equivalent
        /// </summary>
        public override bool Equals(object obj)
        {
            ObjectToken token = obj as ObjectToken;
            return token != null && string.Compare(Id, token.Id, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Overrides Object.GetHashCode()
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        //*******************************************************************
        //
        // Internal Properties
        //
        //*******************************************************************

        #region Internal Properties

        internal RegistryDataKey Attributes
        {
            get
            {
                return _attributes != null ? _attributes : (_attributes = OpenKey("Attributes"));
            }
        }

        internal ISpObjectToken SAPIToken
        {
            get
            {
                return _sapiObjectToken;
            }
        }

        /// <summary>
        /// Returns the Age from a voice token
        /// </summary>
        /// <value></value>
        internal string Age
        {
            get
            {
                string age;
                if (Attributes == null || !Attributes.TryGetString("Age", out age))
                {
                    age = string.Empty;
                }
                return age;
            }
        }


        /// <summary>
        /// Returns the gender
        /// </summary>
        /// <value></value>
        internal string Gender
        {
            get
            {
                string gender;
                if (Attributes == null || !Attributes.TryGetString("Gender", out gender))
                {
                    gender = string.Empty;
                }
                return gender;
            }
        }

        /// <summary>
        /// Returns the Name for the voice
        /// Look first in the Name attribute, if not available then get the default string
        /// </summary>
        /// <value></value>
        internal string TokenName()
        {
            string name = string.Empty;
            if (Attributes != null)
            {
                Attributes.TryGetString("Name", out name);

                if (string.IsNullOrEmpty(name))
                {
                    TryGetString(null, out name);
                }
            }
            return name;
        }


        /// <summary>
        /// Returns the Culture defined in the Language field for a token
        /// </summary>
        /// <returns></returns>
        internal CultureInfo Culture
        {
            get
            {
                CultureInfo culture = null;
                string langId;
                if (Attributes.TryGetString("Language", out langId))
                {
                    culture = SapiAttributeParser.GetCultureInfoFromLanguageString(langId);
                }
                return culture;
            }
        }

        /// <summary>
        /// Returns the Culture defined in the Language field for a token
        /// </summary>
        /// <returns></returns>
        internal string Description
        {
            get
            {
                string description = string.Empty;
                string sCultureId = string.Format(CultureInfo.InvariantCulture, "{0:x}", CultureInfo.CurrentUICulture.LCID);
                if (!TryGetString(sCultureId, out description))
                {
                    TryGetString(null, out description);
                }
                return description;
            }
        }

        #endregion

        //*******************************************************************
        //
        // Internal Methods
        //
        //*******************************************************************

        #region internal Methods

        #region ISpObjectToken Implementation

        public void SetId([MarshalAs(UnmanagedType.LPWStr)] string pszCategoryId, [MarshalAs(UnmanagedType.LPWStr)] string pszTokenId, [MarshalAs(UnmanagedType.Bool)] bool fCreateIfNotExist)
        {
            throw new NotImplementedException();
        }

        public void GetId([MarshalAs(UnmanagedType.LPWStr)] out IntPtr ppszCoMemTokenId)
        {
            ppszCoMemTokenId = Marshal.StringToCoTaskMemUni(Id);
        }

        public void Slot15() { throw new NotImplementedException(); } // void GetCategory(out ISpObjectTokenCategory ppTokenCategory);
        public void Slot16() { throw new NotImplementedException(); } // void CreateInstance(object pUnkOuter, UInt32 dwClsContext, ref Guid riid, ref IntPtr ppvObject);
        public void Slot17() { throw new NotImplementedException(); } // void GetStorageFileName(ref Guid clsidCaller, [MarshalAs(UnmanagedType.LPWStr)] string pszValueName, [MarshalAs(UnmanagedType.LPWStr)] string pszFileNameSpecifier, UInt32 nFolder, [MarshalAs(UnmanagedType.LPWStr)] out string ppszFilePath);
        public void Slot18() { throw new NotImplementedException(); } // void RemoveStorageFileName(ref Guid clsidCaller, [MarshalAs(UnmanagedType.LPWStr)] string pszKeyName, int fDeleteFile);
        public void Slot19() { throw new NotImplementedException(); } // void Remove(ref Guid pclsidCaller);
        public void Slot20() { throw new NotImplementedException(); } // void IsUISupported([MarshalAs(UnmanagedType.LPWStr)] string pszTypeOfUI, IntPtr pvExtraData, UInt32 cbExtraData, object punkObject, ref Int32 pfSupported);
        public void Slot21() { throw new NotImplementedException(); } // void DisplayUI(UInt32 hWndParent, [MarshalAs(UnmanagedType.LPWStr)] string pszTitle, [MarshalAs(UnmanagedType.LPWStr)] string pszTypeOfUI, IntPtr pvExtraData, UInt32 cbExtraData, object punkObject);
        public void MatchesAttributes([MarshalAs(UnmanagedType.LPWStr)] string pszAttributes, [MarshalAs(UnmanagedType.Bool)] out bool pfMatches) { throw new NotImplementedException(); }

        #endregion

        /// <summary>
        /// Check if the token supports the attributes list given in. The
        /// attributes list has the same format as the required attributes given to
        /// SpEnumTokens.
        /// </summary>
        /// <param name="sAttributes"></param>
        /// <returns></returns>
        internal bool MatchesAttributes(string[] sAttributes)
        {
            bool fMatch = true;

            for (int iAttribute = 0; iAttribute < sAttributes.Length; iAttribute++)
            {
                string s = sAttributes[iAttribute];
                fMatch &= HasValue(s) || (Attributes != null && Attributes.HasValue(s));
                if (!fMatch)
                {
                    break;
                }
            }
            return fMatch;
        }

#if false

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCategoryId"></param>
        /// <param name="fCreateIfNotExist"></param>
        /// <returns></returns>
        internal static ObjectTokenCategory CategoryFromId (string sCategoryId, bool fCreateIfNotExist)
        {
            return new ObjectTokenCategory (sCategoryId, fCreateIfNotExist);
        }

        /// <summary>
        /// Remove the specified storage file name and optionally delete the file
        /// </summary>
        /// <param name="clsidCaller"></param>
        /// <param name="sValueName"></param>
        /// <param name="fDeleteFile"></param>
        internal void RemoveStorageFileName (Guid clsidCaller, string sValueName, bool fDeleteFile)
        {
            // SPDBG_FUNC("CSpObjectToken::RemoveStorageFileName");

            ObjectToken cpDataKey = OpenFilesKey (clsidCaller, false);
            if (fDeleteFile)
            {
                DeleteFileFromKey (cpDataKey, sValueName);
            }
            cpDataKey.DeleteValue (sValueName);
        }

        /// <summary>
        /// Get a filename which can be manipulated by this token. Storage files will
        /// be deleted on a Remove call.
        /// clsidCaller - a key will be made in registry below the token with this name and files key beneath that.
        /// sValueName - Value name which will be made in registry to store the file path string.
        /// sFileSpecifier - either null or a path/filename for storage file:
        ///     - if this starts with 'X:\' or '\\' then is assumed to be a full path.
        ///     - otherwise is assumed to be relative to special folders given in the nFolder parameter.
        ///     - if ends with a '\', or is null a unique filename will be created.
        ///     - if the name contains a %d the %d is replaced by a number to give a unique filename.
        ///     - intermediate directories are created.
        ///     - if a relative file is being used the value stored in the registry includes 
        ///         the nFolder value as %nFolder% before the rest of the path. This allows
        ///         roaming to work properly if you pick an nFolder value representing a raoming folder
        /// nFolder - equivalent to the value given to SHGetFolderPath in the Shell API.
        /// ppszFilePath - CoTaskMemAlloc'd returned file path.
        /// 
        /// </summary>
        /// <param name="clsidCaller"></param>
        /// <param name="sValueName"></param>
        /// <param name="sFileSpecifier"></param>
        /// <param name="nFolder"></param>
        /// <param name="fCreate"></param>
        internal string GetStorageFileName (Guid clsidCaller, string sValueName, string sFileSpecifier, Environment.SpecialFolder nFolder, bool fCreate)
        {
            string ppszFilePath = null;

            // SPDBG_FUNC("CSpObjectToken::GetStorageFileName");

            // See if there is already a Files key in the registry for this token
            ObjectToken cpFilesKey = OpenFilesKey (clsidCaller, (int) nFolder != 0);

            string dstrFilePath;  // Path to the file which we return to user.
            string dstrRegPath;   // Path to the string which will be stored in the registry.

            // See if the key we are looking for is present
            cpFilesKey.TryGetString (sValueName, out dstrRegPath);
#if _WIN32_WCE
            if (hr == SPERR_NOT_FOUND && nFolder)
#else
            if (fCreate && dstrRegPath != null)
#endif //_WIN32_WCE
            {
                // Didn't find the key and want to create

                // Calculate the new file path and key value
                FileSpecifierToRegPath (sFileSpecifier, nFolder, out dstrFilePath, out dstrRegPath);
                // Set the key value
                cpFilesKey.SetString (sValueName, dstrRegPath);
            }
            else
            {
                // Found existing entry so convert and return
                RegPathToFilePath (dstrRegPath, out dstrFilePath);
            }
            ppszFilePath = dstrFilePath;

            return ppszFilePath;
        }

        /// <summary>
        /// Determine if the specific type of UI is supported or not
        /// </summary>
        /// <param name="sTypeOfUI"></param>
        /// <returns></returns>
        internal bool IsUISupported (string sTypeOfUI /*, object pvExtraData, int cbExtraData*/)
        {
            // SPDBG_FUNC("CSpObjectToken::IsUISupported");

#if false
	if (m_cpTokenDelegate != null)
	{
		// NTRAID#SPEECH-7392-2000/08/31-robch: Maybe we should first delegate, and if that doesn't work, 
		// try this token's category ui...
		m_cpTokenDelegate.IsUISupported(
					pszTypeOfUI,
					pvExtraData,
					cbExtraData,
					pfSupported);
	}
	else
	{
            /*Guid clsidObject = */
            GetUIObjectClsid (sTypeOfUI);

		ISpTokenUI cpTokenUI = cpTokenUI.CoCreateInstance(clsidObject);

		pfSupported = cpTokenUI.IsUISupported(sTypeOfUI, pvExtraData, cbExtraData, punkObject);
#endif
            //TODO 
            throw new NotImplementedException ();
            //return pfSupported;
        }

        /// <summary>
        /// Remove either a specified caller's section of the token, or the
        /// entire token. We remove the entire token if pclsidCaller == null.
        /// </summary>
        /// <param name="pclsidCaller"></param>
        internal void Remove (Guid pclsidCaller)
        {
            // SPDBG_FUNC("CSpObjectToken::Remove");

            // Remove all the filenames
            RemoveAllStorageFileNames (pclsidCaller);

            // Now go ahead and delete the registry entry which is either
            // the token itself (if pclsidCaller == null) or the clsid's
            // sub key
            if (pclsidCaller == Guid.Empty)
            {
                RegistryDataKey.DeleteRegistryPath (Id, null);
            }
            else
            {
                //TODO
                throw new NotImplementedException ();
                //					string szClsid; //[MAX_PATH];
                //					StringFromGUID2(pclsidCaller, szClsid, sp_countof(szClsid));
                //					RegistryDataKey.SpDeleteRegPath(_dstrTokenId, szClsid);
            }
        }
#endif

        internal T CreateObjectFromToken<T>(string name)
        {
            T instanceValue = default(T);
            string clsid;

            if (!TryGetString(name, out clsid))
            {
                throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
            }

            try
            {
                // Application Class Id
                Type type = Type.GetTypeFromCLSID(new Guid(clsid));

                // Create the object instance
                instanceValue = (T)Activator.CreateInstance(type);

                // Initialize the instance
                ISpObjectWithToken objectWithToken = instanceValue as ISpObjectWithToken;
                if (objectWithToken != null)
                {
                    //IntPtr ite = Marshal.GetComInterfaceForObject (this, typeof (ISpObjectToken2));
                    int hresult = objectWithToken.SetObjectToken((ISpObjectToken)this);
                    if (hresult < 0)
                    {
                        throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
                    }
                }
                else
                {
                    // TODO: We should throw a NotImplementedException exception here
                    Debug.Fail("Cannot query for interface " + typeof(ISpObjectWithToken).GUID + " from COM class " + clsid);
                }
            }
            catch (Exception e)
            {
                //wow I am not really sure we want to do this here....
                if (e is MissingMethodException || e is TypeLoadException || e is FileLoadException || e is FileNotFoundException || e is MethodAccessException || e is MemberAccessException || e is TargetInvocationException || e is InvalidComObjectException || e is NotSupportedException || e is FormatException)
                {
                    throw new ArgumentException(SR.Get(SRID.TokenCannotCreateInstance));
                }
                throw;
            }
            return instanceValue;
        }

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region private Methods

#if false

        /// <summary>
        /// Given a path and file specifier, creates a new filename.
        /// Just the filename is returned, not the path.
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="sFileSpecifier"></param>
        /// <returns></returns>
        private static string GenerateFileName (string sPath, string sFileSpecifier)
        {
            // Is the caller asking for a random filename element in the name
            if (sFileSpecifier == null || sFileSpecifier.Length == 0 || sFileSpecifier.IndexOf (sGenerateFileNameSpecifier) >= 0)
            {
                // Generate a random filename using prefix and suffix
                string dstrFilePrefix;
                string dstrFileSuffix;

                if (sFileSpecifier == null || sFileSpecifier.Length == 0 ||
                    (sFileSpecifier.Length == sGenerateFileNameSpecifier.Length &&
                    sFileSpecifier == sGenerateFileNameSpecifier))
                {
                    // No specific format given so make files of format "SP_xxxx.dat"
                    dstrFilePrefix = sDefaultFilePrefix;
                    dstrFileSuffix = sDefaultFileSuffix;
                }
                else
                {
                    // Extract the prefix and suffix of the random element
                    int iPos = sFileSpecifier.IndexOf (sGenerateFileNameSpecifier);
                    dstrFilePrefix = sFileSpecifier.Substring (0, iPos);
                    dstrFileSuffix = sFileSpecifier.Substring (iPos, sGenerateFileNameSpecifier.Length);
                }

                // Create random GUID to use as part of filename
                Guid guid = Guid.NewGuid ();

                // Convert to string
                string dstrGUID = guid.ToString ();

                StringBuilder dstrRandomString = new StringBuilder ();

                // Remove non-alpha numeric characters
                foreach (char ch in dstrGUID)
                {
                    if (char.IsLetterOrDigit (ch))
                    {
                        dstrRandomString.Append (ch);
                    }
                }

                string sFile = dstrFilePrefix + dstrRandomString + dstrFileSuffix;

                string dstrFileAndPath = sPath + sFile;

                // See if file can be created
                FileStream hFile = new FileStream (dstrFileAndPath, FileMode.CreateNew, FileAccess.Write);

                // Successfully created empty new file, so close and return
                hFile.Close ();

                return sFile;
            }
            else
            {
                string dstrFileAndPath = sPath + sFileSpecifier;

                // Create file if it doesn't already exist
                FileStream hFile = new FileStream (dstrFileAndPath, FileMode.CreateNew, FileAccess.Write);

                // Successfully created empty new file, so close and return
                hFile.Close ();

                // Otherwise we just leave things as they are
                return sFileSpecifier;
            }
        }

        /// <summary>
        /// Creates all non-existent directories in sPath. Assumes all
        /// directories prior to createFrom string offset already exist.
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="createFrom"></param>
        private static void CreatePath (string sPath, int createFrom)
        {
            //if \\ skip \\ find next '\'
            if (createFrom == 0 && sPath.Length >= 2 && sPath.IndexOf ("\\\\") == 0)
            {
                while (sPath [createFrom] == '\\')
                {
                    createFrom++;
                }
                int iPos = sPath.IndexOf ('\\', createFrom);
                if (iPos >= 0)
                {
                    throw new ArgumentException (SR.Get (SRID.NoBackSlash, "createFrom"), "createFrom");
                }
                createFrom = iPos;
            }

            // Skip any '\' (also at start to cope with \\machine network paths
            while (sPath [createFrom] == '\\')
            {
                createFrom++;
            }

            for (int iStart = createFrom; iStart < sPath.Length; iStart++)
            {
                // Scan thought path. Each time reach a '\', copy section and try and create directory
                if (sPath [iStart] == '\\')
                {
                    // Copy last section and trailing slash
                    string dstrIncrementalPath = sPath.Substring (0, iStart);

                    if (!Directory.Exists (dstrIncrementalPath))
                    {
                        Directory.CreateDirectory (dstrIncrementalPath);
                    }
                }
            }

            if (!Directory.Exists (sPath))
            {
                Directory.CreateDirectory (sPath);
            }
        }


        /// <summary>
        /// Given the file specifier string and nFolder value, convert to a reg key and file path.
        /// </summary>
        /// <param name="sFileSpecifier"></param>
        /// <param name="nFolder"></param>
        /// <param name="dstrFilePath"></param>
        /// <param name="dstrRegPath"></param>
        private static void FileSpecifierToRegPath (string sFileSpecifier, Environment.SpecialFolder nFolder, out string dstrFilePath, out string dstrRegPath)
        {
            // Make sure return strings are empty
            dstrFilePath = null;
            dstrRegPath = null;

            // Is it a "X:\" path or a "\\" path
            if (sFileSpecifier != null && sFileSpecifier.Length >= 3 && (sFileSpecifier.IndexOf (":\\", 1) == 1 || sFileSpecifier.IndexOf ("\\\\") == 0))
            {
                // Find last '\' that separates path from base file
                int iBaseFile = sFileSpecifier.LastIndexOf ('\\');
                iBaseFile++;

                // dstrFilePath holds the path with trailing '\'
                dstrFilePath = sFileSpecifier.Substring (0, iBaseFile);
                CreatePath (dstrFilePath, 0);

                // Calculate the new filename
                string sFile = GenerateFileName (dstrFilePath, sFileSpecifier.Substring (iBaseFile + 1));

                // Add fileName to path and copy to reg key
                dstrFilePath = dstrFilePath + @"\" + sFile;
                dstrRegPath = dstrFilePath;
            }

            // It's a relative path
            else
            {
#if _WIN32_WCE
            string szPath = "\\Windows";
#else
                string szPath = Environment.GetFolderPath (nFolder);
#endif

                int ulCreateDirsFrom = szPath.Length;

                // dstrFilePath holds the special folder path no trailing '\'
                dstrFilePath = szPath + sFileStoragePath;

                // Make the %...% folder identifier
#if !_WIN32_WCE
                string sFolder = string.Format (CultureInfo.InvariantCulture, "%{0}%", (int) nFolder);
#else
        string sFolder = "\\Windows";
#endif

                // Add the %...% and path into reg data.
                dstrRegPath = sFolder + sFileStoragePath;

                // both dstrRegPath and dstrFilePath have trailing '\'

                // Now add any fileNameSpecifier directories
                int iBaseFile;
                if (sFileSpecifier == null || sFileSpecifier.Length == 0)
                {
                    iBaseFile = 0;
                }
                else
                {
                    iBaseFile = sFileSpecifier.LastIndexOf ('\\');
                    if (iBaseFile > 0)
                    {
                        // Specifier contains '\'
                        iBaseFile++; // part after last '\' becomes base file

                        int iStart = 0;
                        if (sFileSpecifier [0] == '\\')
                        {
                            iStart++; // Skip initial '\'
                        }

                        // Add file specifier path to file and key
                        dstrFilePath = dstrRegPath = sFileSpecifier.Substring (iStart, iBaseFile - iStart + 1);
                    }
                }

                // Create any new directories
                CreatePath (dstrFilePath, ulCreateDirsFrom);

                // Generate the actual file name
                string sFile = GenerateFileName (dstrFilePath, sFileSpecifier.Substring (iBaseFile + 1, sFileSpecifier.Length - iBaseFile + 1));

                // Add file name to path and reg key
                dstrRegPath += sFile;

                dstrFilePath += sFile;
            }
        }

        /// <summary>
        /// Given a file storage value from the registry, convert to a file path.
        /// This will extract the %...% value and finds the local special folder path.
        /// </summary>
        /// <param name="sRegPath"></param>
        /// <param name="dstrFilePath"></param>
        private static void RegPathToFilePath (string sRegPath, out string dstrFilePath)
        {
            // Is this a reference to a special folder 
            if (sRegPath [0] == '%')
            {
                // Find the second % symbol
                int iPosPercent = sRegPath.LastIndexOf ('%');

                // Convert the string between the %s to a number
                int nFolder = 0;
                if (!int.TryParse (sRegPath.Substring (iPosPercent, iPosPercent - 1), out nFolder))
                {
                    throw new ArgumentException (SR.Get (SRID.InvalidRegistryEntry, "sRegPath"), "sRegPath");
                }

                // Point to start of real path '\'
                if (sRegPath [++iPosPercent] != '\\')
                {
                    throw new ArgumentException (SR.Get (SRID.InvalidRegistryEntry, "sRegPath"), "sRegPath");
                }

#if _WIN32_WCE
        string szPath = @"\Windows";
#else
                string szPath = Environment.GetFolderPath ((Environment.SpecialFolder) nFolder);
#endif

                // filePath now has the special folder path (with no trailing '\')
                dstrFilePath = szPath + sRegPath.Substring (iPosPercent, sRegPath.Length - iPosPercent);
            }
            else
            {
                // Not a special folder so just copy
                dstrFilePath = sRegPath;
            }
        }

#if false
/****************************************************************************
* CSpObjectToken__DisplayUI *
*---------------------------*
*   Description:  
*       Display the specified type of UI
*
*   Return:
*   S_OK on success
*   FAILED(hr) otherwise
******************************************************************** robch */
private void DisplayUI(
    HWND hwndParent, 
    string sTitle, 
    string sTypeOfUI, 
    object pvExtraData, 
    uint cbExtraData, 
    object punkObject)
{
    // SPDBG_FUNC("CSpObjectToken::DisplayUI");
    HRESULT hr;
    CLSID clsidObject;

    if (m_fKeyDeleted)
    {
		// TODO
		throw new ObjectTokenException(SR.Get (SRID.TokenDeleted));
    }
    else if (!IsWindow(hwndParent) || 
             SP_IS_BAD_OPTIONAL_STRING_PTR(sTitle) || 
             SP_IS_BAD_STRING_PTR(sTypeOfUI) ||
             (pvExtraData != null && SPIsBadReadPtr(pvExtraData, cbExtraData)) ||
             (punkObject != null && SP_IS_BAD_INTERFACE_PTR(punkObject)))
    {
        E_INVALIDARG;
    }
    else if (m_cpTokenDelegate != null)
    {
        // NTRAID#SPEECH-7392-2000/08/31-robch: Maybe we should first delegate, and if that doesn't work, 
        // try this token's category ui...
        m_cpTokenDelegate.DisplayUI(
                    hwndParent, 
                    sTitle, 
                    sTypeOfUI, 
                    pvExtraData, 
                    cbExtraData, 
                    punkObject);
    }
    else
    {
        GetUIObjectClsid(sTypeOfUI, &clsidObject);

        ISpTokenUI cpTokenUI = cpTokenUI.CoCreateInstance(clsidObject);

        cpTokenUI.DisplayUI(
                            hwndParent, 
                            sTitle, 
                            sTypeOfUI, 
                            pvExtraData, 
                            cbExtraData, 
                            this, 
                            punkObject);
    }

    SPDBG_REPORT_ON_FAIL(hr);   
    return hr;
}
#endif

        /// <summary>
        /// Open the "Files" subkey of a specified data key's caller's sub key
        /// </summary>
        /// <param name="clsidCaller"></param>
        /// <param name="fCreateKey"></param>
        /// <returns></returns>
        private ObjectToken OpenFilesKey (Guid clsidCaller, bool fCreateKey)
        {
            // SPDBG_FUNC("CSpObjectToken::OpenFilesKey");

            ObjectToken cpClsidKey;

            // Convert the string clsid to a real clsid
            string dstrCLSID = clsidCaller.ToString ();

            // Either create the data key or open it
            if (fCreateKey)
            {
                cpClsidKey = (ObjectToken) CreateKey (dstrCLSID);
            }
            else
            {
                cpClsidKey = (ObjectToken) OpenKey (dstrCLSID);
            }

            // Either crate the files data key or open it
            ObjectToken ppKey;

            if (fCreateKey)
            {
                ppKey = (ObjectToken) cpClsidKey.CreateKey (SPTOKENKEY_FILES);
            }
            else
            {
                ppKey = (ObjectToken) cpClsidKey.OpenKey (SPTOKENKEY_FILES);
            }
            return ppKey;
        }

        /// <summary>
        /// Delete either a specific file (specified by sValueName) or all files
        /// (when sValueName == null) from the specified data key
        /// </summary>
        /// <param name="pDataKey"></param>
        /// <param name="sValueName"></param>
        private void DeleteFileFromKey (ObjectToken pDataKey, string sValueName)
        {
            // SPDBG_FUNC("CSpObjectToken::DeleteFileFromKey");

            // If the value name wasn't specified, we'll delete all the value's files
            if (sValueName == null)
            {
                // Loop thru the values
                foreach (RegistryEntry regEntry in (IEnumerable<RegistryEntry>) pDataKey)
                {
                    try
                    {
                        // Delete the file
                        DeleteFileFromKey (pDataKey, (string) regEntry.Value);
                    }
                    catch (IOException)
                    {
                        // ignore the File IO exception
                    }
                }
            }
            else
            {
                // Get the filename
                string sFile;
                string dstrRegPath = pDataKey.GetString (sValueName);

                // Convert the path stored in the registry to a real file path
                RegPathToFilePath (dstrRegPath, out sFile);

                // And delete the file
                // Ignore errors from DeleteFile, we can't let this stop us
                try
                {
                    File.Delete (sFile);
                }
                catch (IOException)
                {
                }
            }
        }

        /// <summary>
        /// Remove all filenames for a specified caller, or for all callers
        /// if pclsidCaller is null.
        /// </summary>
        /// <param name="pclsidCaller"></param>
        private void RemoveAllStorageFileNames (Guid pclsidCaller)
        {
            // SPDBG_FUNC("CSpObjectToken::RemoveAllStorageFileNames");

            // If the clsid wasn't specified, we'll delete all files from all
            // keys that are clsids
            if (pclsidCaller == Guid.Empty)
            {
                // Loop thru all the keys
                for (int i = 0; ; i++)
                {
                    // Get the next sub key
                    //TODO 
                    string dstrSubKey = null; //((ObjectToken) this).EnumKeys (i);
                    if (dstrSubKey != null)
                    {
                        break;
                    }

                    // If this key looks like a clsid, and it is, recursively call
                    // this function to delete the specific clsid's files
                    Guid clsid = new Guid (dstrSubKey);
                    RemoveAllStorageFileNames (clsid);
                }
            }
            else
            {
                // Open the files data key, and delete all the files
                ObjectToken cpFilesKey = OpenFilesKey (pclsidCaller, false);
                {
                    try
                    {
                        DeleteFileFromKey (cpFilesKey, null);
                    }
                    catch (SystemException)
                    {
                        // If we failed to delete this file then that should not stop us from deleting the other files.
                    }
                }
            }
        }

#endif

#if VSCOMPILE && false

        /// <summary>
        /// Get the UI object's clsid from the registry. First check under the
        /// token's root, then under the category
        /// </summary>
        /// <param name="sTypeOfUI"></param>
        /// <returns></returns>
        private Guid GetUIObjectClsid (string sTypeOfUI)
        {
            // SPDBG_FUNC("CSpObjectToken::GetUIObjectClsid");

            // We'll try and retrive the CLSID as a string from the token ui registry
            // key, then from the category ui registry key. We'll convert to an actual
            // GUID at the end of the function
            string dstrClsid = null;

            //--- Try getting the clsid from token's UI key
            ObjectToken cpTokenUI = (ObjectToken) OpenKey (SPTOKENKEY_UI);

            ObjectToken cpType = (ObjectToken) cpTokenUI.OpenKey (sTypeOfUI);
            if (cpType.TryGetString (SPTOKENVALUE_CLSID, out dstrClsid))
            {
                //--- Try getting the clsid from the category's UI key
                if (CategoryId != null)
                {
                    ObjectTokenCategory cpCategory = CategoryFromId (CategoryId, false);

                    ObjectToken cpTokenUI2 = (ObjectToken) cpCategory.OpenKey (SPTOKENKEY_UI);
                    {
                        ObjectToken cpType2 = (ObjectToken) cpTokenUI2.OpenKey (sTypeOfUI);
                        dstrClsid = cpType2.GetString (SPTOKENVALUE_CLSID);
                    }
                }
            }

            // If we were successful at getting the clsid, convert it
            return new Guid (dstrClsid);
        }

#endif

        #endregion

        //*******************************************************************
        //
        // Private Types
        //
        //*******************************************************************

        #region Private Types

        //--- ISpObjectWithToken ----------------------------------------------------
        [ComImport, Guid("5B559F40-E952-11D2-BB91-00C04F8EE6C0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ISpObjectWithToken
        {
            [PreserveSig]
            int SetObjectToken(ISpObjectToken pToken);
            [PreserveSig]
            int GetObjectToken(IntPtr ppToken);
        };

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region private Fields

        // Specifier used to generate a random filename.
        private const string sGenerateFileNameSpecifier = "{0}";

        private const string SPTOKENVALUE_CLSID = "CLSID";

        private ISpObjectToken _sapiObjectToken;

        private bool _disposeSapiObjectToken;

        private RegistryDataKey _attributes;

#if VSCOMPILE

        // Prefix used for storage files if not otherwise set
        private const string sDefaultFilePrefix = "SP_";

        // Extension used for storage files if not otherwise set
        private const string sDefaultFileSuffix = ".dat";

        private const string SPTOKENKEY_FILES = "Files";

        private const string SPTOKENKEY_UI = "UI";

        // Relative path below special folders where storage files are stored
        private const string sFileStoragePath = "\\Microsoft\\Speech\\Files\\";

#endif
        #endregion
    }

    internal enum VoiceCategory
    {
        Default,
        ScanSoft
    }

    #region SAPI interface

#if SPEECHSERVER
    // This interface is necessary to support "pre-private-SAPI" for MSS (still used by TTS voices)
    [ComImport, Guid("14056589-E16C-11D2-BB90-00C04F8EE6C0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISpObjectTokenMss : ISpDataKey
    {
        // ISpDataKey Methods
        [PreserveSig]
        new int SetData([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, UInt32 cbData, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Byte[] pData);
        [PreserveSig]
        new int GetData([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, ref UInt32 pcbData, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] Byte[] pData);
        [PreserveSig]
        new int SetStringValue([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, [MarshalAs(UnmanagedType.LPWStr)] string pszValue);
        [PreserveSig]
        new int GetStringValue([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszValue);
        [PreserveSig]
        new int SetDWORD([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, UInt32 dwValue);
        [PreserveSig]
        new int GetDWORD([MarshalAs(UnmanagedType.LPWStr)] string pszValueName, ref UInt32 pdwValue);
        [PreserveSig]
        new int OpenKey([MarshalAs(UnmanagedType.LPWStr)] string pszSubKeyName, out ISpDataKey ppSubKey);
        [PreserveSig]
        new int CreateKey([MarshalAs(UnmanagedType.LPWStr)] string pszSubKey, out ISpDataKey ppSubKey);
        [PreserveSig]
        new int DeleteKey([MarshalAs(UnmanagedType.LPWStr)] string pszSubKey);
        [PreserveSig]
        new int DeleteValue([MarshalAs(UnmanagedType.LPWStr)] string pszValueName);
        [PreserveSig]
        new int EnumKeys(UInt32 Index, [MarshalAs(UnmanagedType.LPWStr)] out string ppszSubKeyName);
        [PreserveSig]
        new int EnumValues(UInt32 Index, [MarshalAs(UnmanagedType.LPWStr)] out string ppszValueName);

        // ISpObjectToken Methods
        void SetId([MarshalAs(UnmanagedType.LPWStr)] string pszCategoryId, [MarshalAs(UnmanagedType.LPWStr)] string pszTokenId, [MarshalAs(UnmanagedType.Bool)] bool fCreateIfNotExist);
        void GetId(out IntPtr ppszCoMemTokenId);
        void Slot15(); // void GetCategory(out ISpObjectTokenCategory ppTokenCategory);
        void Slot16(); // void CreateInstance(object pUnkOuter, UInt32 dwClsContext, ref Guid riid, ref IntPtr ppvObject);
        void Slot17(); // void GetStorageFileName(ref Guid clsidCaller, [MarshalAs(UnmanagedType.LPWStr)] string pszValueName, [MarshalAs(UnmanagedType.LPWStr)] string pszFileNameSpecifier, UInt32 nFolder, [MarshalAs(UnmanagedType.LPWStr)] out string ppszFilePath);
        void Slot18(); // void RemoveStorageFileName(ref Guid clsidCaller, [MarshalAs(UnmanagedType.LPWStr)] string pszKeyName, int fDeleteFile);
        void Slot19(); // void Remove(ref Guid pclsidCaller);
        void Slot20(); // void IsUISupported([MarshalAs(UnmanagedType.LPWStr)] string pszTypeOfUI, IntPtr pvExtraData, UInt32 cbExtraData, object punkObject, ref Int32 pfSupported);
        void Slot21(); // void DisplayUI(UInt32 hWndParent, [MarshalAs(UnmanagedType.LPWStr)] string pszTitle, [MarshalAs(UnmanagedType.LPWStr)] string pszTypeOfUI, IntPtr pvExtraData, UInt32 cbExtraData, object punkObject);
        void MatchesAttributes([MarshalAs(UnmanagedType.LPWStr)] string pszAttributes, [MarshalAs(UnmanagedType.Bool)] out bool pfMatches);
    }
#endif
    #endregion
}
