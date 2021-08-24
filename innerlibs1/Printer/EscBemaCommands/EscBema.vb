﻿Imports System.Text
Imports InnerLibs.Printer
Imports InnerLibs.Printer.Command

Namespace EscBemaCommands

    Public Class EscBema
        Implements IPrintCommand

#Region "Properties"

        Public Property FontMode As IFontMode Implements IPrintCommand.FontMode
        Public Property FontWidth As IFontWidth Implements IPrintCommand.FontWidth
        Public Property Alignment As IAlignment Implements IPrintCommand.Alignment
        Public Property PaperCut As IPaperCut Implements IPrintCommand.PaperCut
        Public Property Drawer As IDrawer Implements IPrintCommand.Drawer
        Public Property QrCode As IQrCode Implements IPrintCommand.QrCode
        Public Property Image As IImage Implements IPrintCommand.Image
        Public Property BarCode As IBarCode Implements IPrintCommand.BarCode
        Public Property InitializePrint As IInitializePrint Implements IPrintCommand.InitializePrint

        Public ReadOnly Property ColsNomal As Integer Implements IPrintCommand.ColsNomal
            Get
                Return 50
            End Get
        End Property

        Public ReadOnly Property ColsCondensed As Integer Implements IPrintCommand.ColsCondensed
            Get
                Return 67
            End Get
        End Property

        Public ReadOnly Property ColsExpanded As Integer Implements IPrintCommand.ColsExpanded
            Get
                Return 25
            End Get
        End Property

        Public ReadOnly Property DefaultEncoding As Encoding Implements IPrintCommand.DefaultEncoding
            Get
                Try
                    Return Encoding.GetEncoding(850)

                Catch ex As Exception
                    Debug.WriteLine(ex)
                    Return Encoding.Default
                End Try
            End Get
        End Property

        Public Property Encoding As Encoding Implements IPrintCommand.Encoding
            Get
                Return FontMode.Encoding.NullCoalesce(DefaultEncoding)
            End Get
            Set(value As Encoding)
                value = value.NullCoalesce(DefaultEncoding)

                FontMode.Encoding = value
                FontWidth.Encoding = value
                QrCode.Encoding = value
                BarCode.Encoding = value
            End Set
        End Property


#End Region

#Region "Constructor"

        Public Sub New()
            FontMode = New FontMode()
            FontWidth = New FontWidth()
            Alignment = New Alignment()
            PaperCut = New PaperCut()
            Drawer = New Drawer()
            QrCode = New QrCode()
            Image = New EscPosCommands.Image()
            BarCode = New BarCode()
            InitializePrint = New InitializePrint()
        End Sub

#End Region

#Region "Methods"

        Public Function Separator() As Byte() Implements IPrintCommand.Separator
            Return FontMode.Condensed(PrinterModeState.On).AddTextBytes(New String("-"c, ColsCondensed), DefaultEncoding).AddBytes(FontMode.Condensed(PrinterModeState.Off)).AddLF()
        End Function

        Public Function AutoTest() As Byte() Implements IPrintCommand.AutoTest
            Return New Byte() {&H1D, &HF9, &H29, &H30}
        End Function



#End Region

    End Class

End Namespace