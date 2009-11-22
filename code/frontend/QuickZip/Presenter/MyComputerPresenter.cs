using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using QuickZip.Tools;
using System.Diagnostics;

namespace QuickZip.Presenter
{    
    public class MyComputerPresenter : FolderPresenter
    {
        public MyComputerPresenter(BreadcrumbCorePresenter parentPresenter, int folderLevel, string folderPath, string selectedFolder)
            : base(parentPresenter, folderLevel, folderPath, selectedFolder, new View.FolderView())
        {
            DisplayPath = "Computer";            
        }

        protected override void PollSubFoldersCallback(object state)
        {
            SubFolders = Environment.GetLogicalDrives();
        }

    }
}
