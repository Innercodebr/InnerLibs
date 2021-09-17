﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

''' <summary>
''' Modulo de Conversão de Cores
''' </summary>
''' <remarks></remarks>
Public Module ColorConvert

    Public Function GrayscalePallete(Amount As Integer) As IEnumerable(Of Color)
        Return MonochromaticPallete(Color.White, Amount)
    End Function

    ''' <summary>
    ''' Gera uma paleta de cores monocromatica com <paramref name="Amount"/> amostras a partir de uma <paramref name="Color"/> base.
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    ''' <remarks>A distancia entre as cores será maior se a quantidade de amostras for pequena</remarks>
    Public Function MonochromaticPallete(Color As Color, Amount As Integer) As IEnumerable(Of Color)

        Dim t = New RuleOfThree(Amount, 100, 1, Nothing)

        Dim Percent = t.UnknowValue?.ToSingle()

        Color = Color.White.MergeWith(Color)

        Dim l As New List(Of Color)
        For index = 1 To Amount
            Color = Color.MakeDarker(Percent)
            l.Add(Color)
        Next
        Return l
    End Function

    ''' <summary>
    ''' Retorna  a cor negativa de uma cor
    ''' </summary>
    ''' <param name="TheColor">Cor</param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetNegativeColor(TheColor As Color) As Color
        Return Color.FromArgb(255 - TheColor.R, 255 - TheColor.G, 255 - TheColor.B)
    End Function

    ''' <summary>
    ''' Retorna uma cor de contraste baseado na iluminacao da primeira cor: Uma cor clara se a primeira for escura. Uma cor escura se a primeira for clara
    ''' </summary>
    ''' <param name="TheColor">Primeira cor</param>
    ''' <param name="Percent">Grau de mesclagem da cor escura ou clara</param>
    ''' <returns>Uma cor clara se a primeira cor for escura, uma cor escura se a primeira for clara</returns>
    <Extension()>
    Public Function GetContrastColor(TheColor As Color, Optional Percent As Single = 70) As Color
        Dim a As Double = 1 - (0.299 * TheColor.R + 0.587 * TheColor.G + 0.114 * TheColor.B) / 255
        Dim d = If(a < 0.5, 0, 255)
        Return TheColor.MergeWith(Color.FromArgb(d, d, d), Percent)
    End Function

    ''' <summary>
    ''' Verifica se uma cor é escura
    ''' </summary>
    ''' <param name="TheColor">Cor</param>
    ''' <returns></returns>
    <Extension>
    Public Function IsDark(TheColor As Color) As Boolean
        Dim Y = 0.2126 * TheColor.R + 0.7152 * TheColor.G + 0.0722 * TheColor.B
        Return Y < 128
    End Function

    ''' <summary>
    ''' Verifica se uma clor é clara
    ''' </summary>
    ''' <param name="TheColor">Cor</param>
    ''' <returns></returns>
    <Extension>
    Public Function IsLight(TheColor As Color) As Boolean
        Return Not TheColor.IsDark()
    End Function

    ''' <summary>
    ''' Mescla duas cores a partir de uma porcentagem
    ''' </summary>
    ''' <param name="TheColor">Cor principal</param>
    ''' <param name="AnotherColor">Cor de mesclagem</param>
    ''' <param name="percent">Porcentagem de mescla</param>
    ''' <returns></returns>
    <Extension>
    Public Function MergeWith(TheColor As Color, AnotherColor As Color, Optional Percent As Single = 50) As Color
        Return TheColor.Lerp(AnotherColor, Percent / 100)
    End Function

    ''' <summary>
    ''' Escurece a cor mesclando ela com preto
    ''' </summary>
    ''' <param name="TheColor">Cor</param>
    ''' <param name="percent">porcentagem de mesclagem</param>
    ''' <returns></returns>
    <Extension>
    Public Function MakeDarker(TheColor As Color, Optional Percent As Single = 50) As Color
        Return TheColor.MergeWith(Color.Black, Percent)
    End Function

    ''' <summary>
    ''' Clareia a cor mistuando ela com branco
    ''' </summary>
    ''' <param name="TheColor">Cor</param>
    ''' <param name="percent">Porcentagem de mesclagem</param>
    ''' <returns></returns>
    <Extension>
    Public Function MakeLighter(TheColor As Color, Optional Percent As Single = 50) As Color
        Return TheColor.MergeWith(Color.White, Percent)
    End Function

    ''' <summary>
    ''' Mescla duas cores usando Lerp
    ''' </summary>
    ''' <param name="FromColor">Cor</param>
    ''' <param name="ToColor">Outra cor</param>
    ''' <param name="amount">Indice de mesclagem</param>
    ''' <returns></returns>
    <Extension>
    Public Function Lerp(FromColor As Color, ToColor As Color, Amount As Single) As Color
        ' start colours as lerp-able floats
        Dim sr As Single = FromColor.R, sg As Single = FromColor.G, sb As Single = FromColor.B
        ' end colours as lerp-able floats
        Dim er As Single = ToColor.R, eg As Single = ToColor.G, eb As Single = ToColor.B
        ' lerp the colours to get the difference
        Dim r As Byte = CByte(sr.Lerp(er, Amount)), g As Byte = CByte(sg.Lerp(eg, Amount)), b As Byte = CByte(sb.Lerp(eb, Amount))
        ' return the new colour
        Return Color.FromArgb(r, g, b)
    End Function

    ''' <summary>
    ''' Converte uma cor de sistema para hexadecimal
    ''' </summary>
    ''' <param name="Color">Cor do sistema</param>
    ''' <param name="Hash">parametro indicando se a cor deve ser retornada com ou sem hashsign (#)</param>
    ''' <returns>string contendo o hexadecimal da cor</returns>

    <Extension()>
    Public Function ToHexadecimal(Color As System.Drawing.Color, Optional Hash As Boolean = True) As String
        Return (Color.R.ToString("X2") & Color.G.ToString("X2") & Color.B.ToString("X2")).PrependIf("#", Hash)
    End Function

    ''' <summary>
    ''' Converte uma cor de sistema para CSS RGB
    ''' </summary>
    ''' <param name="Color">Cor do sistema</param>
    ''' <returns>String contendo a cor em RGB</returns>

    <Extension()>
    Public Function ToCssRGB(Color As System.Drawing.Color) As String
        Return "rgb(" & Color.R.ToString() & "," & Color.G.ToString() & "," & Color.B.ToString() & ")"
    End Function

    <Extension()>
    Public Function ToCssRGBA(Color As System.Drawing.Color) As String
        Return "rgba(" & Color.R.ToString() & "," & Color.G.ToString() & "," & Color.B.ToString() & "," & Color.A.ToString() & ")"
    End Function

    <Extension()> Public Function IsHexaDecimalColor(ByVal Text As String) As Boolean
        Text = Text.RemoveFirstEqual("#")
        Dim myRegex As Regex = New Regex("^[a-fA-F0-9]+$")
        Return Text.IsNotBlank AndAlso myRegex.IsMatch(Text)
    End Function

    ''' <summary>
    ''' Gera uma cor a partir de uma palavra
    ''' </summary>
    ''' <param name="Text">Pode ser um texto em branco (Cor aleatória), uma <see cref="KnownColor"/> (retorna aquela cor exata) ou uma palavra qualquer (gera proceduralmente uma cor)</param>
    ''' <returns></returns>
    <Extension> Public Function ToColor(Text As String) As Color
        If Text.IsBlank() Then
            Return Color.Transparent
        End If

        If Text = "random" Then
            Return RandomColor()
        End If

        If Text.IsIn([Enum].GetNames(GetType(KnownColor)), StringComparer.InvariantCultureIgnoreCase) Then Return Color.FromName(Text)

        If Text.IsNumber Then
            Return Color.FromArgb(Text.ToInteger())
        End If

        If Text.IsHexaDecimalColor Then
            Return ColorTranslator.FromHtml("#" & Text.RemoveFirstEqual("#").IfBlank("000000"))
        End If

        Dim coresInt = Text.GetWords.Select(Function(p) p.ToCharArray().Sum(Function(a) AscW(a) ^ 2 * p.Length)).Sum()

        Return Color.FromArgb(255, Color.FromArgb(coresInt))

    End Function

    ''' <summary>
    ''' Gera uma cor aleatória misturandoo ou não os canais RGB
    ''' </summary>
    ''' <param name="Red">-1 para Random ou de 0 a 255 para especificar o valor</param>
    ''' <param name="Green">-1 para Random ou de 0 a 255 para especificar o valor</param>
    ''' <param name="Blue">-1 para Random ou de 0 a 255 para especificar o valor</param>
    ''' <returns></returns>
    Public Function RandomColor(Optional Red As Integer = -1, Optional Green As Integer = -1, Optional Blue As Integer = -1, Optional Alpha As Integer = 255) As Color
        Red = If(Red < 0, RandomNumber(0, 255), Red).LimitRange(Of Integer)(0, 255)
        Green = If(Green < 0, RandomNumber(0, 255), Green).LimitRange(Of Integer)(0, 255)
        Blue = If(Blue < 0, RandomNumber(0, 255), Blue).LimitRange(Of Integer)(0, 255)
        Alpha = Alpha.LimitRange(Of Integer)(0, 255)
        Return Color.FromArgb(Alpha, Red, Green, Blue)
    End Function

    ''' <summary>
    ''' Lista com todas as <see cref="KnownColor"/> convertidas em <see cref="System.Drawing.Color"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property KnowColors As IEnumerable(Of Color)
        Get
            Return [Enum].GetValues(GetType(KnownColor)).Cast(Of KnownColor)().Where(Function(x) x.ToInteger() >= 27).Select(Function(x) Color.FromKnownColor(x))
        End Get
    End Property

    ''' <summary>
    ''' Retorna uma <see cref="KnownColor"/> mais proxima de outra cor
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    <Extension()> Public Function GetClosestKnowColor(Color As Color) As Color
        Dim closest_distance As Double = Double.MaxValue
        Dim closest As Color = Color.White
        For Each kc In KnowColors
            'Calculate Euclidean Distance
            Dim r_dist_sqrd As Double = Math.Pow(CDbl(Color.R) - CDbl(kc.R), 2)
            Dim g_dist_sqrd As Double = Math.Pow(CDbl(Color.G) - CDbl(kc.G), 2)
            Dim b_dist_sqrd As Double = Math.Pow(CDbl(Color.B) - CDbl(kc.B), 2)
            Dim d As Double = Math.Sqrt(r_dist_sqrd + g_dist_sqrd + b_dist_sqrd)
            If d < closest_distance Then
                closest_distance = d
                closest = kc
            End If
        Next
        Return closest
    End Function

    ''' <summary>
    ''' Retorna o nome comum mais proximo a esta cor
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    <Extension()> Public Function GetClosestColorName(Color As Color) As String
        Return Color.GetClosestKnowColor().Name
    End Function

    ''' <summary>
    ''' Retorna o nome da cor
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    <Extension()> Public Function GetColorName(Color As Color) As String
        For Each namedColor In KnowColors
            Return namedColor.Name
        Next
        Return Color.Name
    End Function

    ''' <summary>
    ''' Verifica se uma cor é legivel sobre outra
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <param name="BackgroundColor"></param>
    ''' <param name="Size"></param>
    ''' <returns></returns>
    <Extension()> Public Function IsReadable(Color As Color, BackgroundColor As Color, Optional Size As Integer = 10) As Boolean
        If Color.A = 0 Then Return False
        If BackgroundColor.A = 0 Then Return True
        Dim diff = BackgroundColor.R * 0.299 + BackgroundColor.G * 0.587 + BackgroundColor.B * 0.114 - Color.R * 0.299 - Color.G * 0.587 - Color.B * 0.114
        Return Not ((diff < (1.5 + 141.162 * Math.Pow(0.975, Size)))) AndAlso (diff > (-0.5 - 154.709 * Math.Pow(0.99, Size)))
    End Function

End Module

Public Class HSVColor
    Implements IComparable(Of Integer)
    Implements IComparable(Of HSVColor)
    Implements IComparable(Of System.Drawing.Color)
    Implements IComparable
    Private _h, _s, _v As Double
    Private _name As String
    Private _scolor As Color

    ''' <summary>
    ''' Gera uma <see cref="HSVColor"/> opaca aleatoria
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    Public Shared Function RandomColor(Optional Name As String = Nothing) As HSVColor
        Return New HSVColor(ColorConvert.RandomColor(), Name)
    End Function

    ''' <summary>
    ''' Gera uma <see cref="HSVColor"/> opaca aleatoria dentro de um Mood especifico
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    Public Shared Function RandomColor(Mood As ColorMood, Optional Name As String = Nothing) As HSVColor
        Return RandomColorList(1, Mood).FirstOrDefault()
    End Function

    ''' <summary>
    ''' Gera uma lista com <see cref="HSVColor"/>   aleatorias
    ''' </summary>
    ''' <param name="Quantity"></param>
    ''' <returns></returns>
    Public Shared Function RandomColorList(Quantity As Integer, Mood As ColorMood) As IEnumerable(Of HSVColor)
        Return Enumerable.Range(1, Quantity).Select(Function(x)
                                                        Dim c As HSVColor
                                                        Do
                                                            c = HSVColor.RandomColor()
                                                        Loop While Not c.Mood.HasFlag(Mood)
                                                        Return c
                                                    End Function)
    End Function

    ''' <summary>
    ''' Gera uma <see cref="HSVColor"/>  aleatoria com transparencia
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    Public Shared Function RandomTransparentColor(Optional Name As String = Nothing) As HSVColor
        Return New HSVColor(ColorConvert.RandomColor(), Name) With {.Opacity = Generate.RandomNumber(0, 100)}
    End Function

    ''' <summary>
    ''' Instancia uma nova <see cref="HSVColor"/> transparente
    ''' </summary>
    Sub New()
        Me.New(Color.Transparent)
    End Sub

    Sub New(Image As Image)
        Me.New(Image.GetMostUsedColors(Image.Width * Image.Height).FirstOrDefault())
    End Sub

    ''' <summary>
    ''' Instancia uma nova <see cref="HSVColor"/> a partir de uma <see cref="System.Drawing.Color"/>
    ''' </summary>
    ''' <param name="Color">Cor do sistema</param>
    Sub New(Color As Color)
        FromColor(Color)
    End Sub

    ''' <summary>
    ''' Instancia uma nova <see cref="HSVColor"/> a partir de uma string de cor (colorname, hexadecimal ou string aleatoria) e um Nome
    ''' </summary>
    ''' <param name="Color">Cor</param>
    Sub New(Color As String)
        Me.New(Color.ToColor())
        _name = Color
    End Sub

    ''' <summary>
    ''' Instancia uma nova HSVColor a partir de uma string de cor (colorname, hexadecimal ou  string aleatoria) e um Nome
    ''' </summary>
    ''' <param name="Color">Cor</param>
    ''' <param name="Name">Nome da cor</param>
    Sub New(Color As String, Name As String)
        Me.New(Color.ToColor())
        _name = Name.IfBlank(Color)
    End Sub

    ''' <summary>
    ''' Instancia uma nova HSVColor a partir de uma <see cref="System.Drawing.Color"/> e um Nome
    ''' </summary>
    ''' <param name="Color">Cor</param>
    ''' <param name="Name">Nome da cor</param>
    Sub New(Color As Color, Name As String)
        Me.New(Color)
        _name = Name
    End Sub

    ''' <summary>
    ''' Retorna ou seta o valor ARGB de 32 bits dessa cor
    ''' </summary>
    ''' <returns></returns>
    Public Property ARGB As Integer
        Get
            Return _scolor.ToArgb()
        End Get
        Set(value As Integer)
            _scolor = Color.FromArgb(value)
            FromColor(_scolor)
        End Set
    End Property

    ''' <summary>
    ''' Hue (Matiz)
    ''' </summary>
    ''' <returns></returns>
    Property Hue As Double
        Get
            Return _h
        End Get
        Set(value As Double)
            If _h <> value Then
                _h = value

                While _h < 0
                    _h += 360
                End While

                While _h > 360
                    _h -= 360
                End While

                SetColor()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Saturation (Saturação)
    ''' </summary>
    ''' <returns></returns>
    Property Saturation As Double
        Get
            Return _s
        End Get
        Set(value As Double)
            value = value.LimitRange(0.0, 1.0)
            If _s <> value Then
                _s = value
                SetColor()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Luminância
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Luminance As Double
        Get
            Return (0.2126 * Red) + (0.7152 * Green) + (0.0722 * Blue)
        End Get
    End Property

    ''' <summary>
    ''' Brilho
    ''' </summary>
    ''' <returns></returns>
    Property Brightness As Double
        Get
            Return _v
        End Get
        Set(value As Double)
            value = value.LimitRange(0.0, 1.0)
            If _v <> value Then
                _v = value
                SetColor()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Red (Vermelho)
    ''' </summary>
    ''' <returns></returns>
    Property Red As Integer
        Get
            Return _scolor.R
        End Get
        Set(value As Integer)
            _scolor = Color.FromArgb(Alpha, value.LimitRange(Of Integer)(0, 255), Green, Blue)
            FromColor(_scolor)
        End Set
    End Property

    ''' <summary>
    ''' Green (Verde)
    ''' </summary>
    ''' <returns></returns>
    Property Green As Integer
        Get
            Return _scolor.G
        End Get
        Set(value As Integer)
            _scolor = Color.FromArgb(Alpha, Red, value.LimitRange(Of Integer)(0, 255), Blue)
            FromColor(_scolor)
        End Set
    End Property

    ''' <summary>
    ''' Blue (Azul)
    ''' </summary>
    ''' <returns></returns>
    Property Blue As Integer
        Get
            Return _scolor.B
        End Get
        Set(value As Integer)
            _scolor = Color.FromArgb(Alpha, Red, Green, value.LimitRange(Of Integer)(0, 255))
            FromColor(_scolor)
        End Set
    End Property

    ''' <summary>
    ''' Alpha (Transparencia)
    ''' </summary>
    ''' <returns></returns>
    Property Alpha As Byte
        Get
            Return _scolor.A
        End Get
        Set(value As Byte)
            _scolor = Color.FromArgb(value.LimitRange(Of Byte)(0, 255), Red, Green, Blue)
            FromColor(_scolor)
        End Set
    End Property

    ''' <summary>
    ''' Opacidade (de 1 a 100%)
    ''' </summary>
    ''' <returns></returns>
    Property Opacity As Decimal
        Get
            Return Alpha.ToDecimal().CalculatePercent(255)
        End Get
        Set(value As Decimal)
            Alpha = Decimal.ToByte(CalculateValueFromPercent(value.LimitRange(0, 100), 255).LimitRange(0, 255))
        End Set
    End Property

    ''' <summary>
    ''' Valor hexadecimal desta cor
    ''' </summary>
    ''' <returns></returns>
    Public Property Hexadecimal As String
        Get
            Return _scolor.ToHexadecimal()
        End Get
        Set(value As String)
            If value.IsHexaDecimalColor Then
                _scolor = value.ToColor()
                FromColor(_scolor)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Valor RGBA() desta cor
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CSS As String
        Get
            If Me.Alpha = 255 Then Return _scolor.ToCssRGB() Else Return _scolor.ToCssRGBA()
        End Get
    End Property

    ''' <summary>
    ''' Mood da cor
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Mood As ColorMood
        Get

            Dim m As ColorMood

            If IsDark() Then
                m = ColorMood.Dark
            ElseIf IsLight() Then
                m = ColorMood.Light
            Else
                m = ColorMood.Medium
            End If

            If IsMediumDark() Then
                m = m Or ColorMood.MediumDark
            Else
                m = m Or ColorMood.MediumLight
            End If

            If IsWarm() Then
                m = m Or ColorMood.Warm
            End If

            If IsCool() Then
                m = m Or ColorMood.Cool
            End If

            If IsSad() Then
                m = m Or ColorMood.Sad
            Else
                m = m Or ColorMood.Happy
            End If

            If Opacity < 15 Then
                m = m Or ColorMood.Unvisible
            ElseIf Opacity < 60 Then
                m = m Or ColorMood.SemiVisible
            Else
                m = m Or ColorMood.Visible
            End If

            Return m
        End Get
    End Property

    Public Function CreateSolidImage(Width As Integer, Height As Integer) As Bitmap
        Return New Bitmap(_scolor.CreateSolidImage(Width, Height))
    End Function

    Public Function CreateSolidImage(Optional Size As String = "") As Bitmap
        Dim s = Size.IfBlank("100").ToSize()
        Return CreateSolidImage(s.Width, s.Height)
    End Function

    Public ReadOnly Property ImageSample As Bitmap
        Get
            Return CreateSolidImage().DrawString(Name)
        End Get
    End Property

    ''' <summary>
    ''' Nome atribuido a esta cor
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String
        Get
            Return _name.IfBlank(ClosestColorName)
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

    ''' <summary>
    ''' Nome original mais proximo desta cor
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ClosestColorName As String
        Get
            Return _scolor.GetClosestColorName()
        End Get
    End Property

    ''' <summary>
    ''' Descricao desta cor
    ''' </summary>
    ''' <returns></returns>
    Public Property Description As String

    Private Sub SetColor()

        Dim H, S, V As Double
        Dim alpha = _scolor.A

        H = Me.Hue
        S = Me.Saturation
        V = Me.Brightness

        H = H / 360
        Dim MAX As Byte = 255

        If S > 0 Then
            If H >= 1 Then H = 0
            H = 6 * H
            Dim hueFloor As Integer = CInt(Math.Floor(H))
            Dim a As Byte = CByte(Math.Round(MAX * V * (1.0 - S)))
            Dim b As Byte = CByte(Math.Round(MAX * V * (1.0 - (S * (H - hueFloor)))))
            Dim c As Byte = CByte(Math.Round(MAX * V * (1.0 - (S * (1.0 - (H - hueFloor))))))
            Dim d As Byte = CByte(Math.Round(MAX * V))

            Select Case hueFloor
                Case 0
                    _scolor = Color.FromArgb(alpha, d, c, a)
                Case 1
                    _scolor = Color.FromArgb(alpha, b, d, a)
                Case 2
                    _scolor = Color.FromArgb(alpha, a, d, c)
                Case 3
                    _scolor = Color.FromArgb(alpha, a, b, d)
                Case 4
                    _scolor = Color.FromArgb(alpha, c, a, d)
                Case 5
                    _scolor = Color.FromArgb(alpha, d, a, b)
                Case Else
                    _scolor = Color.FromArgb(0, 0, 0, 0)
            End Select
        Else
            Dim d As Byte = CByte((V * MAX))
            _scolor = Color.FromArgb(alpha, d, d, d)
        End If

        _s = S.LimitRange(0R, 1.0R)
        _v = V.LimitRange(0R, 1.0R)

    End Sub

    Private Sub FromColor(Color As Color)
        _scolor = Color
        Me._name = _scolor.Name

        Dim r As Double = Color.R / 255
        Dim g As Double = Color.G / 255
        Dim b As Double = Color.B / 255

        Dim min As Double = Math.Min(Math.Min(r, g), b)
        Dim max As Double = Math.Max(Math.Max(r, g), b)
        _v = max
        Dim delta = max - min
        If max = 0 OrElse delta = 0 Then
            _s = 0
            _h = 0
        Else
            _s = delta / max
            If (r = max) Then
                'entre amarelo e magenta
                _h = (g - b) / delta
            ElseIf (g = max) Then
                'Entre ciano e amarelo
                _h = 2 + (b - r) / delta
            Else
                'entre magenta e ciano
                _h = 4 + (r - g) / delta
            End If

            _h *= 60
            If _h < 0 Then
                _h += 360
            End If
        End If
    End Sub

    ''' <summary>
    ''' Retorna uma <see cref="System.Drawing.Color"/> desta <see cref="HSVColor"/>
    ''' </summary>
    ''' <returns></returns>
    Public Function ToSystemColor() As Color
        Return Color.FromArgb(Alpha, Red, Green, Blue)
    End Function

    ''' <summary>
    ''' Verifica se uma cor é legivel sobre outra cor
    ''' </summary>
    ''' <param name="BackgroundColor"></param>
    ''' <param name="Size"></param>
    ''' <returns></returns>
    Public Function IsReadable(BackgroundColor As HSVColor, Optional Size As Integer = 10) As Boolean
        Return Me._scolor.IsReadable(BackgroundColor._scolor, Size)
    End Function

    ''' <summary>
    ''' Retorna uma cor mais clara a partir desta cor
    ''' </summary>
    ''' <param name="Percent"></param>
    ''' <returns></returns>
    Public Function MakeLighter(Optional Percent As Single = 50) As HSVColor
        Return New HSVColor(_scolor.MakeLighter(Percent))
    End Function

    ''' <summary>
    ''' Retorna uma cor mais escura a partir desta cor
    ''' </summary>
    ''' <param name="Percent"></param>
    ''' <returns></returns>
    Public Function MakeDarker(Optional Percent As Single = 50) As HSVColor
        Return New HSVColor(_scolor.MakeDarker(Percent))
    End Function

    ''' <summary>
    ''' Verifica se uma cor e considerada clara
    ''' </summary>
    ''' <returns></returns>
    Public Function IsLight() As Boolean
        Return Luminance.IsGreaterThan(160.0R)
    End Function

    ''' <summary>
    ''' Verifica se uma cor e considerada escura
    ''' </summary>
    ''' <returns></returns>
    Public Function IsDark() As Boolean
        Return Luminance.IsLessThan(70.0R)
    End Function

    ''' <summary>
    ''' Verifica se uma cor e considerada Medio Clara
    ''' </summary>
    ''' <returns></returns>
    Public Function IsMediumLight() As Boolean
        Return Luminance > 255 / 2
    End Function

    ''' <summary>
    ''' Verifica se uma cor e considerada Medio Escura
    ''' </summary>
    ''' <returns></returns>
    Public Function IsMediumDark() As Boolean
        Return Not IsMediumLight()
    End Function

    ''' <summary>
    ''' Verifica se uma cor e considerada média
    ''' </summary>
    ''' <returns></returns>
    Public Function IsMedium() As Boolean
        Return Luminance.IsBetweenOrEqual(70.0R, 160.0R)
    End Function

    Public Function IsWarm() As Boolean
        Return Hue.IsLessThan(90.0R) OrElse Hue.IsGreaterThan(270.0R)
    End Function

    Public Function IsCool() As Boolean
        Return Not IsWarm()
    End Function

    Public Function IsSad() As Boolean
        Return Saturation.IsLessThan(0.5) OrElse Brightness.IsLessThan(0.75)
    End Function

    Public Function IssHappy() As Boolean
        Return Not IsSad()
    End Function

    ''' <summary>
    ''' Retorna uma cópia desta cor
    ''' </summary>
    ''' <returns></returns>
    Public Function Clone() As HSVColor
        Return New HSVColor(_scolor, Me.Name) With {.Description = Me.Description}
    End Function

    ''' <summary>
    ''' Retorna a combinação de 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Combine(Color As HSVColor) As HSVColor
        If Color IsNot Nothing Then
            Return New HSVColor() With {.Red = Me.Red Xor Color.Red, .Green = Me.Green Xor Color.Green, .Blue = Me.Blue Xor Color.Blue, .Alpha = Me.Alpha}
        End If
        Return Me.Clone()
    End Function

    ''' <summary>
    ''' Retorna a distancia entre 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Distance(Color As HSVColor) As Double
        Return Math.Sqrt(3 * (Color.Red - Me.Red) * (Color.Red - Me.Red) + 4 * (Color.Green - Me.Green) * (Color.Green - Me.Green) + 2 * (Color.Blue - Me.Blue) * (Color.Blue - Me.Blue))
    End Function

    ''' <summary>
    ''' Retorna uma nova cor a partir da mistura multiplicativa de 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Multiply(Color As HSVColor) As HSVColor
        Dim n = Me.Clone()
        If Color IsNot Nothing Then
            n.Red = (Me.Red / 255 * Color.Red).LimitRange(0, 255)
            n.Green = (Me.Green / 255 * Color.Green).LimitRange(0, 255)
            n.Blue = (Me.Blue / 255 * Color.Blue).LimitRange(0, 255)
        End If
        Return n
    End Function

    ''' <summary>
    ''' Retorna uma nova cor a partir da mistura subtrativa de 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Subtractive(Color As HSVColor) As HSVColor
        Dim n = Me.Clone()
        If Color IsNot Nothing Then
            n.Red = (n.Red + (Color.Red - 255)).LimitRange(0, 255)
            n.Green = (n.Green + (Color.Green - 255)).LimitRange(0, 255)
            n.Blue = (n.Blue + (Color.Blue - 255)).LimitRange(0, 255)
        End If
        Return n
    End Function

    ''' <summary>
    ''' Retorna uma nova cor a partir da mistura aditiva de 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Addictive(Color As HSVColor) As HSVColor
        Dim n = Me.Clone()
        If Color IsNot Nothing Then
            n.Red = (n.Red + Color.Red).LimitRange(0, 255)
            n.Green = (n.Green + Color.Green).LimitRange(0, 255)
            n.Blue = (n.Blue + Color.Blue).LimitRange(0, 255)
        End If
        Return n
    End Function

    ''' <summary>
    ''' Retorna uma nova cor a partir da diferença de 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Difference(Color As HSVColor) As HSVColor
        Dim n = Me.Clone()
        If Color IsNot Nothing Then
            n.Red = (n.Red - Color.Red).LimitRange(0, 255)
            n.Green = (n.Green - Color.Green).LimitRange(0, 255)
            n.Blue = (n.Blue - Color.Blue).LimitRange(0, 255)
        End If
        Return n
    End Function

    ''' <summary>
    ''' Retorna a cor media entre 2 cores
    ''' </summary>
    ''' <param name="Color"></param>
    ''' <returns></returns>
    Public Function Average(Color As HSVColor) As HSVColor
        If Color IsNot Nothing Then
            Return New HSVColor() With {.Red = {Me.Red, Color.Red}.Average(), .Green = {Me.Green, Color.Green}.Average(), .Blue = {Me.Blue, Color.Blue}.Average(), .Alpha = Me.Alpha}
        End If
        Return Me.Clone()
    End Function

    ''' <summary>
    ''' Extrai os tons marrons de uma cor (filtro sépia)
    ''' </summary>
    ''' <returns></returns>
    Public Function Sepia() As HSVColor
        Dim c = Me.Clone()
        c.Red = Math.Round(Red * 0.393 + Green * 0.769 + Blue * 0.189)
        c.Green = Math.Round(Red * 0.349 + Green * 0.686 + Blue * 0.168)
        c.Blue = Math.Round(Red * 0.272 + Green * 0.534 + Blue * 0.131)
        Return c
    End Function

    ''' <summary>
    ''' Extrai a cor negativa desta cor
    ''' </summary>
    ''' <returns></returns>
    Public Function Negative() As HSVColor
        Return New HSVColor(_scolor.GetNegativeColor())
    End Function

    ''' <summary>
    ''' Extrai o cinza desta cor
    ''' </summary>
    ''' <returns></returns>
    Public Function Grey() As HSVColor
        Dim v = 0.35 + 13 * (Red + Green + Blue) / 60
        Return New HSVColor(Drawing.Color.FromArgb(v, v, v))
    End Function

    ''' <summary>
    ''' Cria uma paleta de cores usando esta cor como base e um metodo especifico
    ''' </summary>
    ''' <param name="PalleteType"></param>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function CreatePallete(PalleteType As String, Optional Amount As Integer = 4) As HSVColor()
        Dim rl = New List(Of HSVColor)
        For Each item In Me.Monochromatic(Amount)
            Dim c = CType(item.GetType().GetMethod(PalleteType).Invoke(item, {False}), HSVColor())
            rl.AddRange(c)
        Next
        Return rl.ToArray()
    End Function

    ''' <summary>
    ''' Retorna  novas HSVColor a partir da cor atual, movendo ela N graus na roda de cores
    ''' </summary>
    ''' <param name="excludeMe">Inclui esta cor no array</param>
    ''' <param name="Degrees">Lista contendo os graus que serão movidos na roda de cores.</param>
    ''' <returns></returns>
    Public Function ModColor(ExcludeMe As Boolean, ParamArray Degrees As Integer()) As HSVColor()
        If Not ExcludeMe Then
            Return {Me}.ToArray().Union(ModColor(If(Degrees, {})).ToArray()).ToArray()
        End If
        Return ModColor(If(Degrees, {})).ToArray()
    End Function

    ''' <summary>
    ''' Retorna  novas HSVColor a partir da cor atual, movendo ela N graus na roda de cores
    ''' </summary>
    ''' <param name="Degrees">Lista contendo os graus que serão movidos na roda de cores.</param>
    ''' <returns></returns>
    Public Function ModColor(ParamArray Degrees As Integer()) As HSVColor()
        Return If(Degrees, {}).Select(Function(x) New HSVColor() With {.Hue = ((Me.Hue + x) Mod 360), .Saturation = Me.Saturation, .Brightness = Me.Brightness}).OrderBy(Function(x) x.Hue).ToArray()
    End Function

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

    ''' <summary>
    ''' Retorna as cores Quadraadas (tetradicas) desta cor
    ''' </summary>
    ''' <param name="ExcludeMe"></param>
    ''' <returns></returns>
    Public Function Tetradic(Optional ExcludeMe As Boolean = False) As HSVColor()
        Return Square(ExcludeMe)
    End Function

    ''' <summary>
    ''' Retorna as cores análogas desta cor
    ''' </summary>
    ''' <param name="ExcludeMe"></param>
    ''' <returns></returns>
    Public Function Analogous(Optional ExcludeMe As Boolean = False) As HSVColor()
        Return ModColor(ExcludeMe, 45, -45)
    End Function

    ''' <summary>
    ''' Retorna as cores Quadraadas (tetradicas) desta cor
    ''' </summary>
    ''' <param name="ExcludeMe"></param>
    ''' <returns></returns>
    Public Function Square(Optional ExcludeMe As Boolean = False) As HSVColor()
        Return ModColor(ExcludeMe, 90, 180, 260)
    End Function

    ''' <summary>
    ''' Retorna as cores triadicas desta cor
    ''' </summary>
    ''' <param name="ExcludeMe"></param>
    ''' <returns></returns>
    Public Function Triadic(Optional ExcludeMe As Boolean = False) As HSVColor()
        Return ModColor(ExcludeMe, 120, -120)
    End Function

    ''' <summary>
    ''' Retorna as cores complementares desta cor
    ''' </summary>
    ''' <param name="ExcludeMe"></param>
    ''' <returns></returns>
    Public Function Complementary(Optional ExcludeMe As Boolean = False) As HSVColor()
        Return ModColor(ExcludeMe, 180)
    End Function

    ''' <summary>
    '''  Retorna as cores split-complementares desta cor
    ''' </summary>
    ''' <param name="IncludeMe"></param>
    ''' <returns></returns>
    Public Function SplitComplementary(Optional IncludeMe As Boolean = False) As HSVColor()
        Return ModColor(IncludeMe, 150, 210)
    End Function

    ''' <summary>
    ''' Retorna <paramref name="Amount"/> variacoes cores a partir da cor atual
    ''' </summary>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function Monochromatic(Optional Amount As Decimal = 4) As HSVColor()
        Return MonochromaticPallete(_scolor, Amount).Select(Function(x) New HSVColor(x)).ToArray()
    End Function

    ''' <summary>
    ''' Retorna uma paleta de cores tetradica (Monochromatica + Tetradica)
    ''' </summary>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function TetradicPallete(Optional Amount As Integer = 3) As HSVColor()
        Return Me.Monochromatic(Amount).SelectMany(Function(item) item.Tetradic()).ToArray()
    End Function

    ''' <summary>
    ''' Retorna uma paleta de cores triadica (Monochromatica + Triadica)
    ''' </summary>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function TriadicPallete(Optional Amount As Integer = 3) As HSVColor()
        Return Me.Monochromatic(Amount).SelectMany(Function(item) item.Triadic()).ToArray()

    End Function

    ''' <summary>
    ''' Retorna uma paleta de cores complementares (complementares + monocromatica)
    ''' </summary>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function ComplementaryPallete(Optional Amount As Integer = 3) As HSVColor()
        Return Me.Monochromatic(Amount).SelectMany(Function(item) item.Complementary()).ToArray()
    End Function

    ''' <summary>
    ''' Retorna uma paleta de cores split-complementares (split-complementares + monocromatica)
    ''' </summary>
    ''' <param name="Amount"></param>
    ''' <returns></returns>
    Public Function SplitComplementaryPallete(Optional Amount As Integer = 3) As HSVColor()
        Return Me.Monochromatic(Amount).SelectMany(Function(item) item.SplitComplementary()).ToArray()
    End Function

    Public Function CompareTo(other As Integer) As Integer Implements IComparable(Of Integer).CompareTo
        Return Me.ARGB.CompareTo(other)
    End Function

    Public Function CompareTo(other As HSVColor) As Integer Implements IComparable(Of HSVColor).CompareTo
        Return Me.ARGB.CompareTo(other.ARGB)
    End Function

    Public Function CompareTo(other As Color) As Integer Implements IComparable(Of Color).CompareTo
        Return Me.ARGB.CompareTo(other.ToArgb())
    End Function

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Return Me.ToString().CompareTo(obj?.ToString())
    End Function

    Public Shared Operator +(Color1 As HSVColor, Color2 As HSVColor) As HSVColor
        Return Color1.Combine(Color2)
    End Operator

    Public Shared Operator +(Color1 As Color, Color2 As HSVColor) As HSVColor
        Return New HSVColor(Color1).Combine(Color2)
    End Operator

    Public Shared Operator +(Color1 As HSVColor, Color2 As Color) As HSVColor
        Return New HSVColor(Color2).Combine(Color1)
    End Operator

    Public Shared Operator Mod(Color As HSVColor, Degrees As Integer) As HSVColor
        Return Color.ModColor(True, Degrees).FirstOrDefault
    End Operator

    Public Shared Operator >(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) > 0
    End Operator

    Public Shared Operator <(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) < 0
    End Operator

    Public Shared Operator >=(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) >= 0
    End Operator

    Public Shared Operator <=(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) <= 0
    End Operator

    Public Shared Operator =(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) = 0
    End Operator

    Public Shared Operator <>(Color1 As HSVColor, Color2 As HSVColor) As Boolean
        Return Color1.CompareTo(Color2) <> 0
    End Operator

    Public Shared Operator -(Color1 As HSVColor, Color2 As HSVColor) As HSVColor
        Return Color1.Difference(Color2)
    End Operator

    Public Shared Operator -(Color1 As Color, Color2 As HSVColor) As HSVColor
        Return New HSVColor(Color1).Difference(Color2)
    End Operator

    Public Shared Operator -(Color1 As HSVColor, Color2 As Color) As HSVColor
        Return New HSVColor(Color2).Difference(Color1)
    End Operator

    Public Shared Operator *(Color1 As HSVColor, Color2 As HSVColor) As HSVColor
        Return Color1.Multiply(Color2)
    End Operator

    Public Shared Operator *(Color1 As Color, Color2 As HSVColor) As HSVColor
        Return New HSVColor(Color1).Multiply(Color2)
    End Operator

    Public Shared Operator *(Color1 As HSVColor, Color2 As Color) As HSVColor
        Return New HSVColor(Color2).Multiply(Color1)
    End Operator

    Public Shared Widening Operator CType(Color As HSVColor) As Integer
        Return Color.ARGB
    End Operator

    Public Shared Widening Operator CType(Value As Integer) As HSVColor
        Return New HSVColor(Drawing.Color.FromArgb(Value))
    End Operator

    Public Shared Widening Operator CType(Value As Drawing.Color) As HSVColor
        Return New HSVColor(Value)
    End Operator

    Public Shared Widening Operator CType(Value As HSVColor) As Drawing.Color
        Return Value.ToSystemColor
    End Operator

    Public Shared Widening Operator CType(Value As String) As HSVColor
        Return New HSVColor(Value)
    End Operator

    Public Shared Widening Operator CType(Value As HSVColor) As String
        Return Value.Hexadecimal
    End Operator

End Class

<Flags>
Public Enum ColorMood

    Dark = 1
    MediumDark = 2
    Medium = 4
    MediumLight = 8
    Light = 16

    Sad = 32
    Neutral = 64
    Happy = 128

    Cool = 256
    Warm = 512

    Unvisible = 1024
    SemiVisible = 2048
    Visible = 4096
End Enum