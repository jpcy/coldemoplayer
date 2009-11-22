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
using QuickZip.Presenter;
using QuickZip.View;
using System.Diagnostics;
using System.Collections.ObjectModel;
using QuickZip.Framework;

namespace QuickZip
{
    /// <summary>
    /// Interaction logic for Breadcrumb.xaml
    /// </summary>
    public partial class BreadcrumbControl : UserControl
    {
        public static DependencyProperty SelectedFolderProperty = DependencyProperty.Register("SelectedFolder", typeof(string), typeof(BreadcrumbControl),
            new UIPropertyMetadata("", new PropertyChangedCallback(SelectedFolderChanged)));
        public static DependencyProperty SelectedPresenterProperty = DependencyProperty.Register("SelectedPresenter", typeof(ItemPresenterBase), typeof(BreadcrumbControl),
            new UIPropertyMetadata(null, new PropertyChangedCallback(SelectedFolderChanged)));
        public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(BreadcrumbControl));
        public static DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(int), typeof(BreadcrumbControl),
            new PropertyMetadata(0, new PropertyChangedCallback(ProgressChanged)));
        public static DependencyProperty IsIndeterminateProperty = DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(BreadcrumbControl),
            new PropertyMetadata(false, new PropertyChangedCallback(ProgressChanged)));
        public static DependencyProperty IsTextBoxEnabledProperty = DependencyProperty.Register("IsTextBoxEnabled", typeof(bool), typeof(BreadcrumbControl),
            new PropertyMetadata(true, new PropertyChangedCallback(IsTextBoxEnabledChanged)));

        public string SelectedFolder { get { return GetValue(SelectedFolderProperty) as string; } set { SetValue(SelectedFolderProperty, value); } }
        public ItemPresenterBase SelectedPresenter { get { return GetValue(SelectedPresenterProperty) as ItemPresenterBase; } private set { SetValue(SelectedPresenterProperty, value); } }
        public ImageSource Icon { get { return GetValue(IconProperty) as ImageSource; } set { SetValue(IconProperty, value); } }
        public int Progress { get { return (int)GetValue(ProgressProperty); } set { SetValue(ProgressProperty, value); } }
        public bool IsIndeterminate { get { return (bool)GetValue(IsIndeterminateProperty); } set { SetValue(IsIndeterminateProperty, value); } }
        public bool IsTextBoxEnabled { get { return (bool)GetValue(IsTextBoxEnabledProperty); } set { SetValue(IsTextBoxEnabledProperty, value); } }

        public BreadcrumbCorePresenter CorePresenter { get { return _bcpresenter; } }
        private BreadcrumbCorePresenter _bcpresenter;
        private AutoCompleteTextBoxPresenter _tbPresenter;


        public BreadcrumbControl()
        {
            InitializeComponent();

            _bcpresenter = new BreadcrumbCorePresenter(new BreadcrumbCoreView());
            _bcpresenter.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(presenter_PropertyChanged);
            bc.DataContext = _bcpresenter;
            bc.Content = _bcpresenter.View;

            _tbPresenter = new AutoCompleteTextBoxPresenter(new AutoCompleteTextBoxView());
            tb.DataContext = _tbPresenter;
            tb.Content = _tbPresenter.View;
            _tbPresenter.View.Focus();

            SwitchToFolder();

            Binding selectedFolderBinding = new Binding("SelectedFolder"); 
            selectedFolderBinding.Source = this;
            selectedFolderBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            _tbPresenter.View.SetBinding(SelectedFolderProperty, selectedFolderBinding);
        }

        public void SwitchPresenter(ItemPresenterBase root, ExpanderPresenter expander)
        {
            _bcpresenter.Expander = expander;
            _bcpresenter.Root = root;
            _tbPresenter.Root = root;

            SelectedFolder = "";
            Icon = _bcpresenter.Icon;
        }

        public void SwitchToFolder()
        {
            SwitchPresenter(new MyComputerPresenter(_bcpresenter, -1, "", ""), new ExpanderPresenter(_bcpresenter, new FolderView()));
        }

        void presenter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedFolder")
            {
                SelectedFolder = _bcpresenter.SelectedFolder;
                SelectedPresenter = _bcpresenter.SelectedPresenter;
                Icon = _bcpresenter.Icon;
            }
        }

        public static void SelectedFolderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectedFolderProperty)
            {
                (obj as BreadcrumbControl)._bcpresenter.SelectedFolder = e.NewValue as string;
                (obj as BreadcrumbControl).toggleTextBox.IsChecked = false;
                (obj as BreadcrumbControl).Icon = (obj as BreadcrumbControl)._bcpresenter.Icon;
            }
        }

        public static void ProgressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {            
        }

        public static void IsTextBoxEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {            
            if (!(bool)e.NewValue)
             (obj as BreadcrumbControl).toggleTextBox.IsChecked = false;
        }

        

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine(Keyboard.FocusedElement);
            toggleTextBox.IsChecked = !toggleTextBox.IsChecked;            

            _tbPresenter.View.Focus();            
            _tbPresenter.View.SelectionStart = SelectedFolder.Length;
            //Debug.WriteLine(Keyboard.FocusedElement);
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
