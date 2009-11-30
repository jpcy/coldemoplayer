using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using CDP.Core;
using CDP.Core.Extensions;

namespace HalfLifeDemoSystemTest
{
    class Program
    {
        private enum DemoOperations
        {
            Load = 0,
            Read = 1,
            Write = 2,
            LoadWritten = 3,
            ReadWritten = 4,
            LastIfSkipping = 4,
            WriteNoSkipping = 5,
            Last = 5
        }

        private static ISettings settings;
        private static IDemoManager demoManager;
        private static IFileSystem fileSystem;
        private static TextWriter log;
        private static bool demoError;
        private static DemoOperations currentDemoOperation;
        private static Stopwatch stopwatch = new Stopwatch();
        private static string selectedPath;
        private static readonly string tempDemoFileName = "systemtesttempdemo.dem";
        private static bool writeNoSkipping;
        private static int errorCount = 0;
        private static int okCount = 0;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                selectedPath = args[0].RemoveChars('\"');
            }
            else
            {
                Console.Write("Demo directory path: ");
                selectedPath = Console.ReadLine();
                Console.WriteLine();
            }

            if (!Directory.Exists(selectedPath))
            {
                WriteLine("Directory doesn't exist.");
                return;
            }

            if (args.Length > 0)
            {
                if (args.Length > 1 && args[1] == "-noskip")
                {
                    writeNoSkipping = true;
                }
            }
            else
            {
                Console.Write("Test write without skipping (Y/N)? ");
                char writeNoSkippingChoice = Console.ReadKey().KeyChar;
                writeNoSkipping = (writeNoSkippingChoice == 'Y' || writeNoSkippingChoice == 'y');
                Console.ReadKey();
                Console.WriteLine();
            }

            Console.WriteLine();
            ObjectMappings.Initialise();
            demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.RegisterPlugin(new CDP.HalfLife.Plugin());
            demoManager.RegisterPlugin(new CDP.CounterStrike.Plugin());
            fileSystem = ObjectCreator.Get<IFileSystem>();

            settings = ObjectCreator.Get<ISettings>();
            settings.Load(demoManager);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (log = new StreamWriter(Path.Combine(selectedPath, "cdptestresult.log")))
            {
                EnumerateFolder(selectedPath);
            }

            sw.Stop();

            string tempDemoFullPath = Path.Combine(selectedPath, tempDemoFileName);

            if (File.Exists(tempDemoFullPath))
            {
                File.Delete(tempDemoFullPath);
            }

            Console.WriteLine("Tests complete.");
            Console.WriteLine("\tElapsed time: {0}", sw.Elapsed);
            Console.WriteLine("\t{0} demos are OK.", okCount);
            Console.WriteLine("\t{0} demos with errors.", errorCount);
            Console.WriteLine("\nPress and key to continue.");
            Console.ReadKey();
        }

        static void EnumerateFolder(string path)
        {
            string[] validExtensions = demoManager.GetAllPluginFileExtensions();

            foreach (string fileName in Directory.GetFiles(path).Where(f => validExtensions.Contains(fileSystem.GetExtension(f))))
            {
                // Ignore the temp demo.
                if (fileName == tempDemoFileName)
                {
                    continue;
                }

                CDP.HalfLife.Demo demo = (CDP.HalfLife.Demo)demoManager.CreateDemo(fileName);

                if (demo == null)
                {
                    continue;
                }

                WriteLine("File \'{0}\'", fileName);
                demo.OperationCompleteEvent += demo_OperationCompleteEvent;
                demo.OperationErrorEvent += demo_OperationErrorEvent;

                // Load.
                currentDemoOperation = DemoOperations.Load;
                demoError = false;
                Write("\tLoad: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Load();

                if (demoError)
                {
                    errorCount++;
                    continue;
                }

                // Read.
                currentDemoOperation = DemoOperations.Read;
                demoError = false;
                Write("\tRead: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Read();

                if (demoError)
                {
                    errorCount++;
                    continue;
                }

                // Write.
                currentDemoOperation = DemoOperations.Write;
                demoError = false;
                Write("\tWrite: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Write(Path.Combine(selectedPath, tempDemoFileName));

                if (demoError)
                {
                    errorCount++;
                    continue;
                }

                // Load written demo.
                currentDemoOperation = DemoOperations.LoadWritten;
                demoError = false;
                Write("\tLoad written: ");
                stopwatch.Reset();
                stopwatch.Start();
                CDP.HalfLife.Demo writtenDemo = (CDP.HalfLife.Demo)demoManager.CreateDemo(Path.Combine(selectedPath, tempDemoFileName));

                if (writtenDemo == null)
                {
                    throw new ApplicationException("Error creating demo object for written demo.");
                }

                writtenDemo.OperationCompleteEvent += demo_OperationCompleteEvent;
                writtenDemo.OperationErrorEvent += demo_OperationErrorEvent;
                writtenDemo.Load();

                if (demoError)
                {
                    errorCount++;
                    continue;
                }

                // Read written demo.
                currentDemoOperation = DemoOperations.ReadWritten;
                demoError = false;
                Write("\tRead written: ");
                stopwatch.Reset();
                stopwatch.Start();
                writtenDemo.Read();

                if (demoError)
                {
                    errorCount++;
                    continue;
                }

                // Write (no skipping messages).
                if (writeNoSkipping)
                {
                    currentDemoOperation = DemoOperations.WriteNoSkipping;
                    demoError = false;
                    Write("\tWrite (no skipping): ");
                    stopwatch.Reset();
                    stopwatch.Start();
                    demo.Write(Path.Combine(selectedPath, tempDemoFileName), false);
                }

                okCount++;
            }

            foreach (string folder in Directory.GetDirectories(path))
            {
                EnumerateFolder(folder);
            }
        }

        static void demo_OperationErrorEvent(object sender, Demo.OperationErrorEventArgs e)
        {
            demoError = true;
            stopwatch.Stop();
            WriteLine("ERROR\n");
            WriteLine(e.Exception.ToLogString(e.ErrorMessage));

            if (IsLastOperation())
            {
                LastOperation((Demo)sender);
            }
        }

        static void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            stopwatch.Stop();
            WriteLine("OK [{0}]", stopwatch.Elapsed.ToString());

            if (IsLastOperation())
            {
                LastOperation((Demo)sender);
            }
        }

        static bool IsLastOperation()
        {
            return (currentDemoOperation == DemoOperations.LastIfSkipping || (writeNoSkipping && currentDemoOperation == DemoOperations.Last));
        }

        static void LastOperation(Demo demo)
        {
            WriteLine("\n----------------------------------------\n");
            log.Flush();
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
        }

        static void Write(string format, params object[] args)
        {
            log.Write(format, args);
            Console.Write(format, args);
        }

        static void WriteLine(string format, params object[] args)
        {
            Write(format + "\n", args);
        }
    }
}
