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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using iTunesInfo.Properties;

namespace iTunesInfo
{
    /// <summary>A descriptor list box that also has a toolbar that shows up over the item currently being hover over</summary>
    abstract class ToolbarListBox : DescriptorListBox
    {
        /// <summary>A button on the toolbar</summary>
        protected class Button
        {
            /// <summary>The image for the button</summary>
            public Image Image;
            /// <summary>The name of the button, this is sent as a id string</summary>
            public string Name;
            /// <summary>The tooltip used for the button</summary>
            public string Tooltip;
        }

        /// <summary>The item being hovered over, or ListBox.NoMatches for no item</summary>
        private int hover_index = ListBox.NoMatches;
        /// <summary>The buttons currently being displayed</summary>
        private Button[] current_buttons = null;
        /// <summary>The button that is being hovered over</summary>
        private Button hover_button = null;
        /// <summary>The rectangles for each of the buttons</summary>
        private Rectangle[] button_rects = null;
        /// <summary>The toolbar area, the sum of all of the button_rects</summary>
        private Rectangle toolbar_rect = Rectangle.Empty;

        /// <summary>Create a new ToolbarListBox, which is double buffered to eliminate flicker when changing which item has the toolbar</summary>
        public ToolbarListBox() { this.DoubleBuffered = true; }

        /// <summary>Draws an item in the list, which uses the base class code usually unless the toolbar is being shown for the item then the buttons are drawn</summary>
        /// <param name="e">Draw item event arguments</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Draw the item regularly
            base.OnDrawItem(e);

            // If hovering then draw the toolbar
            if (e.Index == this.hover_index && this.hover_index != ListBox.NoMatches)
            //if ((e.State & DrawItemState.Selected) != 0)
            {
                this.current_buttons = GetButtons(e.Index);
                this.button_rects = new Rectangle[this.current_buttons.Length];
                Graphics g = e.Graphics;
                SolidBrush bg = new SolidBrush(e.BackColor);
                int r = e.Bounds.Right, t = e.Bounds.Top, x = r, y = t + 1, bh = e.Bounds.Height;

                // Go through all the buttons, from right to left
                for (int i = this.current_buttons.Length - 1; i >= 0; --i)
                {
                    Button b = this.current_buttons[i];
                    int w = b.Image.Width;
                    x -= w + 1;

                    // Draw the button
                    g.FillRectangle(bg, x, y, w, b.Image.Height);
                    g.DrawImage(b.Image, x, y);

                    this.button_rects[i] = new Rectangle(x, t, w + 1, bh); // calculate the button rectangle
                }
                this.toolbar_rect = new Rectangle(x, t, r - x, bh); // get the entire toolbar area

                // Cleanup
                bg.Dispose();
            }
        }

        /// <summary>Get the buttons to use for the toolbar</summary>
        /// <param name="index">The index of the item which is having the toolbar drawn</param>
        /// <returns>The set of buttons</returns>
        protected abstract Button[] GetButtons(int index);

        /// <summary>Set which item is being hovered over</summary>
        /// <param name="index">The index to set to, or ListBox.NoMatches to unset it</param>
        private void SetHoverIndex(int index)
        {
            if (this.hover_index != index)
            {
                // Only update it if the index is different

                if (this.hover_index >= 0)
                    // Invalidate the area where the current toolbar is
                    this.Invalidate(this.toolbar_rect);

                this.hover_index = index;
                this.hover_button = null;
                this.ToolTip.SetToolTip(this, this.GetDefaultTooltip(index));

                if (index == ListBox.NoMatches)
                {
                    // No toolbar, clear the fields
                    this.current_buttons = null;
                    this.hover_button = null;
                    this.button_rects = null;
                    this.toolbar_rect = Rectangle.Empty;
                }
            }
            if (index >= 0)
            {
                // Invalidate the area where the (new) toolbar will be
                this.Invalidate(this.GetItemRectangle(index));
            }
        }

        /// <summary>Get the button that is under a particular point</summary>
        /// <param name="p">The point to check</param>
        /// <returns>The button that is under the point, or null if there is no button under the point</returns>
        private Button ButtonFromPoint(Point p)
        {
            if (this.toolbar_rect.Contains(p)) // make sure the point is within the toolbar
                for (int i = 0; i < this.button_rects.Length; ++i)
                    if (this.button_rects[i].Contains(p))
                        return this.current_buttons[i]; // found the right button
            return null;
        }

        /// <summary>Do nothing</summary>
        protected override void OnItemAdded() { base.OnItemAdded(); }
        /// <summary>If the inserted item is before the hovered item then update the toolbar</summary>
        /// <param name="index">The index of the new element</param>
        protected override void OnItemInserted(int index)
        {
            if (index <= this.hover_index)
                this.SetHoverIndex(this.hover_index + 1);
            base.OnItemInserted(index);
        }
        /// <summary>If the removed item has the toolbar, clear the toolbar, otherwise if the removed element is before the item with the toolbar then update the toolbar</summary>
        /// <param name="index">The index of the removed element</param>
        protected override void OnItemRemoved(int index)
        {
            if (index == this.hover_index)
                this.SetHoverIndex(ListBox.NoMatches);
            else if (index < this.hover_index)
                this.SetHoverIndex(this.hover_index - 1);
            base.OnItemRemoved(index);
        }
        /// <summary>Clear the hover index</summary>
        protected override void OnItemsReset() { this.SetHoverIndex(ListBox.NoMatches); base.OnItemsReset(); }

        /// <summary>Set the tooltip for the list control, overridden to support button tooltips</summary>
        /// <param name="pt">The point where the mouse is</param>
        protected override void SetTooltip(Point pt)
        {
            // TODO: need to do this AFTER the toolbar has been redrawn
            int index = this.IndexFromPoint(pt);
            Button b = this.ButtonFromPoint(pt);
            if (this.hover_button != b)
            {
                this.ToolTip.SetToolTip(this, b == null ? this.GetDefaultTooltip(index) : b.Tooltip);
                this.hover_button = b;
            }
        }

        /// <summary>Update which item is showing the toolbar and change the cursor if the mouse moved over the toolbar</summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.SetHoverIndex(this.IndexFromPoint(e.Location));
            // TODO: need to do this AFTER the toolbar has been redrawn
            this.Cursor = this.toolbar_rect.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
            base.OnMouseMove(e);
        }

        /// <summary>Hide the toolbar</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            this.SetHoverIndex(ListBox.NoMatches);
            base.OnMouseLeave(e);
        }

        /// <summary>Possibly register a button click</summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            Button b = this.ButtonFromPoint(e.Location);
            if (b != null)
                this.OnButtonClicked(new PropertyChangedEventArgs(b.Name));
            base.OnMouseClick(e);
        }

        /// <summary>Fired when a toolbar button is clicked; the PropertyName of the event is the button name (see specific derivations for more information)</summary>
        public event PropertyChangedEventHandler ButtonClicked;

        /// <summary>Fires the ButtonClicked event</summary>
        /// <param name="e">The event arguments; the PropertyName of the event is the button name (see specific derivations for more information)</param>
        protected virtual void OnButtonClicked(PropertyChangedEventArgs e) { if (this.ButtonClicked != null) this.ButtonClicked(this, e); }
    }

    /// <summary>A list box for actions, generates button clicks for "up", "down", and "remove"</summary>
    class ActionsListBox : ToolbarListBox
    {
        /// <summary>The up button</summary>
        private static Button Up = new Button { Image = Resources.up, Name = "up", Tooltip = "Move Up" };
        /// <summary>The down button</summary>
        private static Button Down = new Button { Image = Resources.down, Name = "down", Tooltip = "Move Down" };
        /// <summary>The remove button</summary>
        private static Button Remove = new Button { Image = Resources.x, Name = "remove", Tooltip = "Remove" };
        /// <summary>Collection of buttons for up, down, and remove</summary>
        private static Button[] UpDownRemove = new Button[] { Up, Down, Remove };
        /// <summary>Collection of buttons for down and remove</summary>
        private static Button[] DownRemove = new Button[] { Down, Remove };
        /// <summary>Collection of buttons for up and remove</summary>
        private static Button[] UpRemove = new Button[] { Up, Remove };
        /// <summary>Collection of buttons for just remove</summary>
        private static Button[] RemoveOnly = new Button[] { Remove };

        /// <summary>Gets the set of buttons to display for a specific item</summary>
        /// <param name="index">The index of the item</param>
        /// <returns>A collection of buttons</returns>
        protected override ToolbarListBox.Button[] GetButtons(int index)
        {
            // Only show up if it isn't the first item
            // Only show down if it isn't the last item
            bool show_up = index > 0, show_down = index < this.Items.Count - 1;
            return show_up ? (show_down ? UpDownRemove : UpRemove) : (show_down ? DownRemove : RemoveOnly);
        }
    }

    /// <summary>A list box for events, generates button clicks for "edit" (key events only) and "remove"</summary>
    class EventsListBox : ToolbarListBox
    {
        /// <summary>The edit button</summary>
        private static Button Edit = new Button { Image = Resources.edit, Name = "edit", Tooltip = "Change Key" };
        /// <summary>The remove button</summary>
        private static Button Remove = new Button { Image = Resources.x, Name = "remove", Tooltip = "Remove / Clear" };
        /// <summary>Collection of buttons for edit and remove</summary>
        private static Button[] EditRemove = new Button[] { Edit, Remove };
        /// <summary>Collection of buttons for just remove</summary>
        private static Button[] RemoveOnly = new Button[] { Remove };

        /// <summary>Gets the set of buttons to display for a specific item</summary>
        /// <param name="index">The index of the item</param>
        /// <returns>A collection of buttons</returns>
        protected override ToolbarListBox.Button[] GetButtons(int index)
        {
            // Only show the edit button if it is a key event
            return this.Items[index].ToString().StartsWith("Key: ") ? EditRemove : RemoveOnly;
        }
    }
}
