' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning
Imports Microsoft.Win32

Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.CompilerServices.ExceptionUtils

Namespace Microsoft.VisualBasic

    Public Partial Module Interaction

        '============================================================================
        ' Registry functions.
        '============================================================================

        <SupportedOSPlatform("windows")>
        Public Sub DeleteSetting(ByVal AppName As String, Optional ByVal Section As String = Nothing, Optional ByVal Key As String = Nothing)
            Dim AppSection As String
            Dim UserKey As RegistryKey
            Dim AppSectionKey As RegistryKey = Nothing

            CheckPathComponent(AppName)
            AppSection = FormRegKey(AppName, Section)

            Try
                UserKey = Registry.CurrentUser

                If IsNothing(Key) OrElse (Key.Length = 0) Then
                    UserKey.DeleteSubKeyTree(AppSection)
                Else
                    AppSectionKey = UserKey.OpenSubKey(AppSection, True)
                    If AppSectionKey Is Nothing Then
                        Throw New ArgumentException(SR.Format(SR.Argument_InvalidValue1, "Section"))
                    End If

                    AppSectionKey.DeleteValue(Key)
                End If

            Catch ex As Exception
                Throw ex
            Finally
                If AppSectionKey IsNot Nothing Then
                    AppSectionKey.Close()
                End If
            End Try
        End Sub

        <SupportedOSPlatform("windows")>
        Public Function GetAllSettings(ByVal AppName As String, ByVal Section As String) As String(,)
            Dim rk As RegistryKey
            Dim sAppSect As String
            Dim i As Integer
            Dim lUpperBound As Integer
            Dim sValueNames() As String
            Dim sValues(,) As String
            Dim o As Object
            Dim sName As String

            ' Check for empty string in path
            CheckPathComponent(AppName)
            CheckPathComponent(Section)
            sAppSect = FormRegKey(AppName, Section)
            rk = Registry.CurrentUser.OpenSubKey(sAppSect)


            If rk Is Nothing Then
                Return Nothing
            End If

            GetAllSettings = Nothing
            Try
                If rk.ValueCount <> 0 Then
                    sValueNames = rk.GetValueNames()
                    lUpperBound = sValueNames.GetUpperBound(0)
                    ReDim sValues(lUpperBound, 1)

                    For i = 0 To lUpperBound
                        sName = sValueNames(i)

                        'Assign name
                        sValues(i, 0) = sName

                        'Assign value
                        o = rk.GetValue(sName)

                        If (Not o Is Nothing) AndAlso (TypeOf o Is String) Then
                            sValues(i, 1) = o.ToString()
                        End If
                    Next i

                    GetAllSettings = sValues
                End If

            Catch ex As StackOverflowException
                Throw ex
            Catch ex As OutOfMemoryException
                Throw ex

            Catch ex As Exception
                'Consume the exception

            Finally
                rk.Close()
            End Try
        End Function

        <SupportedOSPlatform("windows")>
        Public Function GetSetting(ByVal AppName As String, ByVal Section As String, ByVal Key As String, Optional ByVal [Default] As String = "") As String
            Dim rk As RegistryKey = Nothing
            Dim sAppSect As String
            Dim o As Object

            'Check for empty strings
            CheckPathComponent(AppName)
            CheckPathComponent(Section)
            CheckPathComponent(Key)
            If [Default] Is Nothing Then
                [Default] = ""
            End If

            'Open the sub key
            sAppSect = FormRegKey(AppName, Section)
            Try
                rk = Registry.CurrentUser.OpenSubKey(sAppSect)    'By default, does not request write permission

                'Get the key's value
                If rk Is Nothing Then
                    Return [Default]
                End If

                o = rk.GetValue(Key, [Default])
            Finally
                If rk IsNot Nothing Then
                    rk.Close()
                End If
            End Try

            If o Is Nothing Then
                Return Nothing
            ElseIf TypeOf o Is String Then ' - odd that this is required to be a string when it isn't in GetAllSettings() above...
                Return DirectCast(o, String)
            Else
                Throw New ArgumentException(SR.Argument_InvalidValue)
            End If
        End Function

        <SupportedOSPlatform("windows")>
        Public Sub SaveSetting(ByVal AppName As String, ByVal Section As String, ByVal Key As String, ByVal Setting As String)
            Dim rk As RegistryKey
            Dim sIniSect As String

            ' Check for empty string in path
            CheckPathComponent(AppName)
            CheckPathComponent(Section)
            CheckPathComponent(Key)

            sIniSect = FormRegKey(AppName, Section)
            rk = Registry.CurrentUser.CreateSubKey(sIniSect)

            If rk Is Nothing Then
                'Subkey could not be created
                Throw New ArgumentException(SR.Format(SR.Interaction_ResKeyNotCreated1, sIniSect))
            End If

            Try
                rk.SetValue(Key, Setting)
            Catch ex As Exception
                Throw ex
            Finally
                rk.Close()
            End Try
        End Sub

        '============================================================================
        ' Private functions.
        '============================================================================
        Private Function FormRegKey(ByVal sApp As String, ByVal sSect As String) As String
            Const REGISTRY_INI_ROOT As String = "Software\VB and VBA Program Settings"
            'Forms the string for the key value
            If IsNothing(sApp) OrElse (sApp.Length = 0) Then
                FormRegKey = REGISTRY_INI_ROOT
            ElseIf IsNothing(sSect) OrElse (sSect.Length = 0) Then
                FormRegKey = REGISTRY_INI_ROOT & "\" & sApp
            Else
                FormRegKey = REGISTRY_INI_ROOT & "\" & sApp & "\" & sSect
            End If
        End Function

        Private Sub CheckPathComponent(ByVal s As String)
            If (s Is Nothing) OrElse (s.Length = 0) Then
                Throw New ArgumentException(SR.Argument_PathNullOrEmpty)
            End If
        End Sub

    End Module

End Namespace

