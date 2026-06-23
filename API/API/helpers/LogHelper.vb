Imports System.IO
Imports System.Configuration
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Enum LogLevel
    Info = 1
    Warning = 2
    [Error] = 3
End Enum

Public Class LogHelper
    Private Shared ReadOnly _logPath As String =
        If(ConfigurationManager.AppSettings("LogPath"), "C:\Logs\testapi\")

    Private Shared ReadOnly _minLevel As LogLevel =
        [Enum].Parse(GetType(LogLevel),
            If(ConfigurationManager.AppSettings("LogLevel"), "Info"))

    ' Tên file theo từng mức — tương đương winston transports
    Private Shared ReadOnly _fileError As String = "error.log"
    Private Shared ReadOnly _fileCombined As String = "combined.log"

    ' ─── Public API — tương đương logger.info / logger.warn / logger.error ───

    Public Shared Sub Info(message As String,
                           Optional data As Object = Nothing)
        WriteLog(LogLevel.Info, message, data)
    End Sub

    Public Shared Sub Warning(message As String,
                              Optional data As Object = Nothing)
        WriteLog(LogLevel.Warning, message, data)
    End Sub

    Public Shared Sub [Error](message As String,
                              Optional ex As Exception = Nothing,
                              Optional data As Object = Nothing)
        WriteLog(LogLevel.Error, message, data, ex)
    End Sub

    ' ─── Mask field nhạy cảm ─────────────────────────────────────

    Public Shared Function MaskSensitiveFields(body As String) As Object
        If String.IsNullOrEmpty(body) Then Return Nothing
        Try
            Dim json = JObject.Parse(body)
            For Each field In {"password", "token", "secret",
                               "cardNumber", "cvv"}
                If json(field) IsNot Nothing Then
                    json(field) = "***"
                End If
            Next
            Return json
        Catch
            Return body
        End Try
    End Function


    Private Shared Sub WriteLog(level As LogLevel,
                                message As String,
                                Optional data As Object = Nothing,
                                Optional ex As Exception = Nothing)
        If level < _minLevel Then Return

        Try
            If Not Directory.Exists(_logPath) Then
                Directory.CreateDirectory(_logPath)
            End If

            Dim entry As New JObject()
            entry("timestamp") = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            entry("level") = level.ToString().ToLower()
            entry("message") = message

            If data IsNot Nothing Then
                entry("data") = JToken.FromObject(data)
            End If

            If ex IsNot Nothing Then
                entry("error") = New JObject From {
                    {"type", ex.GetType().Name},
                    {"message", ex.Message},
                    {"stackTrace", ex.StackTrace}
                }
            End If

            Dim logLine = entry.ToString(Formatting.None) & Environment.NewLine

            SyncLock GetType(LogHelper)
                Dim combinedFile = Path.Combine(_logPath,
                    DateTime.Now.ToString("yyyy-MM-dd") & "_" & _fileCombined)
                File.AppendAllText(combinedFile, logLine, System.Text.Encoding.UTF8)

                If level = LogLevel.Error Then
                    Dim errorFile = Path.Combine(_logPath,
                        DateTime.Now.ToString("yyyy-MM-dd") & "_" & _fileError)
                    File.AppendAllText(errorFile, logLine, System.Text.Encoding.UTF8)
                End If
                System.Diagnostics.Debug.WriteLine(logLine)
            End SyncLock

        Catch
            ' Không throw — tránh vòng lặp vô tận
        End Try
    End Sub
End Class