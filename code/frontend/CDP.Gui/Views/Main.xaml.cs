using System.Windows;
using System.Windows.Controls;
using CDP.Core;
using System.IO;

namespace CDP.Gui.Views
{
    internal partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            INavigationService navigationService = ObjectCreator.Get<INavigationService>();

            if (navigationService.CurrentPageTitle != "Main")
            {
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (File.Exists(fileNames[0]))
            {
                IMediator mediator = ObjectCreator.Get<IMediator>();
                mediator.Notify(Messages.SetFolder, new SetFolderMessageParameters(Path.GetDirectoryName(fileNames[0]), Path.GetFileName(fileNames[0])));
            }
        }
    }
}
