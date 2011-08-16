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

namespace iTunesInfo
{
    /// <summary>
    /// A class for describing something. Associates a name and a description. Primarily used for the DescriptorListBox which uses the description as a tooltip.
    /// </summary>
    class Descriptor : IEquatable<Descriptor>
    {
        /// <summary>The name to use for the descriptor, used as the textual representation in a ListBox</summary>
        public string Name;

        /// <summary>The description to use for the descriptor, used as the tooltip in a DescriptorListBox</summary>
        public string Description;

        /// <summary>Create a new Descriptor with a name and a descriptor</summary>
        /// <param name="name">The name to use</param>
        /// <param name="desc">The description to use</param>
        public Descriptor(string name, string desc) { this.Name = name; this.Description = desc; }


        /// <summary>Get the string representation of this object, which is just the name</summary>
        /// <returns>The name of this Descriptor</returns>
        public override string ToString() { return this.Name; }

        /// <summary>The hash code for this object</summary>
        /// <returns>The hash code, simply the sum of the hash codes for the name and description (which isn't completely robust, but that doesn't really matter)</returns>
        public override int GetHashCode() { return this.Name.GetHashCode() + this.Description.GetHashCode(); }


        /// <summary>Checks if this Descriptor equals another object, mainly defers to the class-specific Equals</summary>
        /// <param name="obj">The object to check, or null</param>
        /// <returns>True if the object is a Descriptor and the names and descriptions are equal, using the StringComparison.InvariantCultureIgnoreCase</returns>
        public override bool Equals(object obj) { return this.Equals(obj as Descriptor); }

        /// <summary>Checks if this Descriptor equals another Descriptor</summary>
        /// <param name="other">The Descriptor to check, or null</param>
        /// <returns>True if the names and descriptions are equal, using the StringComparison.InvariantCultureIgnoreCase</returns>
        public bool Equals(Descriptor other) { return other != null && this.Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase) && this.Description == other.Description; }

    }
}
