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

namespace QuickZip.Presenter
{
    public class BreadcrumbCorePresenter : PresenterBase<View.BreadcrumbCoreView>
    {
        private string _selectedFolder = "";
        private ItemPresenterBase _rootPresenter = null;
        private ImageSource _icon = null;        

        public ItemPresenterBase Root { get { return GetRoot(); } set { SetRoot(value); } }
        public ExpanderPresenter Expander { get { return GetExpander(); } set { SetExpander(value); } }
        public string SelectedFolder { get { return _selectedFolder; } set { ChangeFolder(value); } }
        public ItemPresenterBase SelectedPresenter { get { return View.Count == 0 ? GetRoot() : GetFolder(View.Count-1); } }        
        public ImageSource Icon { get { return _icon; } set { _icon = value; OnPropertyChanged("Icon"); } }


        public BreadcrumbCorePresenter(View.BreadcrumbCoreView view)
            : base(view)
        {                        
        }


        internal void SetRoot(Framework.ItemPresenterBase root)
        {            
            View.SetRootItem(root);
            Icon = GetRoot().Icon;
            SelectedFolder = "";
        }

        internal void SetExpander(Framework.ItemPresenterBase expander)
        {            
            View.SetExpander(expander);
        }

        internal ExpanderPresenter GetExpander()
        {
            return View.GetExpander() as ExpanderPresenter;
        }

        internal ItemPresenterBase GetRoot()
        {
            ContentControl cc = View.GetRootView();
            return (ItemPresenterBase)cc.DataContext;
        }

        internal ItemPresenterBase GetLastFolder()
        {
            if (View.Count > 0)
                return GetFolder(View.Count - 1);
            else return GetRoot();
        }

        internal ItemPresenterBase GetFolder(int index)
        {
            if (index == -1 || index >= View.Count) return null;            
            ContentControl cc = View.GetView(index);
            return (ItemPresenterBase)cc.DataContext;
        }

        internal void ChangeFolder(string folderPath) 
        {
            ItemPresenterBase root = GetRoot();
            root.SelectedFolder = "";            
            if (folderPath == root.DisplayPath)
            {
                View.ClearAll();
                _selectedFolder = "";
                OnPropertyChanged("RootFolder");
                return;
            }
            else
            {                
                string[] folderSplit = folderPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string tempPath = ""; string current;
                View.ClearAfterIndex(folderSplit.Length); //It wont be displayed anyway, remove them.                
                ItemPresenterBase currentPresenter = GetRoot();

                for (int i = 0; i < folderSplit.Length; i++)
                {
                    current = folderSplit[i].EndsWith(":") ? folderSplit[i] + "\\" : folderSplit[i]; // e.g. C: ---> C:\
                    string next = i + 1 < folderSplit.Length ? folderSplit[i + 1] : ""; // next select path
                    tempPath = Path.Combine(tempPath, current);  //last path + current path = current full path

                    if (View.Count > i && GetFolder(i).FolderPath == tempPath) //Check if the path is exists and same
                    {
                        //Ignore it, already constructed
                        GetFolder(i).SelectedFolder = ""; //Reset select folder
                    }
                    else
                    {
                        //Construct it.
                        View.ClearAfterIndex(i);
                        currentPresenter.AddFolder(current, next);
                    }
                    currentPresenter = GetFolder(i);
                }
                _selectedFolder = folderPath;
                _rootPresenter = currentPresenter;

                OnPropertyChanged("SelectedFolder");
                OnPropertyChanged("SelectedPresenter");
                Icon = GetLastFolder().Icon;
            }
        }
    }
}