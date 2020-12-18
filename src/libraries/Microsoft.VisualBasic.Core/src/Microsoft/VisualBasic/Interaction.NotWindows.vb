' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning

Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.CompilerServices.ExceptionUtils

Namespace Microsoft.VisualBasic

    Public Partial Module Interaction

        '============================================================================
        ' Registry functions.
        '============================================================================

        <SupportedOSPlatform("windows")>
        Public Sub DeleteSetting(ByVal AppName As String, Optional ByVal Section As String = Nothing, Optional ByVal Key As String = Nothing)
            Throw New PlatformNotSupportedException()
        End Sub

        <SupportedOSPlatform("windows")>
        Public Function GetAllSettings(ByVal AppName As String, ByVal Section As String) As String(,)
            Throw New PlatformNotSupportedException()
        End Function

        <SupportedOSPlatform("windows")>
        Public Function GetSetting(ByVal AppName As String, ByVal Section As String, ByVal Key As String, Optional ByVal [Default] As String = "") As String
            Throw New PlatformNotSupportedException()
        End Function

        <SupportedOSPlatform("windows")>
        Public Sub SaveSetting(ByVal AppName As String, ByVal Section As String, ByVal Key As String, ByVal Setting As String)
            Throw New PlatformNotSupportedException()
        End Sub

    End Module

End Namespace

