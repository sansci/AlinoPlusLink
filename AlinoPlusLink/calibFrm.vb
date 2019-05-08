Imports System
Imports System.Threading
Imports System.IO.Ports
Imports System.ComponentModel

Public Class calibFrm

    'RTC values
    Dim day As Integer
    Dim dat As Integer
    Dim mon As Integer
    Dim yea As Integer
    Dim hr As Integer
    Dim min As Integer
    Dim sec As Integer
    'Functional parameters
    Dim pout As Double
    Dim iout As Double
    Dim vout As Double
    Dim vin As Double
    Dim sysStatus As Long
    Dim faultOV As Long
    Dim faultUV As Long
    Dim faultSC As Long
    'EEPROM Values
    Dim voltKp As Long
    Dim voltKi As Long
    Dim currKp As Long
    Dim currKi As Long
    Dim currRef As Long
    Dim setVolt As Long
    Dim droopFactor As Long

    Dim voltKpOld As Double
    Dim voltKiOld As Double
    Dim currKpOld As Double
    Dim currKiOld As Double
    Dim currRefOld As Long
    Dim setVoltOld As Long
    Dim droopFactorOld As Long

    'General
    Dim ones As Integer
    Dim tens As Integer
    Dim minorRev As Integer
    Dim majorRev As Integer

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged

    End Sub

    Private Sub calibFrm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If (Not (SerialPort1.IsOpen)) Then
            SerialPort1.Open()
        End If

        If modbus.readHoldingRegisters(11, 1001, 4, SerialPort1) Is Nothing Then
            RichTextBox1.AppendText("No Data Received !!" + vbCrLf)
        End If

    End Sub
End Class