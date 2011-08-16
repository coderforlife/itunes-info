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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;

// Required for custom attributes, otherwise you get compiler errors
namespace System.Runtime.CompilerServices { public class ExtensionAttribute : Attribute { } }

namespace iTunesInfo
{
    /// <summary>Extension utility functions</summary>
    static class Extensions
    {
        /// <summary>Clamps a value between two other values</summary>
        /// <param name="val">The value to clamp</param>
        /// <param name="min">The minimum value to return</param>
        /// <param name="max">The maximum value to return</param>
        /// <returns>The value, clamped between min and max</returns>
        public static double Clamp(this double val, double min, double max) { return val < min ? min : (val > max ? max : val); }
        /// <summary>Clamps a value between two other values</summary>
        /// <param name="val">The value to clamp</param>
        /// <param name="min">The minimum value to return</param>
        /// <param name="max">The maximum value to return</param>
        /// <returns>The value, clamped between min and max</returns>
        public static int Clamp(this int val, int min, int max) { return val < min ? min : (val > max ? max : val); }

        /// <summary>The private native brush handle of managed brush</summary>
        private static readonly FieldInfo nativeBrush = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
        /// <summary>Get the native GDI+ brush handle for managed brush object</summary>
        /// <param name="b">The managed brush object</param>
        /// <returns>The native GDI+ brush handle value</returns>
        public static IntPtr GetNativeHandle(this Brush b) { return (IntPtr)nativeBrush.GetValue(b); }

        /// <summary>An object to convert Font objects</summary>
        private static readonly TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
        /// <summary>Convert a Font into a String that can be converted back, useful for serialization</summary>
        /// <param name="f">The font to convert</param>
        /// <returns>A string representation of the font, that can be converted back into a font object</returns>
        public static string ConvertToString(this Font f) { return fontConverter.ConvertToString(f); }
        /// <summary>Convert a String into a Font</summary>
        /// <param name="f">The string to convert, should be made with 'ConvertToString'</param>
        /// <returns>A font created from the string</returns>
        public static Font ConvertToFont(this string s) { return (Font)fontConverter.ConvertFrom(s); }

        /// <summary>Swap two elements in a list</summary>
        /// <param name="l">The list</param>
        /// <param name="a">One of the indices</param>
        /// <param name="b">The other index</param>
        public static void Swap(this IList l, int a, int b) { object t = l[a]; l[a] = l[b]; l[b] = t; }
    }
}
