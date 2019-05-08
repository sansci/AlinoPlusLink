Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel

Public Class meterFrm

    Const PV_Voltage = 40196
    Const PV_Current = 40197
    Const PV_Watts = 40198
    Const PV_Today_Wh = 40199
    Const PV_Today_Wp = 40200
    Const PV_Today_Vp = 40201
    Const Battery_Volts = 40202
    Const Battery_Curr = 40203
    Const Battery_Temp = 40204
    Const Battery_SOC = 40205
    Const Battery_Today_Ah = 40206
    Const Battery_Today_Min = 40207
    Const Battery_Today_Max = 40208
    Const Input_AC_L1_N_Volts = 40209
    Const Input_AC_L1_N_Curr = 40210
    Const Input_AC_L2_N_Volts = 40211
    Const Input_AC_L2_N_Curr = 40212
    Const Input_AC_L1_L2_Volts = 40213
    Const Input_AC_L1_L2_Curr = 40214
    Const Input_AC_L1_N_Power = 40215
    Const Input_AC_L1_N_Energy = 40216
    Const Input_AC_L2_N_Power = 40217
    Const Input_AC_L2_N_Energy = 40218
    Const Input_AC_L1_L2_Power = 40219
    Const Input_AC_L1_L2_Energy = 40220
    Const Inv_AC_L1_N_Power = 40221
    Const Inv_AC_L1_N_Energy = 40222
    Const Inv_AC_L2_N_Power = 40223
    Const Inv_AC_L2_N_Energy = 40224

    Const misc1 = 40225
    Const misc2 = 40226
    Const misc3 = 40227
    Const misc4 = 40228
    Const misc5 = 40229
    Const misc6 = 40230
    Const misc7 = 40231


    Dim rxBuf As Byte()
    Dim sf_vpv As Integer
    Dim sf_ipv As Integer
    Dim sf_wpv As Integer
    Dim sf_whpv As Integer
    Dim sf_vbat As Integer = 100
    Dim sf_ibat As Integer
    Dim sf_soc As Integer
    Dim sf_ah As Integer
    Dim sf_vac As Integer
    Dim sf_iac As Integer
    Dim sf_was As Integer
    Dim sf_whas As Integer
    Dim sf_invwatt As Integer
    Dim sf_invwh As Integer
    Dim sf_temp As Integer
    Dim sf_hrs As Integer
    Dim sf_months As Integer
    Dim sf_days As Integer
    Dim sf_usd As Integer

    Dim enLog As Boolean = False
    Dim toggleFlag As Boolean = False

    Dim sfUpdated As Boolean = False

    Dim inSuffData As Boolean = False

    Dim colourRed As Integer

    Dim file As System.IO.StreamWriter


    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        Me.Close()
    End Sub

    Private Sub meterFrm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Button21.BackColor = Color.Red
        Timer2.Enabled = False
        ToolStripStatusLabel5.Image = ImageList1.Images(0)

        ToolStripButton5.ToolTipText = "COM9 9600 8 None 1"
        ToolStripStatusLabel2.Spring = True
        For Each sp As String In My.Computer.Ports.SerialPortNames
            ToolStripComboBox1.Items.Add(sp)
        Next

        If (ToolStripComboBox1.Items.Count > 0) Then
            ToolStripComboBox1.SelectedIndex = 0
        End If

        ToolStripComboBox2.SelectedIndex = 3
        PictureBox1.Image = ImageList1.Images(0)
        PictureBox2.Image = ImageList1.Images(0)
        PictureBox3.Image = ImageList1.Images(0)
        PictureBox4.Image = ImageList1.Images(0)
        PictureBox5.Image = ImageList1.Images(0)
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        If SerialPort1.IsOpen And sfUpdated Then
            Timer2.Enabled = True
            Timer1.Enabled = True
            rxBuf = Nothing
        Else
            If sfUpdated = False Then
                MsgBox("Update SF Once By clicking the button", vbExclamation)
            Else
                MsgBox("Connect to Serial Port", vbExclamation)
            End If
        End If
    End Sub

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
            MsgBox("Port Is already Open")
        End If

        If (SerialPort1.IsOpen) Then
            ToolStripStatusLabel1.Text = SerialPort1.PortName.ToString + " " + SerialPort1.BaudRate.ToString + " " + SerialPort1.DataBits.ToString + " " + SerialPort1.Parity.ToString + " " + SerialPort1.StopBits.ToString
        Else
            ToolStripStatusLabel1.Text = "Not Connected"
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim combined As Double
        Dim hiByte As Integer
        Dim loByte As Integer
        Dim faultHi As Integer
        Dim faultReg As Long
        rxBuf = modbus.readHoldingRegisters(&HB, 40196, 36, SerialPort1)
        If rxBuf.Length = 1 And rxBuf(0) = 0 Then
            'MsgBox("Insufficient Bytes Recieved (or) No Response", vbCritical)
            ToolStripStatusLabel5.Image = ImageList1.Images(2)
            inSuffData = True
            'Timer1.Enabled = False
            'Timer2.Enabled = False
        Else
            inSuffData = False
            If enLog Then
                file.Write(Now.ToLongTimeString + ",")
            End If

            For i = 0 To rxBuf.Length - 2 Step 2
                hiByte = rxBuf(i)
                loByte = rxBuf(i + 1)
                Select Case (i / 2) + 40196
                    Case PV_Current
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_ipv
                        TextBox25.Text = combined.ToString("N2")
                        If CheckBox8.Checked Then
                            Chart1.Series(7).Points.AddY(combined)
                        End If
                    Case PV_Voltage
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vpv
                        TextBox26.Text = combined.ToString("N2")
                        If CheckBox7.Checked Then
                            Chart1.Series(6).Points.AddY(combined)
                        End If
                    Case PV_Watts
                    Case PV_Today_Wh
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox28.Text = combined.ToString("N2")
                        If CheckBox12.Checked Then
                            Chart1.Series(11).Points.AddY(combined)
                        End If
                    Case PV_Today_Wp
                        'HS1 Temp
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox32.Text = combined.ToString("N2")
                        If CheckBox10.Checked Then
                            Chart1.Series(9).Points.AddY(combined)
                        End If
                    Case PV_Today_Vp
                        'HS2 Temp
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox35.Text = combined.ToString("N2")
                        If CheckBox11.Checked Then
                            Chart1.Series(10).Points.AddY(combined)
                        End If
                    Case Battery_Volts
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox21.Text = combined.ToString("N2")
                        If CheckBox1.Checked Then
                            Chart1.Series(0).Points.AddY(combined)
                        End If
                    Case Battery_Curr
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_ibat
                        TextBox17.Text = combined.ToString("N2")
                    Case Battery_Temp
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox29.Text = combined.ToString("N2")
                    Case Battery_SOC
                    Case Battery_Today_Ah
                        'cc hs1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox24.Text = combined.ToString("N2")
                    Case Battery_Today_Min
                        'cc hs2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox23.Text = combined.ToString("N2")
                        If CheckBox2.Checked Then
                            Chart1.Series(1).Points.AddY(combined + Val(TextBox17.Text))
                        End If
                    Case Battery_Today_Max
                        'Ibat Ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_ibat
                        TextBox18.Text = combined.ToString("N2")
                    Case Input_AC_L1_N_Volts
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vac
                        TextBox1.Text = combined.ToString("N2")
                    Case Input_AC_L2_N_Volts
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vac
                        TextBox14.Text = combined.ToString("N2")
                    Case Input_AC_L1_N_Curr
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_iac
                        TextBox5.Text = combined.ToString("N2")
                        If CheckBox5.Checked Then
                            Chart1.Series(4).Points.AddY(combined)
                        End If
                    Case Input_AC_L2_N_Curr
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_iac
                        TextBox10.Text = combined.ToString("N2")
                        If CheckBox6.Checked Then
                            Chart1.Series(5).Points.AddY(combined)
                        End If
                    Case Input_AC_L1_L2_Volts
                        'Vout Ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vac
                        TextBox3.Text = combined.ToString("N2")
                        If CheckBox3.Checked Then
                            Chart1.Series(2).Points.AddY(combined)
                        End If
                    Case Input_AC_L1_L2_Curr
                        'Vout Ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vac
                        TextBox12.Text = combined.ToString("N2")
                        If CheckBox4.Checked Then
                            Chart1.Series(3).Points.AddY(combined)
                        End If
                    Case Input_AC_L1_L2_Power
                        'Iout Accu Ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_iac
                        TextBox6.Text = combined.ToString("N2")
                    Case Input_AC_L1_L2_Energy
                        'Iout Accu Ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox9.Text = combined.ToString("N2")
                    Case Input_AC_L1_N_Power
                        'Freq Ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox4.Text = combined.ToString("N2")
                    Case Input_AC_L1_N_Energy
                        'Freq Ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox11.Text = combined.ToString("N2")
                    Case Input_AC_L2_N_Power
                        'Iout Sec Ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_iac
                        TextBox7.Text = combined.ToString("N2")
                    Case Input_AC_L2_N_Energy
                        'Iout Sec Ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_iac
                        TextBox8.Text = combined.ToString("N2")
                    Case Inv_AC_L1_N_Power
                        'real power ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_was
                        TextBox30.Text = combined.ToString("N2")
                    Case Inv_AC_L1_N_Energy
                        'energy ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_whas
                        TextBox31.Text = combined.ToString("N2")
                    Case Inv_AC_L2_N_Power
                        'real power ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_was
                        TextBox33.Text = combined.ToString("N2")
                    Case Inv_AC_L2_N_Energy
                        'energy ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_whas
                        TextBox34.Text = combined.ToString("N2")
                    Case misc1
                        'ibat cc ph1
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_ibat
                        TextBox27.Text = combined.ToString("N2")
                        If CheckBox9.Checked Then
                            Chart1.Series(8).Points.AddY(combined + Val(TextBox16.Text))
                        End If
                    Case misc2
                        'ibat cc ph2
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_ibat
                        TextBox16.Text = combined.ToString("N2")
                    Case misc3
                        'vbat ext
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_vbat
                        TextBox22.Text = combined.ToString("N2")
                    Case misc4
                        't amb
                        combined = Val("&H" + ((hiByte << 8) Or (loByte)).ToString("X4")) / sf_temp
                        TextBox15.Text = combined.ToString("N2")
                    Case misc5
                        ' legReg
                        'bat
                        If loByte And &H1 Then
                            PictureBox3.Image = ImageList1.Images(1)
                        Else
                            PictureBox3.Image = ImageList1.Images(0)
                        End If
                        'pv
                        If loByte >> 1 And &H1 Then
                            PictureBox1.Image = ImageList1.Images(1)
                        Else
                            PictureBox1.Image = ImageList1.Images(0)
                        End If
                        'grid
                        If loByte >> 2 And &H1 Then
                            PictureBox2.Image = ImageList1.Images(1)
                        Else
                            PictureBox2.Image = ImageList1.Images(0)
                        End If
                        'fault
                        If loByte >> 4 And &H1 Then
                            PictureBox5.Image = ImageList1.Images(2)
                        Else
                            PictureBox5.Image = ImageList1.Images(0)
                        End If
                        'Output
                        If loByte >> 5 And &H1 Then
                            PictureBox4.Image = ImageList1.Images(1)
                        Else
                            PictureBox4.Image = ImageList1.Images(0)
                        End If
                    Case misc6
                        faultHi = ((hiByte << 8) Or (loByte))
                    Case misc7
                        faultReg = (faultHi << 16 Or (hiByte << 8) Or (loByte))
                        TextBox19.Text = faultReg.ToString("X8")
                End Select

                If enLog Then
                    file.Write(combined.ToString + ",")
                End If

            Next
        End If
        If enLog Then
            file.Write(vbCrLf)
        End If
    End Sub

    Private Sub ToolStripButton5_Click(sender As Object, e As EventArgs) Handles ToolStripButton5.Click
        'Closing Port for reconfiguring
        If (SerialPort1.IsOpen) Then
            SerialPort1.Close()
        End If

        If (SerialPort1.IsOpen = False) Then
            'MsgBox("test OK  " + Parity.None.ToString)
            SerialPort1.PortName = "COM9"
            SerialPort1.BaudRate = 9600
            SerialPort1.Parity = Parity.None
            SerialPort1.StopBits = StopBits.One
            SerialPort1.DataBits = 8

            Try
                SerialPort1.Open()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        Else
            MsgBox("Port Is already Open")
        End If

        If (SerialPort1.IsOpen) Then
            ToolStripStatusLabel4.Text = SerialPort1.PortName.ToString + " " + SerialPort1.BaudRate.ToString + " " + SerialPort1.DataBits.ToString + " " + SerialPort1.Parity.ToString + " " + SerialPort1.StopBits.ToString
        Else
            ToolStripStatusLabel4.Text = "Not Connected"
        End If
    End Sub

    Private Sub Label35_Click(sender As Object, e As EventArgs) Handles Label35.Click

    End Sub

    Private Sub LogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LogToolStripMenuItem.Click
        If enLog = False Then
            enLog = True
            file = My.Computer.FileSystem.OpenTextFileWriter("D:\" + Now.Year.ToString("D4") + "_" + Now.Month.ToString("D2") + "_" + Now.Day.ToString("D2") + "_" + Now.Hour.ToString("D2") + Now.Minute.ToString("D2") + "_" + "log.log", True)
            LogToolStripMenuItem.Checked = True
            ToolStripStatusLabel1.Text = "Logger: ON"
        Else
            enLog = False
            LogToolStripMenuItem.Checked = False
            ToolStripStatusLabel1.Text = "Logger: OFF"
            file.Close()
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If toggleFlag Then
            toggleFlag = False
            If inSuffData Then
                ToolStripStatusLabel5.Image = ImageList1.Images(2)
            Else
                ToolStripStatusLabel5.Image = ImageList1.Images(1)
            End If
        Else
            toggleFlag = True
            ToolStripStatusLabel5.Image = ImageList1.Images(0)
        End If
    End Sub

    Private Sub ToolStripButton6_Click(sender As Object, e As EventArgs) Handles ToolStripButton6.Click
        If SerialPort1.IsOpen Then
            Timer1.Enabled = False
            rxBuf = modbus.readHoldingRegisters(&HB, 40320, 19, SerialPort1)
            If rxBuf.Length >= 2 Then
                For i = 0 To rxBuf.Length - 2 Step 2
                    Select Case i
                        Case 0
                            sf_vpv = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 2
                            sf_ipv = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 4
                            sf_wpv = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 6
                            sf_whpv = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 8
                            sf_vbat = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 10
                            sf_ibat = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 12
                            sf_soc = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 14
                            sf_ah = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 16
                            sf_vac = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 18
                            sf_iac = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 20
                            sf_was = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 22
                            sf_whas = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 24
                            sf_invwatt = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 26
                            sf_invwh = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 28
                            sf_temp = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 30
                            sf_hrs = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 32
                            sf_days = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 34
                            sf_months = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                        Case 36
                            sf_usd = 10 ^ ((rxBuf(i) << 8) Or (rxBuf(i + 1)))
                    End Select
                Next

                sfUpdated = True

            Else
                'MsgBox("Insufficient Bytes Recieved (or) No Response", vbCritical)
                ToolStripStatusLabel5.Image = ImageList1.Images(2)
            End If
            Timer1.Enabled = True
        Else
            MsgBox("Connect to Serial Port", vbExclamation)
        End If
    End Sub

    Private Sub ExportLogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportLogToolStripMenuItem.Click

    End Sub

    Private Sub ToolStripButton7_Click(sender As Object, e As EventArgs) Handles ToolStripButton7.Click
        Timer1.Enabled = False
        Timer2.Enabled = False
    End Sub

    Private Function limit(value As Double, min As Double, max As Double) As Double
        Return If(value <= min, min, If((value >= max), max, value))
    End Function

    Private Sub TextBox21_TextChanged(sender As Object, e As EventArgs) Handles TextBox21.TextChanged
        colourRed = 255 - ((limit(Val(TextBox21.Text), 55.2, 63) - 55.2) / 7.8) * 255
        TextBox21.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox32_TextChanged(sender As Object, e As EventArgs) Handles TextBox32.TextChanged
        colourRed = 255 - ((limit(Val(TextBox32.Text), 25, 90) - 25) / 65) * 255
        TextBox32.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox35_TextChanged(sender As Object, e As EventArgs) Handles TextBox35.TextChanged
        colourRed = 255 - ((limit(Val(TextBox35.Text), 25, 90) - 25) / 65) * 255
        TextBox35.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox24_TextChanged(sender As Object, e As EventArgs) Handles TextBox24.TextChanged
        colourRed = 255 - ((limit(Val(TextBox24.Text), 25, 90) - 25) / 65) * 255
        TextBox24.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox23_TextChanged(sender As Object, e As EventArgs) Handles TextBox23.TextChanged
        colourRed = 255 - ((limit(Val(TextBox23.Text), 25, 90) - 25) / 65) * 255
        TextBox23.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox29_TextChanged(sender As Object, e As EventArgs) Handles TextBox29.TextChanged
        colourRed = 255 - ((limit(Val(TextBox29.Text), 25, 90) - 25) / 65) * 255
        TextBox29.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox5_TextChanged(sender As Object, e As EventArgs) Handles TextBox5.TextChanged
        colourRed = 255 - ((limit(Val(TextBox5.Text), 16.66, 49.98) - 16.66) / 33.32) * 255
        TextBox5.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox6_TextChanged(sender As Object, e As EventArgs) Handles TextBox6.TextChanged
        colourRed = 255 - ((limit(Val(TextBox6.Text), 16.66, 49.98) - 16.66) / 33.32) * 255
        TextBox6.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox10_TextChanged(sender As Object, e As EventArgs) Handles TextBox10.TextChanged
        colourRed = 255 - ((limit(Val(TextBox10.Text), 16.66, 49.98) - 16.66) / 33.32) * 255
        TextBox10.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox9_TextChanged(sender As Object, e As EventArgs) Handles TextBox9.TextChanged
        colourRed = 255 - ((limit(Val(TextBox9.Text), 16.66, 49.98) - 16.66) / 33.32) * 255
        TextBox9.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged

    End Sub

    Private Sub TextBox28_TextChanged(sender As Object, e As EventArgs) Handles TextBox28.TextChanged
        colourRed = 255 - ((limit(Val(TextBox28.Text), 55.2, 63) - 55.2) / 7.8) * 255
        TextBox28.BackColor = Color.FromArgb(255, colourRed, colourRed)
    End Sub

    Private Sub ToolStripButton8_Click(sender As Object, e As EventArgs) Handles ToolStripButton8.Click
        If MsgBox("Are you sure, the chart will be cleared.. ", 4, vbExclamation) = 6 Then
            Chart1.Series(0).Points.Clear()
            Chart1.Series(1).Points.Clear()
            Chart1.Series(2).Points.Clear()
            Chart1.Series(3).Points.Clear()
            Chart1.Series(4).Points.Clear()
            Chart1.Series(5).Points.Clear()
            Chart1.Series(6).Points.Clear()
            Chart1.Series(7).Points.Clear()
            Chart1.Series(8).Points.Clear()
            Chart1.Series(9).Points.Clear()
            Chart1.Series(10).Points.Clear()
        End If
    End Sub

    Private Sub ToolStripButton9_Click(sender As Object, e As EventArgs) Handles ToolStripButton9.Click
        TextBox21.Text = (Val(TextBox21.Text) + 1).ToString
        Chart1.Series(0).Points.AddY(TextBox21.Text)
    End Sub

    Private Sub TableLayoutPanel6_Paint(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel6.Paint

    End Sub

    Private Sub Button21_Click(sender As Object, e As EventArgs) Handles Button21.Click
        Button21.BackColor = Color.Transparent
    End Sub

    Private Sub Button24_Click(sender As Object, e As EventArgs) Handles Button24.Click
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Chart1.SaveImage(SaveFileDialog1.FileName, Drawing.Imaging.ImageFormat.Png)
        End If
    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged

    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged

    End Sub

    Private Sub ToolStripStatusLabel3_Click(sender As Object, e As EventArgs) Handles ToolStripStatusLabel3.Click

    End Sub

    Private Sub Button23_Click(sender As Object, e As EventArgs) Handles Button23.Click
        Chart1.Series(0).Points.Clear()
        Chart1.Series(1).Points.Clear()
        Chart1.Series(2).Points.Clear()
        Chart1.Series(3).Points.Clear()
        Chart1.Series(4).Points.Clear()
        Chart1.Series(5).Points.Clear()
        Chart1.Series(6).Points.Clear()
        Chart1.Series(7).Points.Clear()
        Chart1.Series(8).Points.Clear()
        Chart1.Series(9).Points.Clear()
        Chart1.Series(10).Points.Clear()
        Chart1.Series(11).Points.Clear()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click

    End Sub
End Class