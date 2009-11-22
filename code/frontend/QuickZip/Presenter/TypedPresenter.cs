using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using QuickZip.Tools;
using System.Diagnostics;
using System.Windows.Media;
using System.ComponentModel;
using System.Reflection;
using QuickZip.View;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace QuickZip.Presenter
{
    public class TypedPresenter<T> : Framework.ItemPresenterBase where T : INotifyPropertyChanged
    {
        private readonly string _subModelPropertyPath;
        private readonly string _folderNamePropertyPath;
        private readonly string _iconBitmapPath;
        private readonly string _parentPath;
        private readonly ImageSource _icon;
        private readonly T _model;

        public T Model { get { return _model; } }

        public TypedPresenter(BreadcrumbCorePresenter parentPresenter, int folderLevel, T model,
            string folderNamePropertyPath, string subModelPropertyPath, string iconBitmapPath, string parentPath,
            Framework.IItemView view)
            : base(parentPresenter, view)
        {
            _subModelPropertyPath = subModelPropertyPath;
            _folderNamePropertyPath = folderNamePropertyPath;
            _iconBitmapPath = iconBitmapPath;
            _parentPath = parentPath;
            _model = model;

            DisplayPath = GetProperty( folderNamePropertyPath) as string;
            FolderLevel = folderLevel;

            if (folderLevel <= -1)
                FolderPath = ""; 
            else
                FolderPath = Path.Combine(
                    folderLevel > 0 ? 
                    parentPresenter.GetFolder(folderLevel - 1).FolderPath :
                    parentPresenter.GetRoot().FolderPath
                    , GetProperty( folderNamePropertyPath) as string);


            ThreadPool.QueueUserWorkItem(new WaitCallback(PollSubFoldersCallback));
            model.PropertyChanged += new PropertyChangedEventHandler(model_PropertyChanged);


            _icon =  _iconBitmapPath == null ? null :
                                GetPropertyType(_iconBitmapPath).GetType is Bitmap ?
                                FileToIconConverter.loadBitmap((Bitmap)GetProperty(_iconBitmapPath)) :
                                new BitmapImage(new Uri(GetProperty(_iconBitmapPath) as string));
            OnPropertyChanged("Icon");
        }

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _subModelPropertyPath)
                ThreadPool.QueueUserWorkItem(new WaitCallback(PollSubFoldersCallback));
        }

        public Type GetPropertyType(string property)
        {
            return GetPropertyType(_model, property);
        }

        public Type GetPropertyType(T model, string property)
        {
            Type t = model.GetType();
            return t.GetProperty(property).GetType();
        }

        public object GetProperty(string property)
        {
            return GetProperty(_model, property);
        }

        public object GetProperty(T model, string property)
        {
            Type t = model.GetType();
            return t.GetProperty(property).GetValue(model, null);
        }

        public T GetModel(string folderPath)
        {
            Type t = _model.GetType();
            T[] subModels = (T[])GetProperty(_subModelPropertyPath);
            foreach (T subModel in subModels)
                if (folderPath == GetProperty(subModel, _folderNamePropertyPath) as string)
                    return subModel;
            throw new ArgumentException("Not Found Model");
        }

        public T GetParent()
        {
            if (_parentPath == null)
            {
                Debug.WriteLine("Warning : ParentPath is not defined");
                return default(T);
            }

            return (T)GetProperty(_parentPath);
        }
        

        protected virtual void PollSubFoldersCallback(object state)
        {
            T[] subModels = (T[])GetProperty(_subModelPropertyPath);
            List<string> retVal = new List<string>();
            foreach (T subModel in subModels)
            {
                string header = (string)GetProperty(subModel, _folderNamePropertyPath);
                retVal.Add(header);
            }

            SubFolders = retVal.ToArray();
        }


        public override Framework.ItemPresenterBase ReturnSubFolder(string folder, string selectedItem)
        {
            return new TypedPresenter<T>(ParentPresenter, FolderLevel + 1,
                GetModel(Path.Combine(FolderPath, folder)), _folderNamePropertyPath, _subModelPropertyPath, _iconBitmapPath, _parentPath, _view.DuplicateSelf());
        }

        
        public override ImageSource Icon
        {
            get
            {
                return _icon;
            }
        }

    }
}
