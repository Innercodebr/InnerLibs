﻿Imports InnerLibs.Printer
Imports InnerLibs.Printer.Command

Namespace EscPosCommands

    Friend Class EscPos
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
                Return 48
            End Get
        End Property

        Public ReadOnly Property ColsCondensed As Integer Implements IPrintCommand.ColsCondensed
            Get
                Return 64
            End Get
        End Property

        Public ReadOnly Property ColsExpanded As Integer Implements IPrintCommand.ColsExpanded
            Get
                Return 24
            End Get
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
            Image = New Image()
            BarCode = New BarCode()
            InitializePrint = New InitializePrint()
        End Sub

#End Region

#Region "Methods"

        Public Function Separator() As Byte() Implements IPrintCommand.Separator
            Return FontMode.Condensed(PrinterModeState.On).AddBytes(New String("-"c, ColsCondensed)).AddBytes(FontMode.Condensed(PrinterModeState.Off)).AddLF()
        End Function

        Public Function AutoTest() As Byte() Implements IPrintCommand.AutoTest
            Return New Byte() {29, 40, 65, 2, 0, 0, 2}
        End Function

#End Region

    End Class

End Namespace