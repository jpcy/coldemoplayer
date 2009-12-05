using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Cli
{
    internal class Program
    {
        /// <summary>
        /// Determines whether the program execution is paused when information is presented to the user - i.e. the command window stays open and the user is prompted with "Press any key to continue...".
        /// </summary>
        private static bool canPause = false;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length == 0)
            {
                Console.WriteLine(Strings.Usage);
                Environment.Exit(1);
            }

            // The file name is always the last argument.
            int fileNameArgIndex = args.Length - 1;
            string fileName = args[fileNameArgIndex];

            // Parse arguments that aren't the demo filename.
            for (int i = 0; i < fileNameArgIndex; i++)
            {
                if (args[i] == "-pause")
                {
                    canPause = true;
                }
                else
                {
                    Console.WriteLine(Strings.InvalidArgument);
                    Pause();
                }
            }

            if (!File.Exists(fileName))
            {
                Console.WriteLine(Strings.DemoFileDoesNotExist, fileName);
                Pause();
                Environment.Exit(1);
            }

            // Object mappings.
            Core.ObjectMappings.Initialise();

            // Demo manager and plugins.
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.RegisterPlugin(new HalfLife.Plugin());
            demoManager.RegisterPlugin(new CounterStrike.Plugin());
            demoManager.RegisterPlugin(new Quake3Arena.Plugin());
            demoManager.RegisterPlugin(new QuakeLive.Plugin());
            demoManager.RegisterPlugin(new UnrealTournament2004.Plugin());

            // Settings.
            ISettings settings = ObjectCreator.Get<ISettings>();
            settings.Load(demoManager);

            // Create and load demo.
            Demo demo = demoManager.CreateDemo(fileName);

            if (demo == null)
            {
                Console.WriteLine(Strings.NoMatchingPluginFound, fileName);
                Pause();
                Environment.Exit(1);
            }

            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.Load();

            // Create and verify launcher
            Launcher launcher = demoManager.CreateLauncher(demo);

            if (!launcher.Verify())
            {
                Console.WriteLine(launcher.Message);
                Pause();
                Environment.Exit(1);
            }

            // Write demo.
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationWarningEvent += demo_OperationWarningEvent;
            demo.OperationCancelledEvent += demo_OperationCancelledEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;
            Console.Write(Strings.WritingDemo);
            Console.Write(" [");
            demo.Write(launcher.CalculateDestinationFileName());

            // Launch.
            // TODO: monitor game process.
            launcher.Launch();
        }

        static void Pause()
        {
            if (canPause)
            {
                Console.WriteLine();
                Console.WriteLine(Strings.PauseMessage);
                Console.ReadKey();
            }
        }

        static void demo_OperationErrorEvent(object sender, Demo.OperationErrorEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine(e.ErrorMessage);
            Console.WriteLine(e.Exception);
            Pause();
            Environment.Exit(1);
        }

        static void demo_OperationWarningEvent(object sender, Demo.OperationWarningEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(e.Message);
            Console.WriteLine();
            Console.WriteLine(e.Exception);
            Console.WriteLine();
            Console.Write(Strings.DemoWarningMessage + " ");

            Demo demo = (Demo)sender;

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.Write(Strings.ResumeWritingDemo);
                Console.Write(" [");
                demo.SetOperationWarningResult(Demo.OperationWarningResults.Continue);
            }
            else
            {
                demo.SetOperationWarningResult(Demo.OperationWarningResults.Cancel);
            }
        }

        static void demo_ProgressChangedEvent(object sender, Demo.ProgressChangedEventArgs e)
        {
            if (e.Progress % 10 == 0)
            {
                Console.Write(".");
            }
        }

        static void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            Environment.Exit(1);
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
