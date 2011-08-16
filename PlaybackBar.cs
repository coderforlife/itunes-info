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

using iTunesInfo.Properties;

namespace iTunesInfo
{
    /// <summary>Utility class for generating playback bar images</summary>
    static class PlaybackBar
    {
        /// <summary>If the information has been initialized</summary>
        private static bool inited = false;

        /// <summary>The image used to generate the background of the playback bar, which is a composite</summary>
        private static Bitmap background;
        /// <summary>The image used to generate the filler of the playback bar, which is a composite</summary>
        private static Bitmap fill;
        /// <summary>The scrubber image, drawn at the leading edge of the fill</summary>
        private static Bitmap position;

        /// <summary>The rectangle that describes the source of the start within the background image</summary>
        private static Rectangle bg_start;
        /// <summary>The rectangle that describes the source of the middle within the background image</summary>
        private static Rectangle bg_middle;
        /// <summary>The rectangle that describes the source of the end within the background image</summary>
        private static Rectangle bg_end;
        /// <summary>The rectangle that describes the source of the start within the fill image</summary>
        private static Rectangle fill_start;
        /// <summary>The rectangle that describes the source of the middle within the fill image</summary>
        private static Rectangle fill_middle;

        /// <summary>Cached background images, the index is the width of the image, each element can be null, and the array is resized as necessary</summary>
        private static Bitmap[] backgrounds = new Bitmap[250];
        /// <summary>Cached filler images, the index is the width of the image, each element can be null, and the array is resized as necessary</summary>
        private static Bitmap[] fillers = new Bitmap[250];

        /// <summary>Initialize playback bar generation by getting the necessary images and calculating the source rectangles</summary>
        private static void Init()
        {
            if (!inited)
            {
                // Get the images
                position = Resources.playback_position;
                fill = Resources.playback_fill;
                background = Resources.playback_background;

                // Calculate the fill source rectangles, start is all but last column and middle is the last column
                int fw1 = fill.Width - 1;
                fill_start = new Rectangle(0, 0, fw1, fill.Height);
                fill_middle = new Rectangle(fw1, 0, 1, fill.Height);

                // Calculate the background source rectangle, start is the first half, end is the last half, and middle is the odd column in the middle
                int w = background.Width / 2; // truncates the odd value out
                bg_start = new Rectangle(0, 0, w, background.Height);
                bg_middle = new Rectangle(w, 0, 1, background.Height);
                bg_end = new Rectangle(w + 1, 0, w, background.Height);

                // Create the really small fillers (all just the start of the filler bar)
                Bitmap b = new Bitmap(fw1, fill.Height);
                Graphics g = Graphics.FromImage(b);
                g.DrawImage(fill, 0, 0, fill_start, GraphicsUnit.Pixel);
                g.Dispose();
                for (int i = 0; i < fw1; ++i)
                    fillers[i] = b;

                // Mark the fact that we have initialized
                inited = true;
            }
        }

        /// <summary>Get the height of playback bars</summary>
        public static int Height { get { Init(); return background.Height; } }
        /// <summary>Get the minimum width of playback bars</summary>
        public static int MinWidth { get { Init(); return background.Width; } }

        /// <summary>Get a background for a playback bar of a certain width from the cache, or generate it and add it to the cache</summary>
        /// <param name="width">The width of the playback bar</param>
        /// <returns>The background of the playback bar</returns>
        private static Bitmap GetBackground(int width)
        {
            if (width >= backgrounds.Length || backgrounds[width] == null)
            {
                // Need to draw a background of this size onto a new bitmap since it isn't cached
                Bitmap b = new Bitmap(width, background.Height);
                Graphics g = Graphics.FromImage(b);
                int w = bg_end.Width, middles = width - 2 * w;
                g.DrawImage(background, 0, 0, bg_start, GraphicsUnit.Pixel); // draw start
                for (int i = 0; i < middles; ++i)
                    g.DrawImage(background, w + i, 0, bg_middle, GraphicsUnit.Pixel); // draw middle repeatedly
                g.DrawImage(background, width - w, 0, bg_end, GraphicsUnit.Pixel); // draw end
                g.Dispose();

                // Cache the drawn background
                if (width >= backgrounds.Length)
                    Array.Resize(ref backgrounds, width + 1);
                backgrounds[width] = b;
            }

            // Return the cached background
            return backgrounds[width];
        }

        /// <summary>Get a filler for a playback bar of a certain width from the cache, or generate it and add it to the cache</summary>
        /// <param name="pos">The position of the scrubber within the playback bar</param>
        /// <returns>The filler for the playback bar</returns>
        private static Bitmap GetFiller(int pos)
        {
            if (pos >= fillers.Length || fillers[pos] == null)
            {
                // Need to draw a filler of this size onto a new background since it isn't cached
                Bitmap b = new Bitmap(pos, background.Height);
                Graphics g = Graphics.FromImage(b);
                int w = fill_start.Width, middles = pos - w;
                g.DrawImage(fill, 0, 0, fill_start, GraphicsUnit.Pixel); // draw start
                for (int i = 0; i < middles; ++i)
                    g.DrawImage(fill, w + i, 0, fill_middle, GraphicsUnit.Pixel); // draw middle repeatedly
                g.Dispose();

                // Cache the drawn filler
                if (pos >= fillers.Length)
                    Array.Resize(ref fillers, pos + 1);
                fillers[pos] = b;
            }

            // Return the cached background
            return fillers[pos];
        }

        /// <summary>Create a playback bar image</summary>
        /// <param name="width">The width of the playback bar</param>
        /// <param name="perc">The percentage that the playback bar has progressed, from 0 to 1</param>
        /// <returns>The drawn playback bar, or null for errors</returns>
        public static Bitmap Create(int width, double perc)
        {
            Init();

            // Invalid width
            if (width < background.Width)
                return null;

            // Adjust the percentage and get the width of the filler
            perc = perc.Clamp(0, 1);
            int pos = Convert.ToInt32(perc * width);

            // Create the bitmap already containing the background
            Bitmap b = new Bitmap(GetBackground(width));
            Graphics g = Graphics.FromImage(b);

            // Draw filler
            g.DrawImage(GetFiller(pos), 0, 0);

            // Draw position (scrubber)
            int xpos = (pos - position.Width / 2);
            g.DrawImage(position, xpos.Clamp(0, width - position.Width), 0);

            // Finished
            g.Dispose();
            return b;
        }
    }
}
