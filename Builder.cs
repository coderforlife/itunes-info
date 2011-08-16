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

namespace iTunesInfo
{
    /// <summary>Utility class that adds extension methods to Controls that allow quickly adding new Controls</summary>
    static class Builder
    {
        /// <summary>Shorthand for AnchorStyles.Top | AnchorStyles.Left</summary>
        public static readonly AnchorStyles TopLeftAnchor = AnchorStyles.Top | AnchorStyles.Left;
        /// <summary>Shorthand for AnchorStyles.Top | AnchorStyles.Right</summary>
        public static readonly AnchorStyles TopRightAnchor = AnchorStyles.Top | AnchorStyles.Right;
        /// <summary>Shorthand for AnchorStyles.Bottom | AnchorStyles.Right</summary>
        public static readonly AnchorStyles BottomRightAnchor = AnchorStyles.Bottom | AnchorStyles.Right;
        /// <summary>Shorthand for AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right</summary>
        public static readonly AnchorStyles TopLeftRightAnchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        /// <summary>Shorthand for AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right</summary>
        public static readonly AnchorStyles TopBottomLeftRightAnchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        /// <summary>Add a new control to a container control with autosizing and no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, Point loc) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.AutoSize = true;
            c.Text = text;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, Point loc, Size sz) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.Text = text;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, Point loc, Size sz, AnchorStyles a) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.Text = text;
            c.TabStop = false;
            c.Anchor = a;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, Point loc, Size sz, int tab) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.Text = text;
            c.TabStop = true;
            c.TabIndex = tab;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, Point loc, Size sz, int tab, AnchorStyles a) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.Text = text;
            c.TabStop = true;
            c.TabIndex = tab;
            c.Anchor = a;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with autosizing and no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, Point loc) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.AutoSize = true;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, Point loc, Size sz) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, Point loc, Size sz, AnchorStyles a) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.TabStop = false;
            c.Anchor = a;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, Point loc, Size sz, int tab) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.TabStop = true;
            c.TabIndex = tab;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, Point loc, Size sz, int tab, AnchorStyles a) where C : Control, new()
        {
            C c = new C();
            c.Location = loc;
            c.Size = sz;
            c.TabStop = true;
            c.TabIndex = tab;
            c.Anchor = a;
            parent.Controls.Add(c);
            return c;
        }

        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="dock">The docking style of the control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, DockStyle dock) where C : Control, new()
        {
            C c = new C();
            c.Dock = dock;
            c.Text = text;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="dock">The docking style of the control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, string text, DockStyle dock, int tab) where C : Control, new()
        {
            C c = new C();
            c.Dock = dock;
            c.Text = text;
            c.TabStop = true;
            c.TabIndex = tab;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control with no tab stop</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="dock">The docking style of the control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, DockStyle dock) where C : Control, new()
        {
            C c = new C();
            c.Dock = dock;
            c.TabStop = false;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new control to a container control</summary>
        /// <typeparam name="C">The type of the new control</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="dock">The docking style of the control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new control</returns>
        public static C Add<C>(this Control parent, DockStyle dock, int tab) where C : Control, new()
        {
            C c = new C();
            c.Dock = dock;
            c.TabStop = true;
            c.TabIndex = tab;
            parent.Controls.Add(c);
            return c;
        }

        /// <summary>Add a new panel to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new panel</returns>
        public static Panel AddPanel(this Control parent, Point loc, Size sz, int tab, AnchorStyles a)
        {
            Panel p = parent.Add<Panel>(loc, sz, tab, a);
            p.SuspendLayout();
            return p;
        }
        /// <summary>Add a new group box to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="sz">The size of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new group box</returns>
        public static GroupBox AddGroupBox(this Control parent, string text, Point loc, Size sz, int tab)
        {
            GroupBox g = parent.Add<GroupBox>(text, loc, sz, tab);
            g.SuspendLayout();
            return g;
        }
        /// <summary>Add a new split container to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="fixedPanel">The panel which is the fixed panel</param>
        /// <returns>The new split container</returns>
        public static SplitContainer AddSplit(this Control parent, FixedPanel fixedPanel)
        {
            SplitContainer s = parent.Add<SplitContainer>(DockStyle.Fill, 0);
            s.SuspendLayout();
            s.Panel1.SuspendLayout();
            s.Panel2.SuspendLayout();
            s.FixedPanel = fixedPanel;
            return s;
        }
        /// <summary>Add a new tab to a TabControl</summary>
        /// <param name="parent">The tab control to add to</param>
        /// <param name="text">The text of the tab</param>
        /// <returns>The new tab page</returns>
        public static TabPage AddTab(this TabControl parent, string text)
        {
            TabPage t = new TabPage();
            t.SuspendLayout();
            t.Text = text;
            t.UseVisualStyleBackColor = true;
            t.TabStop = true;
            t.TabIndex = parent.TabCount;
            parent.Controls.Add(t);
            return t;
        }
        /// <summary>Add a new button to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="width">The width of the new button</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <returns>The new button</returns>
        public static Button AddButton(this Control parent, string text, Point loc, int width, int tab, AnchorStyles a)
        {
            Button b = parent.Add<Button>(text, loc, new Size(width, 23), tab, a);
            b.UseVisualStyleBackColor = true;
            return b;
        }
        /// <summary>Add a new button to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="width">The width of the new button</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="a">The anchor style of the new control</param>
        /// <param name="click">The event to use when the button is clicked</param>
        /// <returns>The new button</returns>
        public static Button AddButton(this Control parent, string text, Point loc, int width, int tab, AnchorStyles a, EventHandler click)
        {
            Button b = parent.Add<Button>(text, loc, new Size(width, 23), tab, a);
            b.UseVisualStyleBackColor = true;
            b.Click += click;
            return b;
        }
        /// <summary>Add a new (color) button to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="width">The width of the new button</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="click">The event to use when the button is clicked</param>
        /// <param name="click">The event to use when the background color is changed</param>
        /// <returns>The new (color) button</returns>
        public static Button AddButton(this Control parent, Point loc, int width, int tab, EventHandler click, EventHandler backColorChanged)
        {
            Button b = parent.Add<Button>(loc, new Size(width, 23), tab);
            b.UseVisualStyleBackColor = true;
            b.Click += click;
            b.BackColorChanged += backColorChanged;
            return b;
        }
        /// <summary>Add a new button to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="dock">The docking style of the control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <returns>The new button</returns>
        public static Button AddButton(this Control parent, string text, DockStyle dock, int tab)
        {
            Button b = parent.Add<Button>(text, dock, tab);
            b.UseVisualStyleBackColor = true;
            return b;
        }
        /// <summary>Add a new button to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="dock">The docking style of the control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="click">The event to use when the button is clicked</param>
        /// <returns>The new button</returns>
        public static Button AddButton(this Control parent, string text, DockStyle dock, int tab, EventHandler click)
        {
            Button b = parent.Add<Button>(text, dock, tab);
            b.UseVisualStyleBackColor = true;
            b.Click += click;
            return b;
        }
        /// <summary>Add a new checkbox to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="text">The text of the new control</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="checkedChanged">The event to use when the checkbox checks or unchecks</param>
        /// <returns>The new checkbox</returns>
        public static CheckBox AddCheckBox(this Control parent, string text, Point loc, int tab, EventHandler checkedChanged)
        {
            CheckBox c = new CheckBox();
            c.Location = loc;
            c.AutoSize = true;
            c.Text = text;
            c.TabIndex = tab;
            c.TabStop = true;
            c.UseVisualStyleBackColor = true;
            c.CheckedChanged += checkedChanged;
            parent.Controls.Add(c);
            return c;
        }
        /// <summary>Add a new numeric control to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="min">The minimum value to allow</param>
        /// <param name="max">The maximum value to allow</param>
        /// <param name="loc">The location of the new control</param>
        /// <param name="tab">The tab index of the new control</param>
        /// <param name="valueChanged">The event to use when the values changes</param>
        /// <returns>The new numeric control</returns>
        public static NumericUpDown AddNumeric(this Control parent, decimal min, decimal max, Point loc, int tab, EventHandler valueChanged)
        {
            NumericUpDown n = new NumericUpDown();
            n.BeginInit();
            n.Location = loc;
            n.Size = new Size(50, 20);
            n.Minimum = min;
            n.Maximum = max;
            n.TabIndex = tab;
            n.ValueChanged += valueChanged;
            n.EndInit();
            parent.Controls.Add(n);
            return n;
        }

        /// <summary>Add a new DescriptorListBox to a container control</summary>
        /// <typeparam name="DLB">The type of the new DescriptorListBox</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="tooltip">The tooltip object to use</param>
        /// <param name="items">The list of items to use in the list</param>
        /// <returns>The new DescriptorListBox</returns>
        public static DLB Add<DLB>(this Control parent, ToolTip tooltip, Descriptor[] items) where DLB : DescriptorListBox, new()
        {
            DLB dlb = parent.Add<DLB>(DockStyle.Fill, 0);
            dlb.ToolTip = tooltip;
            if (items != null)
                dlb.Items.AddRange(items);
            return dlb;
        }
        /// <summary>Add a new DescriptorListBox to a container control</summary>
        /// <typeparam name="TLB">The type of the new ToolbarListBox</typeparam>
        /// <param name="parent">The container control to add to</param>
        /// <param name="tooltip">The tooltip object to use</param>
        /// <param name="items">The list of items to use in the list</param>
        /// <param name="buttonClicked">The event to use when a button is clicked in the toolbar</param>
        /// <returns>The new ToolbarListBox</returns>
        public static TLB Add<TLB>(this Control parent, ToolTip tooltip, Descriptor[] items, PropertyChangedEventHandler buttonClicked) where TLB : ToolbarListBox, new()
        {
            TLB tlb = parent.Add<TLB>(tooltip, items);
            tlb.ButtonClicked += buttonClicked;
            return tlb;
        }
        /// <summary>Add a new ChoosableListBox to a container control</summary>
        /// <param name="parent">The container control to add to</param>
        /// <param name="tooltip">The tooltip object to use</param>
        /// <param name="items">The list of items to use in the list</param>
        /// <param name="chooseItem">The event to use when an item is chosen in the list (either double clicked or pressed Enter)</param>
        /// <returns>The new ChoosableListBox</returns>
        public static ChoosableListBox AddChoosableListBox(this Control parent, ToolTip tooltip, Descriptor[] items, ChoosableListBox.ChooseItemHandler chooseItem)
        {
            ChoosableListBox c = parent.Add<ChoosableListBox>(tooltip, items);
            c.ChooseItem += chooseItem;
            return c;
        }
    }
}
