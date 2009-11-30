using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Gui
{
    public enum Messages
    {
        SetFolder,
        SelectedFolderChanged,
        SelectedDemoChanged
    };

    public class SetFolderMessageParameters
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

    public class SelectedFolderChangedMessageParameters
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
