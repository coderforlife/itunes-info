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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.Win32;

namespace iTunesInfo
{
    abstract class TrackDisplay : Form
    {
        /// <summary>The Windows Style to cause a window to be treated like a tool-window which causes it to not show up in the Alt-Tab list</summary>
        private const int WS_EX_TOOLWINDOW = 0x80;

        /// <summary>The controller that created / using this display</summary>
        protected readonly Controller controller;
        /// <summary>The event handler for when the display needs to be repositioned</summary>
        private readonly EventHandler respositionEvent;

        /// <summary>Create a new display that shows track information from iTunes</summary>
        /// <param name="controller">The controller that is using this display</param>
        protected TrackDisplay(Controller controller)
        {
            // Save the iTunes controller
            this.controller = controller;

            // Set general form properties
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "iTunes Track Information"; // should never be seen, but might as well set it
            this.Icon = iTunesInfo.Properties.Resources.icon; // should never be seen, but might as well set it

            // Setup the timer used to auto-close the form
            this.autoCloseTimer.Interval = Controller.DefaultVisibleTime; // the time is changed depending on the VisibleTime setting
            this.autoCloseTimer.Tick += FadeOutNow;

            // Setup the timer used to fade in and out of the form
            this.fadeTimer.Interval = 50; // always a very small value, below the estimated minimum of 60ms
            this.fadeTimer.Tick += FadeUpdate;

            // Setup the timer used to update playback information
            this.playbackTimer.Interval = 1000; // once a second
            this.playbackTimer.Tick += PlaybackUpdate;
            this.playbackTimer.Enabled = true;

            // Detect when the screen resolution changes
            // However this won't detect if they change the Windows Start bar or add/remove/change other application desktop bars
            // The only way to do that is polling the WorkingArea, and that just seems overkill
            // Instead every time the visibility of the box changes we reposition
            SystemEvents.DisplaySettingsChanged += this.respositionEvent = this.RespositionEvent;
        }
        /// <summary>When disposing the form the reposition event needs to be removed from the DisplaySettingsChanged event</summary>
        /// <param name="disposing">True if the Dispose function was called</param>
        protected override void Dispose(bool disposing)
        {
            SystemEvents.DisplaySettingsChanged -= this.respositionEvent;
            base.Dispose(disposing);
        }
        /// <summary>Add the WS_EX_TOOLWINDOW flag to the CreateParams to cause the window to not be in the Alt-Tab list</summary>
        protected override CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= WS_EX_TOOLWINDOW; return cp; } }

        #region Rebuilding and Painting
        /// <summary>A flag indicating the display needs to call the Rebuild function before the next draw</summary>
        protected bool needs_rebuild = true;
        /// <summary>Get a flag indicating the if the display is in a state that requires being rebuilt</summary>
        public bool NeedsRebuilding { get { return needs_rebuild; } }
        /// <summary>When a value changes, mark the display for rebuilding</summary>
        protected void OnValueChanged() { needs_rebuild = true; }
        /// <summary>Fire the OnValueChanged event</summary>
        protected void FireOnValueChanged() { if (this.InvokeRequired) this.Invoke(new Delegates.Action(this.OnValueChanged)); else this.OnValueChanged(); }
        /// <summary>When being invalidated, rebuild if necessary</summary>
        /// <param name="e">The invalidation event arguments</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            if (this.needs_rebuild) { this.Rebuild(); }
            base.OnInvalidated(e);
        }

        /// <summary>A set of information to draw a single line in the display</summary>
        protected abstract class LineInfo
        {
            /// <summary>The box that this line information takes up</summary>
            public Rectangle Box;
            /// <summary>Draw the glow around the content of a line</summary>
            /// <param name="g">The graphics to draw on</param>
            /// <param name="d">The display being draw on</param>
            public abstract void DrawGlow(Graphics g, GlassTrackDisplay d);
            /// <summary>Draw the content of a line</summary>
            /// <param name="g">The graphics to draw on</param>
            /// <param name="d">The display being draw on</param>
            public abstract void Draw(Graphics g, TrackDisplay d);
            /// <summary>Get the width of the line</summary>
            public int Width { get { return this.Box.Width; } }
            /// <summary>Get the height of the line</summary>
            public int Height { get { return this.Box.Height; } }
        }
        /// <summary>A line information for a string</summary>
        protected class StrLineInfo : LineInfo
        {
            /// <summary>The format to use to draw string, must be based on GenericTypographic so that it matches the metrics of TextRenderer.MeasureText</summary>
            public static StringFormat Format = new StringFormat(StringFormat.GenericTypographic);
            static StrLineInfo()
            {
                // Add some flags to the format
                Format.FormatFlags |= StringFormatFlags.NoWrap;
                Format.Trimming = StringTrimming.EllipsisWord;
            }

            /// <summary>The string to be drawn</summary>
            public String Str;
            /// <summary>Create a LineInfo based on a string</summary>
            /// <param name="img">The string to use</param>
            /// <param name="X">The X position</param>
            /// <param name="Y">The Y position</param>
            /// <param name="d">The display that this is for</param>
            public StrLineInfo(string str, int X, int Y, TrackDisplay d)
            {
                this.Str = str;
                Size sz = TextRenderer.MeasureText(str, d.Font);
                this.Box = new Rectangle(X, Y, sz.Width > d.max_content_width ? d.max_content_width : sz.Width, sz.Height);
            }
            /// <summary>Does nothing (glow must be draw with text)</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void DrawGlow(Graphics g, GlassTrackDisplay d) { }
            /// <summary>Draw the text</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void Draw(Graphics g, TrackDisplay d)
            {
                GlassTrackDisplay gd = d as GlassTrackDisplay;
                if (gd != null)
                    Glass.DrawTextGlow(g, this.Str, d.Font, d.ForeColor, this.Box, gd.GlowSize, d.inside_margin);
                else
                    g.DrawString(this.Str, d.Font, d.textcolor, this.Box, Format);
            }
        }
        /// <summary>A line information for an image</summary>
        protected class ImgLineInfo : LineInfo
        {
            /// <summary>The image to be drawn</summary>
            public Bitmap Image = null;
            /// <summary>The glow image to be drawn</summary>
            public Bitmap Glow = null;
            /// <summary>The box that that glow fits in</summary>
            public Rectangle GlowBox;
            /// <summary>Create a LineInfo based on an image</summary>
            /// <param name="img">The image to use</param>
            /// <param name="X">The X position</param>
            /// <param name="Y">The Y position</param>
            /// <param name="d">The display that this is for</param>
            public ImgLineInfo(Bitmap img, int X, int Y, TrackDisplay d)
            {
                // The image is scaled to fit the maximum content width
                this.Image = (img.Width > d.max_content_width) ? new Bitmap(img, d.max_content_width, d.max_content_width * img.Height / img.Width) : this.Image = img;
                this.Box = new Rectangle(X, Y, this.Image.Width, this.Image.Height);
            }
            /// <summary>Draw the glow behind the image</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void DrawGlow(Graphics g, GlassTrackDisplay d)
            {
                if (this.Glow == null)
                {
                    // Generate the glow image
                    this.Glow = Glass.CreateImageGlow(this.Image, d.GlowSize);
                    this.GlowBox = Glass.GetGlowBox(this.Box, d.GlowSize);
                }
                g.DrawImage(this.Glow, this.GlowBox);
            }
            /// <summary>Draw the image</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void Draw(Graphics g, TrackDisplay d) { g.DrawImage(this.Image, this.Box); }
        }
        /// <summary>A line information for a playback bar</summary>
        protected class PlaybackBarLineInfo : LineInfo
        {
            /// <summary>The rectangle that needs to be invalidated on the main form to cause the playback bar to be redrawn</summary>
            public Rectangle InvalidationBox;
            /// <summary>The object that is polled to get the current value for the playback bar</summary>
            public ChangingValue Val;
            /// <summary>The glow image to be drawn</summary>
            public Bitmap Glow;
            /// <summary>The box that that glow fits in</summary>
            public Rectangle GlowBox;
            /// <summary>Create a LineInfo based that uses a playback bar</summary>
            /// <param name="img">The changing value object that is polled to get the current value</param>
            /// <param name="X">The X position</param>
            /// <param name="Y">The Y position</param>
            /// <param name="d">The display that this is for</param>
            public PlaybackBarLineInfo(ChangingValue val, int X, int Y, TrackDisplay d)
            {
                this.Val = val;
                this.Box = new Rectangle(X, Y, PlaybackBar.MinWidth, PlaybackBar.Height); // a dummy width, adjusts later to be the actual width
                this.InvalidationBox = this.Box;
                if (d is GlassTrackDisplay) // the glass display needs an additional BorderSize offset here
                    this.InvalidationBox.Offset(0, ((GlassTrackDisplay)d).BorderSize);
                d.playbacks.Add(this);
            }
            /// <summary>Prepares for drawing (for either DrawGlow or Draw) by making sure the width is correct</summary>
            /// <param name="d">The display being draw on</param>
            private void PrepDraw(TrackDisplay d)
            {
                int w = d.content_width;
                if (w != this.Box.Width)
                {
                    // Need to update the width
                    this.Box.Width = w;
                    this.InvalidationBox.Width = w;
                    this.Glow = null; // causes the glow image to be re-generated as needed
                }
            }
            /// <summary>Draw the glow behind the playback bar</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void DrawGlow(Graphics g, GlassTrackDisplay d) {
                this.PrepDraw(d);
                if (this.Glow == null)
                {
                    // Generate the glow image (using the cache if possible)
                    this.Glow = Glass.GetImageGlow("PlaybackBarValue" + this.Box.Width, PlaybackBar.Create(this.Box.Width, 0), d.GlowSize);
                    this.GlowBox = Glass.GetGlowBox(this.Box, d.GlowSize);
                }
                g.DrawImage(this.Glow, this.GlowBox);
            }
            /// <summary>Draw the playback bar, which is generated from the current changing value</summary>
            /// <param name="g">The graphics to draw onto</param>
            /// <param name="d">The display being draw on</param>
            public override void Draw(Graphics g, TrackDisplay d) { this.PrepDraw(d); g.DrawImage(PlaybackBar.Create(this.Box.Width, Val.Value(d.controller)), this.Box); }
        }

        /// <summary>The timer that updates playback bars</summary>
        private Timer playbackTimer = new Timer();

        /// <summary>The list of playback bars that are being displayed</summary>
        private List<PlaybackBarLineInfo> playbacks = new List<PlaybackBarLineInfo>(4);
        
        /// <summary>When the playback bars need to be updated</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void PlaybackUpdate(object sender, EventArgs e)
        {
            if (this.Visible && this.playbacks.Count > 0)
                foreach (PlaybackBarLineInfo pb in this.playbacks)
                    this.Invalidate(pb.InvalidationBox); // force it to be redrawn
        }

        /// <summary>The current width of the content</summary>
        protected int content_width = 0;

        /// <summary>The set of lines that compose the actually drawn content</summary>
        private LineInfo[] lines;

        /// <summary>Rebuild the display by rebuild the list of LineInfos based on the content</summary>
        /// <remarks>This needs to be called whenever the LineInfos need to be updated, including if there are changes in their rectangles!</remarks>
        private void Rebuild()
        {
            // Setup variables
            string[] lines = this.content.Text.Split('\n');
            int count = lines.Length;
            this.lines = new LineInfo[count];
            this.playbacks.Clear();
            this.content_width = 0;

            // The y pos starts off as the inside_margin
            int y = this.inside_margin;
            // Go through each line
            for (int i = 0; i < count; ++i)
            {
                // Get the LineInfo object
                LineInfo l;
                Bitmap img;
                ChangingValue val;
                if ((img = this.content.GetImage(lines[i])) != null)
                    // An image line
                    l = new ImgLineInfo(img, this.inside_margin, y, this);
                else if ((val = this.content.GetValue(lines[i])) != null)
                    // A playback bar line
                    l = new PlaybackBarLineInfo(val, this.inside_margin, y, this);
                else
                    // A string line
                    l = new StrLineInfo(lines[i], this.inside_margin, y, this);
                this.lines[i] = l;

                // Update the y position and the content width
                y += l.Height + this.line_spacing;
                if (l.Width > this.content_width)
                    this.content_width = l.Width;
            }

            // Done calculating; set the size
            Size extra = ExtraSize();
            this.Size = new Size(this.content_width + 2 * this.inside_margin + extra.Width, y - this.line_spacing + this.inside_margin + extra.Height);
            this.needs_rebuild = false;
        }

        /// <summary>Gets any extra size that must be added to the "Size" property to be resized properly</summary>
        /// <returns>Default implementation is just [0, 0] size</returns>
        protected virtual Size ExtraSize() { return new Size(0, 0); }

        /// <summary>Draw the display, completely custom</summary>
        /// <param name="e">The paint event arguments</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Set graphics to be high quality
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Rebuild, if needed
            if (this.needs_rebuild) { this.Rebuild(); }

            // Prepare the display for being drawn on
            this.PrepareForPainting(g);

            // Draw the glow for all lines
            GlassTrackDisplay gd = this as GlassTrackDisplay;
            if (gd != null)
                foreach (LineInfo l in this.lines)
                    if (g.VisibleClipBounds.IntersectsWith(l.Box))
                        l.DrawGlow(g, gd);

            // Draw all lines
            foreach (LineInfo l in this.lines)
                if (g.VisibleClipBounds.IntersectsWith(l.Box))
                    l.Draw(g, this);
        }

        /// <summary>Prepare the form to be drawn on; this is different for every type of display and is implemented there</summary>
        /// <param name="g">The form graphics</param>
        protected abstract void PrepareForPainting(Graphics g);

        #endregion

        #region Properties
        /// <summary>The brush to draw text with</summary>
        SolidBrush textcolor = new SolidBrush(Controller.DefaultForeColor);
        /// <summary>When the fore-color changes update the textcolor brush and cause the display to rebuild and repaint</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnForeColorChanged(EventArgs e) { textcolor.Color = this.ForeColor; base.OnForeColorChanged(e); this.FireOnValueChanged(); }
        /// <summary>When the font changes cause the display to rebuild and repaint</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); this.FireOnValueChanged(); }

        /// <summary>The default content of the display, used when there is nothing else to display</summary>
        public static readonly Content DefaultContent = new Content() { Text = "No track is playing" };
        /// <summary>The content of the display</summary>
        protected Content content = DefaultContent;
        /// <summary>Get or set the content of the display</summary>
        public Content Content { get { return content; } set { content = value; this.FireOnValueChanged(); } }

        /// <summary>The maximum width of the display</summary>
        protected int max_width = Controller.DefaultMaxWidth;
        /// <summary>The maximum width of the content in the display</summary>
        protected int max_content_width = Controller.DefaultMaxWidth - Controller.DefaultInsideMargin * 2;
        /// <summary>Get or set the maximum width of the display in pixels</summary>
        public int MaxWidth { get { return max_width; } set { if (this.max_width != value) { max_width = value; max_content_width = max_width - inside_margin * 2; this.FireOnValueChanged(); } } }

        /// <summary>The line spacing in the display</summary>
        protected int line_spacing = Controller.DefaultLineSpacing;
        /// <summary>Get or set the line spacing of the display, in pixels; this is the number of extra pixels to place between lines of the display</summary>
        public int LineSpacing { get { return line_spacing; } set { if (this.line_spacing != value) { line_spacing = value; this.FireOnValueChanged(); } } }

        /// <summary>The inside margin of the display</summary>
        protected int inside_margin = Controller.DefaultInsideMargin;
        /// <summary>Get or set the inside margin of the display, in pixels this is the distance of the content from the edge of the form</summary>
        public int InsideMargin { get { return inside_margin; } set { if (this.inside_margin != value) { inside_margin = value; max_content_width = max_width - inside_margin * 2; this.FireOnValueChanged(); } } }

        /// <summary>The outside margin of the display</summary>
        protected int outside_margin = Controller.DefaultOutsideMargin;
        /// <summary>Get or set the outside margin of the display, in pixels; this is the distance the display is placed from the edge of the screen or taskbar</summary>
        public int OutsideMargin { get { return outside_margin; } set { if (this.outside_margin != value) { outside_margin = value; Reposition(); } } }

        /// <summary>The maximum opacity to show the form at</summary>
        protected double max_opacity = Controller.DefaultMaxOpacity;
        /// <summary>Get or set the maximum opacity to display the form at</summary>
        public double MaxOpacity { get { return max_opacity; } set { if (this.max_opacity != value) { max_opacity = value; if (max_opacity > this.Opacity) { this.Opacity = max_opacity; } } } }

        /// <summary>The total number of ticks to run fade animations for</summary>
        protected long fadeTotalTime = Controller.DefaultFadeTime * 10000L;
        /// <summary>Get or set the time to run fade animations, in milliseconds</summary>
        public int FadeTime { get { return (int)(this.fadeTotalTime / 10000); } set { this.fadeTotalTime = value * 10000L; } }

        /// <summary>The timer used to auto-close the display after a set amount of time after becoming fully visible</summary>
        protected Timer autoCloseTimer = new Timer();
        /// <summary>Get or set the amount of time that the display remains fully visible before fading out again, in milliseconds</summary>
        public int VisibleTime { get { return autoCloseTimer.Interval; } set { this.autoCloseTimer.Interval = value; } }

        /// <summary>The position on the desktop where the display is located</summary>
        protected DesktopPos pos = Controller.DefaultDesktopPosition;
        /// <summary>Get or set the position on the desktop where display is located</summary>
        public DesktopPos DesktopPosition { get { return pos; } set { if (this.pos != value) { this.pos = value; this.Reposition(); } } }
        #endregion

        #region Visibility, Opacity, and Positioning
        /// <summary>Set the opacity of the window taking into account the MaxOpacity setting</summary>
        /// <param name="o">The new opacity, a value from 0 to 1, that will be scaled with the MaxOpacity setting</param>
        protected void SetOpacity(double o) { this.Opacity = max_opacity * o.Clamp(0.0, 1.0); }

        /// <summary>Make the window completely visible now without fading in</summary>
        public void ShowNow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Delegates.Action(this.ShowNow));
            }
            else
            {
                this.fadeTimer.Stop();
                this.Opacity = this.max_opacity;
                this.ShowInactiveTopmost();
                this.startAutoCloseTimer();
            }
        }
        /// <summary>Hide the window completely now< without fading out/summary>
        public void HideNow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Delegates.Action(this.HideNow));
            }
            else
            {
                this.stopAutoCloseTimer();
                this.fadeTimer.Stop();
                this.SetOpacity(0.0);
                this.Hide();
            }
        }

        /// <summary>When the window becomes visible reposition it, when it becomes invisible make sure the timers are stopped</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                this.Reposition();
            }
            else
            {
                stopAutoCloseTimer();
                this.fadeTimer.Stop();
            }
        }
        /// <summary>When the window is resized reposition it as well</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnResize(EventArgs e) { base.OnResize(e); this.Reposition(); }
        /// <summary>Calls Reposition() but has an EventHandler signature</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void RespositionEvent(object sender, EventArgs e) { this.Reposition(); }
        /// <summary>Reposition the window using its DesktopPosition setting, translating the NearClock setting if necessary</summary>
        protected void Reposition() { this.Reposition((this.pos == DesktopPos.NearClock) ? Display.GetClockPosition() : this.pos); }
        /// <summary>Reposition the window using a given DesktopPosition</summary>
        /// <param name="p">The DesktopPosition to reposition to, cannot be NearClock</param>
        protected void Reposition(DesktopPos p)
        {
            switch (p)
            {
                case DesktopPos.UpperLeft:  DesktopLocation = new Point(outside_margin, outside_margin); break;
                case DesktopPos.UpperRight: DesktopLocation = new Point(SystemInformation.WorkingArea.Width - this.Width - outside_margin, outside_margin); break;
                case DesktopPos.LowerLeft:  DesktopLocation = new Point(outside_margin, SystemInformation.WorkingArea.Height - this.Height - outside_margin); break;
                case DesktopPos.LowerRight: DesktopLocation = new Point(SystemInformation.WorkingArea.Width - this.Width - outside_margin, SystemInformation.WorkingArea.Height - Height - outside_margin); break;
            }
        }
        #endregion

        #region Auto closing
        /// <summary>If true, the form will close automatically after being fully visible for a set amount of time (VisibleTime)</summary>
        protected bool allowedToAutoClose = true;
        /// <summary>Get and set whether or not the form will close automatically after being fully visible for a set amount of time</summary>
        public bool AllowedToAutoClose {
            get { return this.allowedToAutoClose; }
            set
            {
                if (this.allowedToAutoClose == value) return; // if value isn't changing don't do anything
                this.allowedToAutoClose = value;
                // Make sure to stop or start the auto-close timer based on the new value
                if (value)
                    this.startAutoCloseTimer();
                else
                    this.stopAutoCloseTimer();
            }
        }
        /// <summary>Starts the timer to auto-close the form</summary>
        protected void startAutoCloseTimer()
        {
            if (this.Visible && this.Opacity == this.max_opacity && this.allowedToAutoClose && !this.autoCloseTimer.Enabled)
            {
                // Only if fully visible and it is allowed and not already started
                if (this.InvokeRequired)
                    this.Invoke(new Delegates.Action(this.autoCloseTimer.Start));
                else
                    this.autoCloseTimer.Start();
            }
        }
        /// <summary>Stop the timer to auto-close the form</summary>
        protected void stopAutoCloseTimer()
        {
            if (this.InvokeRequired)
                this.Invoke(new Delegates.Action(this.autoCloseTimer.Stop));
            else
                this.autoCloseTimer.Stop();
        }
        #endregion

        #region Fading
        /// <summary>The timer that handles the fading animation</summary>
        protected Timer fadeTimer = new Timer();
        /// <summary>The direction of the fading process</summary>
        protected bool fadingIn = false;
        /// <summary>The time, in ticks, that the fade started</summary>
        protected long fadeStartTime = 0;

        /// <summary>Start the fading process, assuming that fadingIn, fadeStartTime, and fadeTotalTime already set</summary>
        protected void StartFading()
        {
            if (this.InvokeRequired)
            {
                // Make sure we are starting fading on the right thread
                this.Invoke(new Delegates.Action(this.StartFading));
            }
            else
            {
                // Start the fade updater and make sure the form is properly visible
                this.fadeTimer.Start();
                this.SetOpacity(this.fadingIn ? 0.0 : 1.0);
                this.ShowInactiveTopmost();
            }
        }
        /// <summary>The function to update the fade animation called from a rapidly repeating timer</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void FadeUpdate(object sender, EventArgs e)
        {
            long elapsed = DateTime.Now.Ticks - this.fadeStartTime;
            if (elapsed < this.fadeTotalTime)
            {
                // Update the opacity
                double perc = (double)elapsed / this.fadeTotalTime;
                this.SetOpacity(this.fadingIn ? perc : (1.0 - perc));
            }
            else
            {
                // Hit the end of the fading process
                this.fadeTimer.Stop(); // stop the timer
                if (this.fadingIn)
                {
                    // Now completely visible
                    this.SetOpacity(1.0);
                    this.startAutoCloseTimer();
                }
                else
                {
                    // Now hidden
                    this.SetOpacity(0.0);
                    this.stopAutoCloseTimer();
                    this.Hide();
                }
            }
        }

        /// <summary>Fade in the form now</summary>
        public void FadeIn()
        {
            long now = DateTime.Now.Ticks;
            if (this.Visible)
            {
                if (this.fadeTimer.Enabled)
                {
                    if (!this.fadingIn)
                    {
                        // Fading out, switch directions
                        long elapsed = now - this.fadeStartTime;
                        double perc = (double)elapsed / this.fadeTotalTime;
                        perc = 1.0 - perc.Clamp(0.0, 1.0);
                        this.fadingIn = true;
                        this.fadeStartTime = now - (long)(perc * this.fadeTotalTime);
                    }
                }
            }
            else
            {
                // Start fading in from 0 opacity
                this.fadingIn = true;
                this.fadeStartTime = now;
                this.StartFading();
            }
        }

        /// <summary>Fade out the form now</summary>
        public void FadeOut()
        {
            if (this.Visible)
            {
                // Visible, which means we can fade out 
                long now = DateTime.Now.Ticks;
                if (this.fadeTimer.Enabled)
                {
                    if (this.fadingIn)
                    {
                        // Fading in, switch directions
                        long elapsed = now - this.fadeStartTime;
                        double perc = (double)elapsed / this.fadeTotalTime;
                        perc = 1.0 - perc.Clamp(0.0, 1.0);
                        this.fadingIn = false;
                        this.fadeStartTime = now - (long)(perc * this.fadeTotalTime);
                    }
                }
                else
                {
                    // Start fading out from max opacity
                    this.fadingIn = false;
                    this.fadeStartTime = now;
                    this.StartFading();
                }
            }
        }

        /// <summary>Fade out the form now, as an event handler</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void FadeOutNow(object sender, EventArgs e)
        {
            this.stopAutoCloseTimer();
            this.FadeOut();
        }
        #endregion
    }
}
