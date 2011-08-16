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
        private const int WS_EX_TOOLWINDOW = 0x80;

        protected readonly Controller controller;
        private readonly EventHandler respositionEvent;

        protected TrackDisplay(Controller controller)
        {
            this.controller = controller;

            this.Icon = iTunesInfo.Properties.Resources.icon;

            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.Text = "iTunes Information";
            this.StartPosition = FormStartPosition.Manual;

            this.autoCloseTimer.Interval = 5000;
            this.autoCloseTimer.Tick += FadeOutNow;

            this.fadeTimer.Interval = 50;
            this.fadeTimer.Tick += FadeUpdate;

            this.playbackTimer.Interval = 1000; // once a second
            this.playbackTimer.Tick += PlaybackUpdate;
            this.playbackTimer.Enabled = true;

            // Detect when the screen resolution changes
            // However this won't detect if they change the Windows Start bar or add/remove/change other application desktop bars
            // The only way to do that is polling the WorkingArea, and that just seems overkill
            // Instead every time the visibility of the box changes we reposition
            SystemEvents.DisplaySettingsChanged += this.respositionEvent = this.RespositionEvent;
        }
        protected override void Dispose(bool disposing)
        {
            SystemEvents.DisplaySettingsChanged -= this.respositionEvent;
            base.Dispose(disposing);
        }
        protected override CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.ExStyle |= WS_EX_TOOLWINDOW; return cp; } } // using WS_EX_TOOLWINDOW causes the window to not be in the Alt-Tab list

        #region Rebuilding and Painting
        protected bool needs_rebuild = true;
        public bool NeedsRebuilding { get { return needs_rebuild; } }
        protected void OnValueChanged() { needs_rebuild = true; }
        protected void FireOnValueChanged() { if (this.InvokeRequired) this.Invoke(new Delegates.Action(this.OnValueChanged)); else this.OnValueChanged(); }
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            if (this.needs_rebuild) { this.Rebuild(); }
            base.OnInvalidated(e);
        }

        protected abstract class LineInfo
        {
            public Rectangle Box;
            public abstract void DrawGlow(Graphics g, GlassTrackDisplay d);
            public abstract void Draw(Graphics g, TrackDisplay d);
            public int Width { get { return this.Box.Width; } }
            public int Height { get { return this.Box.Height; } }
        }
        protected class StrLineInfo : LineInfo
        {
            public static StringFormat Format = new StringFormat(StringFormat.GenericTypographic);
            static StrLineInfo()
            {
                Format.FormatFlags |= StringFormatFlags.NoWrap;
                Format.Trimming = StringTrimming.EllipsisWord;
            }

            public String Str;
            public StrLineInfo(string str, int X, int Y, TrackDisplay d)
            {
                this.Str = str;
                Size sz = TextRenderer.MeasureText(str, d.Font);
                this.Box = new Rectangle(X, Y, sz.Width > d.max_content_width ? d.max_content_width : sz.Width, sz.Height);
            }
            public override void DrawGlow(Graphics g, GlassTrackDisplay d) { /* Glow must be draw with text */ }
            public override void Draw(Graphics g, TrackDisplay d)
            {
                GlassTrackDisplay gd = d as GlassTrackDisplay;
                if (gd != null)
                    Glass.DrawTextGlow(g, this.Str, d.Font, d.ForeColor, this.Box, gd.GlowSize, d.inside_margin);
                else
                    g.DrawString(this.Str, d.Font, d.textcolor, this.Box, Format);
            }
        }
        protected class ImgLineInfo : LineInfo
        {
            public Bitmap Image, Glow = null;
            public Rectangle GlowBox;
            public ImgLineInfo(Bitmap img, int X, int Y, TrackDisplay d)
            {
                this.Image = (img.Width > d.max_content_width) ? new Bitmap(img, d.max_content_width, d.max_content_width * img.Height / img.Width) : this.Image = img;
                this.Box = new Rectangle(X, Y, this.Image.Width, this.Image.Height);
                if (d is GlassTrackDisplay)
                {
                    GlassTrackDisplay gd = (GlassTrackDisplay)d;
                    this.Glow = Glass.CreateImageGlow(this.Image, gd.GlowSize);
                    this.GlowBox = Glass.GetGlowBox(this.Box, gd.GlowSize);
                }
            }
            public override void DrawGlow(Graphics g, GlassTrackDisplay d) { g.DrawImage(this.Glow, this.GlowBox); }
            public override void Draw(Graphics g, TrackDisplay d) { g.DrawImage(this.Image, this.Box); }
        }
        protected class PlaybackBarLineInfo : LineInfo
        {
            public Rectangle InvalidationBox;
            public ChangingValue Val;
            public Bitmap Glow;
            public Rectangle GlowBox;
            public PlaybackBarLineInfo(ChangingValue val, int X, int Y, TrackDisplay d)
            {
                this.Val = val;
                this.Box = new Rectangle(X, Y, PlaybackBar.MinWidth, PlaybackBar.Height); // a dummy width, adjusts later to be the actual width
                this.InvalidationBox = this.Box;
                if (d is GlassTrackDisplay)
                {
                    GlassTrackDisplay gd = (GlassTrackDisplay)d;
                    this.InvalidationBox.Offset(0, gd.BorderSize);
                }
                d.playbacks.Add(this);
            }
            private void PrepDraw(TrackDisplay d)
            {
                int w = d.content_width;
                if (w != this.Box.Width)
                {
                    this.Box.Width = w;
                    this.InvalidationBox.Width = w;
                    this.Glow = null;
                }
            }
            public override void DrawGlow(Graphics g, GlassTrackDisplay d) {
                this.PrepDraw(d);
                if (this.Glow == null)
                {
                    GlassTrackDisplay gd = (GlassTrackDisplay)d;
                    this.Glow = Glass.GetImageGlow("PlaybackBarValue" + this.Box.Width, PlaybackBar.Create(this.Box.Width, 0), gd.GlowSize);
                    this.GlowBox = Glass.GetGlowBox(this.Box, gd.GlowSize);
                }
                g.DrawImage(this.Glow, this.GlowBox);
            }
            public override void Draw(Graphics g, TrackDisplay d) { this.PrepDraw(d); g.DrawImage(PlaybackBar.Create(this.Box.Width, Val.Value(d.controller)), this.Box); }
        }

        private Timer playbackTimer = new Timer();
        private List<PlaybackBarLineInfo> playbacks = new List<PlaybackBarLineInfo>(4);
        private void PlaybackUpdate(object sender, EventArgs e)
        {
            if (this.Visible && this.playbacks.Count > 0)
                foreach (PlaybackBarLineInfo pb in this.playbacks)
                    this.Invalidate(pb.InvalidationBox);
        }

        protected int content_width = 0;
        private LineInfo[] lines;
        private void Rebuild()
        {
            string[] lines = this.content.Text.Split('\n');
            int count = lines.Length;
            this.lines = new LineInfo[count];
            this.playbacks.Clear();

            int height = this.inside_margin;
            int width = 0;
            for (int i = 0; i < count; ++i)
            {
                LineInfo l;
                Bitmap img;
                ChangingValue val;
                if ((img = this.content.GetImage(lines[i])) != null)
                    l = new ImgLineInfo(img, this.inside_margin, height, this);
                else if ((val = this.content.GetValue(lines[i])) != null)
                    l = new PlaybackBarLineInfo(val, this.inside_margin, height, this);
                else
                    l = new StrLineInfo(lines[i], this.inside_margin, height, this);
                this.lines[i] = l;
                height += l.Height + this.line_spacing;
                int w = l.Width;
                if (w > width)
                    width = w;
            }

            Size extra = ExtraSize();
            this.Size = new Size(width + 2 * this.inside_margin + extra.Width, height - this.line_spacing + this.inside_margin + extra.Height);
            this.content_width = width;
            this.needs_rebuild = false;
        }
        protected virtual Size ExtraSize() { return new Size(0, 0); }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (this.needs_rebuild) { this.Rebuild(); }

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        protected abstract void PrepareForPainting(Graphics g);

        #endregion

        #region Properties
        SolidBrush textcolor = new SolidBrush(Color.Black);
        protected override void OnForeColorChanged(EventArgs e) { textcolor.Color = this.ForeColor; base.OnForeColorChanged(e); this.FireOnValueChanged(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); this.FireOnValueChanged(); }

        public static readonly Content DefaultContent = new Content() { Text = "No track is playing" };
        protected Content content = DefaultContent;
        public Content Content { get { return content; } set { content = value; this.FireOnValueChanged(); } }

        protected Image img = null;
        public Image Image { get { return img; } set { img = value; this.FireOnValueChanged(); } }

        protected int max_width = 250;
        protected int max_content_width = 238;
        public int MaxWidth { get { return max_width; } set { max_width = value; max_content_width = max_width - inside_margin * 2; this.FireOnValueChanged(); } }

        protected int line_spacing = 3;
        public int LineSpacing { get { return line_spacing; } set { line_spacing = value; this.FireOnValueChanged(); } }

        public override Font Font { get { return base.Font; } set { base.Font = value; this.FireOnValueChanged(); } }

        protected int inside_margin = 6;
        public int InsideMargin { get { return inside_margin; } set { inside_margin = value; max_content_width = max_width - inside_margin * 2; this.FireOnValueChanged(); } }

        protected int outside_margin = 12;
        public int OutsideMargin { get { return outside_margin; } set { outside_margin = value; Reposition(); } }

        protected double max_opacity = 0.8;
        public double MaxOpacity { get { return max_opacity; } set { max_opacity = value; if (max_opacity > this.Opacity) { this.Opacity = max_opacity; } } }

        protected int def_fade_time = 750;
        protected Timer fadeTimer = new Timer();
        public int DefaultFadeTime { get { return def_fade_time; } set { this.fadeTotalTime = value; def_fade_time = value; } }

        protected Timer autoCloseTimer = new Timer();
        public int VisibleTime { get { return autoCloseTimer.Interval; } set { autoCloseTimer.Interval = value; } }

        protected DesktopPos pos = DesktopPos.NearClock;
        public DesktopPos DesktopPosition { get { return pos; } set { pos = value; Reposition(); } }
        #endregion

        protected void SetOpacity(double o) { this.Opacity = max_opacity * o.Clamp(0.0, 1.0); } // incorporates the MaxOpacity setting

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
        protected override void OnResize(EventArgs e) { base.OnResize(e); this.Reposition(); }
        private void RespositionEvent(object sender, EventArgs e) { this.Reposition(); }
        protected void Reposition() { this.Reposition((this.pos == DesktopPos.NearClock) ? Display.GetClockPosition() : this.pos); }
        protected void Reposition(DesktopPos p)
        {
            switch (p)
            {
                case DesktopPos.UpperLeft: DesktopLocation = new Point(outside_margin, outside_margin); break;
                case DesktopPos.UpperRight: DesktopLocation = new Point(SystemInformation.WorkingArea.Width - this.Width - outside_margin, outside_margin); break;
                case DesktopPos.LowerLeft: DesktopLocation = new Point(outside_margin, SystemInformation.WorkingArea.Height - this.Height - outside_margin); break;
                case DesktopPos.LowerRight: DesktopLocation = new Point(SystemInformation.WorkingArea.Width - this.Width - outside_margin, SystemInformation.WorkingArea.Height - Height - outside_margin); break;
            }
        }

        #region Auto closing
        protected bool allowedToAutoClose = true;
        public bool AllowedToAutoClose {
            get { return allowedToAutoClose; }
            set
            {
                if (allowedToAutoClose == value) return;
                allowedToAutoClose = value;
                if (allowedToAutoClose)
                    startAutoCloseTimer();
                else
                    stopAutoCloseTimer();
            }
        }
        protected void startAutoCloseTimer()
        {
            if (this.Visible && this.Opacity == this.max_opacity && this.allowedToAutoClose && !this.autoCloseTimer.Enabled)
            {
                if (this.InvokeRequired)
                    this.Invoke(new Delegates.Action(this.autoCloseTimer.Start));
                else
                    this.autoCloseTimer.Start();
            }
        }
        protected void stopAutoCloseTimer()
        {
            if (this.InvokeRequired)
                this.Invoke(new Delegates.Action(this.autoCloseTimer.Stop));
            else
                this.autoCloseTimer.Stop();
        }
        #endregion

        #region Fading
        protected bool fadingIn = false;
        protected long fadeStartTime = 0, fadeTotalTime = 0; // in Ticks

        protected void StartFading()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Delegates.Action(this.StartFading));
            }
            else
            {
                this.fadeTimer.Start();
                this.SetOpacity(this.fadingIn ? 0.0 : 1.0);
                this.ShowInactiveTopmost();
            }
        }
        protected void FadeUpdate(object sender, EventArgs e)
        {
            long elapsed = DateTime.Now.Ticks - this.fadeStartTime;
            if (elapsed < this.fadeTotalTime)
            {
                double perc = (double)elapsed / this.fadeTotalTime;
                this.SetOpacity(this.fadingIn ? perc : (1.0 - perc));
            }
            else
            {
                this.fadeTimer.Stop();
                if (this.fadingIn)
                {
                    this.SetOpacity(1.0);
                    this.startAutoCloseTimer();
                }
                else
                {
                    this.SetOpacity(0.0);
                    this.stopAutoCloseTimer();
                    this.Hide();
                }
            }
        }

        public void FadeIn()
        {
            long ticks = this.def_fade_time *10000L;
            long now = DateTime.Now.Ticks;
            if (this.Visible)
            {
                if (this.fadeTimer.Enabled)
                {
                    if (!this.fadingIn)
                    {
                        long elapsed = now - this.fadeStartTime;
                        double perc = (double)elapsed / this.fadeTotalTime;
                        perc = 1.0 - perc.Clamp(0.0, 1.0);
                        this.fadingIn = true;
                        this.fadeStartTime = now - (long)(perc * ticks);
                        this.fadeTotalTime = ticks;
                    }
                }
            }
            else
            {
                this.fadingIn = true;
                this.fadeStartTime = now;
                this.fadeTotalTime = ticks;
                this.StartFading();
            }
        }

        public void FadeOut()
        {
            long ticks = this.def_fade_time * 10000L;
            long now = DateTime.Now.Ticks;
            if (this.Visible)
            {
                if (this.fadeTimer.Enabled)
                {
                    if (this.fadingIn)
                    {
                        long elapsed = now - this.fadeStartTime;
                        double perc = (double)elapsed / this.fadeTotalTime;
                        perc = 1.0 - perc.Clamp(0.0, 1.0);
                        this.fadingIn = false;
                        this.fadeStartTime = now - (long)(perc * ticks);
                        this.fadeTotalTime = ticks;
                    }
                }
                else
                {
                    this.fadingIn = false;
                    this.fadeStartTime = now;
                    this.fadeTotalTime = ticks;
                    this.StartFading();
                }
            }
        }

        private void FadeOutNow(object sender, EventArgs e)
        {
            this.stopAutoCloseTimer();
            this.FadeOut();
        }
        #endregion
    }
}
