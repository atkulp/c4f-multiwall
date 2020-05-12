Partial Public Class MonitorSelectForm
    Inherits Form
    Public Event WallpaperSelectedEvent As WallpaperSelectedEventHandler
    Private _myScreen As Screen
    Private _wallpaperFilename As String
    Private _screenIndex As Integer

    Public Sub New(ByVal whichScreen As Screen, ByVal screenIndex As Integer)
        InitializeComponent()

        _myScreen = whichScreen
        _screenIndex = screenIndex
    End Sub

    Public ReadOnly Property MyScreen() As Screen
        Get
            Return _myScreen
        End Get
    End Property

    Public ReadOnly Property WallpaperFilename() As String
        Get
            Return _wallpaperFilename
        End Get
    End Property

    Public ReadOnly Property ScreenIndex() As Integer
        Get
            Return _screenIndex
        End Get
    End Property

    Private Sub CenterOnScreen()
        Dim bounds As Rectangle = _myScreen.WorkingArea
        Me.Left = bounds.Left + ((bounds.Width - Me.Width) / 2)
        Me.Top = bounds.Top
    End Sub

    Private Sub MonitorSelectForm_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs) Handles Me.DragEnter
        ' Only accept file drops
        If e.Data.GetDataPresent(DataFormats.FileDrop, False) = True Then
            e.Effect = DragDropEffects.All
        End If
    End Sub

    Private Sub MonitorSelectForm_DragDrop(ByVal sender As Object, ByVal e As DragEventArgs) Handles Me.DragDrop
        ' It's an array, even for a single file
        Dim files As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())

        _wallpaperFilename = files(0)
        RaiseWallpaperSelectedEvent(files(0))
    End Sub

    Private Sub RaiseWallpaperSelectedEvent(ByVal filename As String)
        Dim e As New WallpaperSelectedEventArgs()
        e.SelectedScreen = _myScreen
        e.WallpaperFilename = filename
        e.ScreenIndex = _screenIndex

        RaiseEvent WallpaperSelectedEvent(Me, e)
    End Sub

    Private Sub MonitorSelectForm_Shown(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Shown
        CenterOnScreen()
    End Sub
End Class

Public Delegate Sub WallpaperSelectedEventHandler(ByVal sender As Object, ByVal e As WallpaperSelectedEventArgs)
