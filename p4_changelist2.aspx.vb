Partial Public Class p4_changelist2
    Inherits System.Web.UI.Page

#Region " Page Events "

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        bt_submit.AccessKey = "g"
        bt_clear.AccessKey = "c"
        txt_p4cl.Attributes.Add("onfocus", "this.select();")
        txt_output.Attributes.Add("onfocus", "this.select();")
        bt_submit.Attributes.Add("onclick", "return uf_validate();")

        'Dim id = WindowsIdentity.GetCurrent()
        'Response.Write("<b>Windows Identity Check</b><br>")
        'Response.Write("Name: " + id.Name + "<br>")
    End Sub

    Private Sub bt_submit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles bt_submit.Click
        Dim ls_output, ls_changeNo As String
        Dim ls_path As String

        'get changelist number
        ls_changeNo = Me.txt_p4cl.Text
        ls_path = ConfigurationManager.AppSettings.Item("gk_p4batch_path")

        'get p4 description
        uf_p4Describe(ls_changeNo, ls_path)
        ls_output = uf_parseFileOutput(ls_changeNo, ls_path)

        're-execute parsing if not connected to perforce server
        If ls_output = "" Then
            uf_p4Connect(ls_path)
            uf_p4Describe(ls_changeNo, ls_path)
            ls_output = uf_parseFileOutput(ls_changeNo, ls_path)
        End If


        'display output
        txt_output.Text = Trim(ls_output)
        txt_output.Focus()
    End Sub

    Private Sub bt_clear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles bt_clear.Click
        Response.Redirect("p4_changelist2.aspx")
    End Sub

#End Region

#Region " Utilities "

    Private Sub uf_p4Connect(ByVal as_path As String)
        'FILEPATH
        Dim strFilePath As String = as_path & "p4_login.bat"

        'START CMD
        Dim psi As New System.Diagnostics.ProcessStartInfo("cmd.exe")
        psi.UseShellExecute = False
        psi.RedirectStandardInput = True
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        psi.WorkingDirectory = as_path

        'DECLARE PROCESS
        Dim proc As System.Diagnostics.Process
        proc = System.Diagnostics.Process.Start(psi)

        'PATH OPENED AS TEXT BY STREAM READER
        Dim strm As System.IO.StreamReader
        strm = System.IO.File.OpenText(strFilePath)

        'OUTPUT 
        Dim sout As System.IO.StreamReader
        sout = proc.StandardOutput

        'INPUT
        Dim sin As System.IO.StreamWriter
        sin = proc.StandardInput

        'COMMAND TO CMD
        While (strm.Peek() <> -1)
            sin.WriteLine(strm.ReadLine())
        End While

        strm.Close()
        sin.WriteLine("EXIT")
        proc.Close()

        'READ THE EXECUTED BATCH FILE
        Dim results As String
        results = sout.ReadToEnd.Trim

        'CLOSE IN/OUT
        sin.Close()
        sout.Close()
    End Sub

    Private Sub uf_execBatchFile(ByVal as_path As String)
        'FILEPATH
        Dim strFilePath As String = as_path & "p4_dscrb.bat"

        'START CMD
        Dim psi As New System.Diagnostics.ProcessStartInfo("cmd.exe")
        psi.UseShellExecute = False
        psi.RedirectStandardInput = True
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        psi.WorkingDirectory = as_path

        'DECLARE PROCESS
        Dim proc As System.Diagnostics.Process
        proc = System.Diagnostics.Process.Start(psi)

        'PATH OPENED AS TEXT BY STREAM READER
        Dim strm As System.IO.StreamReader
        strm = System.IO.File.OpenText(strFilePath)

        'OUTPUT 
        Dim sout As System.IO.StreamReader
        sout = proc.StandardOutput

        'INPUT
        Dim sin As System.IO.StreamWriter
        sin = proc.StandardInput

        'COMMAND TO CMD
        While (strm.Peek() <> -1)
            sin.WriteLine(strm.ReadLine())
        End While

        strm.Close()
        'Dim stEchoFmt As String
        'stEchoFmt = "# {0} run successfully. Exiting"
        'sin.WriteLine(String.Format(stEchoFmt, strFilePath))
        sin.WriteLine("EXIT")
        proc.Close()

        'READ THE EXECUTED BATCH FILE
        Dim results As String
        results = sout.ReadToEnd.Trim

        'CLOSE IN/OUT
        sin.Close()
        sout.Close()

        'WORKS DONE JUST FORMAT THE CHARACTERS TO DISPLAY
        'Dim fmtStdOut As String
        'fmtStdOut = "<font face=courier size=0>{0}</font>"
        'Response.Write(String.Format(fmtStdOut, results.Replace(System.Environment.NewLine, "<br>")))
    End Sub

    Private Sub uf_p4Describe(ByVal as_p4clno As String, ByVal as_path As String)
        Dim FileWriter As IO.StreamWriter

        'Update batch file command content
        '---------------------------------------------------------------------------
        FileWriter = IO.File.CreateText(as_path & "p4_dscrb.bat")
        FileWriter.WriteLine("p4 describe -s " & as_p4clno & " > ./file_lists.txt")

        FileWriter.Flush()
        FileWriter.Close()
        '---------------------------------------------------------------------------        


        'Execute Batch file
        '---------------------------------------------------------------------------
        uf_execBatchFile(as_path)
        '---------------------------------------------------------------------------               
    End Sub

    Private Function uf_parseFileOutput(ByVal as_p4clno As String, ByVal as_path As String) As String
        Dim ls_output As String = ""
        Dim lo_reader As IO.StreamReader
        Dim lb_fileEmpty As Boolean = True

        Dim ls_line, ls_fileName, ls_changeType As String
        Dim li_startPos, li_endPos As Integer
        Dim ls_project As String = ""

        Dim ldt_updatedList As DataTable
        Dim ldt_addedList As DataTable
        Dim ldt_deletedList As DataTable

        ldt_updatedList = uf_tableInit()
        ldt_addedList = uf_tableInit()
        ldt_deletedList = uf_tableInit()


        lo_reader = New IO.StreamReader(as_path & "file_lists.txt")
        While True
            ls_line = lo_reader.ReadLine()

            If ls_line Is Nothing Then
                Exit While

            Else
                lb_fileEmpty = False

                If ls_line.Trim <> "" AndAlso Left(ls_line, 6) = "... //" Then
                    'get project name
                    ls_project = Split(ls_line, "/")(3)

                    li_startPos = ls_line.LastIndexOf("/") + 1
                    li_endPos = ls_line.LastIndexOf("#")
                    ls_fileName = ls_line.Substring(li_startPos, li_endPos - li_startPos)  'holds the current filename

                    'determine the change type of the file
                    li_startPos = ls_line.LastIndexOf(" ") + 1
                    ls_changeType = Trim(ls_line.Substring(li_startPos)).ToUpper  'holds the current change type

                    If ls_changeType = "EDIT" Or ls_changeType = "INTEGRATE" Or ls_changeType = "BRANCH" Then
                        uf_insertData(ldt_updatedList, ls_fileName)
                    ElseIf ls_changeType = "ADD" Then
                        uf_insertData(ldt_addedList, ls_fileName)
                    ElseIf ls_changeType = "DELETE" Then
                        uf_insertData(ldt_deletedList, ls_fileName)
                    End If
                End If
            End If
        End While
        lo_reader.Close()


        'consolidate output
        If ldt_addedList.Rows.Count > 0 Then
            ls_output &= "Created Objects:" & vbCrLf
            ls_output &= "--------------------" & vbCrLf
            ls_output &= uf_displayLists(ldt_addedList)
        End If

        If ldt_updatedList.Rows.Count > 0 Then
            If ls_output <> "" Then
                ls_output &= vbCrLf
            End If

            ls_output &= "Modified Objects:" & vbCrLf
            ls_output &= "--------------------" & vbCrLf
            ls_output &= uf_displayLists(ldt_updatedList)
        End If

        If ldt_deletedList.Rows.Count > 0 Then
            If ls_output <> "" Then
                ls_output &= vbCrLf
            End If

            ls_output &= "Removed Objects:" & vbCrLf
            ls_output &= "--------------------" & vbCrLf
            ls_output &= uf_displayLists(ldt_deletedList)
        End If


        ls_output &= vbCrLf
        ls_output &= "Integration in Dev Complete" & vbCrLf
        ls_output &= "-------------------------------" & vbCrLf

        If ls_project.ToUpper = "APECS" Then
            ls_output &= " > 2003 DEV CL #: " & as_p4clno & vbCrLf
            ls_output &= " > 2005 DEV CL #: xxxxx" & vbCrLf & vbCrLf
        Else 'For Xpt
            ls_output &= "Changelist #: " & as_p4clno & vbCrLf & vbCrLf
        End If
        
        ls_output &= "-- Awaiting for deployment and testing. --"

        'checks if connected to perforce
        If lb_fileEmpty Then
            ls_output = ""
        End If

        Return ls_output
    End Function

    Private Function uf_tableInit() As DataTable
        Dim ldt_output As New DataTable
        ldt_output.Columns.Add("P4_FILE", GetType(String))
        Return ldt_output
    End Function

    Private Sub uf_insertData(ByRef adt_source As DataTable, ByVal ls_newValue As String)
        Dim ldr_insert As DataRow

        ldr_insert = adt_source.NewRow
        ldr_insert.Item("P4_FILE") = ls_newValue
        adt_source.Rows.Add(ldr_insert)
    End Sub

    Private Function uf_displayLists(ByVal adt_source As DataTable) As String
        Dim ls_files As String = ""
        Dim ldr_row As DataRow

        For Each ldr_row In adt_source.Rows
            ls_files &= " - " & CStr(ldr_row.Item("P4_FILE")) & vbCrLf
        Next

        Return ls_files
    End Function

#End Region

End Class
