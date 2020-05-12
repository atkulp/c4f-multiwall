using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MultiWall
{
    public partial class MonitorSelectForm : Form
    {
        public event WallpaperSelectedEventHandler WallpaperSelectedEvent;
        private Screen _myScreen;
        private string _wallpaperFilename;
        private int _screenIndex;

        public MonitorSelectForm(Screen whichScreen, int screenIndex)
        {
            InitializeComponent();

            _myScreen = whichScreen;
            _screenIndex = screenIndex;
        }

        public Screen MyScreen
        {
            get
            {
                return _myScreen;
            }
        }

        public string WallpaperFilename
        {
            get
            {
                return _wallpaperFilename;
            }
        }

        public int ScreenIndex
        {
            get
            {
                return _screenIndex;
            }
        }

        private void CenterOnScreen()
        {
            Rectangle bounds = _myScreen.WorkingArea;
            this.Left = bounds.Left + ((bounds.Width - this.Width) / 2);
            this.Top = bounds.Top;
        }

        private void MonitorSelectForm_DragEnter(object sender, DragEventArgs e)
        {
            // Only accept file drops
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        private void MonitorSelectForm_DragDrop(object sender, DragEventArgs e)
        {
            // It's an array, even for a single file
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            _wallpaperFilename = files[0];
            RaiseWallpaperSelectedEvent(files[0]);
        }

        private void RaiseWallpaperSelectedEvent(string filename)
        {
            if (WallpaperSelectedEvent != null)
            {
                WallpaperSelectedEventArgs e = new WallpaperSelectedEventArgs();
                e.SelectedScreen = _myScreen;
                e.WallpaperFilename = filename;
                e.ScreenIndex = _screenIndex;

                WallpaperSelectedEvent(this, e);
            }
        }

        private void MonitorSelectForm_Shown(object sender, EventArgs e)
        {
            CenterOnScreen();
        }
    }

    public delegate void WallpaperSelectedEventHandler(object sender, WallpaperSelectedEventArgs e);
}