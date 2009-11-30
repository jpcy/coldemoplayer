using System;
using CDP.Core;

namespace CDP.DemoDiagnostic
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Object mappings.
            Core.ObjectMappings.Initialise();

            // Demo manager and plugins.
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.RegisterPlugin(new HalfLife.Plugin());
            demoManager.RegisterPlugin(new CounterStrike.Plugin());
            demoManager.RegisterPlugin(new Source.Plugin());
            demoManager.RegisterPlugin(new Quake3Arena.Plugin());
            demoManager.RegisterPlugin(new QuakeLive.Plugin());

            // Settings.
            ISettings settings = ObjectCreator.Get<ISettings>();
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
