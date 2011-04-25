/*
Demoder.Common
Copyright (c) 2010,2011 Demoder <demoder@demoder.me>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Demoder.Common
{
    public static class Forms
    {
        public static void AutoResizeHeaders(ListView ListView, ColumnHeaderAutoResizeStyle AutoResizeStyle)
        {
            foreach (ColumnHeader ch in ListView.Columns)
                ch.AutoResize(AutoResizeStyle);
        }

        #region ListView column sorting
        /// <summary>
        /// Method to handle sorting of ListViews based on collumns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ListView_ColumnClick(object Sender, ColumnClickEventArgs E)
        {
            ListView lv = (ListView)Sender;
            Forms.ListViewSorter lvs = new Forms.ListViewSorter();

            if (!(lv.ListViewItemSorter is Forms.ListViewSorter))
                lv.ListViewItemSorter = lvs;
            else
                lvs = (Forms.ListViewSorter)lv.ListViewItemSorter;
            if (lvs.LastSort == E.Column)
            {
                if (lv.Sorting == SortOrder.Ascending)
                    lv.Sorting = SortOrder.Descending;
                else
                    lv.Sorting = SortOrder.Ascending;
            }
            else
            {
                lv.Sorting = SortOrder.Descending;
            }
            lvs.ByColumn = E.Column;
            lv.Sort();
        }
        #region This class derives from http://www.java2s.com/Code/CSharp/GUI-Windows-Form/SortaListViewbyAnyColumn.htm
        public class ListViewSorter : System.Collections.IComparer
        {
            #region Members
            private int _column = 0;
            private int _lastColumn = 0;
            #endregion

            #region Public accessors
            public int ByColumn
            {
                get { return _column; }
                set { _column = value; }
            }

            public int LastSort
            {
                get { return _lastColumn; }
                set { _lastColumn = value; }
            }
            #endregion

            #region Methods
            public int Compare(object Obj1, object Obj2)
            {
                if (!(Obj1 is ListViewItem))
                    return (0);
                if (!(Obj2 is ListViewItem))
                    return (0);

                ListViewItem lvi1 = (ListViewItem)Obj2;
                string str1 = lvi1.SubItems[ByColumn].Text;
                ListViewItem lvi2 = (ListViewItem)Obj1;
                string str2 = lvi2.SubItems[ByColumn].Text;
                int result;
                if (lvi1.ListView.Sorting == SortOrder.Ascending)
                    result = String.Compare(str1, str2);
                else
                    result = String.Compare(str2, str1);
                LastSort = ByColumn;
                return (result);
            }
            #endregion Methods
        }
        #endregion class
        #endregion ListView column sorting
    }
}