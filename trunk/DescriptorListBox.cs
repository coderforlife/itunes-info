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
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>A list box that uses Descriptors for item text and uses the descriptions as tooltips, also allowing for items to be bolded</summary>
    class DescriptorListBox : ListBox
    {
        /// <summary>The format used for text items</summary>
        private static StringFormat strFormat = new StringFormat(StringFormat.GenericDefault);
        static DescriptorListBox() { strFormat.Trimming |= StringTrimming.None; }

        /// <summary>The tooltip object that displays tooltips for the descriptions</summary>
        private ToolTip tooltip = null;
        /// <summary>The last index which was hovered over (and a tooltip was shown for), or -1 for an invalid element</summary>
        private int lastHoverIndex = -1;
        /// <summary>The font used for bold items, derived from Font</summary>
        private Font boldFont;
        /// <summary>The brush used to draw the text, based on the ForeColor</summary>
        private SolidBrush textBrush;
        /// <summary>The list of items that are bold</summary>
        private List<bool> boldItems = new List<bool>();

        /// <summary>Create a new descriptor-based list box</summary>
        public DescriptorListBox() : base()
        {
            IntPtr h = this.Handle; // need to force handle to be created so that we receive all the LB WndProc message in a timely fashion
            this.DrawMode = DrawMode.OwnerDrawFixed;                // custom drawing
            this.IntegralHeight = false;                            // allow the height to be anything
            this.ScrollAlwaysVisible = true;                        // always show the scroll bar
            this.boldFont = new Font(this.Font, FontStyle.Bold);
            this.textBrush = new SolidBrush(this.ForeColor);
        }

        /// <summary>Update the bold font when the standard font is changed</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnFontChanged(EventArgs e) { this.boldFont.Dispose(); base.OnFontChanged(e); this.boldFont = new Font(this.Font, FontStyle.Bold); }
        /// <summary>Update the text brush color when the foreground color changes</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnForeColorChanged(EventArgs e) { base.OnForeColorChanged(e); this.textBrush.Color = this.ForeColor; }

        /// <summary>Windows message for when a string is added to the end of the list box</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb775181.aspx</remarks>
        private const int LB_ADDSTRING = 0x180;
        /// <summary>Windows message for when a string is inserted into a list box</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb761321.aspx</remarks>
        private const int LB_INSERTSTRING = 0x181;
        /// <summary>Windows message for when a string is deleted from a list box</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb775183.aspx</remarks>
        private const int LB_DELETESTRING = 0x182;
        /// <summary>Windows message for when all strings are removed from a list box. </summary>
        /// <remarks>http://msdn.microsoft.com/library/bb761325.aspx</remarks>
        private const int LB_RESETCONTENT = 0x184;

        /// <summary>Monitors the LB_ADDSTRING, LB_INSERTSTRING, LB_DELETESTRING, and LB_RESETCONTENT Windows messages, and calls the OnItemInserted, OnItemAdded, OnItemRemoved, and OnItemsReset functions respectively</summary>
        /// <param name="m">The Windows message to process</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == LB_ADDSTRING || m.Msg == LB_INSERTSTRING)
            {
                int index;
                if (m.Msg == LB_INSERTSTRING && (index = m.WParam.ToInt32()) >= 0)
                    OnItemInserted(index);
                else
                    OnItemAdded();
            }
            else if (m.Msg == LB_DELETESTRING) { OnItemRemoved(m.WParam.ToInt32()); }
            else if (m.Msg == LB_RESETCONTENT) { OnItemsReset(); }
            base.WndProc(ref m);
        }

        /// <summary>Whenever a new item is inserted into the list</summary>
        /// <param name="index">The index of the new element</param>
        protected virtual void OnItemInserted(int index)    { boldItems.Insert(index, false); }
        /// <summary>Whenever a new item is added to the end of the list</summary>
        protected virtual void OnItemAdded()                { boldItems.Add(false);}
        /// <summary>Whenever an item is removed from the list</summary>
        /// <param name="index">The index of the removed element</param>
        protected virtual void OnItemRemoved(int index)     { boldItems.RemoveAt(index); }
        /// <summary>Whenever all items are removed from the list</summary>
        protected virtual void OnItemsReset()               { boldItems.Clear(); }

        /// <summary>Possibly update the tooltip depending on what element the mouse moved over</summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.tooltip != null)
                this.SetTooltip(e.Location);
        }

        /// <summary>Draws the item, which is the same as the normally drawing system except that a bold font is sometimes used</summary>
        /// <param name="e">Draw item event arguments</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Get basic information
            int i = e.Index;
            bool invalid = i < 0 || i >= this.Items.Count;

            // Get the font to use, which is either the bold font, a derived bold font, or the event's font
            bool bold = !invalid && this.boldItems[i], disposeFont = bold && e.Font != this.Font;
            Font f = bold ? (disposeFont ? new Font(e.Font, FontStyle.Bold) : this.boldFont) : e.Font;

            // Get the brush to use, which is either the cached brush or a new one
            bool disposeBrush = e.ForeColor != this.ForeColor;
            Brush b = disposeBrush ? new SolidBrush(e.ForeColor) : this.textBrush;

            // Get the text that will be drawn
            string text = invalid ? this.Name : this.Items[i].ToString();

            // Perform the drawing
            e.DrawBackground();
            e.Graphics.DrawString(text, f, b, e.Bounds.Location, strFormat);
            e.DrawFocusRectangle();
            
            // Cleanup possibly created GDI objects
            if (disposeFont)    f.Dispose();
            if (disposeBrush)   b.Dispose();
        }

        /// <summary>Get the tooltip for an item</summary>
        /// <param name="index">The index of the item</param>
        /// <returns>The tooltip for the item, or the empty string for invalid items or if the item is not a Descriptor</returns>
        protected string GetDefaultTooltip(int index)
        {
            if (index >= 0 && index < this.Items.Count)
            {
                // Get the tooltip text for the item, if it is a Descriptor
                Descriptor d = this.Items[index] as Descriptor;
                if (d != null)
                    return d.Description;
            }
            return ""; // empty string for invalid indices
        }

        /// <summary>Set the tooltip for the list control</summary>
        /// <param name="pt">The point where the mouse is</param>
        protected virtual void SetTooltip(Point pt)
        {
            int index = this.IndexFromPoint(pt);
            if (index != this.lastHoverIndex)
            {
                // New element is being hovered over and set the tooltip text
                this.tooltip.SetToolTip(this, this.GetDefaultTooltip(this.lastHoverIndex = index));
            }
        }

        /// <summary>Get or set the ToolTip that displays the tooltips for items in the list</summary>
        public ToolTip ToolTip { get { return this.tooltip; } set { this.tooltip = value; } }

        /// <summary>Gets if a particular item is drawn bolded</summary>
        /// <param name="i">The index of the element</param>
        /// <returns>If the element is drawn bolded</returns>
        public bool GetBold(int i)              { return this.boldItems[i]; }
        /// <summary>Sets if a particular item is drawn bolded</summary>
        /// <param name="i">The index of the element</param>
        /// <param name="bold">If the element is drawn bolded</param>
        public void SetBold(int i, bool bold)   { if (this.boldItems[i] != bold) { this.boldItems[i] = bold; this.Refresh(); } }

        /// <summary>Get the index of an item given the value of the item</summary>
        /// <param name="d">The value of the item to look for</param>
        /// <returns>The index of the item, or -1 if not found</returns>
        public int GetItemIndex(Descriptor d)
        {
            int count = this.Items.Count;
            for (int i = 0; i < count; ++i)
                if (d.Equals(this.Items[i]))
                    return i;
            return -1;
        }

        /// <summary>Get the index of an item given the value of the item</summary>
        /// <param name="name">The string of the item to look for</param>
        /// <returns>The index of the item, or -1 if not found</returns>
        public int GetItemIndex(string name)
        {
            int count = this.Items.Count;
            for (int i = 0; i < count; ++i)
                if (name.Equals(this.Items[i].ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return i;
            return -1;
        }
    }

    /// <summary>A descriptor list where items can be chosen, not just selected</summary>
    class ChoosableListBox : DescriptorListBox
    {
        /// <summary>The delegate for the ChooseItem event</summary>
        /// <param name="items">The list box that generated the event</param>
        /// <param name="index">The index of the item that was chosen</param>
        /// <param name="item">The value of the item that was chosen</param>
        public delegate void ChooseItemHandler(ChoosableListBox items, int index, Descriptor item);

        /// <summary>Event for when an item is chosen, either from double clicking or pressing the Enter key</summary>
        public event ChooseItemHandler ChooseItem;

        /// <summary>Fires the ChooseItem event when an item is chosen</summary>
        /// <param name="index">The index of the item</param>
        /// <param name="item">The value of the item</param>
        protected virtual void OnChooseItem(int index, Descriptor item)
        {
            if (ChooseItem != null)
                ChooseItem(this, index, item);
        }

        /// <summary>Detect if an item was chosen</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            if (this.SelectedIndex >= 0)
                this.OnChooseItem(this.SelectedIndex, this.SelectedItem as Descriptor);
            base.OnDoubleClick(e);
        }

        /// <summary>Detect if an item was chosen (when the key code is Enter)</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && this.SelectedIndex >= 0)
                this.OnChooseItem(this.SelectedIndex, this.SelectedItem as Descriptor);
            base.OnKeyPress(e);
        }
    }
}
