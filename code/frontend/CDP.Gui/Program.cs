using System;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using CDP.Core;

namespace CDP.Gui
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int processId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Object mappings.
            Core.ObjectMappings.Initialise();
            ObjectCreator.Map<IMediator, Mediator>();
            ObjectCreator.Map<INavigationService, NavigationService>();
            ObjectCreator.Map<IFileAssociation, FileAssociation>();
            ObjectCreator.Map<IIpcChannel, IpcChannel>();

            // Ipc.
            IIpcChannel ipcChannel = ObjectCreator.Get<IIpcChannel>();
            IProcessFinder processFinder = ObjectCreator.Get<IProcessFinder>();
            IProcess currentProcess = processFinder.GetCurrentProcess();
            IProcess otherProcess = processFinder.FindByName(currentProcess.Name, currentProcess.FileName, currentProcess.Id).FirstOrDefault();

            if (otherProcess == null)
            {
                AllowSetForegroundWindow(currentProcess.Id);
                ipcChannel.Open();
            }
            else
            {
                SetForegroundWindow(otherProcess.MainWindowHandle);
                string[] args = Environment.GetCommandLineArgs();

                if (args.Length > 1 && File.Exists(args[1]))
                {
                    ipcChannel.Transport(args[1]);
                }

                return;                
            }

            // Demo manager and plugins.
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.AddPlugin(0, new HalfLife.Handler());
            demoManager.AddPlugin(1, new CounterStrike.Handler());
            demoManager.AddPlugin(0, new Quake3Arena.Handler());
            demoManager.AddPlugin(0, new QuakeLive.Handler());
            demoManager.AddPlugin(0, new UnrealTournament2004.Handler());

            // Settings.
            ISettings settings = ObjectCreator.Get<ISettings>();
            settings.Add("DefaultShellCommand", "open");
            settings.Load(demoManager);

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            new ErrorReporter().LogUnhandledException((Exception)e.ExceptionObject);
        }
    }
}
