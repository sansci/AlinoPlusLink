Public Class crc16ccitt
    Public Enum InitialCRCValue
        Zeroes = 0
        NonZero1 = &HFFFF
        NonZero2 = &H1D0F
        'NonZero3 = &H0
    End Enum

    Private Const poly As UShort = &H1021 'polynomial
    Dim table(255) As UShort
    Dim intValue As UShort = 0

    Public Function ComputeCheckSum(ByVal bytes As Byte()) As UShort
        Dim crc As UShort = Me.intValue
        'Dim x As String
        For i As Integer = 0 To bytes.Length - 1
            crc = CUShort(((crc << 8) Xor table(((crc >> 8) Xor (&HFF And bytes(i))))))
            'crc = (crc << 8) ^ (x << 15) ^ (x << 2) ^ x
        Next
        Return crc
    End Function

    Public Function ComputeCheckSumBytes(ByVal bytes As Byte()) As Byte()
        Dim crc As UShort = ComputeCheckSum(bytes)
        Return BitConverter.GetBytes(crc)
    End Function

    Public Sub New(ByVal initialvalue As InitialCRCValue)
        Me.intValue = CUShort(initialvalue)
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
    End Sub
End Class