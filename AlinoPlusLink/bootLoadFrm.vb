Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel
Public Class bootLoadFrm

    'CRC16CCITT related
    Public Enum InitialCRCValue
        Zeroes = 0
        NonZero1 = &HFFFF
        NonZero2 = &H1D0F
        'NonZero3 = &H0
    End Enum

    Private Const poly As UShort = &H1021 'polynomial
    Dim table(255) As UShort
    Dim intValue As UShort = 0

    Enum flasherStat
        F_INIT
        F_START
        F_ERASE
        F_PROGRAM
        F_WAIT
        F_CHECKSUM
        F_ERROR
    End Enum
    Dim bootState As flasherStat
    Dim toggleFlag As Boolean = 0
    Dim srecSNo As Integer = 0
    Dim rxBuf As Byte()
    Dim txBuf As Byte()
    Dim commandRx As UInt16
    Dim totSrec As Integer
    Dim fileCrc As Byte()

    Dim hexContent As String
    Dim lines As List(Of String)
    Dim line As String
    Dim crcLines As List(Of String)
    Dim crcLine As String
    Dim cumulativeFlash As String
    Dim length As Integer
    Dim upperBound As Integer
    Dim bytes As Byte()
    Dim startByte As Integer
    Dim appSize As Integer
    Dim checkSumBytes As Byte()
    Dim checkSum As UInt64
    Dim hexBytes As Byte()

    Dim testTimer As Integer = 0
    Dim rxDelayTimer As Integer
    Dim txValid As Boolean
    Dim invStatus As Boolean

    Dim flashingTimeInSeconds As Integer

    Dim count As Integer
    Dim chunkNo, maxChunk As Integer

    Private Delegate Sub UpdateFormDelegate()
    Private UpdateFormDelegate1 As UpdateFormDelegate


    Private Sub bootLoadFrm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ToolStripButton5.Image = ImageList1.Images(0)
        RichTextBox1.AppendText("Alino Plus - Bootloader" + vbCrLf)
        ToolStripStatusLabel2.Spring = True
        For Each sp As String In My.Computer.Ports.SerialPortNames
            ToolStripComboBox1.Items.Add(sp)
        Next

        If (ToolStripComboBox1.Items.Count > 0) Then
            ToolStripComboBox1.SelectedIndex = 0
        End If

        RichTextBox1.ScrollToCaret()
        ToolStripComboBox2.SelectedIndex = 3
        bootState = flasherStat.F_INIT
        ToolStripLabel3.Text = "INIT"
        ToolStripButton5.Image = ImageList1.Images(1)
        Timer1.Stop()
        Timer2.Stop()
        TextBox3.Clear()

        initTable()
    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        closeSerial()
        Me.Close()
    End Sub

    Private Function closeSerial() As Integer
        Try
            If (SerialPort1.IsOpen) Then
                SerialPort1.Close()
            End If
            Return 1
        Catch ex As Exception
            MsgBox("Serial Port Error !" + ex.ToString)
            Return 0
        End Try
    End Function

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        'Closing Port for reconfiguring
        If (SerialPort1.IsOpen) Then
            SerialPort1.Close()
        End If

        If (SerialPort1.IsOpen = False) Then
            'MsgBox("test OK  " + Parity.None.ToString)
            SerialPort1.PortName = ToolStripComboBox1.Text
            SerialPort1.BaudRate = ToolStripComboBox2.Text
            SerialPort1.Parity = Parity.None
            SerialPort1.StopBits = StopBits.One
            SerialPort1.DataBits = 8

            Try
                SerialPort1.Open()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        Else
            RichTextBox1.AppendText("Port Is already Open" + vbCrLf)
            MsgBox("Port Is already Open")
        End If

        If (SerialPort1.IsOpen) Then
            RichTextBox1.AppendText("Connected to " + SerialPort1.PortName.ToString + " " + SerialPort1.BaudRate.ToString + " " + SerialPort1.DataBits.ToString + " " + SerialPort1.Parity.ToString + " " + SerialPort1.StopBits.ToString + vbCrLf)
            ToolStripStatusLabel1.Text = SerialPort1.PortName.ToString + " " + SerialPort1.BaudRate.ToString + " " + SerialPort1.DataBits.ToString + " " + SerialPort1.Parity.ToString + " " + SerialPort1.StopBits.ToString
        Else
            RichTextBox1.AppendText("Could Not Connected" + vbCrLf)
            ToolStripStatusLabel1.Text = "Not Connected"
        End If
    End Sub

    Private Sub ToolStripComboBox1_Click(sender As Object, e As EventArgs) Handles ToolStripComboBox1.Click

    End Sub

    Private Sub ToolStripButton4_Click(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        ToolStripComboBox1.Items.Clear()
        For Each port In SerialPort.GetPortNames
            ToolStripComboBox1.Items.Add(port)
        Next

        If (ToolStripComboBox1.Items.Count > 0) Then
            ToolStripComboBox1.SelectedIndex = 0
        End If

    End Sub

    Private Function bootInit() As Integer
        'Modbus Write Single Register
        txBuf = New Byte(11) {}
        txBuf = {&HB, &H10, &H4, &H10, &H0, &H1, &H2, &H0, &H1, &H5E, &H60}
        Try
            If (SerialPort1.IsOpen) Then
                SerialPort1.Write(txBuf, 0, 11)
            End If
        Catch ex As Exception
            MsgBox("Serial Port Error - " + ex.Message)
            Return 0
        End Try
        Return 1
    End Function

    Private Function bootErase() As Integer
        'Modbus Write Single Register
        txBuf = New Byte(9) {}
        txBuf = {&H7E, &H7E, &H0, &H2, &H0, &H16, &H10, &H0, &H0}
        Try
            If (SerialPort1.IsOpen) Then
                SerialPort1.Write(txBuf, 0, 9)
            End If
        Catch ex As Exception
            MsgBox("Serial Port Error - " + ex.Message)
            Return 0
        End Try
        Return 1
    End Function

    Private Function bootStart() As Integer
        'Modbus Write Single Register
        txBuf = New Byte(9) {}
        txBuf = {&H7E, &H7E, &H0, &H2, &H0, &H17, &H10, &H0, &H0}
        Try
            If (SerialPort1.IsOpen) Then
                SerialPort1.Write(txBuf, 0, 9)
            End If
        Catch ex As Exception
            MsgBox("Serial Port Error - " + ex.Message)
            Return 0
        End Try
        Return 1
    End Function

    Private Function bootChecksum() As Integer
        'Modbus Write Single Register
        txBuf = New Byte(9) {}
        txBuf = {&H7E, &H7E, &H0, &H4, &H0, &H19, &H10, fileCrc(0), fileCrc(1), &H0, &H0}
        Try
            If (SerialPort1.IsOpen) Then
                SerialPort1.Write(txBuf, 0, 11)
            End If
        Catch ex As Exception
            MsgBox("Serial Port Error - " + ex.Message)
            Return 0
        End Try
        Return 1
    End Function

    Private Sub ToolStripButton5_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        UpdateFormDelegate1 = New UpdateFormDelegate(AddressOf ProcessMessage)
        Dim n As Integer = SerialPort1.BytesToRead 'find number of bytes in buf
        rxBuf = New Byte(n - 1) {} 're dimension storage buffer
        SerialPort1.Read(rxBuf, 0, n) 'read data from the buffer
        Me.Invoke(UpdateFormDelegate1) 'call the delegate 
    End Sub

    Private Sub ProcessMessage()
        If rxBuf.Length > 0 Then
            toggleFlag = Not (toggleFlag)
            If toggleFlag Then
                ''toggleFlag = 0
                ToolStripButton5.Image = ImageList1.Images(0)
            Else
                ''toggleFlag = 1
                ToolStripButton5.Image = ImageList1.Images(1)
            End If
            'RichTextBox1.AppendText("DSP:" + rxBuf.Length.ToString)
            'For i = 0 To rxBuf.Length - 1
            '    RichTextBox1.AppendText(" " + rxBuf(i).ToString("X2") + vbCrLf)
            'Next

            commandRx = rxBuf(0)

            Select Case bootState
                Case flasherStat.F_INIT
                    If commandRx = &H17 Then
                        bootState = flasherStat.F_START
                        RichTextBox1.AppendText("Bootloader Started " + vbCrLf)
                        RichTextBox1.AppendText("Erasing Flash " + vbCrLf)
                        ToolStripLabel3.Text = "START"
                        Timer1.Enabled = True
                        TextBox3.Clear()
                    Else
                        bootState = flasherStat.F_INIT
                        Timer1.Enabled = False
                        Timer2.Enabled = False
                        ToolStripLabel3.Text = "INIT"
                        ToolStripButton5.Image = ImageList1.Images(1)
                        RichTextBox1.AppendText("Bootloading Failed - No Ack Recieved during Init" + vbCrLf)
                        RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                        RichTextBox1.ScrollToCaret()
                    End If
                Case flasherStat.F_START
                    If commandRx = &H16 Then
                        bootState = flasherStat.F_ERASE
                        RichTextBox1.AppendText("Flash Erased " + vbCrLf)
                        ToolStripLabel3.Text = "ERASE"
                        Timer1.Enabled = True
                    Else
                        bootState = flasherStat.F_INIT
                        Timer1.Enabled = False
                        Timer2.Enabled = False
                        ToolStripLabel3.Text = "INIT"
                        ToolStripButton5.Image = ImageList1.Images(1)
                        RichTextBox1.AppendText("Bootloading Failed - No Ack Recieved during Boot Erase" + vbCrLf)
                        RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                        RichTextBox1.ScrollToCaret()
                    End If
                Case flasherStat.F_PROGRAM
                    If commandRx = &H18 Then
                        If srecSNo <= (totSrec - 1) Then
                            bootData()
                            srecSNo += 1
                        Else
                            If CheckBox1.Checked Then
                                bootState = flasherStat.F_CHECKSUM
                                Timer1.Enabled = True
                                ToolStripLabel3.Text = "CHECKSUM"
                                TextBox3.Clear()
                            Else
                                bootState = flasherStat.F_INIT
                                Timer1.Enabled = False
                                Timer2.Enabled = False
                                TextBox3.Clear()
                                ToolStripLabel3.Text = "INIT"
                                ToolStripButton5.Image = ImageList1.Images(1)
                                RichTextBox1.AppendText("Bootloading Complted in " + flashingTimeInSeconds.ToString + " seconds" + vbCrLf)
                                RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                                RichTextBox1.ScrollToCaret()
                            End If
                        End If
                    Else
                        bootState = flasherStat.F_INIT
                        Timer1.Enabled = False
                        Timer2.Enabled = False
                        ToolStripLabel3.Text = "INIT"
                        ToolStripButton5.Image = ImageList1.Images(1)
                        RichTextBox1.AppendText("Bootloading Failed - No Ack Recieved during Programming" + vbCrLf)
                        RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                        RichTextBox1.ScrollToCaret()
                    End If
                Case flasherStat.F_CHECKSUM
                    If commandRx = &H19 Then
                        bootState = flasherStat.F_INIT
                        Timer1.Enabled = False
                        Timer2.Enabled = False
                        ToolStripLabel3.Text = "INIT"
                        TextBox3.Clear()
                        ToolStripButton5.Image = ImageList1.Images(1)
                        RichTextBox1.AppendText("Bootloading Complted in " + flashingTimeInSeconds.ToString + " seconds" + vbCrLf)
                        RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                        RichTextBox1.ScrollToCaret()
                    Else
                        bootState = flasherStat.F_INIT
                        Timer1.Enabled = False
                        Timer2.Enabled = False
                        ToolStripLabel3.Text = "INIT"
                        ToolStripButton5.Image = ImageList1.Images(1)
                        RichTextBox1.AppendText("Bootloading Failed - No Ack Recieved during Checksum" + vbCrLf)
                        RichTextBox1.AppendText("----------------------------------------------------------------------------" + vbCrLf)
                        RichTextBox1.ScrollToCaret()
                    End If
            End Select

            rxBuf = Nothing
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Select Case bootState
            Case flasherStat.F_INIT
                Timer1.Enabled = False
                bootStart()
            Case flasherStat.F_START
                Timer1.Enabled = False
                bootErase()
            Case flasherStat.F_ERASE
                If My.Computer.FileSystem.FileExists(TextBox1.Text) Then
                    lines = System.IO.File.ReadAllLines(TextBox1.Text).ToList
                    totSrec = lines.Count
                    bootState = flasherStat.F_PROGRAM
                    srecSNo = 0
                    RichTextBox1.AppendText("Flash Programming Started " + vbCrLf)
                    ToolStripLabel3.Text = "PROGRAM"
                    TextBox3.Clear()
                End If
            Case flasherStat.F_PROGRAM
                If srecSNo <= (totSrec - 1) Then
                    bootData()
                    srecSNo += 1
                    Timer1.Enabled = False
                Else
                    bootState = flasherStat.F_INIT
                    Timer1.Enabled = False
                    Timer2.Enabled = False
                    ToolStripLabel3.Text = "INIT"
                    RichTextBox1.ScrollToCaret()
                End If
            Case flasherStat.F_CHECKSUM
                Timer1.Enabled = False
                bootChecksum()
                RichTextBox1.AppendText("Checking Flash Checksum " + vbCrLf)
                RichTextBox1.AppendText("Calculated Flash Checksum " + fileCrc(0).ToString + " " + fileCrc(1).ToString + vbCrLf)
                ToolStripLabel3.Text = "CHECKSUM"
        End Select

    End Sub

    Private Function bootData() As Integer
        Dim srecType As String
        Dim dataPart As String
        Dim dataLen As Integer
        Dim payloadPrefix As Byte()
        Dim payloadSuffix As Byte()

        line = lines(srecSNo)
        bytes = New Byte(((line.Length - 2) / 2) + 2) {}

        srecType = line.Substring(0, 2)
        dataLen = Convert.ToByte(line.Substring(2, 2), 16)
        dataPart = line.Substring(4, line.Length - 4)

        If srecType = "S0" Then
            bytes(0) = &H53
            bytes(1) = &H30
        ElseIf srecType = "S2" Then
            bytes(0) = &H53
            bytes(1) = &H32
        ElseIf srecType = "S8" Then
            bytes(0) = &H53
            bytes(1) = &H38
        End If

        'RichTextBox1.AppendText(">>")
        'RichTextBox1.AppendText(bytes(0).ToString("X2"))
        'RichTextBox1.AppendText(bytes(1).ToString("X2"))

        bytes(2) = dataLen

        'RichTextBox1.AppendText(bytes(2).ToString("X2"))

        For j = 0 To dataLen - 1
            bytes(j + 3) = Convert.ToByte(dataPart.Substring(j * 2, 2), 16)
            'RichTextBox1.AppendText(bytes(j + 3).ToString("X2"))
        Next

        'RichTextBox1.AppendText("<<")
        'RichTextBox1.AppendText(vbCrLf)

        payloadPrefix = New Byte(7) {}
        payloadSuffix = New Byte(2) {}

        Dim lenAsByte As Byte()
        lenAsByte = intToBytes(dataLen + 5)

        payloadPrefix = {&H7E, &H7E, &H0, lenAsByte(0), lenAsByte(1), &H18, &H10}
        payloadSuffix = {&H0, &H0}

        Try
            SerialPort1.Write(payloadPrefix, 0, 7)
            SerialPort1.Write(bytes, 0, bytes.Length - 1)
            SerialPort1.Write(payloadSuffix, 0, 2)

            ToolStripStatusLabel4.Text = srecSNo.ToString()
            ToolStripProgressBar1.Value = (srecSNo / totSrec) * 100

        Catch ex As Exception
            MsgBox("Serial Port Error - " + ex.Message)
        End Try

        Return 1
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If OpenFileDialog1.ShowDialog <> Windows.Forms.DialogResult.Cancel Then
            TextBox1.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Try
            'Calculate Flash/File CRC for Writing into Flash
            If CheckBox1.Checked Then
                crcLines = System.IO.File.ReadAllLines(TextBox2.Text).ToList
                cumulativeFlash = ""
                For Each crcLine In crcLines
                    Console.WriteLine(crcLine + " " + crcLine.Length.ToString + " ")
                    If crcLine.Substring(0, 2) = "S2" Then
                        cumulativeFlash = cumulativeFlash + crcLine.Substring(10, (crcLine.Length - 12))
                    End If
                Next

                'cumulativeFlash = System.IO.File.ReadAllText("D:\downloadedHex_Flashed.dat")

                hexBytes = HexToByteArray(cumulativeFlash)

                fileCrc = New Byte(2) {}
                fileCrc = ComputeCheckSumBytes(hexBytes)
            End If

            'State Is IDLE here..
            RichTextBox1.AppendText("Starting Bootload with " + TextBox1.Text + vbCrLf)
            Timer2.Enabled = True
            flashingTimeInSeconds = 0
            bootState = flasherStat.F_INIT
            ToolStripLabel3.Text = "INIT"
            Timer1.Enabled = True
            'bootInit()

        Catch ex As Exception
            MsgBox("File Exception " + ex.Message)
        End Try
    End Sub

    Private Function HexToByteArray(ByVal hex As [String]) As Byte()
        Dim NumberChars As Integer = hex.Length
        Dim bytes As Byte() = New Byte(NumberChars / 2 - 1) {}
        For i As Integer = 0 To NumberChars - 1 Step 2
            bytes(i / 2) = Convert.ToByte(hex.Substring(i, 2), 16)
        Next
        Return bytes
    End Function

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        flashingTimeInSeconds += 1
        If flasherStat.F_START Then
            TextBox3.AppendText("=")
        End If
    End Sub

    Private Function intToBytes(intVal As Integer) As Byte()
        Dim retByte As Byte()

        retByte = New Byte(2) {}
        retByte(0) = intVal And &HFF
        retByte(1) = intVal >> 8 And &HFF

        Return retByte
    End Function

    Private Function ComputeCheckSum(ByVal bytes As Byte()) As UShort
        Dim crc As UShort = Me.intValue
        'Dim x As String
        For i As Integer = 0 To bytes.Length - 1
            crc = CUShort(((crc << 8) Xor table(((crc >> 8) Xor (&HFF And bytes(i))))))
            'crc = (crc << 8) ^ (x << 15) ^ (x << 2) ^ x
        Next
        Return crc
    End Function

    Private Function ComputeCheckSumBytes(ByVal bytes As Byte()) As Byte()
        Dim crc As UShort = ComputeCheckSum(bytes)
        Return BitConverter.GetBytes(crc)
    End Function

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            TextBox2.Enabled = True
            Button2.Enabled = True
        Else
            TextBox2.Enabled = False
            Button2.Enabled = False
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If OpenFileDialog1.ShowDialog <> Windows.Forms.DialogResult.Cancel Then
            TextBox2.Text = OpenFileDialog1.FileName
        End If
    End Sub


    Private Function initTable() As Integer
        intValue = &HFFFF
        Dim temp, a As UShort
        For i As Integer = 0 To table.Length - 1
            temp = 0
            a = CUShort(i << 8)
            For j As Integer = 0 To 7
                If ((temp Xor a) And &H8000) <> 0 Then
                    temp = CUShort((temp << 1) Xor poly)
                Else
                    temp <<= 1
                End If
                a <<= 1
            Next
            table(i) = temp
        Next

        Return 1
    End Function

    Private Sub bootLoadFrm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

    End Sub
End Class