Public Class mainFrm

    Dim testDataHi As Integer = &HF0
    Dim testDataLo As Integer = &HD
    Dim combined As Double
    Dim combinedStr As String

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub mainFrm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        combinedStr = ("&H" + testDataHi.ToString("X2") + testDataLo.ToString("X2"))
        combined = Val(combinedStr)
        Console.WriteLine(combinedStr + "   " + combined.ToString)
        'Me.Visible = False

        'WelcomeScr.Show()

        'System.Threading.Thread.Sleep(3000)

        'WelcomeScr.Close()

        'Me.Visible = True

    End Sub

    Private Sub ToolStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ToolStrip1.ItemClicked

    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        calibFrm.Show()
    End Sub

    Private Sub ToolStripButton4_Click(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        Me.Close()
    End Sub

    Private Sub CalibrationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CalibrationToolStripMenuItem.Click
        calibFrm.Show()
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        bootLoadFrm.Show()
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        meterFrm.Show()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        SplashScreen1.Show()
    End Sub
End Class
