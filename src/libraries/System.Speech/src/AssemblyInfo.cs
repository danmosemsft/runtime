using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
//[assembly: AssemblyTitle ("Microsoft Managed Speech API - Recognition")]
//[assembly: AssemblyDescription ("Speech Recognition Services")]
//[assembly: AssemblyCompany ("Microsoft Corporation")]
//[assembly: AssemblyProduct ("Microsoft (R) Windows (R) Operating System")]
//[assembly: AssemblyCopyright ("Copyright (c) 1998-2006 Microsoft Corporation.")]
//[assembly: AssemblyTrademark ("Microsoft (R) is a registered trademark of Microsoft Corporation. Windows (R) is a registered trademark of Microsoft Corporation.")]
//[assembly: AssemblyCulture ("")]
//[assembly: ComVisible (false)] // Since System.Speech has a native Automation API, COM clients should not use the Managed API directly

//TODO turn this on when generics will be CLS compliant
[assembly: CLSCompliant (true)]

// the symbole VSCOMPILE should never be defines in any build file
#if VSCOMPILE
[assembly: InternalsVisibleTo ("SrgsTest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cf4a0bbf354b6d8d0c39e7bc40dd0be16a32ba9d763e8d04fd9591b92d7269dd09c2c65ec7563ce393aca71913bea13dd6a20d676ed7ddc726f846fce66800bb034903619a1baa520f5f758946cf2b4af6ba7c310d02d092a5cf51be6d52e88633f502474b4f461d850b63219c09a3373a23d73156ee03d6c7b38c36d1211fac")]
[assembly: InternalsVisibleTo ("VsUnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cf4a0bbf354b6d8d0c39e7bc40dd0be16a32ba9d763e8d04fd9591b92d7269dd09c2c65ec7563ce393aca71913bea13dd6a20d676ed7ddc726f846fce66800bb034903619a1baa520f5f758946cf2b4af6ba7c310d02d092a5cf51be6d52e88633f502474b4f461d850b63219c09a3373a23d73156ee03d6c7b38c36d1211fac")]
[assembly: InternalsVisibleTo("TTSIPATest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cf4a0bbf354b6d8d0c39e7bc40dd0be16a32ba9d763e8d04fd9591b92d7269dd09c2c65ec7563ce393aca71913bea13dd6a20d676ed7ddc726f846fce66800bb034903619a1baa520f5f758946cf2b4af6ba7c310d02d092a5cf51be6d52e88633f502474b4f461d850b63219c09a3373a23d73156ee03d6c7b38c36d1211fac")]
[assembly: InternalsVisibleTo("IPATest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cf4a0bbf354b6d8d0c39e7bc40dd0be16a32ba9d763e8d04fd9591b92d7269dd09c2c65ec7563ce393aca71913bea13dd6a20d676ed7ddc726f846fce66800bb034903619a1baa520f5f758946cf2b4af6ba7c310d02d092a5cf51be6d52e88633f502474b4f461d850b63219c09a3373a23d73156ee03d6c7b38c36d1211fac")]
[assembly: AssemblyVersion("1.0.0.0")]
#endif

[assembly: FileIOPermission (SecurityAction.RequestOptional, Unrestricted=true)]
[assembly: EnvironmentPermission (SecurityAction.RequestOptional, Unrestricted=true)]
[assembly: RegistryPermission (SecurityAction.RequestOptional, Unrestricted=true)]
[assembly: SecurityPermission (SecurityAction.RequestOptional, Unrestricted=true)]
[assembly: PermissionSet(SecurityAction.RequestOptional, Unrestricted=true)]
