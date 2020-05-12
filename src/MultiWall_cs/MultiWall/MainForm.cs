using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;

namespace MultiWall
{
    public partial class MainForm : Form
    {
        #region "Class Variables"

        bool appClosing = false, minimizeMessageShown = false;

        Bitmap previewBitmap = null, desktopBitmap = null;
        Rectangle overallBounds;
        
        // Where is 0,0 in the composite Desktop image?
        Point refPoint;

        Screen[] screens;
        MonitorSelectForm[] selectForms;
        string[] wallpaperFilenames;
        Dictionary<string, WeakReference> wallpaperBitmapCache = new Dictionary<string, WeakReference>();

        // This collection isn't actually used to retrieve the Bitmap, but
        // prevents the WeakReference from disappearing while the Bitmap is in use
        Bitmap[] wallpaperBitmaps;

        #endregion

        public MainForm()
        {
            InitializeComponent();

            Init();
        }

        #region "Desktop updating"

        /// <summary>
        /// Modify the preview window (wrapping images as used for Desktop)
        /// </summary>
        private void UpdateDesktopImage()
        {
            using (Graphics g = Graphics.FromImage(desktopBitmap))
            {
                for (int idx = 0; idx < screens.Length; idx++)
                {
                    AddImageToDesktop(g, wallpaperFilenames[idx], screens[idx].Bounds, idx);
                }
            }

            string fn = Wallpaper.SaveWallpaper(desktopBitmap);
            Wallpaper.SetWallpaper(fn, WallpaperStyle.Tiled);
        }
        
        /// <summary>
        /// Adds the given image to the bitmap, but wraps images as needed based on monitor configuration
        /// </summary>
        /// <param name="g">The Graphics of the composite Desktop </param>
        /// <param name="filename">The filename of the image to add</param>
        /// <param name="bounds">The bounds for this screen</param>
        public void AddImageToDesktop(Graphics g, string filename, Rectangle bounds, int idx)
        {
            // Clear the region and verify a selected filename
            g.FillRectangle(Brushes.Black, bounds);

            if( string.IsNullOrEmpty(filename) )
                return;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            Bitmap currentImage = BitmapFromFile(filename, idx);

            Rectangle sourceBounds = new Rectangle(0, 0, currentImage.Width, currentImage.Height);
            double ratio = MaintainAspectRatio(currentImage, ref bounds);

            // If image is drawn outside of viewable area, it must be "wrapped" around
            if (bounds.X < 0 || bounds.Y < 0 ||
                (bounds.X + bounds.Width > desktopBitmap.Width) || (bounds.Y + bounds.Height > desktopBitmap.Height))
            {
                // Draw the first half
                int x = bounds.X, y = bounds.Y;

                if (bounds.Y < 0)
                    y = desktopBitmap.Height + bounds.Y;
                else if (bounds.Y + bounds.Height > desktopBitmap.Height)
                    y = 0 - (desktopBitmap.Height - bounds.Height + bounds.Y);

                if (bounds.X < 0)
                    x = desktopBitmap.Width + bounds.X;
                else if (bounds.X + bounds.Width > desktopBitmap.Width)
                    x = 0 - (desktopBitmap.Width - bounds.Width - bounds.X);

                // Draw original coordinates
                g.DrawImage(currentImage, new Rectangle(x, y, bounds.Width, bounds.Height), new Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel);

                // Draw with corrected Y coordinate
                g.DrawImage(currentImage, new Rectangle(x, bounds.Y, bounds.Width, bounds.Height), new Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel);

                // Draw with corrected X coordinate
                g.DrawImage(currentImage, new Rectangle(bounds.X, y, bounds.Width, bounds.Height), new Rectangle(0, 0, currentImage.Width, currentImage.Height), GraphicsUnit.Pixel);
            }
            else
            {
                // Draw the image once, fully in the viewable area
                g.DrawImage(currentImage, bounds, sourceBounds, GraphicsUnit.Pixel);
            }
        }


        #endregion

        #region "Preview updating"

        /// <summary>
        /// Modify the preview window (non-wrapping images)
        /// </summary>
        private void UpdatePreviewImage()
        {
            using (Graphics g = Graphics.FromImage(previewBitmap))
            {
                for (int idx = 0; idx < screens.Length; idx++)
                {
                    AddImageToPreview(g, wallpaperFilenames[idx], screens[idx].Bounds, idx);
                }
            }

            pictureBox1.Image = previewBitmap;
        }
        /// <summary>
        /// Adds the given image to the preview image
        /// </summary>
        /// <param name="g"></param>
        /// <param name="filename"></param>
        /// <param name="bounds"></param>
        public void AddImageToPreview(Graphics g, string filename, Rectangle bounds, int idx)
        {
            // Clear the region and verify a selected filename
            g.FillRectangle(SystemBrushes.ButtonFace, bounds);

            if (string.IsNullOrEmpty(filename))
                return;

            // Load the image from file or cache
            Bitmap currentImage = BitmapFromFile(filename, idx);

            // Translate negative screens (to the left/above primary monitor)
            // into viewable preview area
            bounds.X += refPoint.X;
            bounds.Y += refPoint.Y;
            
            // Everything is quarter-sized for preview mode
            bounds.X /= 4;
            bounds.Y /= 4;
            bounds.Width /= 4;
            bounds.Height /= 4;

            // Copy bounds to have monitor vs. resized image dimensions
            Rectangle overallBounds = new Rectangle(bounds.Location, bounds.Size);
            double ratio = MaintainAspectRatio(currentImage, ref bounds);

            Pen p = new Pen(Brushes.White, 4);
            g.FillRectangle(Brushes.Black, overallBounds);
            g.DrawImage(currentImage, bounds);
            RenderCaption(g, overallBounds, (idx+1).ToString());
            g.DrawRectangle(p, overallBounds);
        }

        /// <summary>
        /// Render text over a Graphics object
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bounds"></param>
        /// <param name="caption"></param>
        private void RenderCaption(Graphics g, Rectangle bounds, string caption)
        {
            Font captionFont = new Font(FontFamily.GenericSansSerif, bounds.Height/4);
            Rectangle layoutRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            GraphicsPath path = new GraphicsPath();
            path.AddString(caption, captionFont.FontFamily,
                (int)captionFont.Style, (float)captionFont.Height,
                layoutRect, StringFormat.GenericDefault);
            
            Pen p = new Pen(Brushes.Black, 5);
            g.DrawPath(p, path);
            g.FillPath(Brushes.White, path);
        }

        #endregion

        #region "General helper methods"

        /// <summary>
        /// Initializes the application by reading settings, calculating the composite
        /// Desktop, updating the preview, and setting up the drop regions.
        /// </summary>
        private void Init()
        {
            UpdateMonitorBounds();

            // Initialize collections (should really be re-init'd when screens change...)
            wallpaperFilenames = new string[screens.Length];
            wallpaperBitmaps = new Bitmap[screens.Length];
            selectForms = new MonitorSelectForm[screens.Length];

            // If there are no saved filenames, or it was a different size last time, start over.
            // Otherwise, copy the settings to the array
            if (Properties.Settings.Default.SelectedImages == null ||
                Properties.Settings.Default.SelectedImages.Count != screens.Length)
            {
                Properties.Settings.Default.SelectedImages = new StringCollection();
                Properties.Settings.Default.SelectedImages.AddRange(wallpaperFilenames);
            }
            else
            {
                Properties.Settings.Default.SelectedImages.CopyTo(wallpaperFilenames, 0);
            }

            UpdatePreviewImage();

            ShowWallpaperSelectionForms();

            // Detect screen configuration changes
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
        }
        
        /// <summary>
        /// Creates a new Rectangle by translating an existing Rectangle based on a given Point
        /// </summary>
        /// <param name="r">The original rectangle</param>
        /// <param name="p">The reference point (representing 0,0)</param>
        /// <returns>The translated rectangle</returns>
        public Rectangle ZeroRectangle(Rectangle r, Point p)
        {
            return new Rectangle(
                r.X + p.X, r.Y + p.Y,
                r.Width + p.X, r.Height + p.Y);
        }

        /// <summary>
        /// Give a Bitmap and bounds, calculates new bounds to maintain the
        /// Bitmap's aspect ratio.
        /// </summary>
        /// <param name="img">The Bitmap to fit</param>
        /// <param name="bounds">The bounding box in which it should fit.  This is modifed with the calculated bounds</param>
        /// <returns>The multiplier (aspect ratio) used to fit the image into the given bounds</returns>
        private double MaintainAspectRatio(Bitmap img, ref Rectangle bounds)
        {
            int x = 0, y = 0;
            float newRatio, widthRatio, heightRatio;

            widthRatio = ((float)bounds.Width) / ((float)img.Width);
            heightRatio = ((float)bounds.Height) / ((float)img.Height);

            if ((heightRatio < widthRatio))
            {
                newRatio = heightRatio;
                x = (short)((bounds.Width - (img.Width * newRatio)) / 2f);
            }
            else
            {
                newRatio = widthRatio;
                y = (short)((bounds.Height - (img.Height * newRatio)) / 2f);
            }

            int newWidth = (int)(img.Width * newRatio);
            int newHeight = (int)(img.Height * newRatio);

            bounds.X += x;
            bounds.Y += y;
            bounds.Width = newWidth;
            bounds.Height = newHeight;

            return newRatio;
        }

        /// <summary>
        /// Given a filename, retrieves a cached Bitmap or reads from disk.
        /// If filename is not valid, a 1x1 dummy is returned (no error)
        /// </summary>
        /// <param name="filename">The name of the file to locate</param>
        /// <param name="idx">Which screen this is for (used for caching)</param>
        /// <returns></returns>
        public Bitmap BitmapFromFile(string filename, int idx)
        {
            Bitmap currentImage = null;

            // Check cache first (is it there *and* alive?)
            if (wallpaperBitmapCache.ContainsKey(filename))
            {
                // If it's alive, use it.  Otherwise remove it.
                if (wallpaperBitmapCache[filename].IsAlive)
                    currentImage = (Bitmap)wallpaperBitmapCache[filename].Target;
                else
                    wallpaperBitmapCache.Remove(filename);
            }

            // If it's not in cache but it's on disk...
            if (currentImage == null)
            {
                if (File.Exists(filename))
                {
                    using (Stream sr = File.OpenRead(filename))
                    {
                        // Use FromStream, not FromFile to avoid an unnecessary lock
                        currentImage = (Bitmap)Bitmap.FromStream(sr);
                        sr.Close();

                        // Save to cache as WeakReference to reduce memory footprint
                        WeakReference imgRef = new WeakReference(currentImage);
                        wallpaperBitmapCache.Add(filename, imgRef);
                    }
                }
                else
                    currentImage = new Bitmap(1, 1);
            }

            // Update the bitmap for this screen and return it
            wallpaperBitmaps[idx] = currentImage;
            return currentImage;
        }

        #endregion

        #region "UI Helper methods"

        /// <summary>
        /// Shows the preview window and drop regions, and shows the form in the taskbar
        /// </summary>
        private void ShowUserInterface()
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            ShowWallpaperSelectionForms();
        }

        /// <summary>
        /// Hides the preview window and optionally drop regions and hides the
        /// form from the taskbar
        /// </summary>
        private void HideUserInterface()
        {
            this.Visible = false;
            this.ShowInTaskbar = false;

            // Don't hide the drop regions if the checkbox is checked
            if (showDropRegionsToolStripMenuItem.Checked == false)
            {
                HideWallpaperSelectionForms();
            }
        }

        /// <summary>
        /// Determine the overall bounds for all monitors together and create a single Bitmap
        /// </summary>
        private void UpdateMonitorBounds()
        {
            screens = Screen.AllScreens;

            overallBounds = new Rectangle();
            refPoint = new Point();

            foreach (Screen scr in screens)
            {
                overallBounds = BoundsUtilities.AddBounds(overallBounds, scr.Bounds);
            }

            // Screens to the left or above the primary screen cause 0,0 to be other
            // than the top/left corner of the Bitmap
            if (overallBounds.X < 0) refPoint.X = Math.Abs(overallBounds.X);
            if (overallBounds.Y < 0) refPoint.Y = Math.Abs(overallBounds.Y);

            // Cancels out the negative values from offset screens
            Rectangle correctedBounds = ZeroRectangle(overallBounds, refPoint);

            previewBitmap = new Bitmap(correctedBounds.Width / 4, correctedBounds.Height / 4);
            desktopBitmap = new Bitmap(correctedBounds.Width, correctedBounds.Height);
        }

        /// <summary>
        /// Creates and/or shows the drag-and-drop controls on each screen
        /// </summary>
        private void ShowWallpaperSelectionForms()
        {
            for (int idx = 0; idx < screens.Length; idx++ )
            {
                if (selectForms[idx] == null)
                {
                    selectForms[idx] = new MonitorSelectForm(screens[idx], idx);
                    selectForms[idx].WallpaperSelectedEvent += new WallpaperSelectedEventHandler(MonitorSelectForm_WallpaperSelectedEvent);
                }

                selectForms[idx].Show();
            }
        }

        /// <summary>
        /// Hides drag-and-drop controls on each screen
        /// </summary>
        private void HideWallpaperSelectionForms()
        {
            for (int idx = 0; idx < screens.Length; idx++)
            {
                selectForms[idx].Hide();
            }
        }

        #endregion

        #region "Event handlers"

        /// <summary>
        /// Event handled invoked when display settings are changed.  Updates
        /// the composite images automatically.
        /// </summary>
        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateMonitorBounds();
            UpdatePreviewImage();
            UpdateDesktopImage();
        }

        /// <summary>
        /// Fires when one of the drop regions receives a file drop
        /// </summary>
        void MonitorSelectForm_WallpaperSelectedEvent(object sender, WallpaperSelectedEventArgs e)
        {
            // Update the corresponding wallpaper filename
            wallpaperFilenames[e.ScreenIndex] = e.WallpaperFilename;

            // Copy the array of filenames back to settings to save
            Properties.Settings.Default.SelectedImages.Clear();
            Properties.Settings.Default.SelectedImages.AddRange(wallpaperFilenames);
            Properties.Settings.Default.Save();

            UpdatePreviewImage();
            UpdateDesktopImage();
        }

        /// <summary>
        /// Shows the UI when the notify icon is double-clicked
        /// </summary>
        private void appNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowUserInterface();
        }

        /// <summary>
        /// Event handler for the Show Drop Regions menu item.  Toggles state when clicked
        /// </summary>
        private void showDropRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool newCheckState = !showDropRegionsToolStripMenuItem.Checked;

            showDropRegionsToolStripMenuItem.Checked = newCheckState;
            Properties.Settings.Default.ShowDropRegionsAlways = newCheckState;

            if (newCheckState)
            {
                ShowWallpaperSelectionForms();
            }
            else
            {
                HideWallpaperSelectionForms();
            }
        }

        /// <summary>
        /// Shows the UI when Show Preview Window is clicked
        /// </summary>
        private void showPreviewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowUserInterface();
        }

        /// <summary>
        /// Closes the form (closing the application) upon clicking Quit
        /// </summary>
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            appClosing = true;
            this.Close();
        }

        /// <summary>
        /// Saves state and hides the NotifyIcon when closing
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!appClosing)
            {
                HideUserInterface();
                if (!minimizeMessageShown)
                {
                    appNotifyIcon.ShowBalloonTip(5000, "Coding 4 Fun - MultiWall", "MultiWall is minimized to tray", ToolTipIcon.Info);
                    minimizeMessageShown = true;
                }
                e.Cancel = true;
            }
        }

        /// <summary>
        /// The form is truly closed (not hidden) and should save state and hide the NotifyIcon
        /// </summary>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            appNotifyIcon.Visible = false;
            Properties.Settings.Default.Save();
        }

        #endregion


    }
}