﻿Imports System.Text
Imports InnerLibs.Printer
Imports InnerLibs.Printer.Command

Namespace EscBemaCommands

    Friend Class FontMode
        Implements IFontMode

        Public Property Encoding As Encoding Implements IFontMode.Encoding


        Public Function Italic(ByVal value As String) As Byte() Implements IFontMode.Italic
            Return Italic(PrinterModeState.On).AddTextBytes(value, Encoding).AddBytes(Italic(PrinterModeState.Off)).AddLF()
        End Function

        Public Function Italic(ByVal state As PrinterModeState) As Byte() Implements IFontMode.Italic
            Return If(state = PrinterModeState.On, New Byte() {27, "4"c.ToByte()}, New Byte() {27, "5"c.ToByte()})
        End Function

        Public Function Bold(ByVal value As String) As Byte() Implements IFontMode.Bold
            Return Bold(PrinterModeState.On).AddTextBytes(value, Encoding).AddBytes(Bold(PrinterModeState.Off)).AddLF()
        End Function

        Public Function Bold(ByVal state As PrinterModeState) As Byte() Implements IFontMode.Bold
            Return If(state = PrinterModeState.On, New Byte() {27, "E"c.ToByte()}, New Byte() {27, "F"c.ToByte()})
        End Function

        Public Function Underline(ByVal value As String) As Byte() Implements IFontMode.Underline
            Return Underline(PrinterModeState.On).AddTextBytes(value, Encoding).AddBytes(Underline(PrinterModeState.Off)).AddLF()
        End Function

        Public Function Underline(ByVal state As PrinterModeState) As Byte() Implements IFontMode.Underline
            Return If(state = PrinterModeState.On, New Byte() {27, "-"c.ToByte(), 1}, New Byte() {27, "-"c.ToByte(), 0})
        End Function

        Public Function Expanded(ByVal value As String) As Byte() Implements IFontMode.Expanded
            Return Expanded(PrinterModeState.On).AddTextBytes(value, Encoding).AddBytes(Expanded(PrinterModeState.Off)).AddLF()
        End Function

        Public Function Expanded(ByVal state As PrinterModeState) As Byte() Implements IFontMode.Expanded
            Return New Byte() {27, "W"c.ToByte(), If(state = PrinterModeState.On, "1"c.ToByte(), "0"c.ToByte())}
        End Function

        Public Function Condensed(ByVal value As String) As Byte() Implements IFontMode.Condensed
            Return Condensed(PrinterModeState.On).AddTextBytes(value, Encoding).AddBytes(Condensed(PrinterModeState.Off)).AddLF()
        End Function

        Public Function Condensed(ByVal state As PrinterModeState) As Byte() Implements IFontMode.Condensed
            Return If(state = PrinterModeState.On, New Byte() {27, 15}, New Byte() {27, "P"c.ToByte()})
        End Function

    End Class

End Namespace