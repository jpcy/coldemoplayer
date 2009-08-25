using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using QuickZip.Presenter;


namespace QuickZip.View
{
    /// <summary>
    /// Interaction logic for BreadcrumbCoreView.xaml
    /// </summary>
    public partial class BreadcrumbCoreView : UserControl
    {

        internal void AddItem(Framework.ItemPresenterBase presenter)
        {
            ContentControl content = new ContentControl();
            content.DataContext = presenter;
            content.Content = presenter.View;
            itemList.Items.Add(content);
        }

        internal void SetRootItem(Framework.ItemPresenterBase presenter)
        {
            itemList.Items.Clear();
            AddItem(presenter);
        }

        internal void SetExpander(Framework.ItemPresenterBase presenter)
        {
            expander.DataContext = presenter;
            expander.Content = presenter.View;
        }

        internal Framework.ItemPresenterBase GetExpander()
        {
            return expander.DataContext as Framework.ItemPresenterBase;            
        }

        internal void ClearAll(bool clearRoot)
        {
            if (clearRoot)
            {
                itemList.Items.Clear();                
            }
            else
            {
                while (itemList.Items.Count > 1) //Do not remove Root.
                    itemList.Items.RemoveAt(1);
            }


            (GetExpander() as ExpanderPresenter).ClearAll();
            //if (Presenter != null)
            //    Presenter.expand.ClearAll();
        }

        internal void ClearAll()
        {
            ClearAll(false);
        }

        internal void ClearAfterIndex(int index)
        {
            if (index <= -1)
                ClearAll();
            else
            {
                for (int i = itemList.Items.Count - 1; i > index; i--)
                {                    
                    itemList.Items.RemoveAt(i);                                       
                }

            }
        }


        internal ContentControl GetView(int index)
        {
            return (ContentControl)itemList.Items[index + 1];
        }

        internal ContentControl GetRootView()
        {
            return itemList.Items.Count > 0 ? (ContentControl)itemList.Items[0] : null;
        }

        internal int Count { get { return itemList.Items.Count - 1; } }

        public BreadcrumbCoreView()
        {
            InitializeComponent();
        }

        //private void Popup_LostFocus(object sender, RoutedEventArgs e)
        //{
        //}

        public QuickZip.Framework.ItemPresenterBase ItemPresenterBase
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public QuickZip.Presenter.BreadcrumbCorePresenter BreadcrumbCorePresenter
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }


    }
}
