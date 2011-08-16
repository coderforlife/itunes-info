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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>All of the virtual key codes defined in Windows, the names are formatted so they can be converted to nice strings (by substituting ' ' for '_')</summary>
    enum Key : int
    {
        Left_Mouse_Button = 0x01, Right_Mouse_Button,
        Break,
        Middle_Button, X1_Mouse_Button, X2_Mouse_Button,
        // 07    undefined
        Backspace = 0x08, Tab,
        // 0A-0B reserved
        Clear = 0x0C, Enter,
        // 0E-0F undefined
        Shift = 0x10, Ctrl, Alt,
        Pause, Caps_Lock,
        IME_Kana_or_Hangul_Mode = 0x15,
        // 16    undefined
        IME_Junja_Mode = 0x17, IME_Final_Mode, IME_Hanja_or_Kanji_Mode = 0x19,
        // 1A    undefined
        Esc = 0x1B,
        IME_Convert = 0x1C, IME_Nonconvert, IME_Accept, IME_Mode_Change,
        Space = 0x20, Page_Up, Page_Down, End, Home, Left, Up, Right, Down,
        Select = 0x29, Print, Execute,
        Print_Screen = 0x2C, Insert, Delete, Help,
        X0 = 0x30, X1, X2, X3, X4, X5, X6, X7, X8, X9, // Standard numbers
        // 3A-40 undefined
        A = 0x41, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, // Standard ASCII values
        Left_Win = 0x5B, Right_Win, Apps,
        // 5E    reserved
        Sleep = 0x5F,
        NP_0 = 0x60, NP_1, NP_2, NP_3, NP_4, NP_5, NP_6, NP_7, NP_8, NP_9, // Number Pad numbers
        Multiply = 0x6A, Add, Seperator, Subtract, Decimal, Divide,
        F1 = 0x70, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24, 
        // 88-8F unassigned
  	    Num_Lock = 0x90, Scroll_Lock,
        // 92-96 OEM specific
        // 97-9F unassigned
        Left_Shift = 0xA0, Right_Shift, Left_Ctrl, Right_Ctrl, Left_Alt, Right_Alt,
        Browser_Back = 0xA6, Browser_Forward, Browser_Refresh, Browser_Stop, Browser_Search, Browser_Favorites, Browser_Home,
        Volume_Mute = 0xAD, Volume_Down, Volume_Up,
        Media_Next_Track = 0xB0, Media_Prev_Track, Media_Stop, Media_Play_Pause,
        Launch_Mail = 0xB4, Launch_Media, Launch_App_1, Launch_App_2,
        // B8-B9 reserved
        OEM_1 = 0xBA, OEM_Plus, OEM_Comma, OEM_Minus, OEM_Period, OEM_2, OEM_3,
        // C1-D7 reserved
        // D8-DA unassigned
        OEM_4 = 0xDB, OEM_5, OEM_6, OEM_7, OEM_8,
        // E0    reserved
        // E1    OEM specific
        OEM_102 = 0xE2,
        // E3-E4 OEM specific
        IME_Process = 0xE5,
        // E6    OEM specific
        Packet = 0xE7,
        // E8    unassigned
        // E9-F5 OEM specific
        Attn = 0xF6, CrSel, ExSel, Erase_EOF, Play, Zoom,
        No_Name = 0xFC, //reserved for future use
        PA1 = 0xFD, OEM_Clear
        // FF    unknown
    }

    /// <summary>A set of keys</summary>
    class Keys : IEquatable<Keys>, IComparable<Keys>, ICloneable
    {
        /// <summary>Copies the status of the 256 virtual keys to the specified buffer.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms646299.aspx</remarks>
        /// <param name="pbKeyState">The 256-byte array that receives the status data for each virtual key</param>
        /// <returns>If the function succeeds, the return value is true</returns>
        [DllImport("user32.dll")] private static extern int GetKeyboardState(byte[] pbKeyState);

        /// <summary>Total number of virtual key codes</summary>
        /// <remarks>vkCode are from 1 to 254, but GetKeyboardState uses 0-255</remarks>
        public const int Count = 256;

        /// <summary>The state of the keys, true is down</summary>
        private readonly bool[] keys = new bool[Count];

        /// <summary>An empty set of keys, all keys are up</summary>
        public static readonly Keys Empty = new Keys();

        /// <summary>Creates a new, empty, set of keys, all of which are up</summary>
        public Keys() { }

        /// <summary>Creates a new set of keys, with the given keys that are down</summary>
        /// <param name="ks">The list of keys that are down</param>
        public Keys(params Key[] ks) { foreach (Key k in ks) keys[(int)k] = true; }


        /// <summary>Clone this set of keys</summary>
        /// <returns>The new set of keys that is identical to this one</returns>
        public Keys Clone() { Keys k = new Keys(); this.keys.CopyTo(k.keys, 0); return k; }
        /// <summary>Clone this set of keys</summary>
        /// <returns>The new set of keys that is identical to this one</returns>
        object ICloneable.Clone() { return this.Clone(); }
        /// <summary>Copy the key data in this key set to another key set</summary>
        /// <param name="k">The keys to override</param>
        public void CopyTo(Keys k) { this.keys.CopyTo(k.keys, 0); }

        /// <summary>Process a modifier key: if the right or left is pressed mark the generic one</summary>
        /// <param name="x">The generic key</param>
        /// <param name="l">The left key</param>
        /// <param name="r">The right key</param>
        private void DoModifier(Key x, Key l, Key r) { keys[(int)x] = keys[(int)l] || keys[(int)r]; }
        /// <summary>Call DoModifier() for shift, ctrl, and alt</summary>
        private void DoModifiers()
        {
            DoModifier(Key.Shift, Key.Left_Shift, Key.Right_Shift);
            DoModifier(Key.Ctrl, Key.Left_Ctrl, Key.Right_Ctrl);
            DoModifier(Key.Alt, Key.Left_Alt, Key.Right_Alt);
        }
        /// <summary>Process a modifier key only if it was just pressed: if the right or left is pressed mark the generic one</summary>
        /// <param name="x">The generic key</param>
        /// <param name="l">The left key</param>
        /// <param name="r">The right key</param>
        /// <param name="k">The key that was just pressed</param>
        private void DoModifier(Key x, Key l, Key r, Key k) { if (k == l || k == r) keys[(int)x] = keys[(int)l] || keys[(int)r]; }
        /// <summary>Call DoModifier(k) for shift, ctrl, and alt</summary>
        /// <param name="k">The key that was just pressed</param>
        private void DoModifiers(Key k)
        {
            DoModifier(Key.Shift, Key.Left_Shift, Key.Right_Shift, k);
            DoModifier(Key.Ctrl, Key.Left_Ctrl, Key.Right_Ctrl, k);
            DoModifier(Key.Alt, Key.Left_Alt, Key.Right_Alt, k);
        }

        /// <summary>Get or set the state of a key</summary>
        /// <param name="i">The virtual key code</param>
        /// <returns>True if the key is down</returns>
        public bool this[int i] { get { return this.keys[i]; } set { this.keys[i] = value; DoModifiers((Key)i); } }
        /// <summary>Get or set the state of a key</summary>
        /// <param name="i">The key</param>
        /// <returns>True if the key is down</returns>
        public bool this[Key k] { get { return this.keys[(int)k]; } set { this.keys[(int)k] = value; DoModifiers(k); } }

        /// <summary>Get the virtual key code list of all down keys</summary>
        /// <returns>The list of virtual key codes currently down</returns>
        public int[] Get()
        {
            List<int> k = new List<int>();
            for (int i = 0; i < Count; ++i)
                if (keys[i]) k.Add(i);
            return k.ToArray();
        }
        /// <summary>Load the current state of the keyboard into the key set</summary>
        public void LoadCurrentState()
        {
            byte[] keyState = new byte[Count];
            if (GetKeyboardState(keyState) != 0)
                for (int i = 0; i < Count; ++i)
                    keys[i] = (keyState[i] & 0x80) != 0; // high nibble is the key state
            DoModifiers();
        }

        /// <summary>Gets if a Key is either the left or right Windows key</summary>
        /// <param name="k">The key to check</param>
        /// <returns>True if the key is the left or right Windows key</returns>
        public static bool IsWinKey(Key k) { return k >= Key.Left_Win && k <= Key.Right_Win; }
        /// <summary>Gets if a Key is either the shift, ctrl, or alt key, either in left, right, or generic format</summary>
        /// <param name="k">The key to check</param>
        /// <returns>True if the key is a modifier (Left/Right/Generic Shift/Ctrl/Alt)</returns>
        public static bool IsModifier(Key k) { return (k >= Key.Shift && k <= Key.Alt) || (k >= Key.Left_Shift && k <= Key.Right_Alt); }
        /// <summary>Get if the left or right Windows key is pressed in this key set</summary>
        public bool IsWinKeyPressed { get { return this.keys[(int)Key.Left_Win] || this.keys[(int)Key.Right_Win]; } }
        /// <summary>Get the modifier keys currently down, including the left and right Windows keys, as a WinForms Keys enumeration</summary>
        public System.Windows.Forms.Keys Modifiers
        { get {
            System.Windows.Forms.Keys mod = 0;
            if (this.keys[(int)Key.Shift])      mod  = System.Windows.Forms.Keys.Shift;
            if (this.keys[(int)Key.Ctrl])       mod |= System.Windows.Forms.Keys.Control;
            if (this.keys[(int)Key.Alt])        mod |= System.Windows.Forms.Keys.Alt;
            if (this.keys[(int)Key.Left_Win])   mod |= System.Windows.Forms.Keys.LWin;
            if (this.keys[(int)Key.Right_Win])  mod |= System.Windows.Forms.Keys.RWin;
            return mod;
        } }
        /// <summary>Gets if the key set only contains down modifier keys, including the left and right Windows keys</summary>
        public bool IsOnlyModifiers
        { get {
            for (int i = 0; i < Count; ++i)
                if (this.keys[i] && !IsModifier((Key)i) && !IsWinKey((Key)i))
                    return false;
            return true;
        } }

        /// <summary>Gets if the key set is currently all up keys</summary>
        public bool IsEmpty { get { for (int i = 0; i < Count; ++i) { if (this.keys[i]) return false; } return true; } }
        /// <summary>Checks if this key set is simply a subset of another key set</summary>
        /// <param name="k">The key set to compare to</param>
        /// <returns>True if this key set only has down keys that the other key set also has</returns>
        public bool IsSubsetOf(Keys k)
        {
            if (k == null) return false;
            for (int i = 0;                      i < (int)Key.Left_Win;   ++i)  if (keys[i] && !k.keys[i]) return false;
            for (int i = (int)Key.Right_Win + 1; i < (int)Key.Left_Shift; ++i)  if (keys[i] && !k.keys[i]) return false;
            for (int i = (int)Key.Right_Alt + 1; i < Count;               ++i)  if (keys[i] && !k.keys[i]) return false;
            return !IsWinKeyPressed || IsWinKeyPressed && k.IsWinKeyPressed;
        }
        /// <summary>Compare this key set to another key</summary>
        /// <remarks>This ignores status of left vs right keys</remarks>
        /// <param name="k">The key set to compare to</param>
        /// <returns>0 if the key sets are equal, -1 if this key set should come be the other key set, and 1 if this one is after the other</returns>
        public int CompareTo(Keys k)
        {
            if (k == null) return 1;
            for (int i = 0;                    i < (int)Key.Left_Win;   ++i)    if (keys[i] != k.keys[i]) return keys[i] ? 1 : -1;
            for (int i = (int)Key.Right_Win+1; i < (int)Key.Left_Shift; ++i)    if (keys[i] != k.keys[i]) return keys[i] ? 1 : -1;
            for (int i = (int)Key.Right_Alt+1; i < Count;               ++i)    if (keys[i] != k.keys[i]) return keys[i] ? 1 : -1;
            return this.IsWinKeyPressed ? (k.IsWinKeyPressed ? 0 : 1) : -1;
        }
        /// <summary>Check if this key set is the same as another key set</summary>
        /// <remarks>This ignores status of left vs right keys</remarks>
        /// <param name="k">The other key set</param>
        /// <returns>True if the two key sets are the same</returns>
        public bool Equals(Keys k)
        {
            if (k == null) return false;
            for (int i = 0;                    i < (int)Key.Left_Win;   ++i)    if (keys[i] != k.keys[i]) return false;
            for (int i = (int)Key.Right_Win+1; i < (int)Key.Left_Shift; ++i)    if (keys[i] != k.keys[i]) return false;
            for (int i = (int)Key.Right_Alt+1; i < Count;               ++i)    if (keys[i] != k.keys[i]) return false;
            return IsWinKeyPressed == k.IsWinKeyPressed;
        }
        /// <summary>Check if this key set is the same as another object</summary>
        /// <remarks>This ignores status of left vs right keys</remarks>
        /// <param name="k">The other object</param>
        /// <returns>True if the other object is 'Keys' and the two key sets are the same</returns>
        public override bool Equals(object obj) { return this.Equals(obj as Keys); }
        /// <summary>Get the hash code of the key set</summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int x = 23;
            for (int i = 0;                    i < (int)Key.Left_Win;   ++i)    x = hash(x, keys[i]);
            for (int i = (int)Key.Right_Win+1; i < (int)Key.Left_Shift; ++i)    x = hash(x, keys[i]);
            for (int i = (int)Key.Right_Alt+1; i < Count;               ++i)    x = hash(x, keys[i]);
            return hash(x, this.IsWinKeyPressed);
        }
        /// <summary>Calculate a single step of the hashing</summary>
        /// <param name="curHash">The current hash</param>
        /// <param name="b">If the next key is down</param>
        /// <returns>The updated hash code</returns>
        private static int hash(int curHash, bool b) { return 37 * curHash + (b ? 1 : 0); }

        /// <summary>Get a string description of a key</summary>
        /// <remarks>For the standard numbers this returns just the number (as a string), for number pad numbers this return "NP #", for unknown or reserved keys this returns "[#]" where # is the key code</remarks>
        /// <param name="vKey">The virtual key code</param>
        /// <returns>The description / name of the key</returns>
        public static string GetKeyDescription(int vKey)
        {
            Key k = (Key)vKey;
            string s = k.ToString().Replace('_', ' ');
            int x;
            if (int.TryParse(s, out x))
                s = '[' + s + ']';
            else if (s.Length == 2 && s[0] == 'X' && Char.IsDigit(s[1]))
                s = s.Substring(1);
            return s;
        }
        /// <summary>Utility to append a key description onto a string builder, checking if the key is down and adding a '+' if this isn't the first key that is down</summary>
        /// <param name="s">The string builder to append to</param>
        /// <param name="k">The key to check and description to use</param>
        private void Append(StringBuilder s, int k)
        {
            if (this.keys[k])
            {
                if (s.Length != 0) s.Append(" + ");
                s.Append(GetKeyDescription(k));
            }
        }
        /// <summary>Convert an entire key set to a string listing the keys that are down</summary>
        /// <remarks>Plus signs ('+') are used as separators, and Shift, Ctrl, Alt, Win are listed before any other keys</remarks>
        /// <returns>The string representing the down keys</returns>
        public override string ToString() {
            StringBuilder s = new StringBuilder();
            Append(s, (int)Key.Shift);
            Append(s, (int)Key.Ctrl);
            Append(s, (int)Key.Alt);
            if (this.IsWinKeyPressed) {
                if (s.Length != 0) s.Append(" + ");
                s.Append("Win");
            }
            for (int i = 0;                    i < (int)Key.Shift;      ++i)    Append(s, i);
            for (int i = (int)Key.Alt+1;       i < (int)Key.Left_Win;   ++i)    Append(s, i);
            for (int i = (int)Key.Right_Win+1; i < (int)Key.Left_Shift; ++i)    Append(s, i);
            for (int i = (int)Key.Right_Alt+1; i < Count;               ++i)    Append(s, i);
            return s.ToString();
        }
        /// <summary>Convert a string to title case (first letter of each word is capitalized)</summary>
        /// <remarks>"or" is always lowercase and "IME", "OEM", "EOF", and "PA1" are always fully capitalized</remarks>
        /// <param name="s">The string to convert</param>
        /// <returns>The converted string</returns>
        private static string TitleCase(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            string[] words = s.Trim().ToLower().Split();
            foreach (string word in words)
            {
                if (word == "") continue;
                if (word == "or")
                    sb.Append(word);
                else if (word == "ime" || word == "oem" || word == "eof" || word == "pa1")
                    sb.Append(word.ToUpper());
                else
                    sb.Append(Char.ToUpper(word[0])).Append(word.Substring(1));
                sb.Append(' ');
            }
            if (sb.Length > 0)
                // Remove trailing space
                sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
        /// <summary>Get a key set from a string that was created using ToString() [slight variations are allowed]</summary>
        /// <remarks>Both ',' and '+' are allowed as separators, trailing and extra spaces and separators are allowed, all key names that are not understood are ignored</remarks>
        /// <param name="s">The string of key names of pressed keys</param>
        /// <returns>The key set</returns>
        public static Keys FromString(string s)
        {
            if (s == null) return null;
            Keys k = new Keys();
            s = s.Replace(',', '+').Trim().Trim('+').Trim();
            string[] parts = s.Split('+');
            foreach (string part in parts)
            {
                string x = TitleCase(part).Replace(' ', '_');
                if (x.Length == 1 && Char.IsDigit(x[0]))
                    x = 'X' + x;
                else if (x == "Crsel" || x == "Exsel")
                    x = x.Replace('s', 'S');
                if (x.Length > 2 && x[0] == '[' && x[x.Length - 1] == ']')
                {
                    int i;
                    if (Int32.TryParse(x.Substring(1, x.Length - 2), out i) && i >= 0 && i < Count)
                        k[i] = true;
                }
                else if (x == "Win")
                {
                    k[(int)Key.Left_Win] = true;
                }
                else
                {
                    try
                    {
                        k[(Key)Enum.Parse(typeof(Key), x)] = true;
                    }
                    catch (Exception) { }
                }
            }
            return k;
        }
    }

    /// <summary>A utility for monitoring system keyboard events</summary>
    class KeyMonitor
    {
        #region Windows API

        /*[StructLayout(LayoutKind.Sequential)]
        private class KeyboardHookStruct
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }*/

        /// <summary>An application-defined callback function used with the SetWindowsHookEx function. The system calls this function every time a new keyboard input event is about to be posted into a thread input queue.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms644985.aspx</remarks>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message (nCode >= 0 means Action)</param>
        /// <param name="wParam">The identifier of the keyboard message: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP</param>
        /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT (http://msdn.microsoft.com/library/ms644967.aspx) structure</param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.
        /// If nCode is greater than or equal to zero it may return the value returned by CallNextHookEx or return a nonzero value to prevent the system from passing the message to the rest of the hook chain or the target window procedure.
        /// </returns>
        private delegate int HookProc(int nCode, UIntPtr wParam, IntPtr lParam);

        /// <summary>Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for certain types of events.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms644990.aspx</remarks>
        /// <param name="idHook">The type of hook procedure to be installed</param>
        /// <param name="lpfn">A pointer to the hook procedure</param>
        /// <param name="hMod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter</param>
        /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated. If this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure, otherwise NULL</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);
        /// <summary>Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms644993.aspx</remarks>
        /// <param name="idHook">A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function succeeds, the return value is true</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern int UnhookWindowsHookEx(int idHook);
        /// <summary>Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms644974.aspx</remarks>
        /// <param name="idHook">This parameter is ignored</param>
        /// <param name="nCode">The hook code passed to the current hook procedure</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure</param>
        /// <param name="lParam">The lParam value passed to the current hook procedure</param>
        /// <returns>This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value.</returns>
        [DllImport("user32.dll")] private static extern int CallNextHookEx(int idHook, int nCode, UIntPtr wParam, IntPtr lParam);

        /// <summary>Installs a hook procedure that monitors low-level keyboard input events</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms644990.aspx</remarks>
        private const int WH_KEYBOARD_LL = 13;

        /// <summary>When a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.</summary>
        private readonly static UIntPtr WM_KEYDOWN      = new UIntPtr(0x100);
        /// <summary>When a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus.</summary>
        private readonly static UIntPtr WM_KEYUP        = new UIntPtr(0x101);
        /// <summary>When the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has the keyboard focus.</summary>
        private readonly static UIntPtr WM_SYSKEYDOWN   = new UIntPtr(0x104);
        /// <summary>When the user releases a key that was pressed while the ALT key was held down. It also occurs when no window currently has the keyboard focus.</summary>
        private readonly static UIntPtr WM_SYSKEYUP     = new UIntPtr(0x105);

        #endregion

        /// <summary>Singleton KeyMonitor object</summary>
        private static KeyMonitor km = null;
        /// <summary>The private dummy constructor</summary>
        private KeyMonitor() { }
        /// <summary>When the singleton is cleaned up, the monitoring is stopped and errors are ignored</summary>
        ~KeyMonitor() { Stop(false); }

        /// <summary>Event when a key is pressed or released on the system, unless skipped due to the OverridingKeyChange event</summary>
        public static event KeyEventHandler KeyChange;
        /// <summary>Event when a key is pressed or released on the system, and if the e.Handled property is set then the KeyChange event is skipped</summary>
        public static event KeyEventHandler OverridingKeyChange;

        /// <summary>The handle of the hook, returned by SetWindowsHookEx</summary>
        private int hook = 0;
        /// <summary>The thread used to dispatch the events generated by the hook callback and placed on the queue</summary>
        private Thread dispatchThread = null;
        /// <summary>The thread used to setup the hook and add all the events to a queue</summary>
        private Thread hookThread = null;
        /// <summary>The hook callback function</summary>
        private HookProc keyboardHookProc = null;
        /// <summary>The current set of keys that are down</summary>
        private Keys keys = new Keys();
        /// <summary>The queue of key events generated by the hook callback but that haven't been dispatched yet</summary>
        private Queue<KeyEventData> queue = new Queue<KeyEventData>(64);

        /// <summary>The data that is needed to be sent from the hook to the dispatcher</summary>
        private struct KeyEventData
        {
            /// <summary>The virtual key code</summary>
            public int vk;
            /// <summary>True if the key changed to down</summary>
            public bool down;
        }

        /// <summary>Get the current key set state of the system (it may be slightly delayed from the actual system because of processing in the dispatch queue)</summary>
        public static Keys Keys { get { return km.keys; } }

        /// <summary>Add a key event onto the queue and notify the dispatcher thread</summary>
        /// <param name="vk">The virtual key code</param>
        /// <param name="down">True if the key was just pressed down</param>
        private static void Enqueue(int vk, bool down)
        {
            // Gain exclusive access to the queue
            lock (km.queue)
            {
                // Add the new event
                km.queue.Enqueue(new KeyEventData() { vk = vk, down = down });
                // Notify the dispatcher thread
                Monitor.Pulse(km.queue);
            }
        }

        /// <summary>Start monitoring system wide key events and firing the KeyChange events</summary>
        public static void Start()
        {
            if (km == null)
            {
                // Only the first time Start() is called...

                // Create singleton
                km = new KeyMonitor();
                // Make sure that the key monitor is cleaned up when the application ends
                Application.ApplicationExit += delegate(object sender, EventArgs e) { Stop(false); };
            }
            if (km.hook == 0)
            {
                // Not started yet

                // Setup the dispatching thread
                km.dispatchThread = new Thread(HookEventDispatchThread);
                km.dispatchThread.Name = "Keyboard Hook Event Dispatch";
                km.dispatchThread.IsBackground = true;

                // Setup the hook thread
                km.hookThread = new Thread(HookThread);
                km.hookThread.Name = "Keyboard Hook";
                km.hookThread.IsBackground = true;
                km.hookThread.Priority = ThreadPriority.Highest; // make sure Windows won't stop calling the callback due to timeouts

                // Make sure the previously started one has come to a complete stop
                while (km.queue.Count > 0) { Thread.Sleep(1); }

                // Start the threads
                km.dispatchThread.Start();
                km.hookThread.Start();
            }
        }
        /// <summary>Stop monitoring system wide key events and firing the KeyChange events</summary>
        public static void Stop() { Stop(true); }
        /// <summary>Internal function to stop monitoring system wide key events and firing the KeyChange events</summary>
        /// <param name="throwExceptions">True if exceptions should be thrown on errors</param>
        private static void Stop(bool throwExceptions)
        {
            // Shutdown the hook
            int winerr = 0;
            if (km.hook != 0)
            {
                if (UnhookWindowsHookEx(km.hook) == 0)
                    winerr = Marshal.GetLastWin32Error();
                km.hook = 0;
            }

            // Shutdown the dispatching thread by sending it a virtual key code of -1
            if (km.dispatchThread != null)  { Enqueue(-1, true); km.dispatchThread = null; }

            // Abort the hook thread
            if (km.hookThread != null)      { km.hookThread.Abort(); km.hookThread = null; }

            // Possibly throw an exception
            if (winerr != 0 && throwExceptions)
                throw new Win32Exception(winerr);
        }

        /// <summary>The thread that starts the hooking and runs a standard application message loop</summary>
        private static void HookThread()
        {
            // Start the hooking
            km.keyboardHookProc = new HookProc(KeyboardHookProc);
            km.hook = SetWindowsHookEx(WH_KEYBOARD_LL, km.keyboardHookProc, IntPtr.Zero, 0);
            if (km.hook == 0)
            {
                int winerr = Marshal.GetLastWin32Error();
                Stop(false);
                throw new Win32Exception(winerr);
            }

            // Load the current state of the keyboard as a starting point
            km.keys.LoadCurrentState();

            // Run the standard application message loop
            // The hook requires the message loop to function
            try
            {
                Application.Run();
            }
            catch (ThreadAbortException) { /* Done!*/ }
        }

        /// <summary>The hook callback which is called every time any key changes state and simply enqueues the event to be dealt with by the dispatch thread</summary>
        /// <remarks>
        /// http://msdn.microsoft.com/library/ms644985.aspx
        /// This function is deliberately very quick so that Windows has no reason to stop using it as a hook
        /// </remarks>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message (nCode >= 0 means Action)</param>
        /// <param name="wParam">The identifier of the keyboard message: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP</param>
        /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT (http://msdn.microsoft.com/library/ms644967.aspx) structure, of which the first value is the virtual key code</param>
        /// <returns>The result of CallNextHookEx with the same parameters that this function received</returns>
        private static int KeyboardHookProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // Conveniently the virtual key code is the first value in the KBDLLHOOKSTRUCT
                //KeyboardHookStruct k = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                Enqueue(Marshal.ReadInt32(lParam), wParam == WM_SYSKEYDOWN || wParam == WM_KEYDOWN);
            }
            return CallNextHookEx(km.hook, nCode, wParam, lParam);
        }

        /// <summary>The thread that dispatches keyboard events placed onto the queue by the hook</summary>
        private static void HookEventDispatchThread()
        {
            for (; ; )
            {
                KeyEventData data;

                // Gain exclusive access to the queue
                lock (km.queue)
                {
                    // If the queue is empty, wait to be notified of more data
                    if (km.queue.Count == 0) Monitor.Wait(km.queue);
                    // Get the next value
                    data = km.queue.Dequeue();
                }

                if (data.vk == -1)
                {
                    // The virtual key code represents termination, so cleanup and exit the thread
                    km.keys = new Keys();
                    km.queue.Clear();
                    return;
                }
                else if (km.keys[data.vk] == data.down)
                    // The event represents something that is already true, skip it
                    continue;

                // Update the keys information
                km.keys[data.vk] = data.down;

                // Create and dispatch the event
                KeyEventArgs e = new KeyEventArgs((System.Windows.Forms.Keys)data.vk | km.keys.Modifiers);
                if (OverridingKeyChange != null)        OverridingKeyChange(km, e);
                if (!e.Handled && KeyChange != null)    KeyChange(km, e);
            }
        }
    }
}
