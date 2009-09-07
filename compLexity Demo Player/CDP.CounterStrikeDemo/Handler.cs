using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

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

        public override Core.Setting[] Settings
        {
            get
            {
                return new Core.Setting[]
                {
                    new Core.Setting("CsRemoveFadeToBlack", typeof(bool), true)
                };
            }
        }

        private Game game;

        public Handler()
            : this(Core.Settings.Instance, new Core.FileSystem())
        {
        }

        public Handler(Core.ISettings settings, Core.IFileSystem fileSystem)
            : base(settings, fileSystem)
        {
            SettingsView = new SettingsView { DataContext = new SettingsViewModel(settings) };
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();

            // TODO: add CS-specific user message handlers.
        }

        protected override void ReadGameConfig()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Game));

            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "goldsrc", "cstrike.xml")))
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
