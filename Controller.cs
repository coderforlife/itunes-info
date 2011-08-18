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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using iTunesLib;

namespace iTunesInfo
{
    /// <summary>The controller is the main program class program. It manages the iTunes object, monitors the events, runs the necessary actions, and works with the displays.</summary>
    /// <remarks>This is implemented as a form so that it has a complete Windows message loop, the form is never displayed</remarks>
    class Controller : Form
    {
        #region Names

        /// <summary>The descriptors for all of the possible events</summary>
        public static readonly Descriptor[] EventNames = 
        {
            new Descriptor("Play", "When a track starts to play"), new Descriptor("Stop", "When a track stops playing"),
            new Descriptor("TrackInfoChange", "When the information is edited for the current track"),
            new Descriptor("VolumeChanged", "When a track starts to play or the volume in iTunes"),
            new Descriptor("GotFocus", "When the display window gains focus"), new Descriptor("LostFocus", "When the display window loses focus"),
            new Descriptor("Enter", "When the mouse enters the display window"), new Descriptor("Leave", "When the mouse leaves the display window"),
            new Descriptor("Wheel", "When the mouse wheel is used over the display window"),
            new Descriptor("Click", "When the display window is clicked"), new Descriptor("LeftClick", "When the display window is left clicked"), new Descriptor("RightClick", "When the display window is right clicked"), new Descriptor("MiddleClick", "When the display window is middle clicked"), new Descriptor("X1Click", "When the display window is X1 clicked"), new Descriptor("X2Click", "When the display window is X2 clicked"),
            new Descriptor("DoubleClick", "When the display window is double clicked"), new Descriptor("LeftDoubleClick", "When the display window is left doubled clicked"), new Descriptor("RightDoubleClick", "When the display window is right doubled clicked"), new Descriptor("MiddleDoubleClick", "When the display window is middle doubled clicked"), new Descriptor("X1DoubleClick", "When the display window is X1 doubled clicked"), new Descriptor("X2DoubleClick", "When the display window is X2 doubled clicked"),
        };

        /// <summary>The descriptors for all of the possible actions</summary>
        public static readonly Descriptor[] ActionNames = 
        {
            new Descriptor("Play", "Play a track"),
            new Descriptor("Pause", "Pause the current track"),
            new Descriptor("Stop", "Stop the current track"),
            new Descriptor("PlayPause", "Toggle playing of the current track"),

            new Descriptor("BackTrack", "Restart the current track"),
            new Descriptor("NextTrack", "Skip to the next track"),
            new Descriptor("PreviousTrack", "Skip to the previous track"),

            new Descriptor("Rewind", "Start rewinding the current track"),
            new Descriptor("FastForward", "Start fast forwarding the current track"),
            new Descriptor("Resume", "Stop rewinding or fast forwarding the current track"),

            new Descriptor("ToggleShuffle", "Toggle shuffling of the current playlist"),

            new Descriptor("ClearTrackRating", "Remove the rating for the current track"),
            new Descriptor("RateTrack05Stars", "Set the rating of the current track to 1/2 star"),
            new Descriptor("RateTrack1Stars", "Set the rating of the current track to 1 star"),
            new Descriptor("RateTrack15Stars", "Set the rating of the current track to 1 1/2 stars"),
            new Descriptor("RateTrack2Stars", "Set the rating of the current track to 2 stars"),
            new Descriptor("RateTrack25Stars", "Set the rating of the current track to 2 1/2 stars"),
            new Descriptor("RateTrack3Stars", "Set the rating of the current track to 3 stars"),
            new Descriptor("RateTrack35Stars", "Set the rating of the current track to 3 1/2 stars"),
            new Descriptor("RateTrack4Stars", "Set the rating of the current track to 4 stars"),
            new Descriptor("RateTrack45Stars", "Set the rating of the current track to 4 1/2 stars"),
            new Descriptor("RateTrack5Stars", "Set the rating of the current track to 5 stars"),

            new Descriptor("ClearAlbumRating", "Remove the rating for the current album"),
            new Descriptor("RateAlbum05Stars", "Set the rating of the current album to 1/2 star"),
            new Descriptor("RateAlbum1Stars", "Set the rating of the current track to 1 star"),
            new Descriptor("RateAlbum15Stars", "Set the rating of the current track to 1 1/2 stars"),
            new Descriptor("RateAlbum2Stars", "Set the rating of the current track to 2 stars"),
            new Descriptor("RateAlbum25Stars", "Set the rating of the current track to 2 1/2 stars"),
            new Descriptor("RateAlbum3Stars", "Set the rating of the current track to 3 stars"),
            new Descriptor("RateAlbum35Stars", "Set the rating of the current track to 3 1/2 stars"),
            new Descriptor("RateAlbum4Stars", "Set the rating of the current track to 4 stars"),
            new Descriptor("RateAlbum45Stars", "Set the rating of the current track to 4 1/2 stars"),
            new Descriptor("RateAlbum5Stars", "Set the rating of the current track to 5 stars"),

            new Descriptor("ShowTrackInfo", "Show the track info display window"),
            new Descriptor("HideTrackInfo", "Hide the track info display window"),
            new Descriptor("ShowTrackInfoNow", "Show the track info display window now (skips fading)"),
            new Descriptor("HideTrackInfoNow", "Hide the track info display window now (skips fading)"),
            new Descriptor("KeepTrackInfoOpen", "Keep the track info display window open"),
            new Descriptor("AllowTrackInfoToClose", "Allow the track info display window to automatically close"),
            new Descriptor("ShowOptions", "Show the options window"),

            new Descriptor("ToggleMute", "Toggle if iTunes is muted"),
            new Descriptor("VolumeUp", "Raise the iTunes volume"),
            new Descriptor("VolumeDown", "Lower the iTunes volume"),

            new Descriptor("ToggleITunes", "Show or hide the iTunes window"),
            new Descriptor("Quit", "Quit iTunes and the display window"),
            new Descriptor("QuitDisplay", "Quit the display window, but keep iTunes open"),

            new Descriptor("SleepHalfSec", "Pause for half a second before running the next action"),
            new Descriptor("Sleep1Sec", "Pause for a second before running the next action"),
            new Descriptor("Sleep5Sec", "Pause for five seconds before running the next action"),
            new Descriptor("Sleep10Sec", "Pause for ten seconds before running the next action"),
            new Descriptor("Sleep30Sec", "Pause for half a minute before running the next action"),
            new Descriptor("Sleep60Sec", "Pause for a minute before running the next action"),
        };

        /// <summary>Dictionary to convert from the lower-case action names to the camel-case versions, being easier to read</summary>
        public static readonly Dictionary<string, string> ActionNamesConverter = new Dictionary<string,string>()
        {
            {"play", "Play"},
            {"pause", "Pause"},
            {"stop", "Stop"},
            {"playpause", "PlayPause"},

            {"backtrack", "BackTrack"},
            {"nexttrack", "NextTrack"},
            {"previoustrack", "PreviousTrack"},

            {"rewind", "Rewind"},
            {"fastforward", "FastForward"},
            {"resume", "Resume"},

            {"toggleshuffle", "ToggleShuffle"},

            {"cleartrackrating", "ClearTrackRating"},
            {"ratetrack05stars", "RateTrack05Stars"},
            {"ratetrack1stars", "RateTrack1Stars"},
            {"ratetrack15stars", "RateTrack15Stars"},
            {"ratetrack2stars", "RateTrack2Stars"},
            {"ratetrack25stars", "RateTrack25Stars"},
            {"ratetrack3stars", "RateTrack3Stars"},
            {"ratetrack35stars", "RateTrack35Stars"},
            {"ratetrack4stars", "RateTrack4Stars"},
            {"ratetrack45stars", "RateTrack45Stars"},
            {"ratetrack5stars", "RateTrack5Stars"},

            {"clearalbumrating", "ClearAlbumRating"},
            {"ratealbum05stars", "RateAlbum05Stars"},
            {"ratealbum1stars", "RateAlbum1Stars"},
            {"ratealbum15stars", "RateAlbum15Stars"},
            {"ratealbum2stars", "RateAlbum2Stars"},
            {"ratealbum25stars", "RateAlbum25Stars"},
            {"ratealbum3stars", "RateAlbum3Stars"},
            {"ratealbum35stars", "RateAlbum35Stars"},
            {"ratealbum4stars", "RateAlbum4Stars"},
            {"ratealbum45stars", "RateAlbum45Stars"},
            {"ratealbum5stars", "RateAlbum5Stars"},

            {"showtrackinfo", "ShowTrackInfo"},
            {"hidetrackinfo", "HideTrackInfo"},
            {"showtrackinfonow", "ShowTrackInfoNow"},
            {"hidetrackinfonow", "HideTrackInfoNow"},
            {"keeptrackinfoopen", "KeepTrackInfoOpen"},
            {"allowtrackinfotoclose", "AllowTrackInfoToClose"},
            {"showoptions", "ShowOptions"},

            {"togglemute", "ToggleMute"},
            {"volumeup", "VolumeUp"},
            {"volumedown", "VolumeDown"},

            {"toggleitunes", "ToggleITunes"},
            {"quit", "Quit"},
            {"quitdisplay", "QuitDisplay"},

            {"sleephalfsec", "SleepHalfSec"},
            {"sleep1sec", "Sleep1Sec"},
            {"sleep5sec", "Sleep5Sec"},
            {"sleep10sec", "Sleep10Sec"},
            {"sleep30sec", "Sleep30Sec"},
            {"sleep60sec", "Sleep60Sec"},
        };
        #endregion

        #region Actions and Events

        /// <summary>Dictionary for getting an action from it's lower-case name</summary>
        public Dictionary<string, ThreadStart> Name2Action = new Dictionary<string, ThreadStart>();
        /// <summary>Dictionary for getting the lower-case name for an action</summary>
        public Dictionary<ThreadStart, string> Action2Name = new Dictionary<ThreadStart, string>();
        /// <summary>All of the actions for non-key events</summary>
        public new Dictionary<string, List<ThreadStart>> Events = new Dictionary<string, List<ThreadStart>>();
        /// <summary>All of the actions for key events</summary>
        public Dictionary<Keys, List<ThreadStart>> KeyEvents = new Dictionary<Keys, List<ThreadStart>>();

        /// <summary>Create the Name2Action and Action2Name dictionaries</summary>
        private void CreatePossibleActions()
        {
            Name2Action.Clear();
            Action2Name.Clear();

            Name2Action.Add("play", this.Play);
            Name2Action.Add("pause", this.Pause);
            Name2Action.Add("stop", this.Stop);
            Name2Action.Add("playpause", this.PlayPause);

            Name2Action.Add("backtrack", this.BackTrack);
            Name2Action.Add("nexttrack", this.NextTrack);
            Name2Action.Add("previoustrack", this.PreviousTrack);

            Name2Action.Add("rewind", this.Rewind);
            Name2Action.Add("fastforward", this.FastForward);
            Name2Action.Add("resume", this.Resume);

            Name2Action.Add("toggleshuffle", this.ToggleShuffle);

            Name2Action.Add("cleartrackrating", this.RateTrack0);
            Name2Action.Add("ratetrack05stars", this.RateTrack0_5);
            Name2Action.Add("ratetrack1stars", this.RateTrack1);
            Name2Action.Add("ratetrack15stars", this.RateTrack1_5);
            Name2Action.Add("ratetrack2stars", this.RateTrack2);
            Name2Action.Add("ratetrack25stars", this.RateTrack2_5);
            Name2Action.Add("ratetrack3stars", this.RateTrack3);
            Name2Action.Add("ratetrack35stars", this.RateTrack3_5);
            Name2Action.Add("ratetrack4stars", this.RateTrack4);
            Name2Action.Add("ratetrack45stars", this.RateTrack4_5);
            Name2Action.Add("ratetrack5stars", this.RateTrack5);

            Name2Action.Add("clearalbumrating", this.RateAlbum0);
            Name2Action.Add("ratealbum05stars", this.RateAlbum0_5);
            Name2Action.Add("ratealbum1stars", this.RateAlbum1);
            Name2Action.Add("ratealbum15stars", this.RateAlbum1_5);
            Name2Action.Add("ratealbum2stars", this.RateAlbum2);
            Name2Action.Add("ratealbum25stars", this.RateAlbum2_5);
            Name2Action.Add("ratealbum3stars", this.RateAlbum3);
            Name2Action.Add("ratealbum35stars", this.RateAlbum3_5);
            Name2Action.Add("ratealbum4stars", this.RateAlbum4);
            Name2Action.Add("ratealbum45stars", this.RateAlbum4_5);
            Name2Action.Add("ratealbum5stars", this.RateAlbum5);

            Name2Action.Add("showtrackinfo", this.ShowTrackInfo);
            Name2Action.Add("hidetrackinfo", this.HideTrackInfo);
            Name2Action.Add("showtrackinfonow", this.ShowTrackInfoNow);
            Name2Action.Add("hidetrackinfonow", this.HideTrackInfoNow);
            Name2Action.Add("keeptrackinfoopen", this.KeepTrackInfoOpen);
            Name2Action.Add("allowtrackinfotoclose", this.AllowTrackInfoToClose);
            Name2Action.Add("showoptions", this.ShowOptions);

            Name2Action.Add("togglemute", this.ToggleMute);
            Name2Action.Add("volumeup", this.VolumeUp);
            Name2Action.Add("volumedown", this.VolumeDown);

            Name2Action.Add("toggleitunes", this.ToggleITunes);
            Name2Action.Add("quit", this.Quit);
            Name2Action.Add("quitdisplay", this.DisQuit);

            Name2Action.Add("sleephalfsec", this.Sleep_5);
            Name2Action.Add("sleep1sec", this.Sleep1);
            Name2Action.Add("sleep5sec", this.Sleep5);
            Name2Action.Add("sleep10sec", this.Sleep10);
            Name2Action.Add("sleep30sec", this.Sleep30);
            Name2Action.Add("sleep60sec", this.Sleep60);

            foreach (var i in Name2Action)
                Action2Name.Add(i.Value, i.Key);
        }

        /// <summary>Set the events and key events to default choices</summary>
        private void SetEventsToDefaults()
        {
            this.Events.Clear();
            this.Events.Add("play",             new List<ThreadStart>() { this.ShowTrackInfo         });
            this.Events.Add("trackinfochange",  new List<ThreadStart>() { this.ShowTrackInfo         });
            this.Events.Add("gotfocus",         new List<ThreadStart>() { this.KeepTrackInfoOpen     });
            this.Events.Add("lostfocus",        new List<ThreadStart>() { this.AllowTrackInfoToClose });
            this.Events.Add("enter",            new List<ThreadStart>() { this.KeepTrackInfoOpen     });
            this.Events.Add("leave",            new List<ThreadStart>() { this.AllowTrackInfoToClose });
            this.Events.Add("leftclick",        new List<ThreadStart>() { this.HideTrackInfoNow      });
            this.Events.Add("rightclick",       new List<ThreadStart>() { this.ShowOptions           });
            this.Events.Add("leftdoubleclick",  new List<ThreadStart>() { this.NextTrack             });
            this.KeyEvents.Clear();
            this.KeyEvents.Add(new Keys(Key.Ctrl, Key.Alt, Key.Left_Win, Key.P),     new List<ThreadStart>() { this.PlayPause     });
            this.KeyEvents.Add(new Keys(Key.Ctrl, Key.Alt, Key.Left_Win, Key.Left),  new List<ThreadStart>() { this.PreviousTrack });
            this.KeyEvents.Add(new Keys(Key.Ctrl, Key.Alt, Key.Left_Win, Key.Right), new List<ThreadStart>() { this.NextTrack     });
            this.KeyEvents.Add(new Keys(Key.Ctrl, Key.Alt, Key.Left_Win, Key.O),     new List<ThreadStart>() { this.ShowOptions   });
            this.KeyEvents.Add(new Keys(Key.Ctrl, Key.Alt, Key.Left_Win, Key.Q),     new List<ThreadStart>() { this.DisQuit       });
        }

        /// <summary>Get the nice camel-case name of an action from an action</summary>
        /// <param name="a">The action</param>
        /// <returns>The camel-case name</returns>
        public string GetActionName(ThreadStart a) { return ActionNamesConverter[this.Action2Name[a]]; }
        /// <summary>Get the descriptor for an action</summary>
        /// <param name="a">The action</param>
        /// <returns>The descriptor</returns>
        public Descriptor GetActionDesc(ThreadStart a)
        {
            string name = this.Action2Name[a];
            // Cycle through all actions descriptors
            foreach (Descriptor d in ActionNames)
                if (d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) // match the name
                    return d;
            return default(Descriptor);
        }
        /// <summary>Helper function to add an action to an event</summary>
        /// <typeparam name="T">The key for the event (either string or Keys)</typeparam>
        /// <param name="events">The events dictionary (either this.Events or this.KeyEvents)</param>
        /// <param name="evnt">The event key</param>
        /// <param name="index">The index to insert at (must be valid)</param>
        /// <param name="a">The action to add</param>
        private static void AddActionToEventInternal<T>(Dictionary<T, List<ThreadStart>> events, T evnt, int index, ThreadStart a)
        {
            List<ThreadStart> actions;
            if (events.TryGetValue(evnt, out actions))
                actions.Insert(index, a); // already have the event, add the action
            else
                events[evnt] = new List<ThreadStart> { a }; // make a new set of actions for the event
        }
        /// <summary>Add an action by name to an event</summary>
        /// <param name="evnt">The event to add the action to</param>
        /// <param name="index">The index to add the action at (must be valid)</param>
        /// <param name="action">The name of the action to add, case is ignored</param>
        public void AddActionToEvent(string evnt, int index, string action)
        {
            ThreadStart a = this.Name2Action[action.ToLower()];
            if (evnt.StartsWith("Key: "))
                AddActionToEventInternal(this.KeyEvents, Keys.FromString(evnt.Substring(5)), index, a);
            else
                AddActionToEventInternal(this.Events, evnt.ToLower(), index, a);
        }
        /// <summary>Helper function to remove an action from an event</summary>
        /// <typeparam name="T">The key for the event (either string or Keys)</typeparam>
        /// <param name="events">The events dictionary (either this.Events or this.KeyEvents)</param>
        /// <param name="evnt">The event key</param>
        /// <param name="index">The index of the action to remove (must be valid)</param>
        private static void RemoveActionFromEventInternal<T>(Dictionary<T, List<ThreadStart>> events, T evnt, int index)
        {
            List<ThreadStart> actions;
            if (events.TryGetValue(evnt, out actions))
            {
                actions.RemoveAt(index);
                if (actions.Count == 0)
                    events.Remove(evnt);
            }
        }
        /// <summary>Remove an action from an event</summary>
        /// <param name="evnt">The event to remove the action from</param>
        /// <param name="index">The index of the action to remove (must be valid)</param>
        public void RemoveActionFromEvent(string evnt, int index)
        {
            if (evnt.StartsWith("Key: "))
                RemoveActionFromEventInternal(this.KeyEvents, Keys.FromString(evnt.Substring(5)), index);
            else
                RemoveActionFromEventInternal(this.Events, evnt.ToLower(), index);
        }
        /// <summary>Swap actions in an event</summary>
        /// <param name="evnt">The event to change the order of actions for</param>
        /// <param name="index1">The index of one of the actions</param>
        /// <param name="index2">The index of the other action</param>
        public void SwapActionsInEvent(string evnt, int index1, int index2)
        {
            List<ThreadStart> actions;
            bool isKey = evnt.StartsWith("Key: ");
            if (isKey && this.KeyEvents.TryGetValue(Keys.FromString(evnt.Substring(5)), out actions) || !isKey && this.Events.TryGetValue(evnt.ToLower(), out actions))
                actions.Swap(index1, index2);
        }
        /// <summary>Helper function to remove an entire event</summary>
        /// <typeparam name="T">The key for the event (either string or Keys)</typeparam>
        /// <param name="events">The events dictionary (either this.Events or this.KeyEvents)</param>
        /// <param name="evnt">The event key</param>
        private static void RemoveEventInternal<T>(Dictionary<T, List<ThreadStart>> events, T evnt)
        {
            List<ThreadStart> actions;
            if (events.TryGetValue(evnt, out actions))
                actions.Clear(); // ensure that if there are other references they are empty
            events.Remove(evnt);
        }
        /// <summary>Remove an entire event</summary>
        /// <param name="evnt">The event to remove</param>
        public void RemoveEvent(string evnt)
        {
            if (evnt.StartsWith("Key: "))
                RemoveEventInternal(this.KeyEvents, Keys.FromString(evnt.Substring(5)));
            else
                RemoveEventInternal(this.Events, evnt.ToLower());
        }
        #endregion

        #region Settings

        /// <summary>The default display text format, used when no display text format has been set</summary>
        public const string DefaultDisplayTextFormat = "{TrackName}\n{TrackAlbum}\n{TrackArtist}\n{TrackRating}\n{TrackArtwork}";

        /// <summary>The default maximum width</summary>
        public const int    DefaultMaxWidth      = 250;
        /// <summary>The default line spacing</summary>
        public const int    DefaultLineSpacing   = 3;
        /// <summary>The default glow size</summary>
        public const int    DefaultGlowSize      = 8;
        /// <summary>The default inside margin</summary>
        public const int    DefaultInsideMargin  = 6;
        /// <summary>The default outside margin</summary>
        public const int    DefaultOutsideMargin = 12;
        /// <summary>The default max opacity</summary>
        public const double DefaultMaxOpacity    = 0.9;
        /// <summary>The default fade time</summary>
        public const int    DefaultFadeTime      = 200;
        /// <summary>The default visible time</summary>
        public const int    DefaultVisibleTime   = 3000;
        /// <summary>The default desktop position</summary>
        public static readonly DesktopPos DefaultDesktopPosition = DesktopPos.NearClock;
        /// <summary>The default text color</summary>
        public static readonly Color      DefaultTextColor       = Color.Black;
        /// <summary>The default background color</summary>
        public static readonly Color      DefaultBackgroundColor = Color.White;
        //public static readonly Font       DefaultFont             = Form.DefaultFont;

        /// <summary>The current settings</summary>
        protected Dictionary<string, object> settings = new Dictionary<string, object>
        {
            { "MaxWidth",           DefaultMaxWidth         },
            { "LineSpacing",        DefaultLineSpacing      },
            { "GlowSize",           DefaultGlowSize         },
            { "InsideMargin",       DefaultInsideMargin     },
            { "OutsideMargin",      DefaultOutsideMargin    },
            { "MaxOpacity",         DefaultMaxOpacity       },
            { "FadeTime",           DefaultFadeTime         },
            { "VisibleTime",        DefaultVisibleTime      },
            { "DesktopPosition",    DefaultDesktopPosition  },
            { "TextColor",          DefaultTextColor        },
            { "BackgroundColor",    DefaultBackgroundColor  },
            { "Font",               DefaultFont             },
        };

        /// <summary>Set a setting for the TrackDisplay</summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="x">The new value</param>
        private void SetSetting<T>(string name, T x) { SetSetting(name, name, x); }
        /// <summary>Set a setting for the TrackDisplay</summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the setting in the Settings dictionary</param>
        /// <param name="prop">The name of the property in TrackDisplay</param>
        /// <param name="x">The new value</param>
        private void SetSetting<T>(string name, string prop, T x)
        {
            if (!this.settings[name].Equals(x))
            {
                this.settings[name] = x;

                // Set the properties on the TrackDisplay
                typeof(TrackDisplay).GetProperty(prop).SetValue(this.dis, x, null);
                if (this.backup != null)
                    typeof(TrackDisplay).GetProperty(prop).SetValue(this.backup, x, null);

                // Rebuild the form
                InvalidateForm(this.dis);
            }
        }
        /// <summary>Set a setting for the BasicTrackDisplay or GlassTrackDisplay</summary>
        /// <typeparam name="D">The type of display, either BasicTrackDisplay or GlassTrackDisplay</typeparam>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="x">The new value</param>
        private void SetSetting<D, T>(string name, T x) { SetSetting<D, T>(name, name, x); }
        /// <summary>Set a setting for the BasicTrackDisplay or GlassTrackDisplay</summary>
        /// <typeparam name="D">The type of display, either BasicTrackDisplay or GlassTrackDisplay</typeparam>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the setting in the Settings dictionary</param>
        /// <param name="prop">The name of the property in BasicTrackDisplay or GlassTrackDisplay</param>
        /// <param name="x">The new value</param>
        private void SetSetting<D, T>(string name, string prop, T x)
        {
            if (!this.settings[name].Equals(x))
            {
                this.settings[name] = x;

                if (this.dis is D)
                {
                    // Set the setting on the currently visible display
                    typeof(D).GetProperty(prop).SetValue(this.dis, x, null);
                    InvalidateForm(this.dis);
                }
                else if (this.backup != null && this.backup is D)
                    // Set the setting on the backup display
                    typeof(D).GetProperty(prop).SetValue(this.backup, x, null);
            }
        }


        /// <summary>Get or set the display format</summary>
        public string DisplayText         { get { return this.displayFormat; }                            set { if (this.displayFormat != value) { this.displayFormat = value; this.UpdateInfo(); } } }
        /// <summary>Get or set if glass is allowed</summary>
        public bool AllowGlass            { get { return this.glassAllowed; }                            set { if (this.glassAllowed != value) { this.glassAllowed = value; this.SetActiveDisplay(); } } }
        /// <summary>Get or set if iTunes will be minimized if it was started due to this program starting</summary>
        public bool MinimizeOnStart       { get { return this.minimizeOnStart; }                         set { this.minimizeOnStart = value; } }
        /// <summary>Get or set the maximum width of the display, in pixels</summary>
        public int MaxWidth               { get { return (int)this.settings["MaxWidth"]; }               set { SetSetting("MaxWidth", value); } }
        /// <summary>Get or set the line spacing used in the display, in pixels</summary>
        public int LineSpacing            { get { return (int)this.settings["LineSpacing"]; }            set { SetSetting("LineSpacing", value); } }
        /// <summary>Get or set the inside margin of the display, in pixels</summary>
        public int InsideMargin           { get { return (int)this.settings["InsideMargin"]; }           set { SetSetting("InsideMargin", value); } }
        /// <summary>Get or set the outside margin of the display, in pixels</summary>
        public int OutsideMargin          { get { return (int)this.settings["OutsideMargin"]; }          set { SetSetting("OutsideMargin", value); } }
        /// <summary>Get or set the maximum opacity of the display, from 0 to 1</summary>
        public double MaxOpacity          { get { return (double)this.settings["MaxOpacity"]; }          set { SetSetting("MaxOpacity", value); } }
        /// <summary>Get or set the fading time of the display,m in milliseconds</summary>
        public int FadeTime               { get { return (int)this.settings["FadeTime"]; }               set { SetSetting("FadeTime", "DefaultFadeTime", value); } }
        /// <summary>Get or set the amount of time the display is completely visible for, in milliseconds</summary>
        public int VisibleTime            { get { return (int)this.settings["VisibleTime"]; }            set { SetSetting("VisibleTime", value); } }
        /// <summary>Get or set the position on the desktop that the window is used</summary>
        public DesktopPos DesktopPosition { get { return (DesktopPos)this.settings["DesktopPosition"]; } set { SetSetting("DesktopPosition", value); } }
        /// <summary>Get or set the font used in the display</summary>
        public Font DisplayFont           { get { return (Font)this.settings["Font"]; }                  set { SetSetting("Font", value); } }
        /// <summary>Get or set the text color used in the displays</summary>
        public Color TextColor            { get { return (Color)this.settings["TextColor"]; }            set { SetSetting("TextColor", "ForeColor", value); } }
        /// <summary>Get or set the background color of the basic track info displays</summary>
        public Color BackgroundColor      { get { return (Color)this.settings["BackgroundColor"]; }      set { SetSetting<BasicTrackDisplay, Color>("BackgroundColor", "BackColor", value); } }
        /// <summary>Get or set the size of the glow effect for glass track info displays, in pixels</summary>
        public int GlowSize               { get { return (int)this.settings["GlowSize"]; }               set { SetSetting<GlassTrackDisplay, int>("GlowSize", value); } }


        /// <summary>A regular expression to remove symbols from a string</summary>
        private static readonly Regex symbolCleanupRegex = new Regex(@"[\s/\\+_.-]", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>Convert a string to lower-case and remove many symbols and spaces that are in the string</summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The converted string</returns>
        private static string ToLowerAndStripSymbols(string str)
        {
            //return str.Trim().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("+", "").Replace("_", "").Replace("-", "").Replace(".", "").ToLower();
            return symbolCleanupRegex.Replace(str.Trim(), "").ToLower();
        }

        /// <summary>Get a setting from an XML element into the given variable</summary>
        /// <param name="settings">The Settings XML element</param>
        /// <param name="name">The name of the setting</param>
        /// <param name="val">The variable to save into</param>
        /// <returns>True if the variable was set, otherwise the variable was left with its default value</returns>
        private static bool GetSetting(XmlElement settings, string name, ref object val)
        {
            // The node with the value of the setting
            XmlNodeList n = settings.GetElementsByTagName(name);
            if (n != null && n.Count >= 1)
            {
                // Has a value, get it and the type to use
                string text = n[0].InnerText;
                Type T = val.GetType();

                // Get a value from the text
                if (T == typeof(Color))
                    val = ColorTranslator.FromHtml(text);
                else if (T == typeof(Font))
                    val = text.ConvertToFont();
                else if (T.IsEnum)
                    val = Enum.Parse(T, text);
                else
                    val = Convert.ChangeType(text, T);

                return true;
            }
            return false;
        }
        /// <summary>Get a setting from an XML element into the given variable, with a built-in cast</summary>
        /// <typeparam name="T">The type of the variable</typeparam>
        /// <param name="settings">The Settings XML element</param>
        /// <param name="name">The name of the setting</param>
        /// <param name="val">The variable to save into</param>
        private static void GetSetting<T>(XmlElement settings, string name, ref T val)
        {
            object o = val;
            if (GetSetting(settings, name, ref o))
                val = (T)o;
        }
        /// <summary>Get a setting from an XML element into the Settings dictionary</summary>
        /// <param name="settings">The Settings XML element</param>
        /// <param name="name">The name of the setting</param>
        private void GetSetting(XmlElement settings, string name)
        {
            object val = this.settings[name];
            if (GetSetting(settings, name, ref val))
                this.settings[name] = val;
        }
        /// <summary>Read the settings from the XML file</summary>
        /// <param name="settings">The Settings XML element</param>
        private void ReadSettings(XmlElement settings)
        {
            GetSetting(settings, "AllowGlass",      ref this.glassAllowed   );
            GetSetting(settings, "MinimizeOnStart", ref this.minimizeOnStart);
            GetSetting(settings, "MaxWidth"         );
            GetSetting(settings, "LineSpacing"      );
            GetSetting(settings, "GlowSize"         );
            GetSetting(settings, "InsideMargin"     );
            GetSetting(settings, "OutsideMargin"    );
            GetSetting(settings, "MaxOpacity"       );
            GetSetting(settings, "FadeTime"         );
            GetSetting(settings, "VisibleTime"      );
            GetSetting(settings, "DesktopPosition"  );
            GetSetting(settings, "TextColor"        );
            GetSetting(settings, "BackgroundColor"  );
            GetSetting(settings, "Font"             );
        }

        /// <summary>Get the actions from the XML file</summary>
        /// <param name="evt">The event XML element</param>
        /// <returns>The list of actions or null if there are no actions</returns>
        private List<ThreadStart> GetActions(XmlElement evt)
        {
            List<ThreadStart> actions = new List<ThreadStart>();
            foreach (XmlElement action in evt.ChildNodes)
            {
                // Convert the text to an action
                ThreadStart a;
                Name2Action.TryGetValue(ToLowerAndStripSymbols(evt.InnerText), out a);

                // Add it to the list
                if (a != null)
                    actions.Add(a);
            }
            return actions.Count == 0 ? null : actions;
        }
        /// <summary>Read the events from the XML file</summary>
        /// <param name="evts">The Events XML element</param>
        private void ReadEvents(XmlElement evts)
        {
            foreach (XmlElement evt in evts.ChildNodes)
            {
                // Get the actions
                List<ThreadStart> actions = GetActions(evt);
                if (actions == null)
                    continue;

                // Get the event key
                string when = evt.GetAttribute("When");

                // Add the event to the dictionary
                if (evt.LocalName == "KeyEvent")
                    KeyEvents.Add(Keys.FromString(when), actions);
                else // Event
                    Events.Add(ToLowerAndStripSymbols(when), actions);
            }
        }

        /// <summary>A regular expression to cleanup the display format</summary>
        private static readonly Regex displayFormatCleanupRegex = new Regex(@"\n\s+|\s+\n", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>Read the display format from the XML file</summary>
        /// <param name="raw">The raw text in the XML file</param>
        private void ReadDisplayFormat(string raw) { this.displayFormat = displayFormatCleanupRegex.Replace(raw.Trim(), ""); }

        /// <summary>Read all the settings from the XML file or use defaults</summary>
        private void ReadAllSettings()
        {
            // Create the list of possible actions
            CreatePossibleActions();

            // Create the settings for reading the XML file
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.IgnoreComments = true;

            XmlReader reader = null;
            try
            {
                // Load the document
                XmlDocument doc = new XmlDocument();
                doc.Load(reader = XmlReader.Create("settings.xml", settings));
                XmlElement root = doc.DocumentElement;

                // Read the settings, events and actions, and display format
                try { ReadSettings((XmlElement)root.GetElementsByTagName("Settings")[0]);   } catch (Exception) { }
                try { ReadEvents((XmlElement)root.GetElementsByTagName("Events")[0]);       } catch (Exception) { }
                try { ReadDisplayFormat(root.GetElementsByTagName("Display")[0].InnerText); } catch (Exception) { }
            }
            catch (Exception) { /* Use defaults */ }
            finally
            {
                // Cleanup
                if (reader != null)
                    reader.Close();

                // Check to make sure there is at least one event, otherwise use the defaults
                if (this.Events.Count == 0 && this.KeyEvents.Count == 0)
                    this.SetEventsToDefaults();
            }
        }
        
        /// <summary>Write events to the XML file</summary>
        /// <typeparam name="T">The type of event key (either string or Keys)</typeparam>
        /// <param name="xml">The XML writer to use</param>
        /// <param name="elemName">The element name to use for the events (either "Event" or "KeyEvent")</param>
        /// <param name="events">The dictionary of events (either this.Events or this.KeyEvents)</param>
        private void WriteEvents<T>(XmlWriter xml, string elemName, Dictionary<T, List<ThreadStart>> events)
        {
            foreach (var evnt in events)
            {
                if (evnt.Value.Count > 0)
                {
                    // Write all events with more than one action
                    xml.WriteStartElement(elemName);
                    xml.WriteAttributeString("When", evnt.Key.ToString());
                    foreach (ThreadStart a in evnt.Value)
                        xml.WriteElementString("Action", this.GetActionName(a));
                    xml.WriteEndElement();
                }
            }
        }
        /// <summary>Write all the settings to the XML file</summary>
        private void WriteAllSettings()
        {
            // Start the document
            XmlWriter xml = XmlWriter.Create("settings.xml");
            xml.WriteStartDocument();
            xml.WriteStartElement("iTunesInfo");

            // Write the basic settings
            xml.WriteStartElement("Settings");
            xml.WriteElementString("AllowGlass", this.glassAllowed.ToString());
            xml.WriteElementString("MinimizeOnStart", this.minimizeOnStart.ToString());
            foreach (var setting in this.settings)
            {
                object val = setting.Value;
                string text;
                if (val is Color)
                    text = ColorTranslator.ToHtml((Color)val);
                else if (val is Font)
                    text = ((Font)val).ConvertToString();
                else
                    text = val.ToString();
                xml.WriteElementString(setting.Key, text);
            }
            xml.WriteEndElement(); // Settings

            // Write the events / actions
            xml.WriteStartElement("Events");
            WriteEvents(xml, "Event", this.Events);
            WriteEvents(xml, "KeyEvent", this.KeyEvents);
            xml.WriteEndElement(); // Events

            // Write the display format text
            xml.WriteElementString("Display", this.displayFormat);

            // Finish up
            xml.WriteEndElement(); // iTunesInfo
            xml.WriteEndDocument();
            xml.Close();
        }
        #endregion

        #region Helpers for forms
        /// <summary>Show and activate a form, making sure that the Invoke is called as necessary</summary>
        /// <param name="f">The form to show and activate</param>
        private static void ShowForm(Form f)
        {
            if (f.InvokeRequired)
            {
                f.Invoke(new Delegates.Action(f.Show));
                f.BeginInvoke(new Delegates.Action(f.Activate));
            }
            else
            {
                f.Show();
                f.Activate();
            }
        }
        /// <summary>Close a form, making sure that the Invoke is called as necessary</summary>
        /// <param name="f">The form to close</param>
        private static void CloseForm(Form f)
        {
            if (f.InvokeRequired)
                f.BeginInvoke(new Delegates.Action(f.Close));
            else
                f.Close();
        }
        /// <summary>Invalidate an entire form, making sure that the Invoke is called as necessary</summary>
        /// <param name="f">The form to invalidate</param>
        private static void InvalidateForm(Form f)
        {
            if (f.InvokeRequired)
                f.BeginInvoke(new Delegates.Action(f.Invalidate));
            else
                f.Invalidate();
        }
        #endregion

        #region Display Management
        /// <summary>If a glass display is allowed</summary>
        private bool glassAllowed = true;
        /// <summary>If a glass display is currently be used</summary>
        private bool usingGlass = false;
        /// <summary>The primary display, the one being used</summary>
        private TrackDisplay dis;
        /// <summary>The backup display, available to be used if 'glassAllowed' or the DWM state changes</summary>
        private TrackDisplay backup = null;
        /// <summary>Setup a display window by copying all of the settings to it and adding the necessary events</summary>
        /// <param name="dis">The display to setup</param>
        private void SetupDisplay(TrackDisplay dis)
        {
            dis.MaxWidth = (int)this.settings["MaxWidth"];
            dis.LineSpacing = (int)this.settings["LineSpacing"];
            dis.InsideMargin = (int)this.settings["InsideMargin"];
            dis.OutsideMargin = (int)this.settings["OutsideMargin"];
            dis.MaxOpacity = (double)this.settings["MaxOpacity"];
            dis.FadeTime = (int)this.settings["FadeTime"];
            dis.VisibleTime = (int)this.settings["VisibleTime"];
            dis.DesktopPosition = (DesktopPos)this.settings["DesktopPosition"];
            dis.ForeColor = (Color)this.settings["TextColor"];
            dis.Font = (Font)this.settings["Font"];

            GlassTrackDisplay gd = dis as GlassTrackDisplay;
            if (gd != null)
            {
                gd.GlowSize = (int)this.settings["GlowSize"];
            }

            BasicTrackDisplay bd = dis as BasicTrackDisplay;
            if (bd != null)
            {
                bd.BackColor = (Color)this.settings["BackgroundColor"];
            }

            dis.GotFocus += new EventHandler(OnGotFocus);
            dis.LostFocus += new EventHandler(OnLostFocus);
            dis.MouseEnter += new EventHandler(OnMouseEnter);
            dis.MouseLeave += new EventHandler(OnMouseLeave);
            dis.MouseWheel += new MouseEventHandler(OnMouseWheel);
            dis.MouseClick += new MouseEventHandler(OnMouseClick);
            dis.MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);
        }
        /// <summary>Create the display(s) that will be used</summary>
        private void CreateDisplay()
        {
            if (Glass.Supported)
            {
                // Glass is supported need to create both a glass and basic display
                this.usingGlass = true;
                this.SetupDisplay(this.dis = new GlassTrackDisplay(this));
                this.SetupDisplay(this.backup = new BasicTrackDisplay(this));
                this.SetActiveDisplay();
            }
            else
            {
                // Glass is not supported, basic display only 
                this.SetupDisplay(this.dis = new BasicTrackDisplay(this));
            }
        }
        /// <summary>Overridden to detect changes in DWM composition changes</summary>
        /// <param name="m">The Windows message</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Glass.WM_DWMCOMPOSITIONCHANGED)
            {
                // The DWM Composition state changed (Glass.Enabled changed)
                SetActiveDisplay(); // update which display is being shown
                m.Result = IntPtr.Zero;
            }
            base.WndProc(ref m);
        }
        /// <summary>Set the display that is active, if there is more than one display that can be used</summary>
        protected void SetActiveDisplay()
        {
            if (this.backup != null && (!this.glassAllowed && this.usingGlass || this.usingGlass != Glass.Enabled))
            {
                // There is a backup display and it isn't consistent with the current settings
                // Need to toggle which display is showing
                bool vis = this.dis.Visible;
                this.dis.Invoke(new Delegates.Action(this.dis.Hide));
                TrackDisplay x = this.dis;
                this.dis = this.backup;
                this.backup = x;
                this.usingGlass = !this.usingGlass;
                this.UpdateInfo();
                if (vis)
                {
                    if (this.dis.IsHandleCreated)
                        this.dis.BeginInvoke(new Delegates.Action(this.dis.ShowInactiveTopmost));
                    else
                        this.dis.ShowInactiveTopmost();
                }
                this.dis.AllowedToAutoClose = true;
            }
        }
        /// <summary>Close the display forms and destroy them</summary>
        private void ShutdownDisplay()
        {
            if (this.dis != null)
            {
                CloseForm(this.dis);
                this.dis = null;
            }
            if (this.backup != null)
            {
                CloseForm(this.backup);
                this.backup = null;
            }
        }
        #endregion

        #region Options Dialog
        /// <summary>The options dialog</summary>
        private Options options;
        /// <summary>Creates the Options dialog</summary>
        private void CreateOptions()
        {
            this.options = new Options(this);
            IntPtr h = this.options.Handle; // required so that the handle is forcibly created so later we can easily show it from any thread
        }
        /// <summary>Close the Options dialog and destroy it</summary>
        private void ShutdownOptions()
        {
            if (this.options != null)
            {
                CloseForm(this.options);
                this.options = null;
            }
        }
        #endregion

        #region iTunes Object
        /// <summary>True is the iTunes application should be minimized if starting this program caused it to open</summary>
        private bool minimizeOnStart = false;
        /// <summary>True if the iTunes COM connection is currently disabled (deferring calls)</summary>
        private bool itunesComDisabled = true;
        /// <summary>The iTunes application object</summary>
        private iTunesApp itunes;
        /// <summary>The event handler for when iTunes starts playing a track</summary>
        private _IiTunesEvents_OnPlayerPlayEventEventHandler playEvent;
        /// <summary>The event handler for when iTunes stops playing a track</summary>
        private _IiTunesEvents_OnPlayerStopEventEventHandler stopEvent;
        /// <summary>The event handler for when the current track in iTunes has its information change</summary>
        private _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler trackInfoChangedEvent;
        /// <summary>The event handler for when iTunes UI is enabled</summary>
        private _IiTunesEvents_OnUserInterfaceEnabledEventEventHandler uiEnabledEvent;
        /// <summary>The event handler for when iTunes is about to quit</summary>
        private _IiTunesEvents_OnAboutToPromptUserToQuitEventEventHandler aboutToQuitEvent;
        /// <summary>The event handler for when iTunes is quitting</summary>
        private _IiTunesEvents_OnQuittingEventEventHandler quitEvent;
        /// <summary>The event handler for when the sound volume of iTunes changes</summary>
        private _IiTunesEvents_OnSoundVolumeChangedEventEventHandler volumeChangedEvent;
        /// <summary>The event handler for when iTunes disabled (defers) COM calls</summary>
        private _IiTunesEvents_OnCOMCallsDisabledEventEventHandler comCallsDisabledEvent;
        /// <summary>The event handler for when iTunes enabled (un-defers) COM calls</summary>
        private _IiTunesEvents_OnCOMCallsEnabledEventEventHandler comCallsEnabledEvent;
        /// <summary>The event handler for when the iTunes database changes</summary>
        private _IiTunesEvents_OnDatabaseChangedEventEventHandler databaseChangedEvent;
        /// <summary>Gets true if iTunes is currently open</summary>
        private static bool IsITunesOpen
        { get {
                // TODO: Is there a better way? Say, checking for a mutex that iTunes opens?
                return System.Diagnostics.Process.GetProcessesByName("iTunes").Length > 0;
        } }
        /// <summary>Setup the iTunes connection and events</summary>
        private void SetupITunes()
        {
            // Minimize after creating the iTunes connection only if iTunes is not currently open
            bool minimize = this.minimizeOnStart && !IsITunesOpen;

            // Create the iTunes connection
            try
            {
                this.itunes = new iTunesApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem connecting to iTunes:\n"+ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            this.itunesComDisabled = false;

            // Possibly minimize the window
            if (minimize)
                this.itunes.BrowserWindow.Minimized = true;

            // Add the iTunes events
            this.itunes.OnPlayerPlayEvent               += this.playEvent               = new _IiTunesEvents_OnPlayerPlayEventEventHandler(this.OnPlay);
            this.itunes.OnPlayerStopEvent               += this.stopEvent               = new _IiTunesEvents_OnPlayerStopEventEventHandler(this.OnStop);
            this.itunes.OnPlayerPlayingTrackChangedEvent+= this.trackInfoChangedEvent   = new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(this.OnTrackInfoChanged);
            this.itunes.OnUserInterfaceEnabledEvent     += this.uiEnabledEvent          = new _IiTunesEvents_OnUserInterfaceEnabledEventEventHandler(this.OnUIEnabled);
            this.itunes.OnAboutToPromptUserToQuitEvent  += this.aboutToQuitEvent        = new _IiTunesEvents_OnAboutToPromptUserToQuitEventEventHandler(this.OnAboutToQuit);
            this.itunes.OnQuittingEvent                 += this.quitEvent               = new _IiTunesEvents_OnQuittingEventEventHandler(this.OnQuitting);
            this.itunes.OnSoundVolumeChangedEvent       += this.volumeChangedEvent      = new _IiTunesEvents_OnSoundVolumeChangedEventEventHandler(this.OnVolumeChanged);
            this.itunes.OnCOMCallsDisabledEvent         += this.comCallsDisabledEvent   = new _IiTunesEvents_OnCOMCallsDisabledEventEventHandler(this.ComCallsDisabled);
            this.itunes.OnCOMCallsEnabledEvent          += this.comCallsEnabledEvent    = new _IiTunesEvents_OnCOMCallsEnabledEventEventHandler(this.ComCallsEnabled);
            this.itunes.OnDatabaseChangedEvent          += this.databaseChangedEvent    = new _IiTunesEvents_OnDatabaseChangedEventEventHandler(this.OnDatabaseChanged);
        }
        /// <summary>Event is called when COM calls become disabled (deferred)</summary>
        /// <param name="reason">The reason COM calls are being disabled (deferred) [one of Dialog, Quitting, or Other]</param>
        private void ComCallsDisabled(ITCOMDisabledReason reason) { this.itunesComDisabled = true; }
        /// <summary>Event is called when COM calls become enabled (non-deferred) again</summary>
        private void ComCallsEnabled() { this.itunesComDisabled = false; }
        /// <summary>Get if iTunes calls are currently being deferred</summary>
        public bool iTunesDeferred { get { return this.itunesComDisabled; } }
        /// <summary>Close the connection to iTunes which is very important since iTunes otherwise tells the user a plugin isn't responding and other plugins are messed up</summary>
        /// <remarks>This involves removing all the added events, possibly quitting iTunes, and releasing the iTunes object</remarks>
        /// <param name="quit">If true, causes iTunes to quit, otherwise just the connection is closed</param>
        private void ShutdownITunes(bool quit)
        {
            if (this.itunes != null)
            {
                this.itunesComDisabled = true;

                // Remove all the added events
                this.itunes.OnPlayerPlayEvent                   -= this.playEvent;
                this.itunes.OnPlayerStopEvent                   -= this.stopEvent;
                this.itunes.OnPlayerPlayingTrackChangedEvent    -= this.trackInfoChangedEvent;
                this.itunes.OnUserInterfaceEnabledEvent         -= this.uiEnabledEvent;
                this.itunes.OnAboutToPromptUserToQuitEvent      -= this.aboutToQuitEvent;
                this.itunes.OnQuittingEvent                     -= this.quitEvent;
                this.itunes.OnSoundVolumeChangedEvent           -= this.volumeChangedEvent;
                this.itunes.OnCOMCallsDisabledEvent             -= this.comCallsDisabledEvent;
                this.itunes.OnCOMCallsEnabledEvent              -= this.comCallsEnabledEvent;
                this.itunes.OnDatabaseChangedEvent              -= this.databaseChangedEvent;

                // Possibly quit iTunes
                if (quit) this.itunes.Quit();

                // Release the iTunes object
                Marshal.ReleaseComObject(this.itunes);
                this.itunes = null;
            }
        }
        #endregion

        /// <summary>Starts the program</summary>
        /// <remarks>This involves reading all settings in (or using defaults), connecting to iTunes, creating the display(s) and option forms, and starting event monitoring</remarks>
        public Controller()
        {
            this.Icon = iTunesInfo.Properties.Resources.icon; // should never be seen, but might as well set it
            this.Text = "iTunes Track Information Controller"; // should never be seen, but might as well set it
            this.ReadAllSettings();
            this.SetupITunes();
            this.CreateDisplay();
            this.CreateOptions();
            KeyMonitor.KeyChange += new KeyEventHandler(this.OnKeyChanged);
            KeyMonitor.Start();
            this.UpdateInfo();
            this.dis.FadeIn();
        }

        /// <summary>When being destroyed, call the shutdown function to make sure the quit is clean</summary>
        ~Controller() { this.Shutdown(true); }
        
        /// <summary>Quits this program (and possibly iTunes)</summary>
        /// <remarks>This involves stopping event monitoring, closing all forms, closing the iTunes connection (very important!) and saving the settings</remarks>
        /// <param name="itunesQuit">If true, quits iTunes along with this program, otherwise just quits this program</param>
        private void Shutdown(bool itunesQuit)
        {
            KeyMonitor.Stop();
            this.ShutdownDisplay();
            this.ShutdownOptions();
            this.ShutdownITunes(itunesQuit);
            CloseForm(this);
            this.WriteAllSettings();
            Application.Exit();
        }

        /// <summary>Overridden to force the form to always be hidden</summary>
        /// <param name="value">If the form is becoming visible</param>
        protected override void SetVisibleCore(bool value) { base.SetVisibleCore(false); }

        /// <summary>Set the rating for the current track</summary>
        /// <param name="x">The rating, from 0 to 5, rounded to the closest half</param>
        private void SetTrackRating(double x) // 
        {
            IITTrack track = itunes.CurrentTrack;
            if (track != null)
            {
                int rating = Convert.ToInt32(x * 2) * 10;
                track.Rating = rating.Clamp(0, 100);
            }
        }
        /// <summary>Set the album rating for the current track, which only does something for tracks that are URLs, Files, or on a CD</summary>
        /// <param name="x">The rating, from 0 to 5, rounded to the closest half</param>
        private void SetAlbumRating(double x)
        {
            IITTrack track = itunes.CurrentTrack;
            if (track != null)
            {
                int rating = Convert.ToInt32(x * 2) * 10;
                rating = rating.Clamp(0, 100);

                // Get the track as a different type so that the album rating can be set
                IITURLTrack urlT;
                IITFileOrCDTrack fileT;
                if ((urlT = track as IITURLTrack) != null)
                    urlT.AlbumRating = rating;
                else if ((fileT = track as IITFileOrCDTrack) != null)
                    fileT.AlbumRating = rating;
            }
        }

        /// <summary>The display text format string which is used to generate the content that is displayed</summary>
        private string displayFormat = DefaultDisplayTextFormat;

        /// <summary>Update the displayed information by generating the content from the display format</summary>
        protected void UpdateInfo()
        {
            this.dis.Content = (this.itunes.CurrentTrack != null) ? Props.GetDisplayContent(this.itunes, this.displayFormat) : TrackDisplay.DefaultContent;
            InvalidateForm(dis);
        }

        #region Threadstartable Actions
        /// <summary>Cause iTunes to play</summary>
        public void Play()          { this.itunes.Play(); }
        /// <summary>Cause iTunes to pause playback</summary>
        public void Pause()         { this.itunes.Pause(); }
        /// <summary>Cause iTunes to stop playback</summary>
        public void Stop()          { this.itunes.Stop(); }
        /// <summary>Cause iTunes to toggle between play and pause</summary>
        public void PlayPause()     { this.itunes.PlayPause(); }
        /// <summary>Cause iTunes to go return to the start of the current track</summary>
        public void BackTrack()     { this.itunes.BackTrack(); }
        /// <summary>Cause iTunes to skip to the next track</summary>
        public void NextTrack()     { this.itunes.NextTrack(); }
        /// <summary>Cause iTunes to skip to the previous track</summary>
        public void PreviousTrack() { this.itunes.PreviousTrack(); }
        /// <summary>Cause iTunes to begin rewinding</summary>
        public void Rewind()        { this.itunes.Rewind(); }
        /// <summary>Cause iTunes to begin fast forwarding</summary>
        public void FastForward()   { this.itunes.FastForward(); }
        /// <summary>Cause iTunes to resume normal playback (stop rewinding or fast forwarding)</summary>
        public void Resume()        { this.itunes.Resume(); }
        /// <summary>Toggle iTunes' mute</summary>
        public void ToggleMute()    { this.itunes.Mute = !this.itunes.Mute; }
        /// <summary>Toggle iTunes' shuffle</summary>
        public void ToggleShuffle() { if (this.itunes.CanSetShuffle[this.itunes.CurrentPlaylist]) this.itunes.CurrentPlaylist.Shuffle = !this.itunes.CurrentPlaylist.Shuffle; }
        /// <summary>Set the rating of the current track to 0 stars</summary>
        public void RateTrack0()    { this.SetTrackRating(0); }
        /// <summary>Set the rating of the current track to 1/2 star</summary>
        public void RateTrack0_5()  { this.SetTrackRating(0.5); }
        /// <summary>Set the rating of the current track to 1 star</summary>
        public void RateTrack1()    { this.SetTrackRating(1); }
        /// <summary>Set the rating of the current track to 1 1/2 stars</summary>
        public void RateTrack1_5()  { this.SetTrackRating(1.5); }
        /// <summary>Set the rating of the current track to 2 stars</summary>
        public void RateTrack2()    { this.SetTrackRating(2); }
        /// <summary>Set the rating of the current track to 2 1/2 stars</summary>
        public void RateTrack2_5()  { this.SetTrackRating(2.5); }
        /// <summary>Set the rating of the current track to 3 stars</summary>
        public void RateTrack3()    { this.SetTrackRating(3); }
        /// <summary>Set the rating of the current track to 3 1/2 stars</summary>
        public void RateTrack3_5()  { this.SetTrackRating(3.5); }
        /// <summary>Set the rating of the current track to 4 stars</summary>
        public void RateTrack4()    { this.SetTrackRating(4); }
        /// <summary>Set the rating of the current track to 4 1/2 stars</summary>
        public void RateTrack4_5()  { this.SetTrackRating(4.5); }
        /// <summary>Set the rating of the current track to 5 stars</summary>
        public void RateTrack5()    { this.SetTrackRating(5); }
        /// <summary>Set the album rating of the current track to 0 stars</summary>
        public void RateAlbum0()    { this.SetAlbumRating(0); }
        /// <summary>Set the album rating of the current track to 1/2 star</summary>
        public void RateAlbum0_5()  { this.SetAlbumRating(0.5); }
        /// <summary>Set the album rating of the current track to 1 star</summary>
        public void RateAlbum1()    { this.SetAlbumRating(1); }
        /// <summary>Set the album rating of the current track to 1 1/2 stars</summary>
        public void RateAlbum1_5()  { this.SetAlbumRating(1.5); }
        /// <summary>Set the album rating of the current track to 2 stars</summary>
        public void RateAlbum2()    { this.SetAlbumRating(2); }
        /// <summary>Set the album rating of the current track to 2 1/2 stars</summary>
        public void RateAlbum2_5()  { this.SetAlbumRating(2.5); }
        /// <summary>Set the album rating of the current track to 3 stars</summary>
        public void RateAlbum3()    { this.SetAlbumRating(3); }
        /// <summary>Set the album rating of the current track to 3 1/2 stars</summary>
        public void RateAlbum3_5()  { this.SetAlbumRating(3.5); }
        /// <summary>Set the album rating of the current track to 4 stars</summary>
        public void RateAlbum4()    { this.SetAlbumRating(4); }
        /// <summary>Set the album rating of the current track to 4 1/2 stars</summary>
        public void RateAlbum4_5()  { this.SetAlbumRating(4.5); }
        /// <summary>Set the album rating of the current track to 5 stars</summary>
        public void RateAlbum5()    { this.SetAlbumRating(5); }
        /// <summary>Toggle if the main iTunes window is visible</summary>
        public void ToggleITunes()  { this.itunes.BrowserWindow.Visible = !this.itunes.BrowserWindow.Visible; }
        /// <summary>Increase the iTunes sound volume</summary>
        public void VolumeUp()      { itunes.SoundVolume += 1; }
        /// <summary>Decrease the iTunes sound volume</summary>
        public void VolumeDown()    { itunes.SoundVolume -= 1; }
        /// <summary>Show the track information display</summary>
        public void ShowTrackInfo() { dis.FadeIn(); }
        /// <summary>Hide the track information display</summary>
        public void HideTrackInfo() { dis.FadeOut(); }
        /// <summary>Show the track information display now (without fading)</summary>
        public void ShowTrackInfoNow()      { dis.ShowNow(); }
        /// <summary>Hide the track information display now (without fading)</summary>
        public void HideTrackInfoNow()      { dis.HideNow(); }
        /// <summary>Keep the track information display open instead of auto-closing</summary>
        public void KeepTrackInfoOpen()     { dis.AllowedToAutoClose = false; }
        /// <summary>Allow the track information display auto-closing after being open for a delay</summary>
        public void AllowTrackInfoToClose() { dis.AllowedToAutoClose = true; }
        /// <summary>Show the options dialog</summary>
        public void ShowOptions()   { ShowForm(this.options); }
        /// <summary>Quit iTunes and the track information display</summary>
        public void Quit()          { this.Shutdown(true); }
        /// <summary>Quit track information display but leaves iTunes open</summary>
        public void DisQuit()       { this.Shutdown(false); }
        /// <summary>Sleep for 1/2 second</summary>
        public void Sleep_5()       { Thread.Sleep(500); }
        /// <summary>Sleep for 1 second</summary>
        public void Sleep1()        { Thread.Sleep(1000); }
        /// <summary>Sleep for 5 seconds</summary>
        public void Sleep5()        { Thread.Sleep(5000); }
        /// <summary>Sleep for 10 seconds</summary>
        public void Sleep10()       { Thread.Sleep(10000); }
        /// <summary>Sleep for 30 seconds</summary>
        public void Sleep30()       { Thread.Sleep(30000); }
        /// <summary>Sleep for 60 seconds</summary>
        public void Sleep60()       { Thread.Sleep(60000); }
        #endregion

        #region Events
        /// <summary>Execute a list of actions using a thread pool</summary>
        /// <param name="actions">The list of actions to executes</param>
        /// <returns>True if there were actions in the list and they were successfully queued in the thread pool</returns>
        private bool ExecuteActions(List<ThreadStart> actions) { return actions != null && actions.Count > 0 && ThreadPool.QueueUserWorkItem(this.ExecuteActionsThread, actions); }
        /// <summary>Execute a list of actions within a thread</summary>
        /// <param name="param">The thread parameter, a list of ThreadStart objects</param>
        private void ExecuteActionsThread(object param)        { foreach (ThreadStart a in (List<ThreadStart>)param) a(); }
        /// <summary>Execute the actions associated with an event</summary>
        /// <param name="evt">The name of the event to execute</param>
        /// <returns>True if at least one action was executed</returns>
        private bool ExecuteEvent(string evt)                  { List<ThreadStart> t; return this.Events.TryGetValue(evt, out t) && this.ExecuteActions(t); }
        /// <summary>Execute the actions associated with a mouse button event</summary>
        /// <param name="b">The mouse buttons that are pressed</param>
        /// <param name="evt">The base name of the event (e.g. "click")</param>
        private void ExecuteButtonEvents(MouseButtons b, string evt)
        {
            this.ExecuteEvent(evt);
            if ((b & MouseButtons.Left) != 0)       this.ExecuteEvent("left" + evt);
            if ((b & MouseButtons.Right) != 0)      this.ExecuteEvent("right" + evt);
            if ((b & MouseButtons.Middle) != 0)     this.ExecuteEvent("middle" + evt);
            if ((b & MouseButtons.XButton1) != 0)   this.ExecuteEvent("x1" + evt);
            if ((b & MouseButtons.XButton2) != 0)   this.ExecuteEvent("x2" + evt);
        }

        /// <summary>Event is fired when a key is either pressed or released on the system</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The key event arguments</param>
        protected void OnKeyChanged(object sender, KeyEventArgs e)
        {
            Keys k = KeyMonitor.Keys;
            //Debug.WriteLine("OnKeyChanged: "+k);
            List<ThreadStart> t;
            if (this.KeyEvents.TryGetValue(k, out t))
                e.Handled = this.ExecuteActions(t);
        }

        /// <summary>Event is fired when the display gains focus</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnGotFocus(object sender, EventArgs e)              { Debug.WriteLine("OnGotFocus");         this.ExecuteEvent("gotfocus"); }
        /// <summary>Event is fired when the display loses focus</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnLostFocus(object sender, EventArgs e)             { Debug.WriteLine("OnLostFocus");        this.ExecuteEvent("lostfocus"); }
        /// <summary>Event is fired when the mouse enters the display</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnMouseEnter(object sender, EventArgs e)            { Debug.WriteLine("OnMouseEnter");       this.ExecuteEvent("enter"); }
        /// <summary>Event is fired when the mouse leaves the display</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnMouseLeave(object sender, EventArgs e)            { Debug.WriteLine("OnMouseLeave");       this.ExecuteEvent("leave"); }
        /// <summary>Event is fired when the mouse wheel is used on the display</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The mouse event arguments</param>
        protected void OnMouseWheel(object sender, MouseEventArgs e)       { Debug.WriteLine("OnMouseWheel");       this.ExecuteEvent("wheel"); }
        /// <summary>Event is fired when the mouse clicks the display</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The mouse event arguments</param>
        protected void OnMouseClick(object sender, MouseEventArgs e)       { Debug.WriteLine("OnMouseClick");       this.ExecuteButtonEvents(e.Button, "click"); }
        /// <summary>Event is fired when the mouse double clicks the display</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The mouse event arguments</param>
        protected void OnMouseDoubleClick(object sender, MouseEventArgs e) { Debug.WriteLine("OnMouseDoubleClick"); this.ExecuteButtonEvents(e.Button, "doubleclick"); }

        /// <summary>Event is fired when a track has started playing</summary>
        /// <param name="iTrack">Track object that played, of type IITTrack</param>
        protected void OnPlay(object iTrack)                { Debug.WriteLine("OnPlay");             this.UpdateInfo(); this.ExecuteEvent("play"); }
        /// <summary>Event is fired when a track has stopped playing</summary>
        /// <param name="iTrack">Track object that stopped, of type IITTrack</param>
        protected void OnStop(object iTrack)                { Debug.WriteLine("OnStop");             this.UpdateInfo(); this.ExecuteEvent("stop"); }
        /// <summary>Event is fired when information about the currently playing track has changed</summary>
        /// <param name="iTrack">Track object that changed, of type IITTrack</param>
        protected void OnTrackInfoChanged(object iTrack)    { Debug.WriteLine("OnTrackInfoChanged"); this.UpdateInfo(); this.ExecuteEvent("trackinfochange"); }

        /// <summary>Event is fired when iTunes UI is no longer disabled</summary>
        protected void OnUIEnabled()                        { Debug.WriteLine("OnUIEnabled");        /*this.ExecuteEvent("uienabled");*/ }
        /// <summary>Event is fired when iTunes is about to quit</summary>
        protected void OnAboutToQuit()                      { Debug.WriteLine("OnAboutToQuit");      this.Quit(); }
        /// <summary>Event is fired when iTunes is quitting</summary>
        protected void OnQuitting()                         { Debug.WriteLine("OnQuitting");         this.Quit(); }
        /// <summary>Event is fired when the sound volume has changed</summary>
        /// <param name="newVolume">The new volume setting, from 0 to 100</param>
        protected void OnVolumeChanged(int newVolume)       { Debug.WriteLine("OnVolumeChanged");    this.ExecuteEvent("volumechanged"); }

        /// <summary>Event when iTunes changes it's database, currently does nothing</summary>
        /// <remarks>
        /// The arguments are two dimensional arrays of integers (int[][]) with the first dimension is the object and the second dimension is:
        /// 0. Source ID
        /// 1. Playlist ID
        /// 2. Track ID
        /// 3. Track Database ID
        /// </remarks>
        /// <param name="deletedObjectIDs">Deleted objects</param>
        /// <param name="changedObjectIDs">Changed objects</param>
        protected void OnDatabaseChanged(object deletedObjectIDs, object changedObjectIDs)  { Debug.WriteLine("OnDatabaseChanged"); /*this.ExecuteEvent("databasechanged");*/ }

        #endregion
    }
}
