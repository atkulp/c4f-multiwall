Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.Win32
Imports System.IO

Public Enum WallpaperStyle As Integer
    Tiled
    Centered
    Stretched
End Enum


Public Class Wallpaper
    Const SPI_SETDESKWALLPAPER As Integer = 20
    Const SPIF_UPDATEINIFILE As Integer = 1
    Const SPIF_SENDWININICHANGE As Integer = 2

    <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
    Private Shared Function SystemParametersInfo(ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As String, ByVal fuWinIni As Integer) As Integer
    End Function

    Public Shared Function SaveWallpaper(ByVal bmp As Bitmap) As String
        Dim root As String = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        Dim path As String = System.IO.Path.Combine(root, "MultiWallImage.bmp")

        bmp.Save(path, ImageFormat.Bmp)

        Return path
    End Function

    Public Shared Sub SetWallpaper(ByVal filename As String, ByVal style As WallpaperStyle)
        Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey("Control Panel\Desktop", True)

        Select Case style
            Case WallpaperStyle.Stretched
                key.SetValue("WallpaperStyle", "2")
                key.SetValue("TileWallpaper", "0")
                Exit Select
            Case WallpaperStyle.Centered
                key.SetValue("WallpaperStyle", "1")
                key.SetValue("TileWallpaper", "0")
                Exit Select
            Case WallpaperStyle.Tiled
                key.SetValue("WallpaperStyle", "1")
                key.SetValue("TileWallpaper", "1")
                Exit Select
        End Select

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE Or SPIF_SENDWININICHANGE)
    End Sub
End Class
