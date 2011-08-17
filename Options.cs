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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace iTunesInfo
{
    class Options : Form
    {
        #region Utils
        /// <summary>Get the descriptor for a particular set of keys (already converted to a string)</summary>
        /// <param name="keys">The key set as a string (using Keys.ToString())</param>
        /// <returns>The descriptor</returns>
        private static Descriptor GetKeyDescriptor(string keys) { return new Descriptor("Key: " + keys, "When '" + keys + "' are all pressed"); }
        /// <summary>Add a NumericUpDown control along with it's name and unit labels</summary>
        /// <param name="c">The parent control</param>
        /// <param name="name">The name to display before</param>
        /// <param name="unit">The unit to display after</param>
        /// <param name="min">The minimum value to allow</param>
        /// <param name="max">The maximum value to allow</param>
        /// <param name="y">The y position of the labels (the NumericUpDown is 2 pixels higher than this)</param>
        /// <param name="tab">The tab-index of the NumericUpDown control</param>
        /// <param name="valueChanged">The event handler to use when the value changes</param>
        /// <returns>The new NumericUpDown control</returns>
        private static NumericUpDown AddNumericX(Control c, string name, string unit, decimal min, decimal max, int y, int tab, EventHandler valueChanged)
        {
            return AddNumericX(c, name, unit, min, max, 93, y, tab, valueChanged);
        }
        /// <summary>Add a NumericUpDown control along with it's name and unit labels</summary>
        /// <param name="c">The parent control</param>
        /// <param name="name">The name to display before</param>
        /// <param name="unit">The unit to display after</param>
        /// <param name="min">The minimum value to allow</param>
        /// <param name="max">The maximum value to allow</param>
        /// <param name="nx">The horizontal distance between the start of the name label and the start of the NumericUpDown control</param>
        /// <param name="y">The y position of the labels (the NumericUpDown is 2 pixels higher than this)</param>
        /// <param name="tab">The tab-index of the NumericUpDown control</param>
        /// <param name="valueChanged">The event handler to use when the value changes</param>
        /// <returns>The new NumericUpDown control</returns>
        private static NumericUpDown AddNumericX(Control c, string name, string unit, decimal min, decimal max, int nx, int y, int tab, EventHandler valueChanged)
        {
            c.Add<Label>(name, new Point(6, y));
            NumericUpDown n = c.AddNumeric(min, max, new Point(nx, y - 2), tab, valueChanged);
            c.Add<Label>(unit, new Point(nx + 51, y));
            return n;
        }
        #endregion

        /// <summary>The controller object that created this options form and is changed whenever an option is changed</summary>
        private readonly Controller controller;

        /// <summary>Create a new form for setting all of the options in the program</summary>
        /// <param name="controller">The controller that is used to get the current values of the options and which is updated when options are changed</param>
        public Options(Controller controller)
        {
            this.controller = controller;
            this.SetupForm();
        }

        /// <summary>Dialog used to select colors</summary>
        private ColorDialog colors = null;
        /// <summary>Dialog used to select fonts</summary>
        private FontDialog fonts = null;
        /// <summary>Dialog used to select key combinations</summary>
        private KeyDialog keys = null;
        /// <summary>The tooltip object used for this form</summary>
        private ToolTip toolTip;
        /// <summary>Setup all of the components on this form</summary>
        private void SetupForm()
        {
            // Basic form properties
            this.SuspendLayout();
            this.Text = "iTunes Info Options";
            this.Icon = iTunesInfo.Properties.Resources.icon;
            this.ClientSize = new Size(447, 21 + 397);
            this.MinimumSize = new Size(463, 21 + 435);

            // The form's tooltip
            this.toolTip = new ToolTip();

            // The button that is used to show the information box now
            Button b = this.AddButton("Show Box Now", new Point(345, 12), 90, 0, Builder.TopRightAnchor, delegate(object sender, EventArgs e) { this.controller.ShowTrackInfo(); });
            b.FlatStyle = FlatStyle.System;
            b.Margin = new Padding(0);
            b.Size = new Size(90, 19);

            // The tabs control used as the main control of this form
            TabControl tabs = this.Add<TabControl>(new Point(12, 12), new Size(423, 21 + 373), 1, Builder.TopBottomLeftRightAnchor);
            tabs.SuspendLayout();

            // Create all of the tabs
            this.CreateDisplaySettingsTab(tabs);
            this.CreateDisplayTextTab(tabs);
            this.CreateActionsAndEventsTab(tabs);

            // Resume layouts
            tabs.ResumeLayout();
            this.ResumeLayout();
        }

        /// <summary>When trying to close the form just hide it instead (unless the program is quitting)</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!e.Cancel && (e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.UserClosing))
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        /// <summary>When the form becomes visible make sure all the displayed values are up-to-date with the controller state</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                // First tab of options
                this.minOnStart.Checked = this.controller.MinimizeOnStart;
                this.allowGlass.Checked = this.controller.AllowGlass;
                this.maxWidth.Value = this.controller.MaxWidth;
                this.lineSpacing.Value = this.controller.LineSpacing;
                this.glowSize.Value = this.controller.GlowSize;
                this.insideMargin.Value = this.controller.InsideMargin;
                this.outsideMargin.Value = this.controller.OutsideMargin;
                this.maxOpacity.Value = Convert.ToInt32(this.controller.MaxOpacity * 100);
                this.fadeTime.Value = this.controller.FadeTime;
                this.visibleTime.Value = this.controller.VisibleTime;
                this.position.SelectedIndex = (int)this.controller.DesktopPosition;
                this.textColor.BackColor = this.controller.TextColor;
                this.font.Font = this.controller.DisplayFont;
                this.backgroundColor.BackColor = this.controller.BackgroundColor;

                // Second tab options (display text)
                this.displayText.Text = this.controller.DisplayText.Replace("\n", "\r\n");

                // Third tab of options (events and actions)
                foreach (var evnt in this.controller.Events)
                {
                    int idx = this.events.GetItemIndex(evnt.Key), count = evnt.Value.Count;
                    this.events.SetBold(idx, count > 0);
                }
                foreach (var evnt in this.controller.KeyEvents)
                {
                    string keys = evnt.Key.ToString();
                    int idx = this.events.GetItemIndex("Key: " + keys), count = evnt.Value.Count;
                    if (idx == -1)
                    {
                        idx = this.events.Items.Count;
                        this.events.Items.Add(GetKeyDescriptor(keys));
                    }
                    this.events.SetBold(idx, count > 0);
                }
                this.events.SelectedIndex = 0;
            }
        }

        #region Display Options

        private NumericUpDown maxWidth, lineSpacing, insideMargin, outsideMargin, maxOpacity, fadeTime, visibleTime, glowSize;
        private Button textColor, font, backgroundColor;
        private ComboBox position;
        private CheckBox minOnStart, allowGlass;

        /// <summary>Create the Display Settings tab</summary>
        /// <param name="tabs">The tabs to add this tab to</param>
        private void CreateDisplaySettingsTab(TabControl tabs)
        {
            GroupBox g;

            // Create the new tab
            TabPage t = tabs.AddTab("Display Settings");
            t.Padding = new Padding(3);

            // Add the options that are relevant to all types of displays
            this.minOnStart = t.AddCheckBox("Minimize iTunes on Start", new Point(6, 6), 0, delegate(object s, EventArgs e) { this.controller.MinimizeOnStart = this.minOnStart.Checked;               });
            this.maxWidth      = AddNumericX(t, "Max Width:",      "px", 20, 1000,  29,  1, delegate(object s, EventArgs e) { this.controller.MaxWidth        = (int)this.maxWidth.Value;              });
            this.lineSpacing   = AddNumericX(t, "Line Spacing:",   "px", 0,  20,    55,  2, delegate(object s, EventArgs e) { this.controller.LineSpacing     = (int)this.lineSpacing.Value;           });
            this.insideMargin  = AddNumericX(t, "Inside Margin:",  "px", 0,  20,    81,  3, delegate(object s, EventArgs e) { this.controller.InsideMargin    = (int)this.insideMargin.Value;          });
            this.outsideMargin = AddNumericX(t, "Outside Margin:", "px", 0,  20,    107, 4, delegate(object s, EventArgs e) { this.controller.OutsideMargin   = (int)this.outsideMargin.Value;         });
            this.maxOpacity    = AddNumericX(t, "Max Opacity:",    "%",  0,  100,   133, 5, delegate(object s, EventArgs e) { this.controller.MaxOpacity      = (double)(this.maxOpacity.Value / 100); });
            this.fadeTime      = AddNumericX(t, "Fade Time:",      "ms", 0,  10000, 159, 6, delegate(object s, EventArgs e) { this.controller.FadeTime        = (int)this.fadeTime.Value;              });
            this.visibleTime   = AddNumericX(t, "Visible Time:",   "ms", 0,  60000, 185, 7, delegate(object s, EventArgs e) { this.controller.VisibleTime     = (int)this.visibleTime.Value;           });

            t.Add<Label>("Position:", new Point(6, 21 + 191));
            this.position = t.Add<ComboBox>(new Point(93, 21 + 188), new Size(150, 21), 8);
            this.position.DropDownStyle = ComboBoxStyle.DropDownList;
            this.position.Items.AddRange(new object[] { "Near Clock", "Upper Left", "Upper Right", "Lower Right", "Lower Left" });
            this.position.SelectedIndexChanged += delegate(object sender, EventArgs e) { this.controller.DesktopPosition = (DesktopPos)this.position.SelectedIndex; };

            t.Add<Label>("Text Color:", new Point(6, 21 + 220));
            this.textColor = t.AddButton(new Point(93, 21 + 215), 150, 9,
                delegate(object s, EventArgs e) { this.textColor.BackColor = GetColor(this.controller.TextColor); },
                delegate(object s, EventArgs e) { this.font.ForeColor = this.textColor.BackColor; this.controller.TextColor = this.textColor.BackColor; }
            );

            t.Add<Label>("Font:", new Point(6, 21 + 249));
            this.font = t.AddButton(Button.DefaultFont.ConvertToString(), new Point(93, 21 + 244), 101, 10, Builder.TopLeftRightAnchor, this.font_Click);
            this.toolTip.SetToolTip(this.font, this.font.Text);
            this.font.FontChanged += new EventHandler(this.font_FontChanged);


            // Glass Display Options
            g = t.AddGroupBox("Glass Display Options", new Point(6, 21 + 273), new Size(140, 68), 11);
            this.allowGlass = g.AddCheckBox("Allow Glass Display", new Point(9, 19), 0, delegate(object s, EventArgs e) { this.controller.AllowGlass = this.allowGlass.Checked; });
            this.glowSize = AddNumericX(g, "Glow Size:", "px", 0, 20, 69, 44, 1, delegate(object s, EventArgs e) { this.controller.GlowSize = (int)this.glowSize.Value; });
            g.ResumeLayout();


            // Basic Display Options
            g = t.AddGroupBox("Basic Display Options", new Point(152, 21 + 273), new Size(150, 68), 12);
            g.Add<Label>("Background:", new Point(6, 24));
            this.backgroundColor = g.AddButton(new Point(80, 19), 64, 1,
                delegate(object s, EventArgs e) { this.backgroundColor.BackColor = GetColor(this.controller.BackgroundColor); },
                delegate(object s, EventArgs e) { this.controller.BackgroundColor = this.backgroundColor.BackColor; }
            );
            g.ResumeLayout();

            t.ResumeLayout();
        }
        /// <summary>Get a new color using a dialog</summary>
        /// <param name="current">The current color</param>
        /// <returns>The new (or possibly the current) color</returns>
        private Color GetColor(Color current)
        {
            if (this.colors == null)
            {
                this.colors = new ColorDialog();
                this.colors.AnyColor = true;
            }
            this.colors.Color = current;
            return (this.colors.ShowDialog() == DialogResult.OK) ? this.colors.Color : current;
        }
        /// <summary>Show a dialog to select a new font</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void font_Click(object sender, EventArgs e)
        {
            if (this.fonts == null)
                this.fonts = new FontDialog();
            this.fonts.Font = this.controller.DisplayFont;
            this.fonts.Color = this.controller.TextColor;
            if (this.fonts.ShowDialog() == DialogResult.OK)
                this.font.Font = this.fonts.Font;
        }
        /// <summary>Update the font of the button</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void font_FontChanged(object sender, EventArgs e)
        {
            this.font.Text = this.font.Font.ConvertToString();
            this.toolTip.SetToolTip(this.font, this.font.Text);
            this.controller.DisplayFont = this.font.Font;
        }
        #endregion

        #region Display Text
        /// <summary>The text format of the display</summary>
        private TextBox displayText;
        /// <summary>Create the Display Text tab</summary>
        /// <param name="tabs">The tabs to add this tab to</param>
        private void CreateDisplayTextTab(TabControl tabs)
        {
            // Create the new tab
            TabPage t = tabs.AddTab("Display Text");
            t.Padding = new Padding(3);

            // Create the split container
            SplitContainer split = t.AddSplit(FixedPanel.Panel2);
            split.Width = 200;
            split.SplitterDistance = 25;
            split.Panel2MinSize = 175;

            // Left panel is the display format text
            this.displayText = split.Panel1.Add<TextBox>(DockStyle.Fill, 0);
            this.displayText.BorderStyle = BorderStyle.None;
            this.displayText.HideSelection = false;
            this.displayText.Multiline = true;
            this.displayText.ScrollBars = ScrollBars.Both;
            this.displayText.WordWrap = false;
            this.displayText.TextChanged += this.textChanged;

            // Right panel is the list of properties that can be added to the display text
            split.Panel2.AddChoosableListBox(this.toolTip, Props.PropertyNames, this.insertDisplayItem);
            split.Panel2.Add<Label>("Double click to add:", DockStyle.Top);

            // Resume layouts
            split.Panel1.ResumeLayout();
            split.Panel2.ResumeLayout();
            split.ResumeLayout();
            t.ResumeLayout();
        }
        /// <summary>Inserts a chosen display property into the display text</summary>
        /// <param name="items">The choosable list box that generated the event</param>
        /// <param name="index">The index of the item chosen</param>
        /// <param name="item">The item to chosen</param>
        private void insertDisplayItem(ChoosableListBox items, int index, Descriptor item)
        {
            // Get the selected area
            int start = this.displayText.SelectionStart;
            int len = this.displayText.SelectionLength;

            // Update the text with the new property by inserting it at the cursor or replacing the selected text
            string text = this.displayText.Text;
            this.displayText.Text = (len <= 0) ?
                text.Insert(start, "{" + item.Name + "}") :
                text.Substring(0, start) + "{" + item.Name + "}" + text.Substring(start + len);

            // Update the selection position
            this.displayText.SelectionStart = start + item.Name.Length + 2;
            this.displayText.SelectionLength = 0;
        }
        /// <summary>When the text changes update what the actual popup displays</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void textChanged(object sender, EventArgs e) { this.controller.DisplayText = this.displayText.Text.Replace("\r\n", "\n"); }
        #endregion

        #region Events and Actions

        /// <summary>The list of events being displayed</summary>
        private EventsListBox events;
        /// <summary>The list of actions being displayed</summary>
        private ActionsListBox actions;

        /// <summary>Create the Actions and Events tab</summary>
        /// <param name="tabs">The tabs to add this tab to</param>
        private void CreateActionsAndEventsTab(TabControl tabs)
        {
            // Create the new tab
            TabPage t = tabs.AddTab("Events and Actions");

            // Create the main split panel
            SplitContainer split = t.AddSplit(FixedPanel.Panel1);
            split.Width = 175 + 116 * 2 + split.SplitterWidth;
            split.SplitterDistance = 175;
            split.Panel1MinSize = 175;
            split.Panel2MinSize = 116 * 2;

            // Far left panel is the events and a button for creating new key events
            this.events = split.Panel1.Add<EventsListBox>(this.toolTip, Controller.EventNames, this.eventToolButton);
            this.events.SelectedIndexChanged += new EventHandler(this.selectedEvent);
            split.Panel1.AddButton("New Key Event", DockStyle.Bottom, 1, delegate(object sender, EventArgs e) { this.AddKeyEvent(); });

            // Create the second split panel (middle and far right panels)
            SplitContainer split2 = split.Panel2.AddSplit(FixedPanel.Panel2);
            split2.SplitterDistance = 116;
            split2.Panel1MinSize = 116;
            split2.Panel2MinSize = 116;

            // Middle panel is the actions list currently associated with the selected event
            this.actions = split2.Panel1.Add<ActionsListBox>(this.toolTip, null, this.actionToolButton);
            split2.Panel1.Add<Label>("Select an event on the left and add actions from the right.", DockStyle.Top).AutoSize = true;
            split2.Panel1.SizeChanged += new EventHandler(this.actions_SizeChanged);

            // Far right panel is the list of all actions to choose from
            split2.Panel2.AddChoosableListBox(this.toolTip, Controller.ActionNames, this.insertActionItem);

            // Resume layouts
            split2.Panel1.ResumeLayout();
            split2.Panel2.ResumeLayout();
            split2.ResumeLayout();
            split.Panel1.ResumeLayout();
            split.Panel2.ResumeLayout();
            split.ResumeLayout();
            t.ResumeLayout();
        }

        /// <summary>Set the list of actions being displayed</summary>
        /// <param name="actions">The list of actions</param>
        private void SetActionsList(List<ThreadStart> actions)
        {
            foreach (ThreadStart a in actions)
                this.actions.Items.Add(this.controller.GetActionDesc(a));
        }
        /// <summary>When the selected event changes update the list of display actions</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void selectedEvent(object sender, EventArgs e)
        {
            this.actions.Items.Clear();

            if (this.events.SelectedIndex >= 0)
            {
                string name = this.events.SelectedItem.ToString();
                List<ThreadStart> actions;
                if (name.StartsWith("Key: ") ?
                        this.controller.KeyEvents.TryGetValue(Keys.FromString(name.Substring(5)), out actions) :
                        this.controller.Events.TryGetValue(name.ToLower(), out actions))
                    this.SetActionsList(actions);
            }
        }
        /// <summary>When the actions panel changes size, update the label to fit the area properly</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void actions_SizeChanged(object sender, EventArgs e)
        {
            SplitterPanel sp = sender as SplitterPanel;
            Label l;
            if (sp != null && (l = sp.Controls[sp.Controls.Count - 1] as Label) != null)
            {
                l.MaximumSize = new Size(sp.Width, 1000);
                sp.Parent.PerformLayout();
            }
        }

        /// <summary>Add a new key event</summary>
        private void AddKeyEvent()
        {
            if (this.keys == null)
                this.keys = new KeyDialog();
            this.keys.Keys = Keys.Empty;
            if (this.keys.ShowDialog(this) == DialogResult.OK)
            {
                // Add the new key event
                Keys k = this.keys.Keys;
                string name = "Key: " + k.ToString();
                if (this.controller.KeyEvents.ContainsKey(k))
                {
                    // Update the event
                    this.events.SelectedValue = name;
                }
                else
                {
                    // Add a new event
                    this.events.Items.Add(name);
                    this.events.SelectedIndex = this.events.Items.Count - 1;
                }
            }
        }
        /// <summary>Remove the currently selected event</summary>
        private void RemoveEvent()
        {
            int index = this.events.SelectedIndex;
            if (index >= 0)
            {
                string evnt = this.events.SelectedItem.ToString();
                this.controller.RemoveEvent(evnt);
                if (evnt.StartsWith("Key: "))
                    this.events.Items.RemoveAt(index);
                else
                    this.events.SetBold(index, false);
            }
        }
        /// <summary>Change an existing key event</summary>
        private void ChangeKeyEvent()
        {
            int index = this.events.SelectedIndex;
            if (index < 0) return;

            string evnt = this.events.SelectedItem.ToString();
            if (!evnt.StartsWith("Key: ")) return;

            if (this.keys == null)
                this.keys = new KeyDialog();
            this.keys.Keys = Keys.Empty;
            if (this.keys.ShowDialog(this) == DialogResult.OK)
            {
                Keys k = this.keys.Keys, old_k = Keys.FromString(evnt.Substring(5));
                if (k.Equals(old_k)) return;

                // Update the key event name
                this.events.Items[index] = GetKeyDescriptor(k.ToString());

                List<ThreadStart> actions;
                if (this.controller.KeyEvents.TryGetValue(old_k, out actions))
                {
                    // Copy the actions to the new event
                    if (actions.Count > 0)
                    {
                        this.events.SetBold(index, true);
                        this.SetActionsList(actions);
                        this.controller.KeyEvents[k] = actions;
                    }
                    else
                    {
                        this.actions.Items.Clear();
                    }
                    // Remove the event
                    this.controller.KeyEvents.Remove(old_k);
                }
            }
        }
        /// <summary>Event items tool bar button events</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The property (button) that got fired</param>
        private void eventToolButton(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "edit": this.ChangeKeyEvent(); break;
                case "remove": this.RemoveEvent(); break;
            }
        }

        /// <summary>Remove the currently selected action</summary>
        private void RemoveAction()
        {
            int index = this.actions.SelectedIndex;
            if (index >= 0)
            {
                this.actions.Items.RemoveAt(index);
                this.controller.RemoveActionFromEvent(this.events.SelectedItem.ToString(), index);
                if (this.actions.Items.Count == 0)
                    this.events.SetBold(this.events.SelectedIndex, false);
            }
        }
        /// <summary>Swap the current action with one of its neighbors</summary>
        /// <param name="up">If true, swap with the action above, otherwise swap with the item below</param>
        private void SwapActions(bool up)
        {
            int index = this.actions.SelectedIndex, index2 = up ? index - 1 : index + 1;
            if (index >= 0 && index2 >= 0 && index2 < this.actions.Items.Count)
            {
                this.controller.SwapActionsInEvent(this.events.SelectedItem.ToString(), index, index2);
                this.actions.Items.Swap(index, index2);
            }
        }
        /// <summary>Insert an action</summary>
        /// <param name="items">The items list to insert into</param>
        /// <param name="_index">The index of the item being inserted</param>
        /// <param name="action">The action being inserted</param>
        private void insertActionItem(ChoosableListBox items, int _index, Descriptor action)
        {
            int index = this.actions.SelectedIndex, count = this.actions.Items.Count;
            if (index < 0)
                index = count;
            this.actions.Items.Insert(index, action);
            this.controller.AddActionToEvent(this.events.SelectedItem.ToString(), index, action.Name);
            if (count == 0)
                this.events.SetBold(this.events.SelectedIndex, true);
        }
        /// <summary>Action items tool bar button events</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The property (button) that got fired</param>
        private void actionToolButton(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "up": this.SwapActions(true); break;
                case "down": this.SwapActions(false); break;
                case "remove": this.RemoveAction(); break;
            }
        }
        #endregion
    }
}
