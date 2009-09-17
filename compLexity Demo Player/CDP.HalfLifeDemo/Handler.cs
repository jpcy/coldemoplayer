using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace CDP.HalfLifeDemo
{
    public class Handler : Core.DemoHandler
    {
        public enum PlaybackMethods
        {
            Playdemo,
            Viewdemo
        }

        public override string FullName
        {
            get { return "Half-Life"; }
        }

        public override string Name
        {
            get { return "halflife"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dem" }; }
        }

        public override PlayerColumn[] PlayerColumns
        {
            get 
            {
                return new PlayerColumn[]
                {
                    new PlayerColumn("Name", "Name"),
                    new PlayerColumn("cl__updaterate", "UpdateRate"),
                    new PlayerColumn("rate", "Rate"),
                    new PlayerColumn("Steam ID", "SteamId")
                };
            }
        }

        public override Core.Setting[] Settings
        {
            get
            {
                return new Core.Setting[]
                {
                    new Core.Setting("HlPlaybackMethod", typeof(PlaybackMethods), PlaybackMethods.Playdemo),
                    new Core.Setting("HlStartListenServer", typeof(bool), true),
                    new Core.Setting("HlRemoveShowscores", typeof(bool), true)
                };
            }
        }

        public override UserControl SettingsView { get; protected set; }

        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        protected readonly Core.IProcessFinder processFinder = Core.ObjectCreator.Get<Core.IProcessFinder>();
        private readonly byte[] magic = { 0x48, 0x4C, 0x44, 0x45, 0x4D, 0x4F }; // HLDEMO
        private readonly Dictionary<byte, Type> frames = new Dictionary<byte, Type>();
        private readonly Dictionary<byte, Type> engineMessages = new Dictionary<byte, Type>();
        private readonly Dictionary<string, Type> userMessages = new Dictionary<string, Type>();
        private Core.SteamGame[] games;

        public Handler()
        {
            SettingsView = new SettingsView { DataContext = new SettingsViewModel() };
            RegisterMessages();
            ReadGameConfig();
        }

        public override bool IsValidDemo(Stream stream)
        {
            for (int i = 0; i < magic.Length; i++)
            {
                if (stream.ReadByte() != magic[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            return new Launcher();
        }

        public override UserControl CreateAnalysisView()
        {
            return new Analysis.View();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new Analysis.ViewModel((Demo)demo);
        }

        protected virtual void RegisterMessages()
        {
            // Register frames.
            RegisterFrame(typeof(Frames.Loading));
            RegisterFrame(typeof(Frames.Playback));
            RegisterFrame(typeof(Frames.PlaybackSegmentStart));
            RegisterFrame(typeof(Frames.ClientCommand));
            RegisterFrame(typeof(Frames.ClientData));
            RegisterFrame(typeof(Frames.EndOfSegment));
            RegisterFrame(typeof(Frames.Unknown));
            RegisterFrame(typeof(Frames.WeaponChange));
            RegisterFrame(typeof(Frames.PlaySound));
            RegisterFrame(typeof(Frames.ModData));

            // Register engine messages.
            RegisterEngineMessage(typeof(Messages.SvcNop));
            RegisterEngineMessage(typeof(Messages.SvcEvent));
            RegisterEngineMessage(typeof(Messages.SvcSetView));
            RegisterEngineMessage(typeof(Messages.SvcSound));
            RegisterEngineMessage(typeof(Messages.SvcTime));
            RegisterEngineMessage(typeof(Messages.SvcPrint));
            RegisterEngineMessage(typeof(Messages.SvcStuffText));
            RegisterEngineMessage(typeof(Messages.SvcSetAngle));
            RegisterEngineMessage(typeof(Messages.SvcServerInfo));
            RegisterEngineMessage(typeof(Messages.SvcLightStyle));
            RegisterEngineMessage(typeof(Messages.SvcUpdateUserInfo));
            RegisterEngineMessage(typeof(Messages.SvcDeltaDescription));
            RegisterEngineMessage(typeof(Messages.SvcClientData));
            RegisterEngineMessage(typeof(Messages.SvcPings));
            RegisterEngineMessage(typeof(Messages.SvcEventReliable));
            RegisterEngineMessage(typeof(Messages.SvcSpawnBaseline));
            RegisterEngineMessage(typeof(Messages.SvcTempEntity));
            RegisterEngineMessage(typeof(Messages.SvcSignOnNum));
            RegisterEngineMessage(typeof(Messages.SvcSpawnStaticSound));
            RegisterEngineMessage(typeof(Messages.SvcCdTrack));
            RegisterEngineMessage(typeof(Messages.SvcNewUserMessage));
            RegisterEngineMessage(typeof(Messages.SvcPacketEntities));
            RegisterEngineMessage(typeof(Messages.SvcDeltaPacketEntities));
            RegisterEngineMessage(typeof(Messages.SvcChoke));
            RegisterEngineMessage(typeof(Messages.SvcResourceList));
            RegisterEngineMessage(typeof(Messages.SvcNewMoveVars));
            RegisterEngineMessage(typeof(Messages.SvcResourceRequest));
            RegisterEngineMessage(typeof(Messages.SvcHltv));
            RegisterEngineMessage(typeof(Messages.SvcDirector));
            RegisterEngineMessage(typeof(Messages.SvcVoiceInit));
            RegisterEngineMessage(typeof(Messages.SvcSendExtraInfo));
            RegisterEngineMessage(typeof(Messages.SvcTimeScale));

            // Register user messages.
            RegisterUserMessage(typeof(UserMessages.ResetHud));
            RegisterUserMessage(typeof(UserMessages.SayText));
            RegisterUserMessage(typeof(UserMessages.ScoreInfo));
            RegisterUserMessage(typeof(UserMessages.TeamInfo));
            RegisterUserMessage(typeof(UserMessages.TeamScore));
            RegisterUserMessage(typeof(UserMessages.TextMsg));
        }

        protected virtual void ReadGameConfig()
        {
            // Read config/goldsrc/games.xml
            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "goldsrc", "games.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Core.SteamGame[]));
                games = (Core.SteamGame[])serializer.Deserialize(stream);
            }
        }

        public virtual Core.SteamGame FindGame(string gameFolder)
        {
            return games.FirstOrDefault(sg => sg.DemoGameFolders.Contains(gameFolder));
        }

        public Frame CreateFrame(byte id)
        {
            if (!frames.ContainsKey(id))
            {
                return null;
            }

            return (Frame)Activator.CreateInstance(frames[id]);
        }

        public EngineMessage CreateEngineMessage(byte id)
        {
            if (id > Demo.MaxEngineMessageId)
            {
                throw new ApplicationException("Tried to create an engine message with an ID higher than MaxEngineMessageId.");
            }

            if (!engineMessages.ContainsKey(id))
            {
                return null;
            }

            return (EngineMessage)Activator.CreateInstance(engineMessages[id]);
        }

        public UserMessage CreateUserMessage(string name)
        {
            if (!userMessages.ContainsKey(name))
            {
                return new Messages.UnregisteredUserMessage(name);
            }

            return (UserMessage)Activator.CreateInstance(userMessages[name]);
        }

        private void RegisterFrame(Type type)
        {
            Frame instance = Activator.CreateInstance(type) as Frame;

            if (instance == null)
            {
                throw new ApplicationException("Specified type does not inherit from Frame.");
            }

            frames.Add(instance.Id, type);
        }

        private void RegisterEngineMessage(Type type)
        {
            EngineMessage instance = Activator.CreateInstance(type) as EngineMessage;

            if (instance == null)
            {
                throw new ApplicationException("Specified type does not inherit from EngineMessage.");
            }

            engineMessages.Add(instance.Id, type);
        }

        protected void RegisterUserMessage(Type type)
        {
            UserMessage instance = Activator.CreateInstance(type) as UserMessage;

            if (instance == null)
            {
                throw new ApplicationException("Specified type does not inherit from UserMessage.");
            }

            // Remove if the user message is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (userMessages.ContainsKey(instance.Name))
            {
                userMessages.Remove(instance.Name);
            }

            userMessages.Add(instance.Name, type);
        }
    }
}
