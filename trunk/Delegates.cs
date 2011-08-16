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


namespace iTunesInfo
{
    /// <summary>Basic customizable delegates, these were added in .NET 3.5 to the System namespace</summary>
    static class Delegates
    {
        public delegate void Action();
        //public delegate void Action<in T>(T obj); // Already in .NET 2.0 as System.Action<in T>(T obj)
        //public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
        public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
        //public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        //public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        //public delegate TResult Func<out TResult>();
        //public delegate TResult Func<in T, out TResult>(T arg);
        //public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
        //public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
        //public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        //public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
