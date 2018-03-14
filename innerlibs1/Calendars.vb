﻿Imports System.Collections.Specialized
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Windows.Forms
Imports InnerLibs.TimeMachine


''' <summary>
''' Classe que representa um periodo entre 2 datas
''' </summary>
Public Class DateRange

    Public Property StartDate As Date
    Public Property EndDate As Date

    ''' <summary>
    ''' Instancia um novo periodo entre 2 datas
    ''' </summary>
    ''' <param name="StartDate"></param>
    ''' <param name="EndDate"></param>
    Sub New(StartDate As Date, EndDate As Date)
        FixDateOrder(StartDate, EndDate)
        Me.StartDate = StartDate
        Me.EndDate = EndDate
    End Sub

    Function Difference() As TimeFlow
        Return StartDate.GetDifference(EndDate)
    End Function

    Public Function CreateFortnightGroup() As FortnightGroup
        Return FortnightGroup.CreateFromDateRange(Me.StartDate, Me.EndDate)
    End Function

    Public Overrides Function ToString() As String
        Return Difference.ToString
    End Function

    ''' <summary>
    ''' Cria uma lista de datas contendo todas as datas entre os periodos
    ''' </summary>
    ''' <param name="Interval">Intervalo que deverá ser incrementado</param>
    ''' <returns></returns>
    Public Function ToList(Optional Interval As DateInterval = DateInterval.Day) As List(Of Date)
        FixDateOrder(StartDate, EndDate)
        Dim curdate = StartDate
        Dim l As New List(Of Date)
        Do
            l.Add(curdate.Date)
            curdate = DateAdd(Interval, 1, curdate)
        Loop While curdate <= EndDate
        Return l
    End Function

    ''' <summary>
    ''' Verifica se 2 periodos possuem interseção de datas
    ''' </summary>
    ''' <param name="Period">Periodo</param>
    ''' <returns></returns>
    Public Function Overlaps(Period As DateRange) As Boolean
        Select Case True
            Case Period.StartDate <= Me.EndDate And Period.StartDate >= Me.StartDate
                Return True
            Case Me.StartDate <= Period.EndDate And Me.StartDate >= Period.StartDate
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Verifica se 2 periodos coincidem datas (interseção, esta dentro de um periodo de ou contém um periodo)
    ''' </summary>
    ''' <param name="Period"></param>
    ''' <returns></returns>
    ''' 
    Public Function MatchAny(Period As DateRange) As Boolean
        Return Me.Overlaps(Period) Or Me.Contains(Period) Or Me.IsIn(Period)
    End Function

    ''' <summary>
    ''' Verifica se este periodo contém um outro periodo
    ''' </summary>
    ''' <param name="Period"></param>
    ''' <returns></returns>
    Public Function Contains(Period As DateRange) As Boolean
        Return Me.StartDate <= Period.StartDate And Period.EndDate <= Me.EndDate
    End Function

    ''' <summary>
    ''' Verifica se este periodo está dentro de outro periodo
    ''' </summary>
    ''' <param name="Period"></param>
    ''' <returns></returns>
    Public Function IsIn(Period As DateRange) As Boolean
        Return Period.Contains(Me)
    End Function

End Class


''' <summary>
''' Modulo para manipulação de calendário
''' </summary>
''' <remarks></remarks>
Public Module Calendars

    ''' <summary>
    ''' Veirifica se existe intersecção entre dois periodos
    ''' </summary>
    ''' <param name="StartDate1">Data inicial do primeiro periodo</param>
    ''' <param name="EndDate1">Data Final do primeiro periodo</param>
    ''' <param name="StartDate2">Data inicial do segundo periodo</param>
    ''' <param name="EndDate2">Data Final do primeiro periodo</param>
    ''' <returns></returns>
    Public Function IsOverlap(StartDate1 As DateTime, EndDate1 As DateTime, StartDate2 As DateTime, EndDate2 As DateTime) As Boolean
        FixDateOrder(StartDate1, EndDate1)
        FixDateOrder(StartDate2, EndDate2)
        Return (StartDate1 <= EndDate2) And (EndDate1 >= StartDate2)
    End Function

    ''' <summary>
    ''' Retorna um Array de <see cref="DateTime"/> contendo todas as datas entre 2 datas
    ''' </summary>
    ''' <param name="StartDate">Data Inicial</param>
    ''' <param name="EndDate">Data Final</param>
    ''' <param name="Increment">Valor a ser incrementado (padrão 1 dia)</param>
    ''' <returns></returns>
    Public Function DateRange(ByVal StartDate As Date, ByVal EndDate As Date, Optional Increment As TimeSpan = Nothing) As Date()
        FixDateOrder(StartDate, EndDate)
        Dim l As New List(Of Date)
        If IsNothing(Increment) OrElse Increment = TimeSpan.Zero Then Increment = New TimeSpan(1, 0, 0, 0)
        l.Add(StartDate)
        While StartDate < EndDate
            l.Add(l.Last().Add(Increment))
        End While
        l.Add(EndDate)
        Return l.ToArray
    End Function

    ''' <summary>
    ''' Retorna a ultima data do mes a partir de uma outra data
    ''' </summary>
    ''' <param name="[Date]">Data</param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetLastDayOfMonth([Date] As DateTime) As DateTime
        Return New DateTime([Date].Year, [Date].Month, DateTime.DaysInMonth([Date].Year, [Date].Month), [Date].Hour, [Date].Minute, [Date].Second, [Date].Millisecond, [Date].Kind)
    End Function

    ''' <summary>
    ''' Retorna a primeira data do mes a partir de uma outra data
    ''' </summary>
    ''' <param name="[Date]">Data</param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetFirstDayOfMonth([Date] As DateTime) As DateTime
        Return New DateTime([Date].Year, [Date].Month, 1, [Date].Hour, [Date].Minute, [Date].Second, [Date].Millisecond, [Date].Kind)
    End Function

    ''' <summary>
    ''' Verifica se uma data é do mesmo mês e ano que outra data
    ''' </summary>
    ''' <param name="[Date]">Primeira data</param>
    ''' <param name="AnotherDate">Segunda data</param>
    ''' <returns></returns>
    <Extension()>
    Public Function IsSameMonth([Date] As DateTime, AnotherDate As DateTime) As Boolean
        Return [Date].IsBetween(AnotherDate.GetFirstDayOfMonth, AnotherDate.GetLastDayOfMonth)
    End Function
    ''' <summary>
    ''' Verifica se a Data de hoje é um aniversário
    ''' </summary>
    ''' <param name="BirthDate">  Data de nascimento</param>
    ''' <param name="UseTomorrow">Verifica se o aniversario é Amanha</param>
    ''' <returns></returns>
    <Extension>
    Public Function IsAnniversary(BirthDate As Date, Optional CompareWith As Date? = Nothing) As Boolean
        If Not CompareWith.HasValue Then CompareWith = Today
        Return (BirthDate.Day & "/" & BirthDate.Month) = (CompareWith.Value.Day & "/" & CompareWith.Value.Month)
    End Function

    ''' <summary>
    ''' COnverte um datetime para o formato de string do SQL server ou Mysql
    ''' </summary>
    ''' <param name="[Date]">Data</param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToSQLDateString([Date] As DateTime) As String
        Return [Date].Year & "-" & [Date].Month & "-" & [Date].Day & " " & [Date].Hour & ":" & [Date].Minute & ":" & [Date].Second & "." & [Date].Millisecond
    End Function

    ''' <summary>
    ''' Converte uma string dd/mm/aaaa hh:mm:ss.llll para o formato de string do SQL server ou Mysql
    ''' </summary>
    ''' <param name="[Date]">Data</param>
    ''' <returns></returns>
    <Extension>
    Function ToSQLDateString([Date] As String, Optional FromCulture As String = "pt-BR") As String
        Return Convert.ToDateTime([Date], New CultureInfo(FromCulture, False).DateTimeFormat).ToSQLDateString
    End Function

    ''' <summary>
    ''' Retorna uma <see cref="TimeFlow"/> com a diferença entre 2 Datas
    ''' </summary>
    ''' <param name="InitialDate"></param>
    ''' <param name="SecondDate"> </param>
    ''' <returns></returns>
    <Extension>
    Public Function GetDifference(InitialDate As DateTime, SecondDate As DateTime) As TimeFlow
        FixDateOrder(InitialDate, SecondDate)
        Return New TimeFlow(InitialDate, SecondDate)
    End Function

    ''' <summary>
    ''' Troca ou não a ordem das variaveis de inicio e fim de um periodo fazendo com que a StartDate
    ''' sempre seja uma data menor que a EndDate, prevenindo que o calculo entre 2 datas resulte em um
    ''' <see cref="TimeSpan"/> negativo
    ''' </summary>
    ''' <param name="StartDate">Data Inicial</param>
    ''' <param name="EndDate">  Data Final</param>
    Public Sub FixDateOrder(ByRef StartDate As DateTime, ByRef EndDate As DateTime)
        If StartDate > EndDate Then
            Dim temp = StartDate
            StartDate = EndDate
            EndDate = temp
        End If
    End Sub

    ''' <summary>
    ''' Verifica se uma data se encontra entre 2 datas
    ''' </summary>
    ''' <param name="MidDate">   Data</param>
    ''' <param name="StartDate"> Data Inicial</param>
    ''' <param name="EndDate">   Data final</param>
    ''' <param name="IgnoreTime">Indica se o tempo deve ser ignorado na comparação</param>
    ''' <returns></returns>
    <Extension()> Public Function IsBetween(MidDate As DateTime, StartDate As DateTime, EndDate As DateTime, Optional IgnoreTime As Boolean = False) As Boolean
        FixDateOrder(StartDate, EndDate)
        If IgnoreTime Then
            Return StartDate.Date <= MidDate.Date And MidDate.Date <= EndDate.Date
        Else
            Return StartDate <= MidDate And MidDate <= EndDate
        End If
    End Function

    ''' <summary>
    ''' Retorna uma lista com as datas de dias especificos da semana entre 2 datas
    ''' </summary>
    ''' <param name="StartDate">Data inicial</param>
    ''' <param name="EndDate">  data Final</param>
    ''' <param name="Days">     Dias da semana</param>
    ''' <returns></returns>
    <Extension()> Public Function GetBetween(StartDate As DateTime, EndDate As DateTime, ParamArray Days() As DayOfWeek) As List(Of Date)
        Dim dt As New DateRange(StartDate, EndDate)
        If Days IsNot Nothing AndAlso Days.Count > 0 Then
            Return dt.ToList.Where(Function(x) x.DayOfWeek.IsIn(Days)).Select(Function(x) x.Date).ToList
        Else
            Return dt.ToList.Select(Function(x) x.Date).ToList
        End If
    End Function

    ''' <summary>
    ''' Remove o tempo de todas as datas de uma lista e retorna uma nova lista
    ''' </summary>
    ''' <param name="List">Lista que será alterada</param>
    <Extension()> Function ClearTime(List As List(Of Date)) As List(Of Date)
        Return List.Select(Function(x) x.Date).ToList
    End Function

    ''' <summary>
    ''' Retorna uma String no formato "W dias, X horas, Y minutos e Z segundos"
    ''' </summary>
    ''' <param name="TimeElapsed">TimeSpan com o intervalo</param>
    ''' <returns>string</returns>
    <Extension()>
    Public Function ToTimeElapsedString(TimeElapsed As TimeSpan, Optional DayWord As String = "dia", Optional HourWord As String = "hora", Optional MinuteWord As String = "minuto", Optional SecondWord As String = "segundo") As String
        Dim dia As String = (If(TimeElapsed.Days > 0, (If(TimeElapsed.Days = 1, TimeElapsed.Days & " " & DayWord & " ", TimeElapsed.Days & " " & DayWord & "s ")), ""))
        Dim horas As String = (If(TimeElapsed.Hours > 0, (If(TimeElapsed.Hours = 1, TimeElapsed.Hours & " " & HourWord & " ", TimeElapsed.Hours & " " & HourWord & "s ")), ""))
        Dim minutos As String = (If(TimeElapsed.Minutes > 0, (If(TimeElapsed.Minutes = 1, TimeElapsed.Minutes & " " & MinuteWord & " ", TimeElapsed.Minutes & " " & MinuteWord & "s ")), ""))
        Dim segundos As String = (If(TimeElapsed.Seconds > 0, (If(TimeElapsed.Seconds = 1, TimeElapsed.Seconds & " " & SecondWord & " ", TimeElapsed.Seconds & " " & SecondWord & "s ")), ""))

        dia.AppendIf(",", dia.IsNotBlank And (horas.IsNotBlank Or minutos.IsNotBlank Or segundos.IsNotBlank))
        horas.AppendIf(",", horas.IsNotBlank And (minutos.IsNotBlank Or segundos.IsNotBlank))
        minutos.AppendIf(",", minutos.IsNotBlank And (segundos.IsNotBlank))
        Dim current As String = dia & " " & horas & " " & minutos & " " & segundos
        If current.Contains(",") Then
            current = current.Insert(current.LastIndexOf(","), " e ")
            current = current.Remove(current.LastIndexOf(","), 1)
        End If
        Return current.AdjustWhiteSpaces
    End Function

    ''' <summary>
    ''' Retorna uma String baseado no numero do Mês Ex.: 1 -&gt; Janeiro
    ''' </summary>
    ''' <param name="MonthNumber">Numero do Mês</param>
    ''' <returns>String com nome do Mês</returns>

    <Extension()>
    Function ToLongMonthName(MonthNumber As Integer) As String
        Return New Date(Now.Year, MonthNumber, 1).TolongMonthName
    End Function

    ''' <summary>
    ''' Retorna uma String com o nome do mes baseado na data
    ''' </summary>
    ''' <param name="DateTime">Data</param>
    ''' <returns>String com nome do Mês</returns>
    <Extension()>
    Function TolongMonthName(DateTime As Date) As String
        Return DateTime.ToString("MMMM")
    End Function

    ''' <summary>
    ''' Retorna uma String curta baseado no numero do Mês Ex.: 1 -&gt; Jan
    ''' </summary>
    ''' <param name="MonthNumber">Numero do Mês</param>
    ''' <returns>String com nome curto do Mês</returns>

    <Extension()>
    Public Function ToShortMonthName(MonthNumber As Integer) As String
        Return ToLongMonthName(MonthNumber).GetFirstChars(3)
    End Function

    ''' <summary>
    ''' Retorna uma String baseado no numero do Dia da Semana Ex.: 2 -&gt; Segunda-Feira
    ''' </summary>
    ''' <param name="DayNumber">Numero do Dia</param>
    ''' <returns>String com nome do Dia</returns>

    <Extension()>
    Function ToLongDayOfWeekName(DayNumber As Integer) As String
        Return DateTimeFormatInfo.CurrentInfo.GetDayName(DayNumber)
    End Function

    ''' <summary>
    ''' Retorna uma String baseado no numero do Dia da Semana Ex.: 2 -&gt; Seg
    ''' </summary>
    ''' <param name="DayNumber">Numero do Dia</param>
    ''' <returns>String com nome curto do Dia</returns>

    <Extension()>
    Public Function ToShortDayOfWeekName(DayNumber As Integer) As String
        Return ToLongDayOfWeekName(DayNumber).GetFirstChars(3)
    End Function

    ''' <summary>
    ''' Retorna a data de amanhã
    ''' </summary>
    ''' <returns>Data de amanhã</returns>

    Public ReadOnly Property Tomorrow() As DateTime = DateTime.Now.AddDays(1)

    ''' <summary>
    ''' Retorna a data de ontem
    ''' </summary>
    ''' <returns>Data de ontem</returns>

    Public ReadOnly Property Yesterday() As DateTime = DateTime.Now.AddDays(-1)

    ''' <summary>
    ''' Verifica se o dia se encontra no fim de semana
    ''' </summary>
    ''' <param name="YourDate">Uma data qualquer</param>
    ''' <returns>TRUE se for sabado ou domingo, caso contrario FALSE</returns>

    <Extension()>
    Public Function IsWeekend(YourDate As DateTime) As Boolean
        Return (YourDate.DayOfWeek = DayOfWeek.Sunday Or YourDate.DayOfWeek = DayOfWeek.Saturday)
    End Function

    <Extension()>
    Private Function ToGreetingFarewell(Time As DateTime, Optional Language As String = "pt", Optional Farewell As Boolean = False) As String

        Dim bomdia As String = "Bom dia"
        Dim boatarde As String = "Boa tarde"
        Dim boanoite As String = "Boa noite"
        Dim boanoite_despedida As String = boanoite
        Dim seDespedidaManha As String = "tenha um ótimo dia"
        Dim seDespedidaTarde As String = "tenha uma ótima tarde"

        Select Case Language.ToLower()
            Case "en", "eng", "ingles", "english", "inglés"
                bomdia = "Good morning"
                boatarde = "Good afternoon"
                boanoite = "Good evening"
                boanoite_despedida = "Good night"
                seDespedidaManha = "have a nice day"
                seDespedidaTarde = "have a great afternoon"
                Exit Select
            Case "es", "esp", "espanhol", "espanol", "español", "spanish"
                bomdia = "Buenos días"
                boatarde = "Buenas tardes"
                boanoite = "Buenas noches"
                boanoite_despedida = boanoite
                seDespedidaManha = "que tengas un buen día"
                seDespedidaTarde = "que tengas una buena tarde"
                Exit Select
        End Select

        If Time.Hour < 12 Then
            Return If(Farewell, seDespedidaManha, bomdia)
        ElseIf Time.Hour >= 12 AndAlso Time.Hour < 18 Then
            Return If(Farewell, seDespedidaTarde, boatarde)
        Else
            Return If(Farewell, boanoite_despedida, boanoite)
        End If
    End Function

    ''' <summary>
    ''' Transforma um DateTime em uma despedida (Bom dia, Boa tarde, Boa noite)
    ''' </summary>
    ''' <param name="Time">    Horario</param>
    ''' <param name="Language">Idioma da saudação (pt, en, es)</param>
    ''' <returns>Uma string com a despedida</returns>
    <Extension>
    Public Function ToFarewell(Time As DateTime, Optional Language As String = "pt") As String
        Return Time.ToGreetingFarewell(Language, True)
    End Function

    ''' <summary>
    ''' Transforma um DateTime em uma saudação (Bom dia, Boa tarde, Boa noite)
    ''' </summary>
    ''' <param name="Time">    Horario</param>
    ''' <param name="Language">Idioma da saudação (pt, en, es)</param>
    ''' <returns>Uma string com a despedida</returns>
    <Extension>
    Public Function ToGreeting(Time As DateTime, Optional Language As String = "pt") As String
        Return Time.ToGreetingFarewell(Language, False)
    End Function

    ''' <summary>
    ''' Retorna uma saudação
    ''' </summary>
    ''' <param name="Language">Idioma da saudação (pt, en, es)</param>
    ''' <returns>Uma string com a saudação</returns>
    Public ReadOnly Property Greeting(Optional Language As String = "pt") As String
        Get
            Return Now.ToGreetingFarewell(Language)
        End Get
    End Property

    ''' <summary>
    ''' Retorna uma despedida
    ''' </summary>
    ''' <param name="Language">Idioma da despedida (pt, en, es)</param>
    ''' <returns>Uma string com a despedida</returns>
    Public ReadOnly Property Farewell(Optional Language As String = "pt") As String
        Get
            Return Now.ToGreetingFarewell(Language, True)
        End Get
    End Property

    ''' <summary>
    ''' Returna uma lista dupla com os meses
    ''' </summary>
    ''' <param name="ValueType">Apresentação dos meses no valor</param>
    '''<param name="TextType">Apresentação dos meses no texto</param>

    Public ReadOnly Property Months(Optional TextType As TypeOfFill = TypeOfFill.LongName, Optional ValueType As TypeOfFill = TypeOfFill.Number) As List(Of KeyValuePair(Of String, String))
        Get
            Months = New List(Of KeyValuePair(Of String, String))
            For i = 1 To 12
                Dim key As String = ""
                Dim value As String = ""
                Select Case TextType
                    Case TypeOfFill.LongName
                        key = ToLongMonthName(i)
                    Case TypeOfFill.ShortName
                        key = ToShortMonthName(i)
                    Case Else
                        key = i
                End Select
                Select Case ValueType
                    Case TypeOfFill.LongName
                        value = ToLongMonthName(i)
                    Case TypeOfFill.ShortName
                        value = ToShortMonthName(i)
                    Case Else
                        value = i
                End Select
                Months.Add(New KeyValuePair(Of String, String)(key, value))
            Next
            Return Months
        End Get
    End Property

    ''' <summary>
    ''' Returna uma lista dupla com os meses
    ''' </summary>
    ''' <param name="ValueType">Apresentação dos meses no valor</param>
    '''<param name="TextType">Apresentação dos meses no texto</param>

    Public ReadOnly Property WeekDays(Optional TextType As TypeOfFill = TypeOfFill.LongName, Optional ValueType As TypeOfFill = TypeOfFill.Number) As List(Of KeyValuePair(Of String, String))
        Get
            WeekDays = New List(Of KeyValuePair(Of String, String))
            For i = 1 To 7
                Dim key As String = ""
                Dim value As String = ""
                Select Case TextType
                    Case TypeOfFill.LongName
                        key = ToLongDayOfWeekName(i)
                    Case TypeOfFill.ShortName
                        key = ToShortDayOfWeekName(i)
                    Case Else
                        key = i
                End Select
                Select Case ValueType
                    Case TypeOfFill.LongName
                        value = ToLongDayOfWeekName(i)
                    Case TypeOfFill.ShortName
                        value = ToShortDayOfWeekName(i)
                    Case Else
                        value = i
                End Select
                WeekDays.Add(New KeyValuePair(Of String, String)(key, value))
            Next
            Return WeekDays
        End Get
    End Property

    ''' <summary>
    ''' Preenche um HtmlSelect com MESES ou DIAS DA SEMANA
    ''' </summary>
    ''' <param name="Box">Select HTML</param>
    ''' <param name="ValueType">Apresentação dos meses no valor</param>
    '''<param name="TextType">Apresentação dos meses no texto</param>
    <Extension> Public Sub FillWith(Box As UI.HtmlControls.HtmlSelect, CalendarType As CalendarType, Optional TextType As TypeOfFill = TypeOfFill.LongName, Optional ValueType As TypeOfFill = TypeOfFill.Number)
        For Each item In If(CalendarType = CalendarType.Months, Months(TextType, ValueType), WeekDays(TextType, ValueType))
            Box.Items.Add(New ListItem(item.Key, item.Value))
        Next
    End Sub

    ''' <summary>
    ''' Tipo de Apresentação dos Meses/Dias da Semana/Estado
    ''' </summary>

    Enum TypeOfFill

        ''' <summary>
        ''' Numerico
        ''' </summary>
        Number = 2

        ''' <summary>
        ''' Abreviado
        ''' </summary>

        ShortName = 1
        ''' <summary>
        ''' Completo
        ''' </summary>

        LongName = 0
    End Enum

    ''' <summary>
    ''' Elemento do calendário
    ''' </summary>
    Enum CalendarType
        Weekdays
        Months
    End Enum

    ''' <summary>
    ''' Calcula a porcentagem de diferenca entre duas datas de acordo com a data inicial especificada
    ''' </summary>
    ''' <param name="MidDate">  Data do meio a ser verificada (normalmente Now)</param>
    ''' <param name="StartDate">Data Inicial</param>
    ''' <param name="EndDate">  Data Final</param>
    ''' <returns></returns>
    <Extension()>
    Function CalculatePercent(MidDate As DateTime, StartDate As DateTime, EndDate As DateTime) As Decimal
        FixDateOrder(StartDate, EndDate)
        If MidDate < StartDate Then Return 0
        If MidDate > EndDate Then Return 100
        Return (MidDate - StartDate).Ticks * 100 / (EndDate - StartDate).Ticks
    End Function

End Module