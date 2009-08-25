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
using System.Windows.Controls.Primitives;
using QuickZip.Tools;
using QuickZip.Presenter;
using QuickZip.Framework;

namespace QuickZip.View
{        
    /// <summary>
    /// Interaction logic for FolderView.xaml
    /// </summary>
    public partial class FolderView : IItemView
    {        
        public FolderView()
        {            
            InitializeComponent();
            this.AddHandler(Button.ClickEvent, 
                (RoutedEventHandler)delegate(object sender, RoutedEventArgs e) 
                { if (!(e.OriginalSource is ToggleButton)) Presenter.ChangeCurrentFolder(); });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate(); 

            
        }

        public Framework.ItemPresenterBase Presenter
        {
            get { return DataContext as Framework.ItemPresenterBase; }
        }

        #region IItemView Members

        public IItemView DuplicateSelf()
        {
            return new FolderView();
        }

        #endregion

        /// <summary>
        /// FolderPresenter Link
        /// </summary>
        public QuickZip.Presenter.FolderPresenter FolderPresenter
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public QuickZip.Presenter.MyComputerPresenter MyComputerPresenter
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
