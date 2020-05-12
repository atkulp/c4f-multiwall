using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.IO;

namespace MultiWall
{
    public enum WallpaperStyle : int
    {
        Tiled, Centered, Stretched
    }


    public class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(
            int uAction, int uParam, string lpvParam, int fuWinIni);

        public static string SaveWallpaper(Bitmap bmp)
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string path = Path.Combine(root, "MultiWallImage.bmp");

            bmp.Save(path, ImageFormat.Bmp);

            return path;
        }

        public static void SetWallpaper(string filename, WallpaperStyle style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);

            switch (style)
            {
                case WallpaperStyle.Stretched:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Centered:
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Tiled:
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}


