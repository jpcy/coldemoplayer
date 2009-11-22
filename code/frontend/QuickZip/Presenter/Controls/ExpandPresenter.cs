using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using QuickZip.Tools;
using System.Diagnostics;
using System.Windows.Controls;
using QuickZip.Framework;

namespace QuickZip.Presenter
{
    public class ExpanderPresenter : Framework.ItemPresenterBase
    {
        public ExpanderPresenter(BreadcrumbCorePresenter parentPresenter, IItemView view)
            : base(parentPresenter, -1, "", "", view)
        {
            DisplayPath = "";
            SubFolders = new string[] { };
        }

        public void Insert(string folder, int level)
        {
            List<string> retVal = new List<string>(SubFolders);
            if (retVal.IndexOf(folder) != -1)
                retVal.Remove(folder);

            retVal.Insert(level, folder);
            SubFolders = retVal.ToArray();
        }

        public void Add(string folder)
        {
            List<string> retVal = new List<string>(SubFolders);
            if (retVal.IndexOf(folder) == -1)
            {
                retVal.Insert(0, folder);
                SubFolders = retVal.ToArray();
            }
        }

        public void Remove(string folder)
        {
            List<string> retVal = new List<string>(SubFolders);
            if (retVal.IndexOf(folder) != -1)
            {
                retVal.Remove(folder);
                SubFolders = retVal.ToArray();
            }
        }

        public void ClearAll()
        {
            SubFolders = new String[] { };
        }

        

    }
}
