using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.IO;

namespace CDP
{
    public interface IShell
    {
        ShellFolder GetRoot();
        void PopulateTopLevel(ShellFolder root);
        void PopulateChildren(ShellFolder folder);
    }

    public class Shell : IShell
    {
        private enum SpecialFolders
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

        public ShellFolder GetRoot()
        {
            Shell32.Shell shell32 = new Shell32.ShellClass();
            Shell32.Folder myComputer = shell32.NameSpace(SpecialFolders.MyComputer);
            return new ShellFolder
            {
                Name = myComputer.Title,
                IsRoot = true
            };
        }

        public void PopulateTopLevel(ShellFolder root)
        {
            Shell32.Shell shell32 = new Shell32.ShellClass();
            Shell32.Folder myComputer = shell32.NameSpace(SpecialFolders.MyComputer);

            foreach (Shell32.FolderItem fi in myComputer.Items())
            {
                root.Folders.Add(new ShellFolder
                {
                    Name = fi.Name,
                    Path = fi.Path,
                    //Icon = (ImageSource)GetIcon(fi.Path, false),
                    Shell = fi
                });
            }
        }

        public void PopulateChildren(ShellFolder folder)
        {
            /*foreach (string directory in Directory.GetDirectories(folder.Path))
            {
                folder.Folders.Add(new ShellFolder
                {
                    Name = fi.Name,
                    Path = fi.Path,
                    Icon = (ImageSource)GetIcon(fi.Path, false),
                    Shell = fi
                });
            }*/

            PopulateListWithChildren(folder.Folders, (Shell32.Folder)folder.Shell.GetFolder);
        }

        private void PopulateListWithChildren(ObservableCollection<ShellFolder> list, Shell32.Folder parent)
        {
            foreach (Shell32.FolderItem fi in parent.Items())
            {
                if (fi.IsFolder && !fi.IsBrowsable)
                {
                    list.Add(new ShellFolder
                    {
                        Name = fi.Name,
                        Path = fi.Path,
                        Icon = (ImageSource)GetIcon(fi.Path, false),
                        Shell = fi
                    });
                }
            }
        }

        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("Kernel32.dll")]
        private static extern uint GetFileAttributes(string fileName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private BitmapSource GetIcon(string path, bool open)
        {
            /*if (floppy)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("data\\floppy.png", UriKind.Relative);
                image.EndInit();
                return image;
            }*/

            /*
             * Passing in the file attributes to SHGetFileInfo is noticebly faster.
             * See http://www.codeguru.com/cpp/com-tech/shell/article.php/c4511/
             */
            uint fileAttributes = GetFileAttributes(path);
            SHGFI flags = SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_USEFILEATTRIBUTES;

            if (open)
            {
                flags |= SHGFI.SHGFI_OPENICON;
            }

            SHFILEINFO info = new SHFILEINFO();
            SHGetFileInfo(path, fileAttributes, out info, (uint)Marshal.SizeOf(info), flags);

            if (info.hIcon != IntPtr.Zero)
            {
                try
                {
                    return Imaging.CreateBitmapSourceFromHIcon(info.hIcon, new Int32Rect(0, 0, 16, 16), BitmapSizeOptions.FromEmptyOptions());
                }
                catch (ArgumentException)
                {
                    // Ugly fix for dodgy HRESULT, see http://code.google.com/p/coldemoplayer/issues/detail?id=1
                    return null;
                }
                finally
                {
                    DestroyIcon(info.hIcon);
                }
            }

            return null;
        }
    }
}
