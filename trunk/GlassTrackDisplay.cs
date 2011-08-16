//  iTunesInfo: Displays iTunes information and uses system keyboard shortcuts to control iTunes
//  Copyright (C) 2011  Jeffrey Bush <jeff@coderforlife.com>
//
//  This file is part of iTunesInfo
//
//  iTunesInfo is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  iTunesInfo is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with iTunesInfo. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>An iTunesTrackDisplay that uses glass for the entire window and draws with a glow</summary>
    class GlassTrackDisplay : TrackDisplay
    {
        /// <summary>Create a new glass iTunesTrackDisplay that uses </summary>
        /// <param name="controller">The iTunesController using this display</param>
        public GlassTrackDisplay(Controller controller) : base(controller)
        {
            // We need to use something with a border otherwise glass has issues
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // Make the caption text and buttons invisible
            Glass.MakeCaptionInvisible(this);
        }

        /// <summary>Whenever the handle is created the glass effect needs to be re-applied</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            Glass.ForEntireForm(this);
            base.OnHandleCreated(e);
        }

        #region Make titlebar have no height

        /// <summary>Sent when the size and position of a window's client area must be calculated, allowing an application can control the content of the window's client area when the size or position of the window changes</summary>
        /// 
        /// <remarks>http://msdn.microsoft.com/library/ms632634.aspx</remarks>
        private const int WM_NCCALCSIZE = 0x0083;

        /// <summary>
        /// This value indicates that, upon return from WM_NCCALCSIZE, the rectangles specified by the rgrc[1] and rgrc[2] members of the NCCALCSIZE_PARAMS structure contain valid destination and source area rectangles respectively. Both rectangles are in parent-relative or screen-relative coordinates. This flag cannot be combined with any other flags.
        /// This return value allows an application to implement more elaborate client-area preservation strategies, such as centering or preserving a subset of the client area.
        /// </summary>
        /// <remarks>http://msdn.microsoft.com/library/ms632634.aspx</remarks>
        private static readonly IntPtr WVR_VALIDRECTS = new IntPtr(0x0400);

        /// <summary>Contains information that an application can use while processing the WM_NCCALCSIZE message to calculate the size, position, and valid contents of the client area of a window</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms632606.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct NCCALCSIZE_PARAMS
        {
            /// <summary>
            /// Upon receipt of message: the new coordinates of a window that has been moved or resized
            /// When returning:          the coordinates of the new client rectangle resulting from the move or resize
            /// </summary>
            public RECT rect0;
            /// <summary>
            /// Upon receipt of message: the coordinates of the window before it was moved or resized 
            /// When returning:          the valid destination rectangle
            /// </summary>
            public RECT rect1;
            /// <summary>
            /// Upon receipt of message: the coordinates of the window's client area before the window was moved or resized
            /// When returning:          the valid source rectangle
            /// </summary>
            public RECT rect2; 

            /// <summary>
            /// A pointer to a WINDOWPOS (http://msdn.microsoft.com/library/ms632612.aspx) structure that contains the size and position values specified in the operation that moved or resized the window.
            /// </summary>
            public IntPtr lppos;
        }

        /// <summary>True if the borderSize and captionHeight variables have been properly set</summary>
        private bool haveMetrics = false;
        /// <summary>The size of the borders on the left, right, bottom, and top of the window, all assumed to be the same</summary>
        private int borderSize = 0;
        // /// <summary>The height of the caption bar</summary>
        //private int captionHeight;

        /// <summary>Get the size of the border on each side of the window</summary>
        public int BorderSize { get { return this.borderSize; } }

        /// <summary>Processes the WM_NCCALCSIZE Windows message and passes other messages to the base class</summary>
        /// <param name="m">The Windows message to process</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCCALCSIZE && m.WParam != IntPtr.Zero && m.Result == IntPtr.Zero)
            {
                // wParam != 0 indicates this message needs processing
                // lParam is a NCCALCSIZE_PARAMS structure
                // Result == 0 means this message hasn't already been processed
                NCCALCSIZE_PARAMS nc = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(NCCALCSIZE_PARAMS));

                if (!this.haveMetrics)
                {
                    // Get the size of the border, assuming all borders are the same width and that it doesn't change
                    this.borderSize = nc.rect2.left - nc.rect1.left;
                    //this.captionHeight = nc.rect2.top - nc.rect1.top - this.borderSize;
                    this.haveMetrics = true;
                }

                // The new client rectangle is the entire new window without the border
                //nc.rect0.top += this.border_size; // for some reason if this number is any larger then an entire title bar is drawn...
                nc.rect0.bottom -= this.borderSize;
                nc.rect0.right -= this.borderSize;
                nc.rect0.left += this.borderSize;

                // The destination rectangle is the entire new client rectangle
                nc.rect1 = nc.rect0;

                // The source rectangle is the entire old client rectangle
                //nc.rect2 = nc.rect2

                // Copy the modified data back into the lParam, set the result, and return
                Marshal.StructureToPtr(nc, m.LParam, false);
                m.Result = WVR_VALIDRECTS;
                return;
            }
            base.WndProc(ref m);
        }
        #endregion

        /// <summary>The size of the glow effect behind items that are drawn on the glass</summary>
        protected int glow_size = 8;
        /// <summary>Get or set the size of the glow effect behind items that are drawn on the glass, setting causes the display to rebuild</summary>
        public int GlowSize { get { return glow_size; } set { glow_size = value; this.FireOnValueChanged(); } }

        /// <summary>Get the amount of extra size required for this type of track display</summary>
        /// <returns>Returns 2 * BorderSize for each dimension</returns>
        protected override Size ExtraSize() { return new Size(2 * this.borderSize, 2 * this.borderSize); }

        /// <summary>Prepares for painting the display, which for a glass display is adjusting for the caption bar and forcing the glass to be drawn</summary>
        /// <param name="g">The graphics to draw to</param>
        protected override void PrepareForPainting(Graphics g)
        {
            // Need to take into account that the top of the client rectangle can't be moved down properly
            g.TranslateTransform(0, this.borderSize);

            // Background
            Glass.Show(g, this.ClientRectangle);
        }
    }
}
