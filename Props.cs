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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using iTunesLib;

namespace iTunesInfo
{
    /// <summary>The content of the track display</summary>
    class Content
    {
        /// <summary>The special character that marks the beginning and end of an image</summary>
        public const char ImageMarkerCh = '\x11';
        /// <summary>The special character that marks the beginning and end of an image as a string</summary>
        public const string ImageMarker = "\x11";
        /// <summary>The special character that marks the beginning and end of a changing value</summary>
        public const char ValueMarkerCh = '\x12';
        /// <summary>The special character that marks the beginning and end of a changing value as a string</summary>
        public const string ValueMarker = "\x12";

        /// <summary>The text of the content, with embedded markers for images and changing values</summary>
        public string Text = "";
        /// <summary>The images used in the content</summary>
        public List<Bitmap> Images = new List<Bitmap>();
        /// <summary>The changing values used in the content</summary>
        public List<ChangingValue> Values = new List<ChangingValue>();

        /// <summary>Add an image to the content</summary>
        /// <param name="img">The image to add</param>
        /// <returns>The string to add to the text content to identify the image</returns>
        public string AddImage(Bitmap img)
        {
            // TODO: guarantee that it is on it's own line
            this.Images.Add(img);
            return ImageMarker + (this.Images.Count - 1) + ImageMarker;
        }
        /// <summary>Add a changing value to the content</summary>
        /// <param name="pbv">The changing value to add</param>
        /// <returns>The string to add to the text content to identify the changing value</returns>
        public string AddValue(ChangingValue pbv)
        {
            // TODO: guarantee that it is on it's own line
            this.Values.Add(pbv);
            return ValueMarker + (this.Values.Count - 1) + ValueMarker;
        }

        /// <summary>Get an item from string</summary>
        /// <param name="s">The string</param>
        /// <param name="m">The marker for the item</param>
        /// <returns>The item that the string represents, or null if the string doesn't represent an item</returns>
        private static T GetVal<T>(string s, char m, List<T> a) where T : class
        {
            int x, l = s.Length;
            return l > 0 && s[0] == m && s[l - 1] == m && Int32.TryParse(s.Substring(1, l - 2), out x) && x >= 0 && x < a.Count ? a[x] : (T)null;
        }
        
        /// <summary>Get an image from the content</summary>
        /// <param name="line">The line of text from the content</param>
        /// <returns>The image or null</returns>
        public Bitmap GetImage(string line) { return GetVal(line, ImageMarkerCh, this.Images); }
        /// <summary>Get a changing value from the content</summary>
        /// <param name="line">The line of text from the content</param>
        /// <returns>The changing value or null</returns>
        public ChangingValue GetValue(string line) { return GetVal(line, ValueMarkerCh, this.Values); }
    }

    /// <summary>A value that may change over time which should be polled</summary>
    abstract class ChangingValue
    {
        /// <summary>The last value that was retrieved</summary>
        private double last = 0;
        /// <summary>The internal value-getting property</summary>
        protected abstract double ValueInternal { get; }
        /// <summary>Get the current value, or possibly a cached value if iTunes is currently deferring calls</summary>
        /// <param name="controller">The iTunes Controller object</param>
        /// <returns>The value, possibly just repeating the previous value</returns>
        public double Value(Controller controller)
        {
            if (!controller.iTunesDisabled)
                this.last = this.ValueInternal;
            return this.last;
        }
    }

    /// <summary>Utility class for parsing the content of the track display and substituting properties for items in curly braces</summary>
    static class Props
    {
        #region Contexts
        /// <summary>The context type</summary>
        private enum Context
        {
            App, Visual, EQPreset,                                      //iTunes, iTunes.CurrentVisual, iTunes.CurrentEQPreset
            Track, FileOrCDTrack, URLTrack,                             //iTunes.CurrentTrack
            Playlist, AudioCDPlaylist, LibraryPlaylist, UserPlaylist,   //iTunes.CurrentPlaylist
            Source, iPodSource,                                         //iTunes.CurrentPlaylist.Source
        }

        /// <summary>Array of both Context.FileOrCDTrack and Context.URLTrack</summary>
        private static readonly Context[] ContextAnyTrack = { Context.FileOrCDTrack, Context.URLTrack };

        /// <summary>The Type variables for the different context types</summary>
        private static readonly Type[] propCntxtTypes =
        {
            typeof(IiTunes/*iTunesApp*/), typeof(IITVisual), typeof(IITEQPreset),
            typeof(IITTrack), typeof(IITFileOrCDTrack), typeof(IITURLTrack),
            typeof(IITPlaylist), typeof(IITAudioCDPlaylist), typeof(IITLibraryPlaylist), typeof(IITUserPlaylist),
            typeof(IITSource), typeof(IITIPodSource),
        };
        /// <summary>The context objects</summary>
        private static readonly object[] propCntxtObjs = new object[12];
        #endregion

        #region Enum Names
        /// <summary>The names of the items in the RatingKind enumeration</summary>
        private static readonly string[] RatingKindNames = { "User", "Computed" };
        /// <summary>The names of the items in the TrackKind enumeration</summary>
        private static readonly string[] TrackKindNames = { "Unknown", "File", "CD", "URL", "Device", "Shared Library" };
        /// <summary>The names of the items in the VideoKind enumeration</summary>
        private static readonly string[] VideoKindNames = { "None", "Movie", "Music Video", "TV Show" };
        /// <summary>The names of the items in the PlaylistKind enumeration</summary>
        private static readonly string[] PlaylistKindNames = { "Unknown", "Library", "User", "CD", "Device", "Radio tuner" };
        /// <summary>The names of the items in the PlaylistRepeatMode enumeration</summary>
        private static readonly string[] PlaylistRepeatModeNames = { "Off", "One song", "Entire playlist" };
        /// <summary>The names of the items in the PlaylistSpecialKind enumeration</summary>
        private static readonly string[] PlaylistSpecialKindNames = { "None", "Purchased Music", "Party Shuffle", "Podcasts", "Folder", "Videos", "Music", "Movies", "TV Shows", "Audiobooks" };
        /// <summary>The names of the items in the SourceKind enumeration</summary>
        private static readonly string[] SourceKindNames = { "Unknown", "Library", "iPod", "Audio CD", "MP3 CD", "Device", "Radio tuner", "Shared library" };
        /// <summary>The names of the items in the PlayerState enumeration</summary>
        private static readonly string[] PlayerStateNames = { "Stopped", "Playing", "Fast forwarding", "Rewinding" };
        /// <summary>The names of the items in the VisualSize enumeration</summary>
        private static readonly string[] VisualSizeNames = { "Small", "Medium", "Large" };
        #endregion

        #region Prop Objects
        /// <summary>A basic property. It gets the current value of a named property in a particular context</summary>
        private class Prop
        {
            /// <summary>The contexts used by this property</summary>
            protected Context[] cntxts;
            /// <summary>The properties of the contexts used by this property</summary>
            protected PropertyInfo[] propInfos;

            /// <summary>Get the PropertyInfo reflection object for a context and property name</summary>
            /// <param name="c">The context</param>
            /// <param name="p">The property name</param>
            /// <returns>The PropertyInfo object</returns>
            protected static PropertyInfo GetPropInfo(Context c, string p) { return propCntxtTypes[(int)c].GetProperty(p); }
            /// <summary>Get the raw value of a property in a particular context</summary>
            /// <param name="c">The context</param>
            /// <param name="p">The PropertyInfo reflection object</param>
            /// <returns>The raw value or null if it isn't available</returns>
            protected static object GetRawValue(Context c, PropertyInfo p)
            {
                object o = propCntxtObjs[(int)c];
                if (o != null)
                    try { return p.GetValue(o, null); } catch { }
                return null;
            }

            /// <summary>Gets the raw value object of a particular contexts / property-info</summary>
            /// <param name="i">The index in the cntxts / propInfos array</param>
            /// <returns>The raw value or null if it isn't available</returns>
            protected object GetRawValue(int i) { return GetRawValue(this.cntxts[i], this.propInfos[i]); }
            /// <summary>Gets the raw value object, searching through the contexts / property-infos until one is actually available</summary>
            /// <returns>The raw value or null if it isn't available</returns>
            protected object GetRawValue()
            {
                for (int i = 0; i < this.cntxts.Length; ++i)
                {
                    object o = propCntxtObjs[(int)this.cntxts[i]];
                    if (o != null)
                        try { return this.propInfos[i].GetValue(o, null); } catch { }
                }
                return null;
            }

            /// <summary>A blank constructor for use by subclasses to start from scratch</summary>
            protected Prop() { }

            /// <summary>Create a property for a named property of the given context</summary>
            /// <param name="cntxt">The context of the property</param>
            /// <param name="prop">The name of the property</param>
            public Prop(Context cntxt, string prop)
            {
                this.cntxts = new Context[1]{ cntxt };
                this.propInfos = new PropertyInfo[1] { GetPropInfo(cntxt, prop) };
            }

            /// <summary>Create a property for a named property of the given contexts; using the first available context</summary>
            /// <param name="cntxts">The contexts of the property; the first available is used</param>
            /// <param name="prop">The name of the property</param>
            public Prop(Context[] cntxts, string prop)
            {
                this.cntxts = cntxts;
                this.propInfos = new PropertyInfo[cntxts.Length];
                for (int i = 0; i < cntxts.Length; ++i)
                    this.propInfos[i] = GetPropInfo(cntxts[i], prop);
            }

            /// <summary>Gets if the current value of the property is available. The base class returns true if the value is not null.</summary>
            public virtual bool Available { get { return GetRawValue() != null; } }
            /// <summary>Gets if the current value of the property represents true. The base class returns true if the string value is not null and not empty.</summary>
            public virtual bool IsTrue { get { object val = GetRawValue(); return val != null && !string.IsNullOrEmpty(val.ToString()); } }
            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, is specific to the type of property. This base class ignores the format.</param>
            /// <returns>The value as a string</returns>
            public virtual string GetValue(string format)
            {
                object val = GetRawValue();
                string str = val == null ? null : val.ToString();
                return string.IsNullOrEmpty(str) ? "\0" : str;
            }
        }
        /// <summary>A boolean property</summary>
        private class BooleanProp : Prop
        {
            public BooleanProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public BooleanProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Get true if the property is available and the value of the boolean is true</summary>
            public override bool IsTrue { get { object val = GetRawValue(); return val != null && (bool)val; } }
        }
        /// <summary>A property for IFormattable values (DateTime, Integer, Double)</summary>
        private abstract class DirectFormattableProp : Prop
        {
            public DirectFormattableProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public DirectFormattableProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Convert to a string using a default format</summary>
            /// <param name="f">The value to convert</param>
            /// <returns>The result of the conversion</returns>
            public virtual string FallbackToString(IFormattable f) { return f.ToString(); }

            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, passed to the IFormattable.ToString()</param>
            /// <returns>The value of the property as a string</returns>
            public override string GetValue(string format)
            {
                object val = GetRawValue();
                if (val != null)
                {
                    IFormattable f = (IFormattable)val;
                    if (!string.IsNullOrEmpty(format))
                        try { return f.ToString(format, null); }
                        catch { }
                    return FallbackToString(f);
                }
                return "\0";
            }
        }
        /// <summary>A property for date values</summary>
        private class DateProp : DirectFormattableProp
        {
            public DateProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public DateProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Convert to a string using the general date format "g"</summary>
            /// <param name="f">The value to convert</param>
            /// <returns>The result of the conversion</returns>
            public override string FallbackToString(IFormattable f) { return f.ToString("g", null); }
        }
        /// <summary>A property for time values, which are represented as integer seconds</summary>
        private class TimeProp : Prop
        {
            public TimeProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public TimeProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, passed to DateTime.ToString()</param>
            /// <returns>The value of the property as a string</returns>
            public override string GetValue(string format)
            {
                object val = GetRawValue();
                if (val != null)
                {
                    // Get the number of seconds
                    int secs = (int)val;
                    if (!string.IsNullOrEmpty(format))
                    {
                        // Format the time
                        //TimeSpan x = new TimeSpan(0, 0, secs); // Does not work before .NET 4, but would be preferred
                        DateTime x = new DateTime(0, 0, 0, 0, 0, secs);
                        try { return x.ToString(format); }
                        catch { }
                    }
                    return secs.ToString(); // just give the text
                }
                return "\0";
            }
        }
        /// <summary>A property for integer values</summary>
        private class IntegerProp : DirectFormattableProp
        {
            public IntegerProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public IntegerProp(Context[] cntxts, string prop) : base(cntxts, prop) { }
        }
        /// <summary>A property for decimal values</summary>
        private class DoubleProp : DirectFormattableProp
        {
            public DoubleProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public DoubleProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Convert to a string using with optional decimal places</summary>
            /// <param name="f">The value to convert</param>
            /// <returns>The result of the conversion</returns>
            public override string FallbackToString(IFormattable f) { return f.ToString("0.#", null); }
        }
        /// <summary>A property for enumeration values</summary>
        private class EnumProp : Prop
        {
            /// <summary>The names for the enumeration values, in numerical order starting from 0</summary>
            private string[] names;

            /// <summary>Create a property for a named property of the given context, with a set of names for the enumeration values</summary>
            /// <param name="cntxt">The context of the property</param>
            /// <param name="prop">The name of the property</param>
            /// <param name="names">The names for the enumeration values, in numerical order starting from 0</param>
            public EnumProp(Context cntxt, string prop, string[] names) : base(cntxt, prop) { this.names = names; }

            /// <summary>Create a property for a named property of the given contexts; using the first available context, with a set of names for the enumeration values</summary>
            /// <param name="cntxts">The contexts of the property; the first available is used</param>
            /// <param name="prop">The name of the property</param>
            /// <param name="names">The names for the enumeration values, in numerical order starting from 0</param>
            public EnumProp(Context[] cntxts, string prop, string[] names) : base(cntxts, prop) { this.names = names; }

            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, which is not used</param>
            /// <returns>The value of the property as a string</returns>
            public override string GetValue(string format)
            {
                object val = GetRawValue();
                if (val != null)
                {
                    int x = Convert.ToInt32(val); // convert the value to an integer
                    if (x >= 0 && x < this.names.Length)
                        return this.names[x];
                }
                return "\0";
            }
        }
        /// <summary>A property for rating types, which are given as an integer from 0 to 100</summary>
        private class RatingProp : Prop
        {
            public RatingProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public RatingProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Gets if the current value of the property represents true. Returns true if available and not equal to 0.</summary>
            public override bool IsTrue { get { object val = GetRawValue(); return val != null && ((int)val > 0); } }

            /// <summary>Get the current value of the property as a special string</summary>
            /// <param name="format">The format of the string returned, which is not used</param>
            /// <returns>A special string describing the image</returns>
            public override string GetValue(string format)
            {
                object val = GetRawValue();
                if (val != null)
                {
                    int rating = Convert.ToInt32(val); // the integer rating value, from 0 to 100
                    if (rating > 0) // TODO: have a format that allows rating=0 to still show up and different pictures?
                        return content.AddImage(Rating.Create(rating));
                }
                return "\0";
            }
        }
        /// <summary>Properties for file sizes, allowing the formats: b (default), k, m, g, or * (picks an appropriate unit and appends the unit)</summary>
        private abstract class SizeProp : Prop
        {
            /// <summary>The number of bytes in a kilobyte</summary>
            public const int KB = 1024;
            /// <summary>The number of bytes in a megabyte</summary>
            public const int MB = 1024 * 1024;
            /// <summary>The number of bytes in a gigabyte</summary>
            public const int GB = 1024 * 1024 * 1024;

            public SizeProp() : base() { }
            public SizeProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public SizeProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Used to format the size</summary>
            /// <param name="size">The size in bytes</param>
            /// <param name="format">The format to use: b (default), k, m, g, or *</param>
            /// <returns>The formatted value</returns>
            public static string FormatSize(double size, string format)
            {
                char f = format.Length == 1 ? format[0] : '\0';
                switch (f)
                {
                    case 'k': return (size / KB).ToString("0.#");
                    case 'm': return (size / MB).ToString("0.#");
                    case 'g': return (size / GB).ToString("0.#");
                    case '*':
                        if (size < KB) return size.ToString() + " bytes";
                        if (size < MB) return (size / KB).ToString("0.#") + " KB";
                        if (size < GB) return (size / MB).ToString("0.#") + " MB";
                        return (size / GB).ToString("0.#") + " GB";
                    case 'b':
                    default:
                        return size.ToString();
                }
            }
        }
        /// <summary>A property for track file sizes, that are stored using a single integer or 2 integers</summary>
        private class TrackSizeProp : SizeProp
        {
            /// <summary>A property that displays the size of a track</summary>
            public TrackSizeProp() : base()
            {
                // The size properties that can be used
                this.cntxts = new Context[3] { Context.Track, Context.FileOrCDTrack, Context.FileOrCDTrack };
                this.propInfos = new PropertyInfo[3] { GetPropInfo(this.cntxts[0], "Size"), GetPropInfo(this.cntxts[1], "Size64High"), GetPropInfo(this.cntxts[2], "Size64Low") };
            }
            /// <summary>The internal function for getting the value</summary>
            /// <returns>The file size, or UInt64.MaxValue if it isn't available</returns>
            private ulong GetValue() {
                // Try to get the combined value from Size64High and Size64Low
                object valH = GetRawValue(1), valL = GetRawValue(2);
                if (valH != null && valL != null)
                {
                    int high = Convert.ToInt32(valH), low = Convert.ToInt32(valL);
                    return (((ulong)high) << 32) + (ulong)low;
                }
                // Now try to use just Size
                object val = GetRawValue(0);
                if (val != null)
                    return (ulong)Convert.ToInt32(val);
                return UInt64.MaxValue; // Failed
            }
            /// <summary>This property is available if either the Size or both the Size64High and Size64Low values are available</summary>
            public override bool Available  { get { return GetRawValue(0) != null || (GetRawValue(1) != null && GetRawValue(2) != null); } }
            /// <summary>This property is true if it is available and not 0</summary>
            public override bool IsTrue     { get { ulong val = GetValue(); return val != 0 && val != UInt64.MaxValue; } }
            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, one of: b (default), k, m, g, or * (picks an appropriate unit and appends the unit)</param>
            /// <returns>The value of the property as a string</returns>
            public override string GetValue(string format)
            {
                ulong size = this.GetValue();
                return (size != UInt64.MaxValue) ? FormatSize((double)size, format) : "\0";
            }
        }
        /// <summary>A property for sizes that uses a double to store the value</summary>
        private class DoubleSizeProp : SizeProp
        {
            public DoubleSizeProp(Context cntxt, string prop) : base(cntxt, prop) { }
            public DoubleSizeProp(Context[] cntxts, string prop) : base(cntxts, prop) { }

            /// <summary>Gets if the current value of the property represents true. Returns true if available and greater than 0.</summary>
            public override bool IsTrue { get { object val = GetRawValue(); return val != null && ((double)val > 0); } }
            /// <summary>Get the current value of the property as a string</summary>
            /// <param name="format">The format of the string returned, one of: b (default), k, m, g, or * (picks an appropriate unit and appends the unit)</param>
            /// <returns>The value of the property as a string</returns>
            public override string GetValue(string format)
            {
                object val = GetRawValue();
                return (val != null) ? FormatSize((double)val, format) : "\0";
            }
        }
        /// <summary>A property for track artwork</summary>
        private class TrackArtworkProp : Prop
        {
            /// <summary>A property that displays track artwork</summary>
            public TrackArtworkProp() : base() {}

            /// <summary>Gets the artwork to use</summary>
            /// <returns>The current track's artwork collection</returns>
            private IITArtworkCollection GetArtwork()
            {
                IITTrack t = propCntxtObjs[(int)Context.Track] as IITTrack;
                return (t != null) ? t.Artwork : null;
            }
            /// <summary>This property is available if there is at least one piece of artwork</summary>
            public override bool Available  { get { IITArtworkCollection a = GetArtwork(); return a != null && a.Count > 0; } }
            /// <summary>This property is true if there is at least one piece of artwork</summary>
            public override bool IsTrue     { get { IITArtworkCollection a = GetArtwork(); return a != null && a.Count > 0; } }
            /// <summary>Get the current value of the property as a special string</summary>
            /// <param name="format">The format can be blank or a number greater than 1, and values higher than the total available pieces of artwork just become the last index</param>
            /// <returns>A special string describing the image</returns>
            public override string GetValue(string format)
            {
                IITArtworkCollection a = GetArtwork();
                if (a != null && a.Count > 0)
                {
                    // Show at least some artwork
                    int count = a.Count;
                    int idx = 1; // 1-based indexing
                    if (format.Length > 0 && Int32.TryParse(format, out idx)) // try to use the format
                        idx = idx.Clamp(1, count);
                    
                    // Need to save the image then load the image
                    string temp = Path.GetTempFileName();
                    a[idx].SaveArtworkToFile(temp);
                    FileStream fs = new FileStream(temp, FileMode.Open, FileAccess.Read);
                    Bitmap img = new Bitmap(fs);
                    fs.Close();
                    File.Delete(temp);

                    // Add the image to the content
                    return content.AddImage(img);
                }
                return "\0";
            }
        }
        /// <summary>A property for a playback bar</summary>
        private class PlaybackProp : Prop
        {
            /// <summary>The changing value for playback percentage</summary>
            private class PlaybackValue : ChangingValue
            {
                /// <summary>The iTunes app</summary>
                iTunesApp app;
                /// <summary>Create a new PlaybackValue</summary>
                /// <param name="app">The iTunes app</param>
                public PlaybackValue(iTunesApp app) { this.app = app; }
                /// <summary>Get the current playback percentage, from 0 to 1</summary>
                protected override double ValueInternal
                { get {
                    try
                    {
                        return (double)this.app.PlayerPosition / this.app.CurrentTrack.Duration;
                    }
                    catch { return -1; }
                } }
            }
            /// <summary>A property that display a playback bar</summary>
            public PlaybackProp() : base(Context.App, "PlayerPosition") { } // allow base to handle Available and IsTrue by giving it the PlayerPosition property
            /// <summary>Get the property as a special string so that it will be changing</summary>
            /// <param name="format">The format of the string returned, which is not used</param>
            /// <returns>A special string describing the changing value</returns>
            public override string GetValue(string format) { return this.IsTrue ? content.AddValue(new PlaybackValue((iTunesApp)propCntxtObjs[(int)Context.App])) : "\0"; }
        }
        #endregion

        #region Properties
        /// <summary>All of the properties that can be used, and the object that can perform the content fill-in</summary>
        private static Dictionary<string, Prop> available_props = new Dictionary<string, Prop>()
        {
            // Track
            {"trackname",                   new Prop(Context.Track, "Name")},
            {"trackartist",                 new Prop(Context.Track, "Artist")},
            {"albumartist",                 new Prop(Context.FileOrCDTrack, "AlbumArtist")},
            {"trackalbum",                  new Prop(Context.Track, "Album")},
            {"trackyear",                   new IntegerProp(Context.Track, "Year")},
            {"trackbpm",                    new IntegerProp(Context.Track, "BPM")},             // tempo
            {"trackgrouping",               new Prop(Context.Track, "Grouping")},
            {"trackcomposer",               new Prop(Context.Track, "Composer")},
            {"trackcomment",                new Prop(Context.Track, "Comment")},
            {"trackgenre",                  new Prop(Context.Track, "Genre")},
            {"trackcategory",               new Prop(ContextAnyTrack, "Category")},
            {"trackdescription",            new Prop(ContextAnyTrack, "Description")},
            {"tracklongdescription",        new Prop(ContextAnyTrack, "LongDescription")},
            {"tracklyrics",                 new Prop(Context.FileOrCDTrack, "Lyrics")},
            {"trackartwork",                new TrackArtworkProp()},

            {"trackshow",                   new Prop(Context.FileOrCDTrack, "Show")},
            {"trackseasonnumber",           new Prop(Context.FileOrCDTrack, "SeasonNumber")},
            {"trackepisodeid",              new Prop(Context.FileOrCDTrack, "EpisodeID")},
            {"trackepisodenumber",          new Prop(Context.FileOrCDTrack, "EpisodeNumber")},
            {"trackreleasedate",            new DateProp(Context.FileOrCDTrack, "ReleaseDate")},

            {"tracksortalbum",              new Prop(Context.FileOrCDTrack, "SortAlbum")},
            {"tracksortalbumartist",        new Prop(Context.FileOrCDTrack, "SortAlbumArtist")},
            {"tracksortartist",             new Prop(Context.FileOrCDTrack, "SortArtist")},
            {"tracksortcomposer",           new Prop(Context.FileOrCDTrack, "SortComposer")},
            {"tracksortname",               new Prop(Context.FileOrCDTrack, "SortName")},
            {"tracksortshow",               new Prop(Context.FileOrCDTrack, "SortShow")},

            {"albumtracknumber",            new IntegerProp(Context.Track, "TrackNumber")},
            {"albumtrackcount",             new IntegerProp(Context.Track, "TrackCount")},
            {"albumdiscnumber",             new IntegerProp(Context.Track, "DiscNumber")},
            {"albumdisccount",              new IntegerProp(Context.Track, "DiscCount")},
            {"albumiscompilation",          new BooleanProp(Context.Track, "Compilation")},
            {"trackispartofgaplessalbum",   new BooleanProp(Context.Track, "PartOfGaplessAlbum")},

            {"trackplayorderindex",         new IntegerProp(Context.Track, "PlayOrderIndex")},  // 1-based
            {"trackvolumeadjustment",       new IntegerProp(Context.Track, "VolumeAdjustment")},// from -100% to 100%
            {"trackeq",                     new Prop(Context.Track, "EQ")},
            {"trackisexcludedfromshuffle",  new BooleanProp(Context.FileOrCDTrack, "ExcludeFromShuffle")},
            {"trackispodcast",              new BooleanProp(ContextAnyTrack, "Podcast")},
            {"trackrating",                 new RatingProp(Context.Track, "Rating")},
            {"albumrating",                 new RatingProp(ContextAnyTrack, "AlbumRating")},
            {"trackratingvalue",            new IntegerProp(Context.Track, "Rating")},          // 0 to 100
            {"albumratingvalue",            new IntegerProp(ContextAnyTrack, "AlbumRating")},   // 0 to 100
            {"trackratingkind",             new EnumProp(ContextAnyTrack, "RatingKind", RatingKindNames)},
            {"albumratingkind",             new EnumProp(ContextAnyTrack, "AlbumRatingKind", RatingKindNames)},
            {"trackkind",                   new EnumProp(Context.Track, "Kind", TrackKindNames)},
            {"trackvideokind",              new EnumProp(Context.FileOrCDTrack, "VideoKind", VideoKindNames)},

            {"trackdateadded",              new DateProp(Context.Track, "DateAdded")},
            {"trackmodificationdate",       new DateProp(Context.Track, "ModificationDate")},
            {"trackplayeddate",             new DateProp(Context.Track, "PlayedDate")},
            {"trackskippeddate",            new DateProp(Context.FileOrCDTrack, "SkippedDate")},
            {"trackplayedcount",            new IntegerProp(Context.Track, "PlayedCount")},
            {"trackskippedcount",           new IntegerProp(Context.FileOrCDTrack, "SkippedCount")},
            {"trackisunplayed",             new BooleanProp(Context.FileOrCDTrack, "Unplayed")},
            {"trackisenabled",              new BooleanProp(Context.Track, "Enabled")},
            
            {"trackduration",               new TimeProp(Context.Track, "Duration")},
            {"trackstart",                  new TimeProp(Context.Track, "Start")},
            {"trackfinish",                 new TimeProp(Context.Track, "Finish")},
            {"tracktime",                   new Prop(Context.Track, "Time")}, // already in mm:ss format
            {"trackbookmarktime",           new TimeProp(Context.FileOrCDTrack, "BookmarkTime")},
            {"trackremembersbookmark",      new BooleanProp(Context.FileOrCDTrack, "RememberBookmark")},

            {"trackkindasstring",           new Prop(Context.Track, "KindAsString")},           // something like "AAC Audio File"
            {"trackbitrate",                new IntegerProp(Context.Track, "BitRate")},         // in kbps
            {"tracksamplerate",             new IntegerProp(Context.Track, "SampleRate")},      // in Hz
            {"tracksize",                   new TrackSizeProp()},
            {"tracklocation",               new Prop(Context.FileOrCDTrack, "Location")},
            {"trackurl",                    new Prop(Context.URLTrack, "URL")},

            // Playlist
            {"playlistname",                new Prop(Context.Playlist, "Name")},
            {"playlistkind",                new EnumProp(Context.Playlist, "Kind", PlaylistKindNames)},
            {"playlistduration",            new TimeProp(Context.Playlist, "Duration")},
            {"playlisttime",                new Prop(Context.Playlist, "Time")}, // already in mm:ss format
            {"playlistisshuffling",         new BooleanProp(Context.Playlist, "Shuffle")},
            {"playlistsize",                new DoubleSizeProp(Context.Playlist, "Size")},
            {"playlistrepeatmode",          new EnumProp(Context.Playlist, "SongRepeat", PlaylistRepeatModeNames)},
            {"playlistisvisible",           new BooleanProp(Context.Playlist, "Visible")},
            {"cdartist",                    new Prop(Context.AudioCDPlaylist, "Artist")},
            {"cdcomposer",                  new Prop(Context.AudioCDPlaylist, "Composer")},
            {"cdgenre",                     new Prop(Context.AudioCDPlaylist, "Genre")},
            {"cdyear",                      new IntegerProp(Context.AudioCDPlaylist, "Year")},
            {"cdiscompilation",             new BooleanProp(Context.AudioCDPlaylist, "Compilation")},
            {"cddisccount",                 new IntegerProp(Context.AudioCDPlaylist, "DiscCount")},
            {"cddiscnumber",                new IntegerProp(Context.AudioCDPlaylist, "DiscNumber")},
            {"playlistisshared",            new BooleanProp(Context.UserPlaylist, "Shared")},
            {"playlistissmart",             new BooleanProp(Context.UserPlaylist, "Smart")},
            {"playlistspecialkind",         new EnumProp(Context.UserPlaylist, "SpecialKind", PlaylistSpecialKindNames)},
            // Context.UserPlaylist.Parent (IITUserPlaylist **iParentPlayList)

            // Playlist Source
            {"sourcename",                  new Prop(Context.Source, "Name")},
            {"sourcekind",                  new EnumProp(Context.Source, "Kind", SourceKindNames)},
            {"sourcecapacity",              new DoubleSizeProp(Context.Source, "Capacity")},
            {"sourcefreespace",             new DoubleSizeProp(Context.Source, "FreeSpace")},
            {"ipodsoftwareversion",         new Prop(Context.iPodSource, "SoftwareVersion")},

            // App
            {"currentstreamtitle",          new Prop(Context.App, "CurrentStreamTitle")},
            {"currentstreamurl",            new Prop(Context.App, "CurrentStreamURL")},
            {"playerstate",                 new EnumProp(Context.App, "PlayerState", PlayerStateNames)},
            {"playerposition",              new TimeProp(Context.App, "PlayerPosition")},
            {"playbackbar",                 new PlaybackProp()},
            {"ismuted",                     new BooleanProp(Context.App, "Mute")},
            {"volume",                      new IntegerProp(Context.App, "SoundVolume")},

            // Visual
            {"visualsenabled",              new BooleanProp(Context.App, "VisualsEnabled")},
            {"fullscreenvisuals",           new BooleanProp(Context.App, "FullScreenVisuals")},
            {"visualsize",                  new EnumProp(Context.App, "VisualSize", VisualSizeNames)},
            {"visualname",                  new Prop(Context.Visual, "Name")},

            // EQ Preset
            {"eqisenabled",                 new BooleanProp(Context.App, "EQEnabled")},
            {"eqname",                      new Prop(Context.EQPreset, "Name")},
            {"eqismodifable",               new BooleanProp(Context.EQPreset, "Modifiable")},
            {"eqpremaplevel",               new DoubleProp(Context.EQPreset, "Preamp")}, // -12.0 db to +12.0 db
            {"eq32hzbandlevel",             new DoubleProp(Context.EQPreset, "Band1")}, // -12.0 db to +12.0 db
            {"eq64hzbandlevel",             new DoubleProp(Context.EQPreset, "Band2")}, // -12.0 db to +12.0 db
            {"eq125hzbandlevel",            new DoubleProp(Context.EQPreset, "Band3")}, // -12.0 db to +12.0 db
            {"eq250hzbandlevel",            new DoubleProp(Context.EQPreset, "Band4")}, // -12.0 db to +12.0 db
            {"eq500hzbandlevel",            new DoubleProp(Context.EQPreset, "Band5")}, // -12.0 db to +12.0 db
            {"eq1khzbandlevel",             new DoubleProp(Context.EQPreset, "Band6")}, // -12.0 db to +12.0 db
            {"eq2khzbandlevel",             new DoubleProp(Context.EQPreset, "Band7")}, // -12.0 db to +12.0 db
            {"eq4khzbandlevel",             new DoubleProp(Context.EQPreset, "Band8")}, // -12.0 db to +12.0 db
            {"eq8khzbandlevel",             new DoubleProp(Context.EQPreset, "Band9")}, // -12.0 db to +12.0 db
            {"eq16khzbandlevel",            new DoubleProp(Context.EQPreset, "Band10")}, // -12.0 db to +12.0 db
        };

        /// <summary>Descriptions of all of the available properties</summary>
        public readonly static Descriptor[] PropertyNames = 
        {
            // Track
            new Descriptor("TrackName",                   "The name of the current track"),
            new Descriptor("TrackArtist",                 "The artist of the current track"),
            new Descriptor("AlbumArtist",                 "The artist of the current album"),
            new Descriptor("TrackAlbum",                  "The album of the current track"),
            new Descriptor("TrackYear",                   "The year of the current track"),
            new Descriptor("TrackBPM",                    "The tempo in BPM of the current track"),
            new Descriptor("TrackGrouping",               "The grouping for the current track"),
            new Descriptor("TrackComposer",               "The composer of the current track"),
            new Descriptor("TrackComment",                "The comment for the current track"),
            new Descriptor("TrackGenre",                  "The genre of the current track"),
            new Descriptor("TrackCategory",               "The category for the current track"),
            new Descriptor("TrackDescription",            "The description of the current track"),
            new Descriptor("TrackLongDescription",        "The long description of the current track"),
            new Descriptor("TrackLyrics",                 "The lyrics for the current track"),
            new Descriptor("TrackArtwork",                "The main artwork for the current track. Must be on its own line."),

            new Descriptor("TrackShow",                   "The name of the show for the current track"),
            new Descriptor("TrackSeasonNumber",           "The season number for the current track"),
            new Descriptor("TrackEpisodeId",              "The episode id for the current track"),
            new Descriptor("TrackEpisodeNumber",          "The episode number for the current track"),
            new Descriptor("TrackReleaseDate",            "The release date for the current track"),

            new Descriptor("TrackSortAlbum",              "The album name used for sorting the current track"),
            new Descriptor("TrackSortAlbumArtist",        "The album artist used for sorting the current track"),
            new Descriptor("TrackSortArtist",             "The artist used for sorting the current track"),
            new Descriptor("TrackSortComposer",           "The composer used for sorting the current track"),
            new Descriptor("TrackSortName",               "The name used for sorting the current track"),
            new Descriptor("TrackSortShow",               "The show name used for sorting the current track"),

            new Descriptor("AlbumTrackNumber",            "The track number for the current track"),
            new Descriptor("AlbumTrackCount",             "The total number of tracks in the current album"),
            new Descriptor("AlbumDiscNumber",             "The disc number for the current track"),
            new Descriptor("AlbumDiscCount",              "The total number of discs in the current album"),
            new Descriptor("AlbumIsCompilation",          "'True' if the current album is a compilation"),
            new Descriptor("TrackIsPartOfGaplessAlbum",   "'True' if the current track is part of a gapless album"),

            new Descriptor("TrackPlayOrderIndex",         "The play order index of the current track, 1-based"),
            new Descriptor("TrackVolumeAdjustment",       "The volume adjustment for the current track, from -100 to 100 in %"),
            new Descriptor("TrackEQ",                     "The name of the current track's EQ"),
            new Descriptor("TrackIsExcludedFromShuffle",  "'True' if the current track is excluded from shuffle"),
            new Descriptor("TrackIsPodcast",              "'True' if the current track is a podcast"),
            new Descriptor("TrackRating",                 "The current track rating drawn as stars. Must be on its own line."),
            new Descriptor("AlbumRating",                 "The current album rating drawn as stars. Must be on its own line."),
            new Descriptor("TrackRatingValue",            "The current track rating, a value from 0 to 100"),
            new Descriptor("AlbumRatingValue",            "The current album rating, a value from 0 to 100"),
            new Descriptor("TrackRatingKind",             "The kind of rating for the current track: User or Computed"),
            new Descriptor("AlbumRatingKind",             "The kind of rating for the current album: User or Computed"),
            new Descriptor("TrackKind",                   "The kind of the current track: Unknown, File, CD, URL, Device, or Shared Library"),
            new Descriptor("TrackVideoKind",              "The kind of video of the current track: None, Movie, Music Video, or TV Show"),

            new Descriptor("TrackDateAdded",              "The date and time the current track was added"),
            new Descriptor("TrackModificationDate",       "The date and time the current track was last modified"),
            new Descriptor("TrackPlayedDate",             "The date and time the current track was last played"),
            new Descriptor("TrackSkippedDate",            "The date and time the current track was last skipped"),
            new Descriptor("TrackPlayedCount",            "The number of times the current track has been played"),
            new Descriptor("TrackSkippedCount",           "The number of times the current track has been skipped"),
            new Descriptor("TrackIsUnplayed",             "'True' if the current track is unplayed"),
            new Descriptor("TrackIsEnabled",              "'True' if the current track is enabled"),

            new Descriptor("TrackDuration:mm':'ss",       "The duration of the current track"),
            new Descriptor("TrackStart:mm':'ss",          "The time at which the current track starts"),
            new Descriptor("TrackFinish:mm':'ss",         "The time at which the current track finishes"),
            new Descriptor("TrackTime",                   "The duration of the current track, always in mm:ss format"),
            new Descriptor("TrackBookmarkTime:mm':'ss",   "The bookmark time of the current track"),
            new Descriptor("TrackRemembersBookmark",      "'True' if the current track remembers the bookmark time"),

            new Descriptor("TrackKindAsString",           "The kind of track, e.g. 'AAC Audio File'"),
            new Descriptor("TrackBitRate",                "The bit rate of the current track, in kbps"),
            new Descriptor("TrackSampleRate",             "The sample rate of the current track, in Hz"),
            new Descriptor("TrackSize:*",                 "The size of the current track"),
            new Descriptor("TrackLocation",               "The location of the current track"),
            new Descriptor("TrackURL",                    "The URL of the current track"),

            // Playlist
            new Descriptor("PlaylistName",                "The name of the current playlist"),
            new Descriptor("PlaylistKind",                "The kind of the current playlist: Unknown, Library, User, CD, Device, or Radio tuner"),
            new Descriptor("PlaylistDuration:mm':'ss",    "The duration of the entire current playlist"),
            new Descriptor("PlaylistTime",                "The duration of the entire current playlist, always in mm:ss format"),
            new Descriptor("PlaylistIsShuffling",         "'True' if the current playlist is shuffling"),
            new Descriptor("PlaylistSize:*",              "The size of the entire of the entire playlist"),
            new Descriptor("PlaylistRepeatMode",          "The repeat mode of the current playlist: Off, One song, or Entire playlist"),
            new Descriptor("PlaylistIsVisible",           "'True' if the current playlist is visible"),
            new Descriptor("CDArtist",                    "The artist of the current CD"),
            new Descriptor("CDComposer",                  "The composer of the current CD"),
            new Descriptor("CDGenre",                     "The genre of the current CD"),
            new Descriptor("CDYear",                      "The year of the current CD"),
            new Descriptor("CDIsCompilation",             "'True' if the current CD is a compilation"),
            new Descriptor("CDDiscNumber",                "The disc number in the album for the current CD"),
            new Descriptor("CDDiscCount",                 "The total number of discs in the album for the current CD"),
            new Descriptor("PlaylistIsShared",            "'True' if the current playlist is shared"),
            new Descriptor("PlaylistIsSmart",             "'True' if the current playlist is a smart playlist"),
            new Descriptor("PlaylistSpecialKind",         "The special king of the current playlist: None, Purchased Music, Party Shuffle, Podcasts, Folder, Videos, Music, Movies, TV Shows, or Audiobooks"),
            // Context.UserPlaylist.Parent (IITUserPlaylist **iParentPlayList)

            // Playlist Source
            new Descriptor("SourceName",                  "The name of the current track source"),
            new Descriptor("SourceKind",                  "The kind of the current track source: Unknown, Library, iPod, Audio CD, MP3 CD, Device, Radio tuner, or Shared library"),
            new Descriptor("SourceCapacity:*",            "The capacity of the current track source"),
            new Descriptor("SourceFreeSpace:*",           "The free space of the current track source"),
            new Descriptor("iPodSoftwareVersion",         "The version of the iPod software, if the currently playing track is on an iPod"),

            // App
            new Descriptor("CurrentStreamTitle",          "The title of the current stream"),
            new Descriptor("CurrentStreamURL",            "The URL of the current stream"),
            new Descriptor("PlayerState",                 "The current playback state: Stopped, Playing, Fast forwarding, or Rewinding"),
            new Descriptor("PlayerPosition:mm':'ss",      "The current playback time"),
            new Descriptor("PlaybackBar",                 "A bar that shows the playback of the current track. Must be on its own line."),
            new Descriptor("IsMuted",                     "'True' is iTunes is currently muted"),
            new Descriptor("Volume",                      "The current iTunes volume"),

            // Visual
            new Descriptor("VisualsEnabled",              "'True' if visuals are enabled"),
            new Descriptor("FullScreenVisuals",           "'True' if full screen visuals are enabled"),
            new Descriptor("VisualSize",                  "The size of the current visuals being used: Small, Medium, or Large"),
            new Descriptor("VisualName",                  "The name of the current visuals being used"),

            // EQ Preset
            new Descriptor("EQIsEnabled",                 "'True' if EQ is enabled"),
            new Descriptor("EQName",                      "The name of the current EQ"),
            new Descriptor("EQIsModifable",               "'True' if the current EQ is modifiable"),
            new Descriptor("EQPremapLevel",               "The EQ preamp level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ32hzBandLevel",             "The EQ 32hz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ64hzBandLevel",             "The EQ 64hz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ125hzBandLevel",            "The EQ 125hz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ250hzBandLevel",            "The EQ 250hz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ500hzBandLevel",            "The EQ 500hz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ1khzBandLevel",             "The EQ 1khz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ2khzBandLevel",             "The EQ 2khz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ4khzBandLevel",             "The EQ 4khz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ8khzBandLevel",             "The EQ 8khz band level, from -12.0 to +12.0 in db"),
            new Descriptor("EQ16khzBandLevel",            "The EQ 16khz band level, from -12.0 to +12.0 in db"),
        };
        #endregion

        #region Content Parsing
        /// <summary>The content currently being worked on</summary>
        private static Content content;

        /// <summary>The regular expression for finding properties that need to be replaced</summary>
        private static readonly Regex propertyRegex = new Regex(@"{({|(?<name>\w+)(:(?<format>[^}]*))?})", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        /// <summary>A regular expression for cleaning up the content by removing empty lines</summary>
        private static readonly Regex cleanup1Regex = new Regex("\n(\0+)\n", RegexOptions.ExplicitCapture | RegexOptions.Compiled); // => \n
        /// <summary>A regular expression for cleaning up the content by removing starting empty lines at the start and end</summary>
        private static readonly Regex cleanup2Regex = new Regex("^([\0\n]+)(\n|$)|(^|\n)([\0\n]+)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled); // => 

        /// <summary>The regular expression handler</summary>
        /// <param name="target">The regular expression target found</param>
        /// <returns>The content string</returns>
        private static string UpdateProperty(Match target)
        {
            // A double {{ is just a {
            if (target.Value == "{{") return "{";

            // Get the name and format from the target
            string name = target.Groups["name"].Value.ToLower();
            string format = target.Groups["format"].Value.ToLower(); // empty string if no format

            // Find the Prop object, and get the value from it
            Prop p;
            return available_props.TryGetValue(name, out p) ? p.GetValue(format) : "\0";
        }
        
        /// <summary>Get the display content</summary>
        /// <param name="itunes">The current iTunes variable</param>
        /// <param name="format">The format of the content</param>
        /// <returns>The generated content of the display</returns>
        public static Content GetDisplayContent(iTunesApp itunes, string format)
        {
            // Make sure only one thread is generating content at once
            lock (propCntxtObjs)
            {
                // Start a new content object
                content = new Content();

                // Setup the contexts
                propCntxtObjs[(int)Context.App] = itunes;
                propCntxtObjs[(int)Context.Visual] = itunes.CurrentVisual;
                propCntxtObjs[(int)Context.EQPreset] = itunes.CurrentEQPreset;

                IITTrack track = itunes.CurrentTrack;
                propCntxtObjs[(int)Context.Track] = track;
                propCntxtObjs[(int)Context.FileOrCDTrack] = track as IITFileOrCDTrack;
                propCntxtObjs[(int)Context.URLTrack] = track as IITURLTrack;

                IITPlaylist playlist = itunes.CurrentPlaylist;
                propCntxtObjs[(int)Context.Playlist] = playlist;
                propCntxtObjs[(int)Context.AudioCDPlaylist] = playlist as IITAudioCDPlaylist;
                propCntxtObjs[(int)Context.LibraryPlaylist] = playlist as IITLibraryPlaylist;
                propCntxtObjs[(int)Context.UserPlaylist] = playlist as IITUserPlaylist;

                IITSource source = playlist.Source;
                propCntxtObjs[(int)Context.Source] = source;
                propCntxtObjs[(int)Context.iPodSource] = source as IITIPodSource;

                // Use a regular expression to generate the content, see UpdateProperty
                content.Text = cleanup1Regex.Replace(cleanup2Regex.Replace(propertyRegex.Replace(format, UpdateProperty), ""), "\n").Replace("\0", "").Trim();

                // Return the content
                Content c = content;
                content = null;
                return c;
            }
        }
        #endregion
    }
}
