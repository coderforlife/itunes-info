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
        private static Descriptor GetKeyDescriptor(string keys) { return new Descriptor("Key: " + keys, "When '" + keys + "' are all pressed"); }
        private static NumericUpDown AddNumericX(Control c, string name, string unit, decimal min, decimal max, int y, int tab, EventHandler valueChanged)
        {
            return AddNumericX(c, name, unit, min, max, 93, y, tab, valueChanged);
        }
        private static NumericUpDown AddNumericX(Control c, string name, string unit, decimal min, decimal max, int nx, int y, int tab, EventHandler valueChanged)
        {
            c.Add<Label>(name, new Point(6, y));
            NumericUpDown n = c.AddNumeric(min, max, new Point(nx, y - 2), tab, valueChanged);
            c.Add<Label>(unit, new Point(nx + 51, y));
            return n;
        }


        private readonly Controller controller;
        private ColorDialog colors = null;
        private FontDialog fonts = null;
        private KeyDialog keys = null;

        public Options(Controller controller)
        {
            this.controller = controller;
            this.SetupComponent();
        }

        private ToolTip toolTip;
        private TabControl tabs;
        private void SetupComponent()
        {
            this.SuspendLayout();

            this.Text = "iTunes Info Options";
            this.Icon = iTunesInfo.Properties.Resources.icon;
            this.ClientSize = new Size(447, 21 + 397);
            this.MinimumSize = new Size(463, 21 + 435);

            this.toolTip = new ToolTip();

            Button b = this.AddButton("Show Box Now", new Point(345, 12), 90, 0, Builder.TopRightAnchor, delegate(object sender, EventArgs e) { this.controller.ShowTrackInfo(); });
            b.FlatStyle = FlatStyle.System;
            b.Margin = new Padding(0);
            b.Size = new Size(90, 19);

            this.tabs = this.Add<TabControl>(new Point(12, 12), new Size(423, 21 + 373), 1, Builder.TopBottomLeftRightAnchor);
            this.tabs.SuspendLayout();

            this.CreateDisplaySettingsTab();
            this.CreateDisplayTextTab();
            this.CreateActionsAndEventsTab();

            this.tabs.ResumeLayout();
            this.ResumeLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!e.Cancel && (e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.UserClosing))
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                this.minimizeOnStart.Checked = this.controller.MinimizeOnStart;
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
                this.displayText.Text = this.controller.DisplayText.Replace("\n", "\r\n");
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
        private CheckBox minimizeOnStart, allowGlass;
        private void CreateDisplaySettingsTab()
        {
            GroupBox g;
            TabPage t = this.tabs.AddTab("Display Settings");
            t.Padding = new Padding(3);

            this.minimizeOnStart = t.AddCheckBox("Minimize iTunes on Start", new Point(6, 6), 0, this.minimizeOnStart_ValueChanged);

            this.maxWidth      = AddNumericX(t, "Max Width:",      "px", 20, 1000,  21+8,   1, this.maxWidth_ValueChanged);
            this.lineSpacing   = AddNumericX(t, "Line Spacing:",   "px", 0,  20,    21+34,  2, this.lineSpacing_ValueChanged);
            this.insideMargin  = AddNumericX(t, "Inside Margin:",  "px", 0,  20,    21+60,  3, this.insideMargin_ValueChanged);
            this.outsideMargin = AddNumericX(t, "Outside Margin:", "px", 0,  20,    21+86,  4, this.outsideMargin_ValueChanged);
            this.maxOpacity    = AddNumericX(t, "Max Opacity:",    "%",  0,  100,   21+112, 5, this.maxOpacity_ValueChanged);
            this.fadeTime      = AddNumericX(t, "Fade Time:",      "ms", 0,  10000, 21+138, 6, this.fadeTime_ValueChanged);
            this.visibleTime   = AddNumericX(t, "Visible Time:",   "ms", 0,  60000, 21+164, 7, this.visibleTime_ValueChanged);

            t.Add<Label>("Position:", new Point(6, 21 + 191));
            this.position = t.Add<ComboBox>(new Point(93, 21 + 188), new Size(150, 21), 8);
            this.position.DropDownStyle = ComboBoxStyle.DropDownList;
            this.position.Items.AddRange(new object[] { "Near Clock", "Upper Left", "Upper Right", "Lower Right", "Lower Left" });
            this.position.SelectedIndexChanged += this.position_SelectedIndexChanged;

            t.Add<Label>("Text Color:", new Point(6, 21 + 220));
            this.textColor = t.AddButton(new Point(93, 21 + 215), 150, 9, this.textColor_Click, this.textColor_BackColorChanged);

            t.Add<Label>("Font:", new Point(6, 21 + 249));
            this.font = t.AddButton(Button.DefaultFont.ConvertToString(), new Point(93, 21 + 244), 101, 10, Builder.TopLeftRightAnchor, this.font_Click);
            this.toolTip.SetToolTip(this.font, this.font.Text);
            this.font.FontChanged += new EventHandler(this.font_FontChanged);


            // Glass Display Options
            g = t.AddGroupBox("Glass Display Options", new Point(6, 21 + 273), new Size(140, 68), 11);
            this.allowGlass = g.AddCheckBox("Allow Glass Display", new Point(9, 19), 0, this.allowGlass_CheckedChanged);
            this.glowSize = AddNumericX(g, "Glow Size:", "px", 0, 20, 69, 44, 1, this.glowSize_ValueChanged);
            g.ResumeLayout();


            // Basic Display Options
            g = t.AddGroupBox("Basic Display Options", new Point(152, 21 + 273), new Size(150, 68), 12);
            g.Add<Label>("Background:", new Point(6, 24));
            this.backgroundColor = g.AddButton(new Point(80, 19), 64, 1, this.backgroundColor_Click, this.backgroundColor_BackColorChanged);
            g.ResumeLayout();

            t.ResumeLayout();
        }
        private void maxWidth_ValueChanged(object sender, EventArgs e) { this.controller.MaxWidth = (int)this.maxWidth.Value; }
        private void lineSpacing_ValueChanged(object sender, EventArgs e) { this.controller.LineSpacing = (int)this.lineSpacing.Value; }
        private void insideMargin_ValueChanged(object sender, EventArgs e) { this.controller.InsideMargin = (int)this.insideMargin.Value; }
        private void outsideMargin_ValueChanged(object sender, EventArgs e) { this.controller.OutsideMargin = (int)this.outsideMargin.Value; }
        private void maxOpacity_ValueChanged(object sender, EventArgs e) { this.controller.MaxOpacity = (double)(this.maxOpacity.Value / 100); }
        private void fadeTime_ValueChanged(object sender, EventArgs e) { this.controller.FadeTime = (int)this.fadeTime.Value; }
        private void visibleTime_ValueChanged(object sender, EventArgs e) { this.controller.VisibleTime = (int)this.visibleTime.Value; }
        private void position_SelectedIndexChanged(object sender, EventArgs e) { this.controller.DesktopPosition = (DesktopPos)this.position.SelectedIndex; }
        private void minimizeOnStart_ValueChanged(object sender, EventArgs e) { this.controller.MinimizeOnStart = this.minimizeOnStart.Checked; }
        private void allowGlass_CheckedChanged(object sender, EventArgs e) { this.controller.AllowGlass = this.allowGlass.Checked; }
        private void glowSize_ValueChanged(object sender, EventArgs e) { this.controller.GlowSize = (int)this.glowSize.Value; }
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
        private void backgroundColor_Click(object sender, EventArgs e) { this.backgroundColor.BackColor = GetColor(this.controller.BackgroundColor); }
        private void backgroundColor_BackColorChanged(object sender, EventArgs e) { this.controller.BackgroundColor = this.backgroundColor.BackColor; }
        private void textColor_Click(object sender, EventArgs e) { this.textColor.BackColor = GetColor(this.controller.TextColor); }
        private void textColor_BackColorChanged(object sender, EventArgs e) { this.font.ForeColor = this.textColor.BackColor; this.controller.TextColor = this.textColor.BackColor; }
        private void font_Click(object sender, EventArgs e)
        {
            if (this.fonts == null)
                this.fonts = new FontDialog();
            this.fonts.Font = this.controller.DisplayFont;
            this.fonts.Color = this.controller.TextColor;
            if (this.fonts.ShowDialog() == DialogResult.OK)
                this.font.Font = this.fonts.Font;
        }
        private void font_FontChanged(object sender, EventArgs e)
        {
            this.font.Text = this.font.Font.ConvertToString();
            this.toolTip.SetToolTip(this.font, this.font.Text);
            this.controller.DisplayFont = this.font.Font;
        }
        #endregion

        #region Display Text
        private TextBox displayText;
        private void CreateDisplayTextTab()
        {
            TabPage t = this.tabs.AddTab("Display Text");
            t.Padding = new Padding(3);

            SplitContainer split = t.AddSplit(FixedPanel.Panel2);
            split.Width = 200;
            split.SplitterDistance = 25;
            split.Panel2MinSize = 175;

            this.displayText = split.Panel1.Add<TextBox>(DockStyle.Fill, 0);
            this.displayText.BorderStyle = BorderStyle.None;
            this.displayText.HideSelection = false;
            this.displayText.Multiline = true;
            this.displayText.ScrollBars = ScrollBars.Both;
            this.displayText.WordWrap = false;
            this.displayText.TextChanged += this.textChanged;

            split.Panel2.AddChoosableListBox(this.toolTip, Props.PropertyNames, this.insertDisplayItem);
            split.Panel2.Add<Label>("Double click to add:", DockStyle.Top);

            split.Panel1.ResumeLayout();
            split.Panel2.ResumeLayout();
            split.ResumeLayout();
            t.ResumeLayout();
        }
        private void insertDisplayItem(ChoosableListBox items, int index, Descriptor item)
        {
            int start = this.displayText.SelectionStart;
            int len = this.displayText.SelectionLength;
            string text = this.displayText.Text;
            this.displayText.Text = (len <= 0) ?
                text.Insert(start, "{" + item.Name + "}") :
                text.Substring(0, start) + "{" + item.Name + "}" + text.Substring(start + len);
            this.displayText.SelectionStart = start + item.Name.Length + 2;
            this.displayText.SelectionLength = 0;
        }
        private void textChanged(object sender, EventArgs e) { this.controller.DisplayText = this.displayText.Text.Replace("\r\n", "\n"); }
        #endregion

        #region Events and Actions

        private EventsListBox events;
        private ActionsListBox actions;
        private void CreateActionsAndEventsTab()
        {
            TabPage t = this.tabs.AddTab("Events and Actions");

            SplitContainer split = t.AddSplit(FixedPanel.Panel1);
            split.Width = 175 + 116 * 2 + split.SplitterWidth;
            split.SplitterDistance = 175;
            split.Panel1MinSize = 175;
            split.Panel2MinSize = 116 * 2;

            this.events = split.Panel1.Add<EventsListBox>(this.toolTip, Controller.EventNames, this.eventToolButton);
            this.events.SelectedIndexChanged += new EventHandler(this.selectedEvent);
            split.Panel1.AddButton("New Key Event", DockStyle.Bottom, 1, new EventHandler(this.newKeyEvent));

            SplitContainer split2 = split.Panel2.AddSplit(FixedPanel.Panel2);
            split2.SplitterDistance = 116;
            split2.Panel1MinSize = 116;
            split2.Panel2MinSize = 116;

            this.actions = split2.Panel1.Add<ActionsListBox>(this.toolTip, null, this.actionToolButton);
            split2.Panel1.Add<Label>("Select an event on the left and add actions from the right.", DockStyle.Top).AutoSize = true;
            split2.Panel1.SizeChanged += new EventHandler(this.actions_SizeChanged);

            split2.Panel2.AddChoosableListBox(this.toolTip, Controller.ActionNames, this.insertActionItem);

            split2.Panel1.ResumeLayout();
            split2.Panel2.ResumeLayout();
            split2.ResumeLayout();
            split.Panel1.ResumeLayout();
            split.Panel2.ResumeLayout();
            split.ResumeLayout();
            t.ResumeLayout();
        }

        private void SetActionsList(List<ThreadStart> actions)
        {
            foreach (ThreadStart a in actions)
                this.actions.Items.Add(this.controller.GetActionDesc(a));
        }
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


        private void AddKeyEvent()
        {
            if (this.keys == null)
                this.keys = new KeyDialog();
            this.keys.Keys = Keys.Empty;
            if (this.keys.ShowDialog(this) == DialogResult.OK)
            {
                Keys k = this.keys.Keys;
                string name = "Key: " + k.ToString();
                if (this.controller.KeyEvents.ContainsKey(k))
                {
                    this.events.SelectedValue = name;
                }
                else
                {
                    this.events.Items.Add(name);
                    this.events.SelectedIndex = this.events.Items.Count - 1;
                }
            }
        }
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

                this.events.Items[index] = GetKeyDescriptor(k.ToString());

                List<ThreadStart> actions;
                if (this.controller.KeyEvents.TryGetValue(old_k, out actions))
                {
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
                    this.controller.KeyEvents.Remove(old_k);
                }
            }
        }
        private void newKeyEvent(object sender, EventArgs e) { AddKeyEvent(); }
        private void eventToolButton(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "edit": this.ChangeKeyEvent(); break;
                case "remove": this.RemoveEvent(); break;
            }
        }


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
        private void SwapActions(bool up)
        {
            int index = this.actions.SelectedIndex, index2 = up ? index - 1 : index + 1;
            if (index >= 0 && index2 >= 0 && index2 < this.actions.Items.Count)
            {
                this.controller.SwapActionsInEvent(this.events.SelectedItem.ToString(), index, index2);
                this.actions.Items.Swap(index, index2);
            }
        }
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
