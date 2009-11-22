using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickZip.Framework
{

    public class ItemPresenterBase : Notifier
    {
        protected readonly IItemView _view;
        public IItemView View { get { return _view; } }

        private Presenter.BreadcrumbCorePresenter _parentPresenter;

        private string[] _subFolders = new string[0];
        private string _folderPath;
        private string _selectedFolder;
        private string _displayPath;
        private int _folderLevel;

        public bool HitTestVisible {set { changedHitTestVisible(value); } }
        public string[] SubFolders { get { return _subFolders; } set { _subFolders = value; OnPropertyChanged("SubFolders"); OnPropertyChanged("HasSubFolders"); } }
        public bool HasSubFolders { get { return _subFolders.Length > 0; } }
        public string FolderPath { get { return _folderPath; } set { _folderPath = value; OnPropertyChanged("FolderPath"); } }
        public bool ShowDisplayPath { get { return _displayPath != ""; } }
        public bool IsExpander { get { return this is Presenter.ExpanderPresenter; } }
        public string DisplayPath { get { return _displayPath; } set { _displayPath = value; OnPropertyChanged("DisplayPath"); } }
        public string SelectedFolder { get { return _selectedFolder; } set { _selectedFolder = value; OnPropertyChanged("SelectedFolder"); OnPropertyChanged("SelectedIndex"); } }
        public int SelectedIndex { get { return getSelectedIndex(); } set { changeSelectedIndex(value); } }
        public Presenter.BreadcrumbCorePresenter ParentPresenter { get { return _parentPresenter; } set { _parentPresenter = value; } }
        public int FolderLevel { get { return _folderLevel; } set { _folderLevel = value; } } //Index in parent Breadcrumb
        public virtual ImageSource Icon { get { return null; } }

        public virtual ItemPresenterBase ReturnSubFolder(string folder, string selectedItem)
        {
            throw new NotImplementedException();
        }

        public virtual bool FolderExists(string folder)
        {
            return true;
        }

        public void AddFolder(string folderPath, string selectedItem)
        {
            ItemPresenterBase retVal = ReturnSubFolder(folderPath, selectedItem);
            ParentPresenter.View.AddItem(retVal);
        }

        private int getSelectedIndex()
        {
            if (SelectedFolder == "") return -1;
            for (int i = 0; i < SubFolders.Length; i++)
                if (String.Equals(SubFolders[i], SelectedFolder, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            return -1;
        }

        private void changedHitTestVisible(bool value)
        {
            if (!value)
                _parentPresenter.Expander.Add(DisplayPath);
            else _parentPresenter.Expander.Remove(DisplayPath);

        }

        //private string getActualPath()
        //{
        //    string retVal = "";
        //    for (int i = 0; i <= FolderLevel; i++)
        //        retVal += ParentPresenter.GetFolder(i).FolderPath;
        //    return retVal;
        //}

        private void changeSelectedIndex(int newValue)
        {
            if (newValue == -1)
                SelectedFolder = "";
            else
            {
                SelectedFolder = SubFolders[newValue];
                _parentPresenter.ChangeFolder(SelectedFolder);
            }
        }

        internal void ChangeCurrentFolder()
        {
            SelectedIndex = -1;
            ParentPresenter.ChangeFolder(FolderPath);
        }

        internal ItemPresenterBase(Presenter.BreadcrumbCorePresenter parentPresenter, IItemView view) : base()
        {
            this._parentPresenter = parentPresenter;
            this._view = view;
        }

        public ItemPresenterBase(Presenter.BreadcrumbCorePresenter parentPresenter, int folderLevel,
            string folderPath, string selectedFolder, IItemView view)
            : base()
        {
            this._parentPresenter = parentPresenter;
            this._view = view;
            this._folderLevel = folderLevel;
            _folderPath = folderPath;
            _displayPath = folderPath;
            _selectedFolder = Path.Combine(folderPath, selectedFolder);
        }
    }
}
