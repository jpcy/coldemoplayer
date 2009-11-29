using System;
using CDP.Core;
using System.Globalization;
using System.Threading;

namespace CDP.Gui
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Object mappings.
            Core.ObjectMappings.Initialise();
            ObjectCreator.Map<IMediator, Mediator>();
            ObjectCreator.Map<INavigationService, NavigationService>();
            ObjectCreator.Map<IFileAssociation, FileAssociation>();

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
