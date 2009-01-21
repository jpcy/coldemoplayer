using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public class ExtendedListView : ListView
    {
        private const Int32 resizeColumnsWaitTime = 50; // ms
        private ExtendedGridViewColumn lastSortedOnColumn = null;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        private ScrollViewer scrollViewer;
        private Thread resizeColumnsThread = null;
        private Boolean sortOnItemsChanged = true;

        #region New Dependency Properties

        public string ColumnHeaderSortedAscendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedAscendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedAscendingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderSortedAscendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedAscendingTemplateProperty =
            DependencyProperty.Register("ColumnHeaderSortedAscendingTemplate", typeof(string), typeof(ExtendedListView), new UIPropertyMetadata(""));


        public string ColumnHeaderSortedDescendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedDescendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedDescendingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderSortedDescendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedDescendingTemplateProperty =
            DependencyProperty.Register("ColumnHeaderSortedDescendingTemplate", typeof(string), typeof(ExtendedListView), new UIPropertyMetadata(""));


        public string ColumnHeaderNotSortedTemplate
        {
            get { return (string)GetValue(ColumnHeaderNotSortedTemplateProperty); }
            set { SetValue(ColumnHeaderNotSortedTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderNotSortedTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderNotSortedTemplateProperty =
            DependencyProperty.Register("ColumnHeaderNotSortedTemplate", typeof(string), typeof(ExtendedListView), new UIPropertyMetadata(""));

        #endregion

        public ExtendedListView()
        {
            Loaded += new RoutedEventHandler(ExtendedListView_Loaded);
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(ExtendedListView_IsVisibleChanged);
        }

        /// <summary>
        /// Executes when the control is initialized completely the first time through. Runs only once.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            // A bit hackish, but saves having to define these in XAML for every single listview.
            ColumnHeaderSortedAscendingTemplate = "HeaderTemplateArrowUp";
            ColumnHeaderSortedDescendingTemplate = "HeaderTemplateArrowDown";
            ColumnHeaderNotSortedTemplate = "HeaderTemplateTransparent";

            // add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            Initialise();

            base.OnInitialized(e);
        }

        public void Initialise()
        {
            // cast the ListView's View to a GridView
            GridView gridView = this.View as GridView;
            if (gridView == null)
            {
                return;
            }

            // determine which column is marked as IsDefaultSortColumn
            ExtendedGridViewColumn defaultGridViewColumn = null;

            foreach (GridViewColumn gridViewColumn in gridView.Columns)
            {
                ExtendedGridViewColumn sortableGridViewColumn = gridViewColumn as ExtendedGridViewColumn;
                if (sortableGridViewColumn != null && sortableGridViewColumn.IsDefaultSortColumn)
                {
                    defaultGridViewColumn = sortableGridViewColumn;
                    break;
                }
            }

            // if the default sort column is defined, sort the data and then update the templates as necessary.
            if (defaultGridViewColumn != null)
            {
                lastSortedOnColumn = defaultGridViewColumn;
                Sort(defaultGridViewColumn.SortPropertyName, ListSortDirection.Ascending);
                SetColumnHeaderTemplate(defaultGridViewColumn, ListSortDirection.Ascending);
                SelectedIndex = 0;
            }

            // Set the "not sorted" template to all the other columns. This is so column header are correctly auto-sized.
            foreach (GridViewColumn gridViewColumn in gridView.Columns)
            {
                ExtendedGridViewColumn sortableGridViewColumn = gridViewColumn as ExtendedGridViewColumn;

                if (sortableGridViewColumn != null && sortableGridViewColumn != defaultGridViewColumn)
                {
                    SetColumnHeaderTemplate(sortableGridViewColumn, null);
                }
            }
        }

        void ExtendedListView_Loaded(object sender, RoutedEventArgs e)
        {
            FindScrollViewer((DependencyObject)this);
        }

        void ExtendedListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                ResizeColumns();
            }
        }

        void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // FIXME: was causing continuous resizing
            //ResizeColumns();
        }

        private void FindScrollViewer(DependencyObject start)
        {
            if (start == null)
            {
                return;
            }

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual child = VisualTreeHelper.GetChild(start, i) as Visual;

                if (child is ScrollViewer)
                {
                    scrollViewer = (ScrollViewer)child;
                    scrollViewer.SizeChanged += new SizeChangedEventHandler(scrollViewer_SizeChanged);
                }

                FindScrollViewer(child);
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ResizeColumns();

            if (sortOnItemsChanged)
            {
                Sort();
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            INotifyCollectionChanged itemCollection = newValue as INotifyCollectionChanged;

            if (itemCollection == null)
            {
                return;
            }

            itemCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(itemCollection_CollectionChanged);
        }

        private void itemCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in e.NewItems)
            {
                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            }
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ResizeColumns();
        }

        public void ResizeColumns()
        {
            if (resizeColumnsThread == null || !resizeColumnsThread.IsAlive)
            {
                resizeColumnsThread = new Thread(new ThreadStart(ResizeColumnsWorker));
                resizeColumnsThread.Start();
            }
        }

        private void ResizeColumnsWorker()
        {
            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Thread.Sleep(resizeColumnsWaitTime);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(ResizeColumnsWorker));
                return;
            }

            GridView view = View as GridView;

            if (view == null)
            {
                return;
            }

            // autosize all columns
            foreach (GridViewColumn column in view.Columns)
            {
                column.Width = column.ActualWidth;
            }

            UpdateLayout();

            foreach (GridViewColumn column in view.Columns)
            {
                column.Width = Double.NaN;
            }

            UpdateLayout();

            // count column widths that aren't low priority or fill columns
            Double controlWidth = (scrollViewer == null ? ActualWidth : scrollViewer.ViewportWidth);
            Double normalColumnWidthTotal = 0;
            ExtendedGridViewColumn lowPriorityColumn = null;
            ExtendedGridViewColumn fillColumn = null;

            foreach (ExtendedGridViewColumn column in view.Columns)
            {
                if (column.IsLowPriority)
                {
                    lowPriorityColumn = column;
                }
                else if (column.IsFillColumn)
                {
                    fillColumn = column;
                }
                else
                {
                    normalColumnWidthTotal += column.ActualWidth;                    
                }
            }

            // size the low priority column
            if (lowPriorityColumn != null)
            {
                Double desiredWidth = controlWidth - (normalColumnWidthTotal + (fillColumn == null ? 0 : fillColumn.ActualWidth)) - 4; // padding

                if (desiredWidth > 0 && desiredWidth < lowPriorityColumn.ActualWidth)
                {
                    lowPriorityColumn.Width = desiredWidth;
                }
            }

            // size the fill column
            if (fillColumn != null)
            {
                Double desiredWidth = controlWidth - (normalColumnWidthTotal + (lowPriorityColumn == null ? 0 : lowPriorityColumn.ActualWidth)) - 4; // padding

                if (desiredWidth > 0 && desiredWidth > fillColumn.ActualWidth)
                {
                    fillColumn.Width = desiredWidth;
                }
            }
        }

        /// <summary>
        /// Event Handler for the ColumnHeader Click Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            // ensure that we clicked on the column header and not the padding that's added to fill the space.
            if (headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            // attempt to cast to the ExtendedGridViewColumn object.
            ExtendedGridViewColumn sortableGridViewColumn = headerClicked.Column as ExtendedGridViewColumn;

            // ensure that the column header is the correct type and a sort property has been set.
            if (sortableGridViewColumn == null || String.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
            {
                return;
            }

            ListSortDirection direction = ListSortDirection.Ascending;
            bool newSortColumn = false;

            // determine whether this is a new sort, or a switch in sort direction.
            if (lastSortedOnColumn == null || String.IsNullOrEmpty(lastSortedOnColumn.SortPropertyName) || !String.Equals(sortableGridViewColumn.SortPropertyName, lastSortedOnColumn.SortPropertyName, StringComparison.InvariantCultureIgnoreCase))
            {
                newSortColumn = true;
            }
            else if (lastDirection == ListSortDirection.Ascending)
            {
                direction = ListSortDirection.Descending;
            }

            Sort(sortableGridViewColumn.SortPropertyName, direction);
            SetColumnHeaderTemplate(sortableGridViewColumn, direction);

            // Remove arrow from previously sorted header
            if (newSortColumn && lastSortedOnColumn != null)
            {
                SetColumnHeaderTemplate(lastSortedOnColumn, null);
            }

            lastSortedOnColumn = sortableGridViewColumn;
        }

        private void SetColumnHeaderTemplate(ExtendedGridViewColumn column, ListSortDirection? direction)
        {
            if (direction == null)
            {
                column.HeaderTemplate = TryFindResource(ColumnHeaderNotSortedTemplate) as DataTemplate;
            }
            else if (direction == ListSortDirection.Ascending)
            {
                column.HeaderTemplate = TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
            }
            else
            {
                column.HeaderTemplate = TryFindResource(ColumnHeaderSortedDescendingTemplate) as DataTemplate;
            }
        }

        /// <summary>
        /// Helper method that sorts the data.
        /// </summary>
        /// <param name="sortBy"></param>
        /// <param name="direction"></param>
        private void Sort(string sortBy, ListSortDirection direction)
        {
            sortOnItemsChanged = false;
            lastDirection = direction;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription(sortBy, direction));
                dataView.Refresh();
            }

            sortOnItemsChanged = true;
        }

        public void Sort()
        {
            if (lastSortedOnColumn == null)
            {
                return;
            }

            Sort(lastSortedOnColumn.SortPropertyName, lastDirection);
            SelectedIndex = -1;
        }
    }
}
