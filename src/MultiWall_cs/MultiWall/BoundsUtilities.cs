using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MultiWall
{
    class BoundsUtilities
    {
        public static Rectangle AddBounds(Rectangle sourceBounds, Rectangle newBounds)
        {
            if (newBounds.Right > sourceBounds.Right)
                sourceBounds.Width += (newBounds.Right - sourceBounds.Width);

            if (newBounds.Bottom > sourceBounds.Bottom)
                sourceBounds.Height += (newBounds.Bottom - sourceBounds.Height);

            if (newBounds.Left < sourceBounds.Left)
            {
                sourceBounds.X = newBounds.X;
            }

            if (newBounds.Top < sourceBounds.Top)
            {
                sourceBounds.Y = newBounds.Y;
            }

            return sourceBounds;
        }
    }
}
