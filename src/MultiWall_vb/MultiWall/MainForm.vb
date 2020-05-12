Imports System.Collections.Specialized
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.Win32
Imports System.String

Public Class MainForm
    Inherits Form

#Region "Class Variables"

    Private appClosing As Boolean = False, minimizeMessageShown As Boolean = False

    Private previewBitmap As Bitmap = Nothing, desktopBitmap As Bitmap = Nothing
    Private overallBounds As Rectangle

    ' Where is 0,0 in the composite Desktop image?
    Private refPoint As Point

    Private screens As Screen()
    Private selectForms As MonitorSelectForm()
    Private wallpaperFilenames As String()
    Private wallpaperBitmapCache As New Dictionary(Of String, WeakReference)()

    ' This collection isn't actually used to retrieve the Bitmap, but
    ' prevents the WeakReference from disappearing while the Bitmap is in use
    Private wallpaperBitmaps As Bitmap()

#End Region

    Public Sub New()
        InitializeComponent()

        Init()
    End Sub

#Region "Desktop updating"

    ''' <summary>
    ''' Modify the preview window (wrapping images as used for Desktop)
    ''' </summary>
    Private Sub UpdateDesktopImage()
        Using g As Graphics = Graphics.FromImage(desktopBitmap)
            For idx As Integer = 0 To screens.Length - 1
                AddImageToDesktop(g, wallpaperFilenames(idx), screens(idx).Bounds, idx)
            Next
        End Using

        Dim fn As String = Wallpaper.SaveWallpaper(desktopBitmap)
        Wallpaper.SetWallpaper(fn, WallpaperStyle.Tiled)
    End Sub

    ''' <summary>
    ''' Adds the given image to the bitmap, but wraps images as needed based on monitor configuration
    ''' </summary>
    ''' <param name="g">The Graphics of the composite Desktop </param>
    ''' <param name="filename">The filename of the image to add</param>
    ''' <param name="bounds">The bounds for this screen</param>
    Public Sub AddImageToDesktop(ByVal g As Graphics, ByVal filename As String, ByVal bounds As Rectangle, ByVal idx As Integer)
        ' Clear the region and verify a selected filename
        g.FillRectangle(Brushes.Black, bounds)

        If String.IsNullOrEmpty(filename) Then
            Return
        End If

        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic

        Dim currentImage As Bitmap = BitmapFromFile(filename, idx)

        Dim sourceBounds As New Rectangle(0, 0, currentImage.Width, currentImage.Height)
        Dim ratio As Double = MaintainAspectRatio(currentImage, bounds)

        ' If image is drawn outside of viewable area, it must be "wrapped" around
        If bounds.X < 0 OrElse bounds.Y < 0 OrElse (bounds.X + bounds.Width > desktopBitmap.Width) OrElse (bounds.Y + bounds.Height > desktopBitmap.Height) Then
            ' Draw the first half
            Dim x As Integer = bounds.X, y As Integer = bounds.Y

            If bounds.Y < 0 Then
                y = desktopBitmap.Height + bounds.Y
            ElseIf bounds.Y + bounds.Height > desktopBitmap.Height Then
                y = 0 - (desktopBitmap.Height - bounds.Height + bounds.Y)
            End If

            If bounds.X < 0 Then
                x = desktopBitmap.Width + bounds.X
            ElseIf bounds.X + bounds.Width > desktopBitmap.Width Then
                x = 0 - (desktopBitmap.Width - bounds.Width - bounds.X)
            End If

            ' Draw original coordinates
            g.DrawImage(currentImage, New Rectangle(x, y, bounds.Width, bounds.Height), New Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel)

            ' Draw with corrected Y coordinate
            g.DrawImage(currentImage, New Rectangle(x, bounds.Y, bounds.Width, bounds.Height), New Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel)

            ' Draw with corrected X coordinate
            g.DrawImage(currentImage, New Rectangle(bounds.X, y, bounds.Width, bounds.Height), New Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel)
        Else
            ' Draw the image once, fully in the viewable area
            g.DrawImage(currentImage, bounds, sourceBounds, GraphicsUnit.Pixel)
        End If
    End Sub


#End Region

#Region "Preview updating"

    ''' <summary>
    ''' Modify the preview window (non-wrapping images)
    ''' </summary>
    Private Sub UpdatePreviewImage()
        Using g As Graphics = Graphics.FromImage(previewBitmap)
            For idx As Integer = 0 To screens.Length - 1
                AddImageToPreview(g, wallpaperFilenames(idx), screens(idx).Bounds, idx)
            Next
        End Using

        pictureBox1.Image = previewBitmap
    End Sub

    ''' <summary>
    ''' Adds the given image to the preview image
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="filename"></param>
    ''' <param name="bounds"></param>
    Public Sub AddImageToPreview(ByVal g As Graphics, ByVal filename As String, ByVal bounds As Rectangle, ByVal idx As Integer)
        ' Clear the region and verify a selected filename
        g.FillRectangle(SystemBrushes.ButtonFace, bounds)

        If String.IsNullOrEmpty(wallpaperFilenames(idx)) Then
            Return
        End If

        ' Load the image from file or cache
        Dim currentImage As Bitmap = BitmapFromFile(filename, idx)

        ' Translate negative screens (to the left/above primary monitor)
        ' into viewable preview area
        bounds.X += refPoint.X
        bounds.Y += refPoint.Y

        ' Everything is quarter-sized for preview mode
        bounds.X /= 4
        bounds.Y /= 4
        bounds.Width /= 4
        bounds.Height /= 4

        ' Copy bounds to have monitor vs. resized image dimensions
        Dim overallBounds As New Rectangle(bounds.Location, bounds.Size)
        Dim ratio As Double = MaintainAspectRatio(currentImage, bounds)

        Dim p As New Pen(Brushes.White, 4)
        g.FillRectangle(Brushes.Black, overallBounds)
        g.DrawImage(currentImage, bounds)
        RenderCaption(g, overallBounds, (idx + 1).ToString())
        g.DrawRectangle(p, overallBounds)
    End Sub

    ''' <summary>
    ''' Render text over a Graphics object
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="bounds"></param>
    ''' <param name="caption"></param>
    Private Sub RenderCaption(ByVal g As Graphics, ByVal bounds As Rectangle, ByVal caption As String)
        Dim captionFont As New Font(FontFamily.GenericSansSerif, bounds.Height / 4)
        Dim layoutRect As New Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height)
        Dim path As New GraphicsPath()
        path.AddString(caption, captionFont.FontFamily, CInt(captionFont.Style), _
            CSng(captionFont.Height), layoutRect, StringFormat.GenericDefault)

        Dim p As New Pen(Brushes.Black, 5)
        g.DrawPath(p, path)
        g.FillPath(Brushes.White, path)
    End Sub

#End Region

#Region "General helper methods"

    ''' <summary>
    ''' Initializes the application by reading settings, calculating the composite
    ''' Desktop, updating the preview, and setting up the drop regions.
    ''' </summary>
    Private Sub Init()
        UpdateMonitorBounds()

        ' Initialize collections (should really be re-init'd when screens change...)
        wallpaperFilenames = New String(screens.Length - 1) {}
        wallpaperBitmaps = New Bitmap(screens.Length - 1) {}
        selectForms = New MonitorSelectForm(screens.Length - 1) {}

        ' If there are no saved filenames, or it was a different size last time, start over.
        ' Otherwise, copy the settings to the array
        If My.Settings.SelectedImages Is Nothing OrElse My.Settings.SelectedImages.Count <> screens.Length Then
            My.Settings.SelectedImages = New StringCollection()
            My.Settings.SelectedImages.AddRange(wallpaperFilenames)
        Else
            My.Settings.SelectedImages.CopyTo(wallpaperFilenames, 0)
        End If

        UpdatePreviewImage()

        ShowWallpaperSelectionForms()

        ' Detect screen configuration changes
        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf SystemEvents_DisplaySettingsChanged
    End Sub

    ''' <summary>
    ''' Creates a new Rectangle by translating an existing Rectangle based on a given Point
    ''' </summary>
    ''' <param name="r">The original rectangle</param>
    ''' <param name="p">The reference point (representing 0,0)</param>
    ''' <returns>The translated rectangle</returns>
    Public Function ZeroRectangle(ByVal r As Rectangle, ByVal p As Point) As Rectangle
        Return New Rectangle(r.X + p.X, r.Y + p.Y, r.Width + p.X, r.Height + p.Y)
    End Function

    ''' <summary>
    ''' Give a Bitmap and bounds, calculates new bounds to maintain the
    ''' Bitmap's aspect ratio.
    ''' </summary>
    ''' <param name="img">The Bitmap to fit</param>
    ''' <param name="bounds">The bounding box in which it should fit.  This is modifed with the calculated bounds</param>
    ''' <returns>The multiplier (aspect ratio) used to fit the image into the given bounds</returns>
    Private Function MaintainAspectRatio(ByVal img As Bitmap, ByRef bounds As Rectangle) As Double
        Dim x As Integer = 0, y As Integer = 0
        Dim newRatio As Single, widthRatio As Single, heightRatio As Single

        widthRatio = CSng(bounds.Width) / CSng(img.Width)
        heightRatio = CSng(bounds.Height) / CSng(img.Height)

        If (heightRatio < widthRatio) Then
            newRatio = heightRatio
            x = CShort(((bounds.Width - (img.Width * newRatio)) / 2.0F))
        Else
            newRatio = widthRatio
            y = CShort(((bounds.Height - (img.Height * newRatio)) / 2.0F))
        End If

        Dim newWidth As Integer = CInt((img.Width * newRatio))
        Dim newHeight As Integer = CInt((img.Height * newRatio))

        bounds.X += x
        bounds.Y += y
        bounds.Width = newWidth
        bounds.Height = newHeight

        Return newRatio
    End Function

    ''' <summary>
    ''' Given a filename, retrieves a cached Bitmap or reads from disk.
    ''' If filename is not valid, a 1x1 dummy is returned (no error)
    ''' </summary>
    ''' <param name="filename">The name of the file to locate</param>
    ''' <param name="idx">Which screen this is for (used for caching)</param>
    ''' <returns></returns>
    Public Function BitmapFromFile(ByVal filename As String, ByVal idx As Integer) As Bitmap
        Dim currentImage As Bitmap = Nothing

        ' Check cache first (is it there *and* alive?)
        If wallpaperBitmapCache.ContainsKey(filename) Then
            ' If it's alive, use it.  Otherwise remove it.
            If wallpaperBitmapCache(filename).IsAlive Then
                currentImage = DirectCast(wallpaperBitmapCache(filename).Target, Bitmap)
            Else
                wallpaperBitmapCache.Remove(filename)
            End If
        End If

        ' If it's not in cache but it's on disk...
        If currentImage Is Nothing Then
            If File.Exists(filename) Then
                Using sr As Stream = File.OpenRead(filename)
                    ' Use FromStream, not FromFile to avoid an unnecessary lock
                    currentImage = DirectCast(Bitmap.FromStream(sr), Bitmap)
                    sr.Close()

                    ' Save to cache as WeakReference to reduce memory footprint
                    Dim imgRef As New WeakReference(currentImage)
                    wallpaperBitmapCache.Add(filename, imgRef)
                End Using
            Else
                currentImage = New Bitmap(1, 1)
            End If
        End If

        ' Update the bitmap for this screen and return it
        wallpaperBitmaps(idx) = currentImage
        Return currentImage
    End Function

#End Region

#Region "UI Helper methods"

    ''' <summary>
    ''' Shows the preview window and drop regions, and shows the form in the taskbar
    ''' </summary>
    Private Sub ShowUserInterface()
        Me.Visible = True
        Me.ShowInTaskbar = True
        ShowWallpaperSelectionForms()
    End Sub

    ''' <summary>
    ''' Hides the preview window and optionally drop regions and hides the
    ''' form from the taskbar
    ''' </summary>
    Private Sub HideUserInterface()
        Me.Visible = False
        Me.ShowInTaskbar = False

        ' Don't hide the drop regions if the checkbox is checked
        If showDropRegionsToolStripMenuItem.Checked = False Then
            HideWallpaperSelectionForms()
        End If
    End Sub

    ''' <summary>
    ''' Determine the overall bounds for all monitors together and create a single Bitmap
    ''' </summary>
    Private Sub UpdateMonitorBounds()
        screens = Screen.AllScreens

        overallBounds = New Rectangle()
        refPoint = New Point()

        For Each scr As Screen In screens
            overallBounds = BoundsUtilities.AddBounds(overallBounds, scr.Bounds)
        Next

        ' Screens to the left or above the primary screen cause 0,0 to be other
        ' than the top/left corner of the Bitmap
        If overallBounds.X < 0 Then
            refPoint.X = Math.Abs(overallBounds.X)
        End If
        If overallBounds.Y < 0 Then
            refPoint.Y = Math.Abs(overallBounds.Y)
        End If

        ' Cancels out the negative values from offset screens
        Dim correctedBounds As Rectangle = ZeroRectangle(overallBounds, refPoint)

        previewBitmap = New Bitmap(CInt(correctedBounds.Width / 4), CInt(correctedBounds.Height / 4))
        desktopBitmap = New Bitmap(correctedBounds.Width, correctedBounds.Height)
    End Sub

    ''' <summary>
    ''' Creates and/or shows the drag-and-drop controls on each screen
    ''' </summary>
    Private Sub ShowWallpaperSelectionForms()
        For idx As Integer = 0 To screens.Length - 1
            If selectForms(idx) Is Nothing Then
                selectForms(idx) = New MonitorSelectForm(screens(idx), idx)

                ' Add the event handler explicitly to share the same event handler with all selection dialos
                AddHandler selectForms(idx).WallpaperSelectedEvent, AddressOf MonitorSelectForm_WallpaperSelectedEvent
            End If

            selectForms(idx).Show()
        Next
    End Sub

    ''' <summary>
    ''' Hides drag-and-drop controls on each screen
    ''' </summary>
    Private Sub HideWallpaperSelectionForms()
        For idx As Integer = 0 To screens.Length - 1
            selectForms(idx).Hide()
        Next
    End Sub

#End Region

#Region "Event handlers"

    ''' <summary>
    ''' Event handled invoked when display settings are changed.  Updates
    ''' the composite images automatically.
    ''' </summary>
    Private Sub SystemEvents_DisplaySettingsChanged(ByVal sender As Object, ByVal e As EventArgs)
        UpdateMonitorBounds()
        UpdatePreviewImage()
        UpdateDesktopImage()
    End Sub

    ''' <summary>
    ''' Fires when one of the drop regions receives a file drop
    ''' </summary>
    Private Sub MonitorSelectForm_WallpaperSelectedEvent(ByVal sender As Object, ByVal e As WallpaperSelectedEventArgs)
        ' Update the corresponding wallpaper filename
        wallpaperFilenames(e.ScreenIndex) = e.WallpaperFilename

        ' Copy the array of filenames back to settings to save
        My.Settings.SelectedImages.Clear()
        My.Settings.SelectedImages.AddRange(wallpaperFilenames)
        My.Settings.Save()

        UpdatePreviewImage()
        UpdateDesktopImage()
    End Sub

    ''' <summary>
    ''' Shows the UI when the notify icon is double-clicked
    ''' </summary>
    Private Sub appNotifyIcon_MouseDoubleClick(ByVal sender As Object, ByVal e As MouseEventArgs) Handles appNotifyIcon.MouseDoubleClick
        ShowUserInterface()
    End Sub

    ''' <summary>
    ''' Event handler for the Show Drop Regions menu item.  Toggles state when clicked
    ''' </summary>
    Private Sub showDropRegionsToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles showDropRegionsToolStripMenuItem.Click
        Dim newCheckState As Boolean = Not showDropRegionsToolStripMenuItem.Checked

        showDropRegionsToolStripMenuItem.Checked = newCheckState
        My.Settings.ShowDropRegionsAlways = newCheckState

        If newCheckState Then
            ShowWallpaperSelectionForms()
        Else
            HideWallpaperSelectionForms()
        End If
    End Sub

    ''' <summary>
    ''' Shows the UI when Show Preview Window is clicked
    ''' </summary>
    Private Sub showPreviewWindowToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles showPreviewWindowToolStripMenuItem.Click
        ShowUserInterface()
    End Sub

    ''' <summary>
    ''' Closes the form (closing the application) upon clicking Quit
    ''' </summary>
    Private Sub quitToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles quitToolStripMenuItem.Click
        appClosing = True
        Me.Close()
    End Sub

    ''' <summary>
    ''' Saves state and hides the NotifyIcon when closing
    ''' </summary>
    Private Sub MainForm_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        If Not appClosing Then
            HideUserInterface()
            If Not minimizeMessageShown Then
                appNotifyIcon.ShowBalloonTip(5000, "Coding 4 Fun - MultiWall", "MultiWall is minimized to tray", ToolTipIcon.Info)
                minimizeMessageShown = True
            End If
            e.Cancel = True
        End If
    End Sub

    ''' <summary>
    ''' The form is truly closed (not hidden) and should save state and hide the NotifyIcon
    ''' </summary>
    Private Sub MainForm_FormClosed(ByVal sender As Object, ByVal e As FormClosedEventArgs) Handles Me.FormClosed
        appNotifyIcon.Visible = False
        My.Settings.Save()
    End Sub

#End Region


End Class
