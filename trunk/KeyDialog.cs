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
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>A dialog that grabs a key combination from the user</summary>
    class KeyDialog : Form
    {
        /// <summary>The event handler for system key events</summary>
        private readonly KeyEventHandler keyChangeEvent;

        /// <summary>The current set of keys of the dialog</summary>
        private Keys keys = Keys.Empty;

        /// <summary>True if the last key set being set was empty</summary>
        private bool lastWasEmpty = false;

        /// <summary>The label that displays the description of the keys</summary>
        private Label keyText;

        /// <summary>The button that allows the user to select the keys</summary>
        private Button ok;

        /// <summary>Create a new key dialog</summary>
        public KeyDialog()
        {
            // Get the system key event handler
            this.keyChangeEvent = this.OnKeyChanged;

            // Setup the form properties
            this.SuspendLayout();
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new Size(417, 116);
            this.Text = "Type New Key Combination";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;

            // Setup the main portion of the dialog, with labels
            Panel panel = this.AddPanel(new Point(0, 0), new Size(417, 69), 1, Builder.TopBottomLeftRightAnchor);
            panel.BackColor = SystemColors.Window;
            panel.Add<Label>("Type a combination of keys that you wish to associate with a new set of actions.", new Point(12, 12));
            this.keyText = panel.Add<Label>(new Point(12, 37), new Size(393, 20), Builder.TopLeftRightAnchor);
            this.keyText.BorderStyle = BorderStyle.Fixed3D;
            this.keyText.BackColor = SystemColors.Control;
            this.keyText.TextAlign = ContentAlignment.MiddleCenter;
            panel.ResumeLayout();

            // Setup the okay and cancel buttons
            this.AcceptButton = this.ok = this.AddButton("OK", new Point(249, 81), 75, 2, Builder.BottomRightAnchor, null);
            this.AcceptButton.DialogResult = DialogResult.OK;
            this.ok.Enabled = false;
            this.CancelButton = this.AddButton("Cancel", new Point(330, 81), 75, 3, Builder.BottomRightAnchor, null);
            this.CancelButton.DialogResult = DialogResult.Cancel;

            this.ResumeLayout();
        }

        /// <summary>Get or set the key set of this dialog</summary>
        public Keys Keys { get { return this.keys; } set { this.SetKeys(value, false); } }

        /// <summary>When the dialog changes visibility the key event needs to be removed or added</summary>
        /// <param name="e">The event arguments</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
                KeyMonitor.OverridingKeyChange += this.keyChangeEvent;
            else
                KeyMonitor.OverridingKeyChange -= this.keyChangeEvent;
            base.OnVisibleChanged(e);
        }

        /// <summary>Called when a key is pressed or raised in the system</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The key arguments of the event</param>
        protected void OnKeyChanged(object sender, KeyEventArgs e)
        {
            SetKeys(KeyMonitor.Keys, true);
            e.Handled = true;
        }

        /// <summary>Sets the keys of the dialog</summary>
        /// <param name="k">The keys to set to</param>
        /// <param name="automatic">True if this automatic setting (from the system wide event) or false if being set directly from code</param>
        private void SetKeys(Keys k, bool automatic)
        {
            if (!this.keys.Equals(k) && (!automatic || this.lastWasEmpty || !k.IsSubsetOf(this.keys)))
            {
                // Only set the keys if:
                //   The new one isn't the same as the current one
                //   and one of:
                //     It is being set directly
                //     The most recent key set was empty (even if it didn't become the current one)
                //     The current key set is a subset of the new key set

                // Call set keys core, and focus the window if it is there
                if (!this.IsHandleCreated || this.IsDisposed)
                    SetKeysCore(k, automatic, false);
                else
                    try
                    { this.Invoke(new Delegates.Action<Keys, bool, bool>(SetKeysCore), k, automatic, true); }
                    catch (ObjectDisposedException)
                    { SetKeysCore(k, automatic, false); }
            }

            // Set the fact that the last was empty
            // This is used so that if you release all the keys you can start over
            this.lastWasEmpty = k.IsEmpty;
        }

        /// <summary>Sets the keys of the dialog, after some pre-checking done be SetKeys</summary>
        /// <param name="k">The keys to set to</param>
        /// <param name="automatic">True if this automatic setting (from the system wide event)</param>
        /// <param name="doFocus">If the okay button should be focused</param>
        private void SetKeysCore(Keys k, bool automatic, bool doFocus)
        {
            if (!automatic || doFocus)
            {
                // Set the keys to a copy of the keys and change the display of the keys
                this.keys = k.Clone();
                this.keyText.Text = this.keys.ToString();

                // Enable/Disable the ok button 
                bool was_enabled = this.ok.Enabled;
                this.ok.Enabled = !this.keys.IsOnlyModifiers;

                // Potentially focus the ok button
                if (doFocus && !was_enabled && this.ok.Enabled)
                    this.ok.Focus();
            }
        }
    }
}
