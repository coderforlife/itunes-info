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
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>A basic iTunes Track Information Display</summary>
    class BasicTrackDisplay : TrackDisplay
    {
        /// <summary>Create a new basic iTunes Track Information Display, which has no system-drawn border</summary>
        /// <param name="controller">The iTunesController using this display</param>
        public BasicTrackDisplay(Controller controller) : base(controller)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Controller.DefaultBackgroundColor;
        }

        /// <summary>The back color is important property used, so tell the base iTunes Track Information Display that the display needs to be rebuilt</summary>
        /// <remarks>Also cases the base OnBackColorChanged</remarks>
        /// <param name="e">The arguments for the event</param>
        protected override void OnBackColorChanged(EventArgs e) { this.FireOnValueChanged(); base.OnBackColorChanged(e); }

        /// <summary>Prepares for painting the display, which for a basic display is just drawing the background color</summary>
        /// <param name="g">The graphics to draw to</param>
        protected override void PrepareForPainting(Graphics g) { g.Clear(this.BackColor); }
    }
}
