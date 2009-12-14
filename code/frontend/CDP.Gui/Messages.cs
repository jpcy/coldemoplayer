using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Gui
{
    internal enum Messages
    {
        SetFolder,
        SetSelectedDemoName,
        SelectedFolderChanged,
        SelectedDemoChanged
    };

    internal class SetFolderMessageParameters
    {
        public string Path { get; private set; }
        public string FileNameToSelect { get; private set; }

        public SetFolderMessageParameters(string path, string fileNameToSelect)
        {
            if (path == null)
            {
                throw new ArgumentNullException("folderPath");
            }

            Path = path;
            FileNameToSelect = fileNameToSelect;
        }
    }

    internal class SelectedFolderChangedMessageParameters
    {
        public string Path { get; private set; }
        public string FileNameToSelect { get; private set; }

        public SelectedFolderChangedMessageParameters(string path, string fileNameToSelect)
        {
            if (path == null)
            {
                throw new ArgumentNullException("folderPath");
            }

            Path = path;
            FileNameToSelect = fileNameToSelect;
        }
    }
}
