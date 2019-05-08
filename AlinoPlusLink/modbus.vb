Imports System.IO.Ports
Module modbus
    Public Function computeCRC(rxBuf() As Byte, n As Integer) As Byte()
        Dim u8CrcHi As Byte = &HFF
        Dim u8CrcLo As Byte = &HFF
        Dim u8Index As Byte
        Dim crc As Byte()
        Dim nShdw As Integer = n
        Dim u8CRCHiArray() As Byte = {&H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40, &H1, &HC0, &H80, &H41, &H1, &HC0, &H80, &H41, &H0, &HC1, &H81, &H40}
        Dim u8CRCLoArray() As Byte = {&H0, &HC0, &HC1, &H1, &HC3, &H3, &H2, &HC2, &HC6, &H6, &H7, &HC7, &H5, &HC5, &HC4, &H4, &HCC, &HC, &HD, &HCD, &HF, &HCF, &HCE, &HE, &HA, &HCA, &HCB, &HB, &HC9, &H9, &H8, &HC8, &HD8, &H18, &H19, &HD9, &H1B, &HDB, &HDA, &H1A, &H1E, &HDE, &HDF, &H1F, &HDD, &H1D, &H1C, &HDC, &H14, &HD4, &HD5, &H15, &HD7, &H17, &H16, &HD6, &HD2, &H12, &H13, &HD3, &H11, &HD1, &HD0, &H10, &HF0, &H30, &H31, &HF1, &H33, &HF3, &HF2, &H32, &H36, &HF6, &HF7, &H37, &HF5, &H35, &H34, &HF4, &H3C, &HFC, &HFD, &H3D, &HFF, &H3F, &H3E, &HFE, &HFA, &H3A, &H3B, &HFB, &H39, &HF9, &HF8, &H38, &H28, &HE8, &HE9, &H29, &HEB, &H2B, &H2A, &HEA, &HEE, &H2E, &H2F, &HEF, &H2D, &HED, &HEC, &H2C, &HE4, &H24, &H25, &HE5, &H27, &HE7, &HE6, &H26, &H22, &HE2, &HE3, &H23, &HE1, &H21, &H20, &HE0, &HA0, &H60, &H61, &HA1, &H63, &HA3, &HA2, &H62, &H66, &HA6, &HA7, &H67, &HA5, &H65, &H64, &HA4, &H6C, &HAC, &HAD, &H6D, &HAF, &H6F, &H6E, &HAE, &HAA, &H6A, &H6B, &HAB, &H69, &HA9, &HA8, &H68, &H78, &HB8, &HB9, &H79, &HBB, &H7B, &H7A, &HBA, &HBE, &H7E, &H7F, &HBF, &H7D, &HBD, &HBC, &H7C, &HB4, &H74, &H75, &HB5, &H77, &HB7, &HB6, &H76, &H72, &HB2, &HB3, &H73, &HB1, &H71, &H70, &HB0, &H50, &H90, &H91, &H51, &H93, &H53, &H52, &H92, &H96, &H56, &H57, &H97, &H55, &H95, &H94, &H54, &H9C, &H5C, &H5D, &H9D, &H5F, &H9F, &H9E, &H5E, &H5A, &H9A, &H9B, &H5B, &H99, &H59, &H58, &H98, &H88, &H48, &H49, &H89, &H4B, &H8B, &H8A, &H4A, &H4E, &H8E, &H8F, &H4F, &H8D, &H4D, &H4C, &H8C, &H44, &H84, &H85, &H45, &H87, &H47, &H46, &H86, &H82, &H42, &H43, &H83, &H41, &H81, &H80, &H40}

        crc = New Byte(2) {}
        Dim i As Integer = 0

        While (n)
            u8Index = u8CrcHi Xor rxBuf(i)
            u8CrcHi = u8CrcLo Xor u8CRCHiArray(u8Index)
            u8CrcLo = u8CRCLoArray(u8Index)
            i = i + 1
            n = n - 1
        End While

        crc(0) = u8CrcLo
        crc(1) = u8CrcHi

        rxBuf(nShdw) = u8CrcHi
        rxBuf(nShdw + 1) = u8CrcLo

        Return crc
    End Function


    Public Function readHoldingRegisters(ByVal slaveID As Short, ByVal addr As Integer, noOfWords As Short, SerialPort1 As SerialPort) As Byte()
        Dim txBuf As Byte()
        Dim rxDataInt As Byte()
        Dim rxBuf As Byte()
        Dim dataToRead As Integer
        Dim txDataCount As Integer
        Dim rxDataCount As Integer
        Dim rxFunctionCode As Integer
        Dim rxAddress As Integer
        Dim fcReadHoldingRegs As Single = 3
        Dim fcWriteMultipleRegs As Single = 16
        Dim calcCRC As Byte()
        Dim recCRC As Byte()
        Dim errCnt As Double = 0


        txDataCount = noOfWords * 2
        txBuf = New Byte(8) {}

        txBuf(0) = BitConverter.GetBytes(slaveID)(0)

        txBuf(1) = fcReadHoldingRegs

        txBuf(2) = BitConverter.GetBytes(addr)(1)
        txBuf(3) = BitConverter.GetBytes(addr)(0)

        txBuf(4) = BitConverter.GetBytes(noOfWords)(1)
        txBuf(5) = BitConverter.GetBytes(noOfWords)(0)

        computeCRC(txBuf, 6)

        Try
            SerialPort1.Write(txBuf, 0, 8)

            Threading.Thread.Sleep(400) 'Time to wait for response

            dataToRead = SerialPort1.BytesToRead
            Console.WriteLine("Data To Read -> " + dataToRead.ToString + "/" + txDataCount.ToString)

            rxBuf = New Byte(dataToRead) {}
            SerialPort1.Read(rxBuf, 0, dataToRead)

            If (dataToRead = txDataCount + 5) Then

                calcCRC = computeCRC(rxBuf, dataToRead - 2)
                recCRC = {rxBuf(dataToRead - 1), rxBuf(dataToRead - 2)}
                'Check CRC
                If (calcCRC(0) = recCRC(0) And calcCRC(1) = recCRC(1)) Then
                    For i = 0 To dataToRead - 3
                        Select Case i
                            Case 0
                                rxAddress = rxBuf(i)
                            Case 1
                                rxFunctionCode = rxBuf(i)
                            Case 2
                                Console.WriteLine(vbCrLf)
                                rxDataCount = rxBuf(i)
                                rxDataInt = New Byte(rxDataCount) {}
                            Case 3 To dataToRead - 3
                                rxDataInt(i - 3) = rxBuf(i)
                                Console.WriteLine(rxDataInt(i - 3).ToString + " ")
                        End Select
                        Console.Write(rxBuf(i).ToString + " ")
                    Next

                    'If BitConverter.IsLittleEndian Then
                    '    Array.Reverse(rxDataInt, 0, txDataCount)
                    'End If

                Else
                    'Error 
                    errCnt += 1
                End If
            Else
                If rxBuf Is Nothing Then
                    Return Nothing
                End If

                For i = 0 To dataToRead - 3
                    Select Case i
                        Case 0
                            rxAddress = rxBuf(i)
                        Case 1
                            rxFunctionCode = rxBuf(i)
                        Case 2
                            Console.WriteLine(vbCrLf)
                            rxDataCount = rxBuf(i)
                            rxDataInt = New Byte(rxDataCount) {}
                        Case 3 To dataToRead - 3
                            rxDataInt(i - 3) = rxBuf(i)
                            Console.WriteLine(rxDataInt(i - 3).ToString + " ")
                    End Select
                    Console.Write(rxBuf(i).ToString + " ")
                Next

                If BitConverter.IsLittleEndian Then
                    Array.Reverse(rxDataInt, 0, txDataCount)
                End If
            End If

        Catch ex As Exception
            Return {0}
            MsgBox("COM Error..." & vbCrLf & ex.Message)
        End Try
        Return rxDataInt
    End Function
End Module
