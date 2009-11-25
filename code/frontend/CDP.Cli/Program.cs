using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length < 1)
            {
                Console.WriteLine(Strings.Usage);
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine(Strings.DemoFileDoesNotExist, args[0]);
                Environment.Exit(1);
            }

            // Object mappings.
            Core.ObjectMappings.Initialise();

            // Demo manager and plugins.
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.AddPlugin(0, new HalfLife.Handler());
            demoManager.AddPlugin(1, new CounterStrike.Handler());
            demoManager.AddPlugin(0, new Quake3Arena.Handler());
            demoManager.AddPlugin(0, new QuakeLive.Handler());
            demoManager.AddPlugin(0, new UnrealTournament2004.Handler());

            // Settings.
            ISettings settings = ObjectCreator.Get<ISettings>();

            foreach (Setting setting in demoManager.GetAllDemoHandlerSettings())
            {
                settings.Add(setting);
            }

            settings.Load();

            // Create and load demo.
            Demo demo = demoManager.CreateDemo(args[0]);

            if (demo == null)
            {
                Console.WriteLine(Strings.NoMatchingPluginFound, args[0]);
                Environment.Exit(1);
            }

            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.Load();

            // Create and verify launcher
            Launcher launcher = demoManager.CreateLauncher(demo);

            if (!launcher.Verify())
            {
                Console.WriteLine(launcher.Message);
                Environment.Exit(1);
            }

            // Write demo.
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;
            Console.Write(Strings.WritingDemo);
            Console.Write(" [");
            demo.Write(launcher.CalculateDestinationFileName());

            // Launch.
            // TODO: monitor game process.
            launcher.Launch();
        }

        static void demo_OperationErrorEvent(object sender, Demo.OperationErrorEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine(e.ErrorMessage);
            Console.WriteLine(e.Exception);
            Environment.Exit(1);
        }

        static void demo_ProgressChangedEvent(object sender, Demo.ProgressChangedEventArgs e)
        {
            if (e.Progress % 10 == 0)
            {
                Console.Write(".");
            }
        }

        static void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine("]");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            new ErrorReporter().LogUnhandledException((Exception)e.ExceptionObject);
        }
    }
}
