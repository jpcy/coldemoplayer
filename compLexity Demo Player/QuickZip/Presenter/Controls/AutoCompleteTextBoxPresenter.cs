using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;
using QuickZip.View;
using System.Windows.Media;
using QuickZip.Framework;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace QuickZip.Presenter
{
    public class AutoCompleteTextBoxPresenter : PresenterBase<View.AutoCompleteTextBoxView>
    {
        private string _selectedFolder = "";
        private ItemPresenterBase _rootPresenter = null;
        private ItemPresenterBase _currentPresenter = null;
        private string[] _suggestions = new string[] { };
        private Visibility _visibility;

        public Visibility Visibility { get { return _visibility; } set { _visibility = value; } }
        public bool IsVisible { get { return Visibility == Visibility.Visible; } }
        public ItemPresenterBase Root { get { return _rootPresenter; } set { _rootPresenter = value; } }
        public string SelectedFolder { get { return _selectedFolder; } set { ChangeSelectFolder(value); } }
        public string[] Suggestions { get { return _suggestions; } set { _suggestions = value; OnPropertyChanged("Suggestions"); } }        

        public AutoCompleteTextBoxPresenter(View.AutoCompleteTextBoxView view)
            : base(view)
        {

        }

        private ItemPresenterBase lookup(string folderPath)
        {
            
            if (folderPath == null)
                return null;
            string[] folderSplit = folderPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            ItemPresenterBase current = _rootPresenter;
            foreach (string folder in folderSplit)
                if (current.FolderExists(folder))
                    current = current.ReturnSubFolder(folder, "");
                else return null;

            return current;
        }

        public void ChangeSelectFolder(string value)
        {
            _selectedFolder = value;
            OnPropertyChanged("SelectedFolder");

            if (IsVisible && _rootPresenter != null && _selectedFolder != "")
            {
                ItemPresenterBase folderPresenter = lookup(_selectedFolder.EndsWith(":\\") ? _selectedFolder : Path.GetDirectoryName(_selectedFolder));
                if (folderPresenter != null)
                {                    
                    _currentPresenter = folderPresenter;
                    
                    if (_currentPresenter.SubFolders.Length == 0)
                        folderPresenter.PropertyChanged += new PropertyChangedEventHandler(folderPresenter_PropertyChanged);
                    else loadSubFolders();
                }
                else Suggestions = new string[] { };
            }

        }

        private void loadSubFolders()
        {
            if (Keyboard.FocusedElement == View)
            {
                if (_currentPresenter == null)
                    Suggestions = new string[] { };
                else
                {
                    List<string> retVal = new List<string>();
                    foreach (string subFolder in _currentPresenter.SubFolders)
                        if (subFolder.ToLower().StartsWith(_selectedFolder.ToLower()))
                            retVal.Add(subFolder);
                    Suggestions = retVal.ToArray();

                }
                View.CheckPopupIsOpen();
            }
        }

        void folderPresenter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SubFolders")
                if (_currentPresenter != null)
                    _currentPresenter.PropertyChanged -= new PropertyChangedEventHandler(folderPresenter_PropertyChanged);
            _dispatcher.Invoke(
                            new ThreadStart(delegate
                            {
                                loadSubFolders();
                            }));
        }





    }
}
