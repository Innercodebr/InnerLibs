﻿Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

''' <summary>
''' Módulo para criação de arquivos baseados em Array de Bytes()
''' </summary>
''' <remarks></remarks>
Public Module Files

    ''' <summary>
    ''' Salva um anexo para um diretório
    ''' </summary>
    ''' <param name="attachment"></param>
    ''' <param name="Directory"></param>
    ''' <returns></returns>
    <Extension()> Public Function SaveMailAttachment(ByVal attachment As System.Net.Mail.Attachment, Directory As DirectoryInfo) As FileInfo
        Directory = Directory.FullName.ToDirectoryInfo
        Return SaveMailAttachment(attachment, Directory.FullName & "\" & attachment.Name.IfBlank(attachment.ContentId))
    End Function

    ''' <summary>
    ''' Salva um anexo para um caminho
    ''' </summary>
    ''' <param name="attachment"></param>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    <Extension()> Public Function SaveMailAttachment(ByVal attachment As System.Net.Mail.Attachment, Path As String) As FileInfo
        Path.ToDirectoryInfo
        If Path.IsDirectoryPath Then
            Path = Path & "\" & attachment.Name.IfBlank(attachment.ContentId)
        End If
        Dim writer As BinaryWriter = New BinaryWriter(New FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        writer.Write(attachment.ToBytes)
        writer.Close()
        Return New FileInfo(Path)
    End Function

    ''' <summary>
    ''' Salva um anexo para Byte()
    ''' </summary>
    ''' <param name="attachment"></param>
    ''' <returns></returns>
    <Extension()> Public Function ToBytes(ByVal attachment As System.Net.Mail.Attachment) As Byte()
        Return attachment.ContentStream.ToBytes
    End Function

    ''' <summary>
    ''' Converte um stream em Bytes
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ToBytes(ByVal stream As Stream) As Byte()
        Using ms As New MemoryStream()
            stream.CopyTo(ms)
            Return ms.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' Transforma um  Array de Bytes em um arquivo
    ''' </summary>
    ''' <param name="Bytes">A MAtriz com os Bytes  a ser escrita</param>
    ''' <param name="FilePath">Caminho onde o arquivo será gravado</param>
    ''' <returns>Um Fileinfo contendo as informações do arquivo criado</returns>

    <Extension()>
    Public Function WriteToFile(Bytes As Byte(), FilePath As String) As FileInfo
        If FilePath.IsFilePath Then
            Dim p = New FileInfo(FilePath)
            p.Directory.FullName.ToDirectoryInfo()
            File.WriteAllBytes(p.FullName, Bytes)
            Debug.WriteLine(FilePath, "File Written")
            Return p
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Transforma um  Array de Bytes em um arquivo
    ''' </summary>
    ''' <param name="Bytes">A MAtriz com os Bytes  a ser escrita</param>
    ''' <param name="FilePath">Caminho onde o arquivo será gravado</param>
    ''' <returns>Um Fileinfo contendo as informações do arquivo criado</returns>
    <Extension()> Public Function WriteToFile(Bytes As Byte(), FilePath As String, DateTime As DateTime) As FileInfo
        FilePath = FilePath.Replace($"#timestamp#", DateTime.Ticks.ToString)
        FilePath = FilePath.Replace($"#datedir#", $"{DateTime.Year}\{DateTime.Month}\{DateTime.Day}")
        For Each item As String In {"d", "dd", "ddd", "dddd", "hh", "HH", "m", "mm", "M", "MM", "MMM", "MMMM", "s", "ss", "t", "tt", "Y", "YY", "YYY", "YYYY", "f", "ff", "fff", "ffff", "fffff", "ffffff", "fffffff"}
            FilePath = FilePath.SensitiveReplace($"#{item}#", DateTime.ToString(item))
        Next
        Return WriteToFile(Bytes, FilePath).With(Function(x) x.LastWriteTime = DateTime)
    End Function



    ''' <summary>
    ''' Transforma um arquivo em um Array de Bytes
    ''' </summary>
    ''' <param name="File">O arquivo a ser convertido</param>
    ''' <returns>Um array do tipo Byte()</returns>

    <Extension()>
    Public Function ToBytes(File As FileInfo) As Byte()
        Dim fInfo As New FileInfo(File.FullName)
        Using fStream As New FileStream(File.FullName, FileMode.Open, FileAccess.Read)
            Using br As New BinaryReader(fStream)
                Return br.ReadBytes(CInt(fInfo.Length))
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Salva um texto em um arquivo
    ''' </summary>
    ''' <param name="Text">TExto</param>
    ''' <param name="FilePath">Caminho do arquivo</param>
    ''' <returns>Um Fileinfo contendo as informações do arquivo criado</returns>
    <Extension()>
    Public Function WriteToFile(Text As String, FilePath As String, Optional Append As Boolean = False, Optional Enconding As Encoding = Nothing) As FileInfo
        Path.GetDirectoryName(FilePath).ToDirectoryInfo()
        Using s As New StreamWriter(FilePath, Append, If(Enconding, New UTF8Encoding(False)))
            s.Write(Text)
            s.Close()
        End Using
        Return New FileInfo(FilePath)
    End Function

    ''' <summary>
    ''' Salva um texto em um arquivo
    ''' </summary>
    ''' <param name="Text">TExto</param>
    ''' <param name="File">Arquivo</param>
    ''' <returns>Um Fileinfo contendo as informações do arquivo criado</returns>
    <Extension()>
    Public Function WriteToFile(Text As String, File As FileInfo, Optional Append As Boolean = False, Optional Enconding As Encoding = Nothing) As FileInfo
        Return Text.WriteToFile(File.FullName, Append, Enconding)
    End Function

    ''' <summary>
    ''' Retorna o nome do diretorio onde o arquivo se encontra
    ''' </summary>
    ''' <param name="Path">Caminho do arquivo</param>
    ''' <returns>o nome do diretório sem o caminho</returns>
    <Extension()> Public Function GetLatestDirectoryName(Path As FileInfo) As String
        Return IO.Path.GetDirectoryName(Path.DirectoryName)
    End Function

End Module