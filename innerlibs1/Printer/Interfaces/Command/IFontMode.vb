﻿Imports System.Text

Namespace Printer.Command

    Public Interface IFontMode
        Property Encoding As Encoding

        Function Italic(ByVal value As String) As Byte()

        Function Italic(ByVal state As PrinterModeState) As Byte()

        Function Bold(ByVal value As String) As Byte()

        Function Bold(ByVal state As PrinterModeState) As Byte()

        Function Underline(ByVal value As String) As Byte()

        Function Underline(ByVal state As PrinterModeState) As Byte()

        Function Expanded(ByVal value As String) As Byte()

        Function Expanded(ByVal state As PrinterModeState) As Byte()

        Function Condensed(ByVal value As String) As Byte()

        Function Condensed(ByVal state As PrinterModeState) As Byte()

    End Interface

End Namespace