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
using System.Threading;
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>The main program entry point class</summary>
    static class Program
    {
        /// <summary>The name of the mutex used to force only one instance to be running at a time</summary>
        private const string MutexName = "TheITunesTrackInfoDisplayMutex";

        /// <summary>The main program entry point</summary>
        [STAThread]
        static void Main()
        {
            // Check to make sure the program isn't already open
            bool createdNew;
            Mutex m = new Mutex(true, MutexName, out createdNew);
            if (createdNew)
            {
                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(true); // interferes with the Glass displays

                // Start the controller program, which does all the heavy lifting
                Application.Run(new Controller());
            }
            else
            {
                // Mutex is already created (aka program is already started)
            }
        }
    }
}
