using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CDP
{
    public class ShellFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }
        public bool IsRoot { get; set; }
        public Shell32.FolderItem Shell { get; set; }
        public ObservableCollection<ShellFolder> Folders { get; set; }

        public ShellFolder()
        {
            IsRoot = false;
            Folders = new ObservableCollection<ShellFolder>();
        }
    }
}
