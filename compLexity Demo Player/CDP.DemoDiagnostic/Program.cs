using System;
using CDP.Core;

namespace CDP.DemoDiagnostic
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            // Object mappings.
            Core.ObjectMappings.Initialise();

            // Demo manager and plugins.
            IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
            demoManager.AddPlugin(0, new HalfLifeDemo.Handler());
            demoManager.AddPlugin(1, new CounterStrikeDemo.Handler());

            // Settings.
            ISettings settings = ObjectCreator.Get<ISettings>();

            foreach (Setting setting in demoManager.GetAllDemoHandlerSettings())
            {
                settings.Add(setting);
            }

            settings.Load();

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
