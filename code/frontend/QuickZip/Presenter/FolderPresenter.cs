using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using QuickZip.Tools;
using System.Diagnostics;
using System.Windows.Media;
using QuickZip.Framework;
using System.Windows.Media.Imaging;

namespace QuickZip.Presenter
{
    public class FolderPresenter : Framework.ItemPresenterBase
    {
        public FolderPresenter(BreadcrumbCorePresenter parentPresenter, int folderLevel, string folderPath, string selectedFolder, View.FolderView view)
            : base(parentPresenter, folderLevel, folderPath, selectedFolder, view)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(PollSubFoldersCallback));
        }

        protected virtual void PollSubFoldersCallback(object state)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(FolderPath);
                var subDirs = from d in di.GetDirectories() select d.FullName;
                SubFolders = subDirs.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public override bool FolderExists(string folder)
        {
            try
            {
                return Directory.Exists(Path.Combine(FolderPath, folder));
            }
            catch
            {
                return false;
            }
        }

        public override ItemPresenterBase ReturnSubFolder(string folder, string selectedItem)
        {
            if (folder.EndsWith(":"))
                folder += "\\";

            return new FolderPresenter(ParentPresenter, this.FolderLevel + 1, Path.Combine(FolderPath, folder), selectedItem, new View.FolderView());            
        }


        Tools.FileToIconConverter fti = new FileToIconConverter();
        public override ImageSource Icon
        {
            get
            {
                if (FolderPath == "")
                    return new BitmapImage(new Uri("pack://application:,,,/QuickZip;Component/Resources/computer.jpg"));
                return fti.Convert(FolderPath, null, null, null) as ImageSource;
            }
        }

    }
}
