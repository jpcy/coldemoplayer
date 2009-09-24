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
            WriteNoSkipping = 3,
            Last = 3
        }

        private static ISettings settings;
        private static IDemoManager demoManager;
        private static IFileSystem fileSystem;
        private static TextWriter log;
        private static bool demoError;
        private static DemoOperations currentDemoOperation;
        private static Stopwatch stopwatch = new Stopwatch();
        private static string selectedPath;
        private static readonly string tempDemoFileName = "demo.tmp";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                selectedPath = args[1];
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

            ObjectMappings.Initialise();
            settings = ObjectCreator.Get<ISettings>();
            demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.AddPlugin(0, new CDP.HalfLifeDemo.Handler());
            demoManager.AddPlugin(1, new CDP.CounterStrikeDemo.Handler());
            fileSystem = ObjectCreator.Get<IFileSystem>();

            using (log = new StreamWriter(Path.Combine(selectedPath, "cdptestresult.log")))
            {
                EnumerateFolder(selectedPath);
            }

            string tempDemoFullPath = Path.Combine(selectedPath, tempDemoFileName);

            if (File.Exists(tempDemoFullPath))
            {
                File.Delete(tempDemoFullPath);
            }

            Console.WriteLine("Tests complete. Press and key to continue.");
            Console.ReadKey();
        }

        static void EnumerateFolder(string path)
        {
            string[] validExtensions = demoManager.ValidDemoExtensions();

            foreach (string fileName in Directory.GetFiles(path).Where(f => validExtensions.Contains(fileSystem.GetExtension(f))))
            {
                CDP.HalfLifeDemo.Demo demo = (CDP.HalfLifeDemo.Demo)demoManager.CreateDemo(fileName);

                if (demo == null)
                {
                    continue;
                }

                WriteLine("File \'{0}\'", fileName);
                demo.OperationCompleteEvent += demo_OperationCompleteEvent;
                demo.OperationErrorEvent += demo_OperationErrorEvent;
                demoError = false;
                
                // Load.
                currentDemoOperation = DemoOperations.Load;
                Write("\tLoad: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Load();

                if (demoError)
                {
                    continue;
                }

                // Read.
                currentDemoOperation = DemoOperations.Read;
                Write("\tRead: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Read();

                if (demoError)
                {
                    continue;
                }

                // Write.
                currentDemoOperation = DemoOperations.Write;
                Write("\tWrite: ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Write(Path.Combine(selectedPath, tempDemoFileName));

                if (demoError)
                {
                    continue;
                }

                // Write (no skipping messages).
                currentDemoOperation = DemoOperations.WriteNoSkipping;
                Write("\tWrite (no skipping): ");
                stopwatch.Reset();
                stopwatch.Start();
                demo.Write(Path.Combine(selectedPath, tempDemoFileName), false);
            }

            foreach (string folder in Directory.GetDirectories(path))
            {
                EnumerateFolder(folder);
            }
        }

        static void demo_OperationErrorEvent(object sender, Demo.OperationErrorEventArgs e)
        {
            stopwatch.Stop();
            WriteLine("ERROR\n");
            WriteLine(e.Exception.ToLogString(e.ErrorMessage));

            if (currentDemoOperation == DemoOperations.Last)
            {
                LastOperation((Demo)sender);
            }
        }

        static void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            stopwatch.Stop();
            WriteLine("OK [{0}]", stopwatch.Elapsed.ToString());

            if (currentDemoOperation == DemoOperations.Last)
            {
                LastOperation((Demo)sender);
            }
        }

        static void LastOperation(Demo demo)
        {
            WriteLine("----------------------------------------");
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
