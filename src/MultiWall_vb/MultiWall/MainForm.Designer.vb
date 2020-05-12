<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.toolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.showDropRegionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.appNotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.notifyIconContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.showPreviewWindowToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.toolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.quitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.pictureBox1 = New System.Windows.Forms.PictureBox
        Me.notifyIconContextMenuStrip.SuspendLayout()
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'toolStripSeparator1
        '
        Me.toolStripSeparator1.Name = "toolStripSeparator1"
        Me.toolStripSeparator1.Size = New System.Drawing.Size(207, 6)
        '
        'showDropRegionsToolStripMenuItem
        '
        Me.showDropRegionsToolStripMenuItem.Name = "showDropRegionsToolStripMenuItem"
        Me.showDropRegionsToolStripMenuItem.Size = New System.Drawing.Size(210, 22)
        Me.showDropRegionsToolStripMenuItem.Text = "Show Drop Regions"
        '
        'appNotifyIcon
        '
        Me.appNotifyIcon.ContextMenuStrip = Me.notifyIconContextMenuStrip
        Me.appNotifyIcon.Icon = CType(resources.GetObject("appNotifyIcon.Icon"), System.Drawing.Icon)
        Me.appNotifyIcon.Text = "Coding 4 Fun - MultiWall"
        Me.appNotifyIcon.Visible = True
        '
        'notifyIconContextMenuStrip
        '
        Me.notifyIconContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.showPreviewWindowToolStripMenuItem, Me.toolStripSeparator2, Me.showDropRegionsToolStripMenuItem, Me.toolStripSeparator1, Me.quitToolStripMenuItem})
        Me.notifyIconContextMenuStrip.Name = "contextMenuStrip1"
        Me.notifyIconContextMenuStrip.Size = New System.Drawing.Size(211, 82)
        '
        'showPreviewWindowToolStripMenuItem
        '
        Me.showPreviewWindowToolStripMenuItem.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.showPreviewWindowToolStripMenuItem.Name = "showPreviewWindowToolStripMenuItem"
        Me.showPreviewWindowToolStripMenuItem.Size = New System.Drawing.Size(210, 22)
        Me.showPreviewWindowToolStripMenuItem.Text = "Show Preview Window"
        '
        'toolStripSeparator2
        '
        Me.toolStripSeparator2.Name = "toolStripSeparator2"
        Me.toolStripSeparator2.Size = New System.Drawing.Size(207, 6)
        '
        'quitToolStripMenuItem
        '
        Me.quitToolStripMenuItem.Name = "quitToolStripMenuItem"
        Me.quitToolStripMenuItem.Size = New System.Drawing.Size(210, 22)
        Me.quitToolStripMenuItem.Text = "Quit"
        '
        'pictureBox1
        '
        Me.pictureBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.pictureBox1.Location = New System.Drawing.Point(12, 12)
        Me.pictureBox1.Name = "pictureBox1"
        Me.pictureBox1.Size = New System.Drawing.Size(454, 170)
        Me.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pictureBox1.TabIndex = 1
        Me.pictureBox1.TabStop = False
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(478, 194)
        Me.Controls.Add(Me.pictureBox1)
        Me.Name = "MainForm"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Coding 4 Fun - MultiWall"
        Me.notifyIconContextMenuStrip.ResumeLayout(False)
        CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents toolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents showDropRegionsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents appNotifyIcon As System.Windows.Forms.NotifyIcon
    Private WithEvents notifyIconContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Private WithEvents showPreviewWindowToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents toolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents quitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents pictureBox1 As System.Windows.Forms.PictureBox

End Class
