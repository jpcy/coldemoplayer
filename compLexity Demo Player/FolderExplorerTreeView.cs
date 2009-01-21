using System;
using System.Collections.Generic;
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
using System.IO;
using System.Runtime.InteropServices;

namespace compLexity_Demo_Player
{
    public class FolderExplorerTreeView : TreeView
    {
        private enum ShellFolder
        {
            Desktop = Shell32.ShellSpecialFolderConstants.ssfDESKTOP,
            DesktopDirectory = Shell32.ShellSpecialFolderConstants.ssfDESKTOPDIRECTORY,
            MyComputer = Shell32.ShellSpecialFolderConstants.ssfDRIVES,
            MyDocuments = Shell32.ShellSpecialFolderConstants.ssfPERSONAL,
            MyPictures = Shell32.ShellSpecialFolderConstants.ssfMYPICTURES,
            History = Shell32.ShellSpecialFolderConstants.ssfHISTORY,
            Favorites = Shell32.ShellSpecialFolderConstants.ssfFAVORITES,
            Fonts = Shell32.ShellSpecialFolderConstants.ssfFONTS,
            ControlPanel = Shell32.ShellSpecialFolderConstants.ssfCONTROLS,
            TemporaryInternetFiles = Shell32.ShellSpecialFolderConstants.ssfINTERNETCACHE,
            MyNetworkPlaces = Shell32.ShellSpecialFolderConstants.ssfNETHOOD,
            NetworkNeighborhood = Shell32.ShellSpecialFolderConstants.ssfNETWORK,
            ProgramFiles = Shell32.ShellSpecialFolderConstants.ssfPROGRAMFILES,
            RecentFiles = Shell32.ShellSpecialFolderConstants.ssfRECENT,
            StartMenu = Shell32.ShellSpecialFolderConstants.ssfSTARTMENU,
            Windows = Shell32.ShellSpecialFolderConstants.ssfWINDOWS,
            Printers = Shell32.ShellSpecialFolderConstants.ssfPRINTERS,
            RecycleBin = Shell32.ShellSpecialFolderConstants.ssfBITBUCKET,
            Cookies = Shell32.ShellSpecialFolderConstants.ssfCOOKIES,
            ApplicationData = Shell32.ShellSpecialFolderConstants.ssfAPPDATA,
            SendTo = Shell32.ShellSpecialFolderConstants.ssfSENDTO,
            StartUp = Shell32.ShellSpecialFolderConstants.ssfSTARTUP
        }

        private object dummyNode = null;

        public String CurrentFolderPath
        {
            get
            {
                if (this.SelectedItem != null)
                {
                    TreeViewItem tvi = (TreeViewItem)this.SelectedItem;
                    Shell32.FolderItem folderItem = (Shell32.FolderItem)tvi.Tag;

                    if (Directory.Exists(folderItem.Path))
                    {
                        return folderItem.Path;
                    }
                }

                return "";
            }

            set
            {
                if (value == CurrentFolderPath)
                    return;

                String path = value;

                // don't bother drilling unless the directory exists
                if (!Directory.Exists(path))
                    return;

                // remove trailing slash
                if (path.EndsWith("\\"))
                {
                    path = path.Remove(path.Length - 1, 1);
                }

                String[] pathComponents = path.Split('\\');

                // append trailing slash if path is a drive (ends with semicolon)
                if (pathComponents[0].EndsWith(":"))
                {
                    pathComponents[0] += "\\";
                }

                // clear out "My Computer" children's, children (replace with dummyNode)
                TreeViewItem root = (TreeViewItem)this.Items[0];

                foreach (TreeViewItem tvi in root.Items)
                {
                    tvi.Items.Clear();
                    tvi.Items.Add(dummyNode);
                    tvi.IsExpanded = false;
                }

                for (Int32 i = 0; i < pathComponents.Length; i++)
                {
                    Boolean foundMatch = false;

                    foreach (TreeViewItem tvi in root.Items)
                    {
                        Shell32.FolderItem folderItem = (Shell32.FolderItem)tvi.Tag;

                        // drives use path, folders use name
                        String compare = (i == 0 ? folderItem.Path : folderItem.Name);

                        if (String.Equals(compare, pathComponents[i], StringComparison.CurrentCultureIgnoreCase))
                        {
                            foundMatch = true;
                            ExpandTreeItem(tvi);
                            root = tvi;
                            tvi.IsExpanded = true;

                            if (i == pathComponents.Length - 1)
                            {
                                tvi.IsSelected = true;
                            }

                            break;
                        }
                    }

                    if (!foundMatch)
                    {
                        return; // abort, couldn't find folder
                    }
                }
            }
        }

        static FolderExplorerTreeView()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(FolderExplorerTreeView), new FrameworkPropertyMetadata(typeof(FolderExplorerTreeView)));
        }

        public FolderExplorerTreeView()
        {
            Initialized += new EventHandler(FolderExplorerTreeView_Initialized);
        }

        private void FolderExplorerTreeView_Initialized(object sender, EventArgs e)
        {
            Shell32.Shell shell32 = new Shell32.ShellClass();
            Shell32.Folder folderDesktop = shell32.NameSpace(ShellFolder.Desktop);
            Shell32.Folder folderMyComputer = shell32.NameSpace(ShellFolder.MyComputer);

            // find my computer folder item
            Shell32.FolderItem folderItemMyComputer = null;

            foreach (Shell32.FolderItem fi in folderDesktop.Items())
            {
                if (fi.Name == folderMyComputer.Title)
                {
                    folderItemMyComputer = fi;
                    break;
                }
            }

            if (folderItemMyComputer == null)
            {
                throw new ApplicationException("Error finding \"My Computer\" folder item.");
            }

            // add my computer
            TreeViewItem root = CreateTreeItem(folderItemMyComputer);
            Items.Add(root);

            // iterate through the "My Computer" namespace and populate the first level nodes
            foreach (Shell32.FolderItem item in folderMyComputer.Items())
            {
                if (item.IsFileSystem)
                {
                    TreeViewItem tvi = CreateTreeItem(item);
                    tvi.Items.Add(dummyNode);
                    tvi.Expanded += new RoutedEventHandler(folder_Expanded);
                    tvi.PreviewMouseDown += new MouseButtonEventHandler(folder_PreviewMouseDown);
                    root.Items.Add(tvi);
                }
            }

            // expand "My Computer"
            root.IsExpanded = true;
        }

        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            ExpandTreeItem(tvi);
        }

        void folder_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;

            ChangeIcon(tvi, false);

            tvi.Items.Clear();
            tvi.Items.Add(dummyNode);

            e.Handled = true;
        }

        void folder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*TreeViewItem tvi = (TreeViewItem)sender;

            if (tvi.IsSelectionActive)
            {
                tvi.IsExpanded = true;
            }*/
        }

        private void ExpandTreeItem(TreeViewItem tvi)
        {
            Shell32.Shell shell32 = new Shell32.ShellClass();

            // don't expand my computer
            if (((Shell32.FolderItem)tvi.Tag).Name == shell32.NameSpace(ShellFolder.MyComputer).Title)
            {
                return;
            }

            if (tvi.Items.Count == 1 && tvi.Items[0] == dummyNode)
            {
                tvi.Items.Clear();

                Shell32.FolderItem folderItem = (Shell32.FolderItem)tvi.Tag;
                Shell32.Folder folder = (Shell32.Folder)folderItem.GetFolder;

                String[] strFolders = Directory.GetDirectories(folderItem.Path);

                foreach (String s in strFolders)
                {
                    Shell32.Shell shell = new Shell32.ShellClass();
                    Shell32.Folder folderTemp = shell.NameSpace(folderItem.Path);
                    Shell32.FolderItem folderItemTemp = (Shell32.FolderItem)folderTemp.ParseName(s.Substring(s.LastIndexOf('\\') + 1));

                    TreeViewItem newItem = CreateTreeItem(folderItemTemp);
                    newItem.Items.Add(dummyNode);
                    newItem.Expanded += new RoutedEventHandler(folder_Expanded);
                    newItem.Collapsed += new RoutedEventHandler(folder_Collapsed);
                    newItem.PreviewMouseDown += new MouseButtonEventHandler(folder_PreviewMouseDown);
                    tvi.Items.Add(newItem);
                }

                if (tvi.Items.Count > 0)
                {
                    // change to opened icon
                    ChangeIcon(tvi, true);
                }
            }
        }

        private TreeViewItem CreateTreeItem(Shell32.FolderItem folderItem)
        {
            TreeViewItem tvi = new TreeViewItem();
            //tvi.Header = folderItem.Name;
            tvi.Tag = folderItem;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Margin = new Thickness(1);
            tvi.Header = panel;

            Image image = new Image();
            image.Source = ExtractIcons.GetIcon(folderItem.Path, false);
            image.Margin = new Thickness(0, 0, 4, 0);

            TextBlock text = new TextBlock();
            text.VerticalAlignment = VerticalAlignment.Center;
            text.Text = folderItem.Name;

            panel.Children.Add(image);
            panel.Children.Add(text);

            return tvi;
        }

        private void ChangeIcon(TreeViewItem tvi, Boolean open)
        {
            Shell32.FolderItem folderItem = (Shell32.FolderItem)tvi.Tag;

            StackPanel panel = (StackPanel)tvi.Header;

            Image image = new Image();
            image.Source = ExtractIcons.GetIcon(folderItem.Path, open);
            image.Margin = new Thickness(0, 0, 4, 0);

            panel.Children.RemoveAt(0);
            panel.Children.Insert(0, image);
        }
    }

    public class ExtractIcons
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
        };

        private enum SHGFI
        {
            SHGFI_ICON = 0x000000100,     // get icon
            SHGFI_DISPLAYNAME = 0x000000200,     // get display name
            SHGFI_TYPENAME = 0x000000400,     // get type name
            SHGFI_ATTRIBUTES = 0x000000800,     // get attributes
            SHGFI_ICONLOCATION = 0x000001000,     // get icon location
            SHGFI_EXETYPE = 0x000002000,     // return exe type
            SHGFI_SYSICONINDEX = 0x000004000,     // get system icon index
            SHGFI_LINKOVERLAY = 0x000008000,     // put a link overlay on icon
            SHGFI_SELECTED = 0x000010000,     // show icon in selected state
            SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
            SHGFI_LARGEICON = 0x000000000,     // get large icon
            SHGFI_SMALLICON = 0x000000001,     // get small icon
            SHGFI_OPENICON = 0x000000002,     // get open icon
            SHGFI_SHELLICONSIZE = 0x000000004,     // get shell size icon
            SHGFI_PIDL = 0x000000008,     // pszPath is a pidl
            SHGFI_USEFILEATTRIBUTES = 0x000000010     // use passed dwFileAttribute
        }

        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("Kernel32.dll")]
        private static extern UInt32 GetFileAttributes(String fileName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public static BitmapSource GetIcon(String path, Boolean open)
        {
            /*
             * Passing in the file attributes to SHGetFileInfo is noticebly faster.
             * See http://www.codeguru.com/cpp/com-tech/shell/article.php/c4511/
             */
            UInt32 fileAttributes = GetFileAttributes(path);

            SHFILEINFO info = new SHFILEINFO();
            SHGFI flags = SHGFI.SHGFI_USEFILEATTRIBUTES | SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON;

            if (open)
            {
                flags |= SHGFI.SHGFI_OPENICON;
            }

            SHGetFileInfo(path, fileAttributes, out info, (uint)Marshal.SizeOf(info), flags);

            if (info.hIcon != IntPtr.Zero)
            {
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(info.hIcon, new Int32Rect(0, 0, 16, 16), BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(info.hIcon);

                return bs;
            }

            return null;
        }
    }
}
