﻿/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonWPF {
    /// <summary>
    /// Generic Visual helper.
    /// </summary>
    public static class VisualHelper {
        /// <summary>
        /// Find a child object in a WPF visual tree.
        /// </summary>
        /// <remarks>
        /// Sample usage:
        ///   GridViewHeaderRowPresenter headerRow = listView.GetVisualChild&lt;GridViewHeaderRowPresenter&gt;();
        ///   
        /// From https://social.msdn.microsoft.com/Forums/vstudio/en-US/7d0626cb-67e8-4a09-a01e-8e56ee7411b2/gridviewcolumheader-radiobuttons?forum=wpf
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="referenceVisual"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(this Visual referenceVisual) where T : Visual {
            Visual child = null;
            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceVisual); i++) {
                child = VisualTreeHelper.GetChild(referenceVisual, i) as Visual;
                if (child != null && child is T) {
                    break;
                } else if (child != null) {
                    child = GetVisualChild<T>(child);
                    if (child != null && child is T) {
                        break;
                    }
                }
            }
            return child as T;
        }
    }

    /// <summary>
    /// Helper functions for working with a ListView.
    /// 
    /// ListViews are generalized to an absurd degree, so simple things like "what column did
    /// I click on" and "what row is at the top" that were easy in WinForms are not provided
    /// by WPF.
    /// </summary>
    public static class ListViewExtensions {
        /// <summary>
        /// Figures out which item index is at the top of the window.  This only works for a
        /// ListView with a VirtualizingStackPanel.
        /// </summary>
        /// <remarks>
        /// See https://stackoverflow.com/q/2926722/294248 for an alternative approach that
        /// uses hit-testing, as well as a copy of this approach.
        /// </remarks>
        /// <returns>The item index, or -1 if the list is empty.</returns>
        public static int GetTopItemIndex(this ListView lv) {
            if (lv.Items.Count == 0) {
                return -1;
            }

            VirtualizingStackPanel vsp = lv.GetVisualChild<VirtualizingStackPanel>();
            if (vsp == null) {
                Debug.Assert(false, "ListView does not have a VirtualizingStackPanel");
                return -1;
            }
            return (int) vsp.VerticalOffset;
        }

        /// <summary>
        /// Scrolls the ListView so that the specified item is at the top.  The standard
        /// ListView.ScrollIntoView() makes the item visible but doesn't ensure a
        /// specific placement.
        /// </summary>
        /// <remarks>
        /// Equivalent to setting myListView.TopItem in WinForms.
        /// </remarks>
        public static void ScrollToTopItem(this ListView lv, object item) {
            ScrollViewer sv = lv.GetVisualChild<ScrollViewer>();
            sv.ScrollToBottom();
            lv.ScrollIntoView(item);
        }

        /// <summary>
        /// Returns the ListViewItem that was clicked on, or null if an LVI wasn't the target
        /// of a click (e.g. off the bottom of the list).
        /// </summary>
        public static ListViewItem GetClickedItem(this ListView lv, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // Should start at something like a TextBlock.  Walk up the tree until we hit the
            // ListViewItem.
            while (dep != null && !(dep is ListViewItem)) {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) {
                return null;
            }
            return (ListViewItem)dep;
        }

        /// <summary>
        /// Determines which column was the target of a mouse click.  Only works for ListView
        /// with GridView.
        /// </summary>
        /// <remarks>
        /// There's just no other way to do this with ListView.  With DataGrid you can do this
        /// somewhat reasonably
        /// (https://blog.scottlogic.com/2008/12/02/wpf-datagrid-detecting-clicked-cell-and-row.html),
        /// but ListView just doesn't want to help.
        /// </remarks>
        /// <returns>Column index, or -1 if the click was outside the columns (e.g. off the right
        ///   edge).</returns>
        public static int GetClickEventColumn(this ListView lv, MouseButtonEventArgs e) {
            // There's a bit of padding that seems to offset things.  Not sure how to account
            // for it, so for now just fudge it.
            const int FUDGE = 4;

            Point p = e.GetPosition(lv);
            GridView gv = (GridView)lv.View;
            double startPos = FUDGE;
            for (int index = 0; index < gv.Columns.Count; index++) {
                GridViewColumn col = gv.Columns[index];

                if (p.X < startPos + col.ActualWidth) {
                    return index;
                }
                startPos += col.ActualWidth;
            }

            return -1;
        }
    }
}
