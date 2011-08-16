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

using iTunesInfo.Properties;

namespace iTunesInfo
{
    /// <summary>Utility class for generating star-rating images</summary>
    static class Rating
    {
        /// <summary>If the information has been initialized</summary>
        private static bool inited = false;

        /// <summary>The star image</summary>
        private static Bitmap star; // '\x2605'
        /// <summary>The half-star image</summary>
        private static Bitmap half; // '\x00BD'
        
        /// <summary>The height offset between the star and half-star images</summary>
        private static int half_y_offset;

        /// <summary>The cache of different stars, odd indexes have half stars, evens are full stars</summary>
        private static Bitmap[] cache = new Bitmap[11];

        /// <summary>Initialize star-rating generation by getting the necessary images</summary>
        private static void Init()
        {
            if (!inited)
            {
                // Get the images
                star = Resources.star;
                half = Resources.half;

                // The height difference between the star and half images
                half_y_offset  = (star.Height - half.Height) / 2;

                // Mark the fact that we have initialized
                inited = true;
            }
        }

        /// <summary>Get the height of the star-rating image</summary>
        public static int Height { get { Init(); return star.Height; } }
        /// <summary>Get the width of the star-rating image</summary>
        public static int Width { get { Init(); return star.Width * 5; } }

        /// <summary>Create a star-rating image, possibly from cache</summary>
        /// <param name="rating">The value of the rating, from 0 to 100</param>
        /// <returns>The drawn star-rating image</returns>
        public static Bitmap Create(int rating)
        {
            Init();

            int sw = star.Width; // the width of a single star

            // Calculate the number of stars and if there is a half-star
            rating = rating.Clamp(0, 100);
            int stars = rating / 20, half_point = rating % 20;
            if (half_point >= 15) ++stars;
            bool has_half = half_point > 5 && half_point < 15;

            // The cache id for this image
            int id = 2 * stars + (has_half ? 1 : 0);

            if (cache[id] == null)
            {
                // Need to draw the image since it isn't in the cache
                Bitmap b = new Bitmap(sw * 5, star.Height);
                Graphics g = Graphics.FromImage(b);

                // Draw the stars
                for (int i = 0; i < stars; ++i)
                    g.DrawImage(star, i * sw, 0);

                // Draw the half-star
                if (has_half)
                    g.DrawImage(half, stars * sw, half_y_offset);

                g.Dispose();

                // Save the image into the cache
                cache[id] = b;
            }

            // Use the image in the cache
            return cache[id];
        }
    }
}
