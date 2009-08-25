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
using QuickZip.Framework;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace QuickZip.View
{
    /// <summary>
    /// Interaction logic for TextBox.xaml
    /// </summary>
    public partial class AutoCompleteTextBoxView : TextBox 
    {
        //public static DependencyProperty CurrentPresenterProperty = DependencyProperty.Register("CurrentPresenter", typeof(ItemPresenterBase), typeof(BreadcrumbControl),
        //    new UIPropertyMetadata(null));

        //public ItemPresenterBase CurrentPresenter
        //{
        //    get { return GetValue(CurrentPresenterProperty) as ItemPresenterBase; } 
        //    set { SetValue(CurrentPresenterProperty, value); } }

        internal Popup Popup { get { return this.Template.FindName("PART_Popup", this) as Popup; } }
        internal ListBox ItemList { get { return this.Template.FindName("PART_ItemList", this) as ListBox; } }
        //12-25-08 : Add Ghost image when picking from ItemList
        internal TextBlock TempVisual { get { return this.Template.FindName("PART_TempVisual", this) as TextBlock; } }
        internal ScrollViewer Host { get { return this.Template.FindName("PART_ContentHost", this) as ScrollViewer; } }
        internal UIElement TextBoxView { get { foreach (object o in LogicalTreeHelper.GetChildren(Host)) return o as UIElement; return null; } }
        private bool _loaded = false;
        private string _lastPath;

        public AutoCompleteTextBoxView()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _loaded = true;
            
            this.KeyDown += new KeyEventHandler(AutoCompleteTextBox_KeyDown);
            this.PreviewKeyDown += new KeyEventHandler(AutoCompleteTextBox_PreviewKeyDown);
            ItemList.PreviewMouseDown += new MouseButtonEventHandler(ItemList_PreviewMouseDown);
            ItemList.KeyDown += new KeyEventHandler(ItemList_KeyDown);
            
            this.Focus();
        }

        public AutoCompleteTextBoxPresenter Presenter { get { return DataContext as AutoCompleteTextBoxPresenter; } }

        void updateSource()
        {
            if (this.GetBindingExpression(TextBox.TextProperty) != null)
                this.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        void AutoCompleteTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Popup.IsOpen = false;
                updateSource();                
            }
        }

        public void CheckPopupIsOpen()
        {
            Popup.IsOpen = Presenter.Suggestions.Length > 0;
            TextBoxView.Focus();
        }

        void AutoCompleteTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //12-25-08 - added PageDown Support
            if (ItemList.Items.Count > 0 && !(e.OriginalSource is ListBoxItem))
                switch (e.Key)
                {
                    case Key.Up:
                    case Key.Down:
                    case Key.Prior:
                    case Key.Next:
                        ItemList.Focus();
                        ItemList.SelectedIndex = 0;
                        ListBoxItem lbi = ItemList.ItemContainerGenerator.ContainerFromIndex(ItemList.SelectedIndex) as ListBoxItem;
                        lbi.Focus();
                        _lastPath = Text;
                        e.Handled = true;
                        break;

                }
        }

        void ItemList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is ListBoxItem)
            {

                ListBoxItem tb = e.OriginalSource as ListBoxItem;

                e.Handled = true;
                switch (e.Key)
                {
                    case Key.Enter:
                        Text = (tb.Content as string); updateSource(); break;
                    //12-25-08 - added "\" support when picking in list view
                    case Key.Oem5:
                        Text = (tb.Content as string) + "\\";
                        break;
                    //12-25-08 - roll back if escape is pressed
                    case Key.Escape:
                        Text = _lastPath.TrimEnd('\\') + "\\";
                        break;
                    default: e.Handled = false; break;
                }
                //12-25-08 - Force focus back the control after selected.
                if (e.Handled)
                {
                    Keyboard.Focus(this);
                    Popup.IsOpen = false;
                    this.Select(Text.Length, 0); //Select last char
                }
            }
        }

        void ItemList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock tb = e.OriginalSource as TextBlock;
                if (tb != null)
                {
                    Text = tb.Text;
                    updateSource();
                    Popup.IsOpen = false;
                    e.Handled = true;
                }
            }
        }


        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Presenter.SelectedFolder = this.Text;
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
                    
        }


    }
}
