using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

namespace CDP.CounterStrikeDemo
{
    public class Handler : HalfLifeDemo.Handler
    {
        public override string FullName
        {
            get { return "Counter-Strike"; }
        }

        public override string Name
        {
            get { return "cstrike"; }
        }

        private readonly Core.Config config;
        private readonly Core.Adapters.IPath pathAdapter;
        private Game game;

        public Handler()
            : this(Core.Config.Instance, new Core.Adapters.Path())
        {
        }

        public Handler(Core.Config config, Core.Adapters.IPath pathAdapter)
        {
            this.config = config;
            this.pathAdapter = pathAdapter;
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();

            // TODO: add CS-specific user message handlers.
        }

        protected override void ReadGameConfig()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Game));

            using (StreamReader stream = new StreamReader(pathAdapter.Combine(config.ProgramPath, "config", "goldsrc", "cstrike.xml")))
            {
                game = (Game)serializer.Deserialize(stream);
            }
        }

        public override Core.SteamGame FindGame(string gameFolder)
        {
            return game;
        }

        public override bool IsValidDemo(Stream stream)
        {
            if (!base.IsValidDemo(stream))
            {
                return false;
            }

            // magic (8 bytes) + demo protocol (4 bytes) + network protocol (4 bytes) + map name (260 bytes) = 276
            stream.Seek(276, SeekOrigin.Begin);

            using (BinaryReader br = new BinaryReader(stream))
            {
                Core.BitReader buffer = new Core.BitReader(br.ReadBytes(260));
                string gameFolder = buffer.ReadString();
                return game.DemoGameFolders.Contains(gameFolder, StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}
