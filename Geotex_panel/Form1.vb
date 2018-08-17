Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel
Imports System.Data.OleDb
Imports System.Windows.Forms.DataVisualization.Charting

Public Class Form1
    Dim comOpen As String
    Dim readbuffer As String
    Dim port As Array
    Dim time As String
    Dim p_length As Integer
    Dim tempChart As New Series
    Dim provider As String
    Dim datafile As String
    Dim connstring As String
    Dim cnn As New OleDbConnection
    Dim cmd As New OleDbCommand
    Dim userMsg As String
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        userMsg = Microsoft.VisualBasic.InputBox("Please Enter Password", "GeoTex Password Manager", "Enter your password here", 500, 300)
        If userMsg = "geotech7" Then

        Else
            MessageBox.Show("Wrong Password")
            End
        End If
        port = IO.Ports.SerialPort.GetPortNames()
        ComboBox1.Items.AddRange(port)
        p_length = port.Length
        Timer1.Enabled = True
        Chart1.Series.Clear()
        Chart1.Titles.Add("Variation of RPM")
        tempChart.Name = "RPM"
        tempChart.ChartType = SeriesChartType.Line
        Chart1.Series.Add(tempChart)
    End Sub

    Private Sub SerialPort1_DataReceived(ByVal sender As System.Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        If comOpen Then
            Try
                readbuffer = SerialPort1.ReadLine
                Me.Invoke(New EventHandler(AddressOf updateTemp))
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        SerialPort1.PortName = ComboBox1.Text
        SerialPort1.BaudRate = CInt(ComboBox2.Text)

        Try
            SerialPort1.Open()
            comOpen = SerialPort1.IsOpen
        Catch ex As Exception
            comOpen = False
            MsgBox(ex.Message)

        End Try
        Button1.Enabled = False
        Button2.Enabled = True
        ComboBox1.Enabled = False
        ComboBox2.Enabled = False

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If comOpen Then
            SerialPort1.DiscardInBuffer()
            SerialPort1.Close()
            comOpen = False
            Button1.Enabled = True
            Button2.Enabled = False
            ComboBox1.Enabled = True
            ComboBox2.Enabled = True

        End If
        ComboBox1.Text = ""
        ComboBox2.Text = ""
        ListBox1.ClearSelected()
        ListBox2.ClearSelected()
    End Sub

    Public Sub updateTemp(ByVal sender As Object, ByVal e As System.EventArgs)
        'Update temperature display as it comes in
        Dim read As String
        read = readbuffer.Replace(vbCr, "").Replace(vbLf, "")
        TextBox2.Text = read
        Dim cnn As New OleDbConnection
        Dim cmd As New OleDbCommand
        provider = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
        datafile = "D:\mydata1.mdb"
        connstring = provider & datafile
        cnn.ConnectionString = connstring
        Try
            cnn.Open()
            cmd.Connection = cnn
            cmd.CommandText = "Insert into Table1([time], [rpm]) values(@time, @rpm)"
            cmd.Parameters.AddWithValue("@time", time)
            cmd.Parameters.AddWithValue("@Password", TextBox2.Text)
            cmd.ExecuteNonQuery()
            cnn.Close()
            'MsgBox("Data added")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        tempChart.Points.AddXY(time, read)
        ListBox1.Items.Insert(0, read)
        Dim h As Double
        Dim t As Double
        h = Val(read)
        t = (((h / 77.2254) ^ 4) + 1) ^ 0.5
        TextBox4.Text = Decimal.Round(t, 3, MidpointRounding.AwayFromZero)
        ListBox2.Items.Insert(0, TextBox4.Text)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ComboBox1.Items.Clear()
        port = IO.Ports.SerialPort.GetPortNames()
        ComboBox1.Items.AddRange(port)
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        port = IO.Ports.SerialPort.GetPortNames()
        time = TimeOfDay
        If p_length <> port.Length Then
            ComboBox1.Items.Clear()
            ComboBox1.Items.AddRange(port)
            p_length = port.Length
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Timer1.Enabled = False
        SerialPort1.DiscardInBuffer()
        SerialPort1.Close()
        comOpen = False
        ListBox1.ClearSelected()
        ListBox2.ClearSelected()
        End
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Dim x As Integer
        Dim y As Integer
        x = TextBox1.Text
        TextBox3.Text = (77.2254 * (Math.Abs((x * x) - 1) ^ 0.25))
        y = 0.4051 + (0.03658 * (TextBox3.Text)) + (0.003401 * ((TextBox3.Text) ^ 2))
        If (x = 0) Then
            TextBox3.Text = 0
        ElseIf (x = 2) Then
            y = 57
        ElseIf (x = 3) Then
            y = 72
        End If
        If TextBox1.Text > 0 Then
            Dim b() As Byte = BitConverter.GetBytes(y)
            SerialPort1.Write(b, 0, 1)
        Else
            Dim b() As Byte = BitConverter.GetBytes(0)
            SerialPort1.Write(b, 0, 1)
        End If
    End Sub
End Class
