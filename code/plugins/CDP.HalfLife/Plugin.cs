using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace CDP.HalfLife
{
    public class Plugin : Core.Plugin
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

        public override uint Priority
        {
            get { return 0; }
        }

        public override string[] FileExtensions
        {
            get { return new string[] { "dem" }; }
        }

        public override PlayerColumn[] PlayerColumns
        {
            get 
            {
                return new PlayerColumn[]
                {
                    new PlayerColumn(Strings.PlayerNameColumnHeader, "Name"),
                    new PlayerColumn(Strings.PlayerClientUpdateRateColumnHeader, "UpdateRate"),
                    new PlayerColumn(Strings.PlayerRateColumnHeader, "Rate"),
                    new PlayerColumn(Strings.PlayerSteamIdColumnHeader, "SteamId")
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

        private UserControl settingsView;
        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        protected readonly Core.IProcessFinder processFinder = Core.ObjectCreator.Get<Core.IProcessFinder>();
        private readonly byte[] magic = { 0x48, 0x4C, 0x44, 0x45, 0x4D, 0x4F }; // HLDEMO
        private readonly Dictionary<byte, Type> frames = new Dictionary<byte, Type>();
        private readonly Dictionary<byte, Type> engineMessages = new Dictionary<byte, Type>();
        private readonly Dictionary<string, Type> userMessages = new Dictionary<string, Type>();
        protected Game[] games;

        public Plugin()
        {
            RegisterMessages();

            // Read config/goldsrc/games.xml
            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "goldsrc", "games.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Game[]));
                games = (Game[])serializer.Deserialize(stream);
            }
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (stream.Length < magic.Length)
            {
                return false;
            }

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

        public override UserControl CreateSettingsView(Core.Demo demo)
        {
            if (settingsView == null)
            {
                settingsView = new SettingsView { DataContext = new SettingsViewModel() };
            }

            return settingsView;
        }

        protected virtual void RegisterMessages()
        {
            // Register frames.
            RegisterFrame<Frames.Loading>();
            RegisterFrame<Frames.Playback>();
            RegisterFrame<Frames.PlaybackSegmentStart>();
            RegisterFrame<Frames.ClientCommand>();
            RegisterFrame<Frames.ClientData>();
            RegisterFrame<Frames.EndOfSegment>();
            RegisterFrame<Frames.Unknown>();
            RegisterFrame<Frames.WeaponChange>();
            RegisterFrame<Frames.PlaySound>();
            RegisterFrame<Frames.ModData>();

            // Register engine messages.
            RegisterEngineMessage<Messages.SvcNop>();
            RegisterEngineMessage<Messages.SvcEvent>();
            RegisterEngineMessage<Messages.SvcSetView>();
            RegisterEngineMessage<Messages.SvcSound>();
            RegisterEngineMessage<Messages.SvcTime>();
            RegisterEngineMessage<Messages.SvcPrint>();
            RegisterEngineMessage<Messages.SvcStuffText>();
            RegisterEngineMessage<Messages.SvcSetAngle>();
            RegisterEngineMessage<Messages.SvcServerInfo>();
            RegisterEngineMessage<Messages.SvcLightStyle>();
            RegisterEngineMessage<Messages.SvcUpdateUserInfo>();
            RegisterEngineMessage<Messages.SvcDeltaDescription>();
            RegisterEngineMessage<Messages.SvcClientData>();
            RegisterEngineMessage<Messages.SvcPings>();
            RegisterEngineMessage<Messages.SvcEventReliable>();
            RegisterEngineMessage<Messages.SvcSpawnBaseline>();
            RegisterEngineMessage<Messages.SvcTempEntity>();
            RegisterEngineMessage<Messages.SvcSetPause>();
            RegisterEngineMessage<Messages.SvcSignOnNum>();
            RegisterEngineMessage<Messages.SvcCenterPrint>();
            RegisterEngineMessage<Messages.SvcSpawnStaticSound>();
            RegisterEngineMessage<Messages.SvcIntermission>();
            RegisterEngineMessage<Messages.SvcFinale>();
            RegisterEngineMessage<Messages.SvcCdTrack>();
            RegisterEngineMessage<Messages.SvcWeaponAnim>();
            RegisterEngineMessage<Messages.SvcRoomType>();
            RegisterEngineMessage<Messages.SvcAddAngle>();
            RegisterEngineMessage<Messages.SvcNewUserMessage>();
            RegisterEngineMessage<Messages.SvcPacketEntities>();
            RegisterEngineMessage<Messages.SvcDeltaPacketEntities>();
            RegisterEngineMessage<Messages.SvcChoke>();
            RegisterEngineMessage<Messages.SvcResourceList>();
            RegisterEngineMessage<Messages.SvcNewMoveVars>();
            RegisterEngineMessage<Messages.SvcResourceRequest>();
            RegisterEngineMessage<Messages.SvcCustomization>();
            RegisterEngineMessage<Messages.SvcFileTransferFailed>();
            RegisterEngineMessage<Messages.SvcHltv>();
            RegisterEngineMessage<Messages.SvcDirector>();
            RegisterEngineMessage<Messages.SvcVoiceInit>();
            RegisterEngineMessage<Messages.SvcVoiceData>();
            RegisterEngineMessage<Messages.SvcSendExtraInfo>();
            RegisterEngineMessage<Messages.SvcTimeScale>();
            RegisterEngineMessage<Messages.SvcResourceLocation>();

            // Register user messages.
            RegisterUserMessage<UserMessages.DeathMsg>();
            RegisterUserMessage<UserMessages.ResetHud>();
            RegisterUserMessage<UserMessages.SayText>();
            RegisterUserMessage<UserMessages.ScoreInfo>();
            RegisterUserMessage<UserMessages.ScreenFade>();
            RegisterUserMessage<UserMessages.TeamInfo>();
            RegisterUserMessage<UserMessages.TeamScore>();
            RegisterUserMessage<UserMessages.TextMsg>();
        }

        public Game FindGame(string gameFolder)
        {
            return games.FirstOrDefault(g => g.DemoGameFolders.Contains(gameFolder));
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
                return new UserMessages.UnregisteredUserMessage(name);
            }

            return (UserMessage)Activator.CreateInstance(userMessages[name]);
        }
        
        private void RegisterFrame<T>() where T : Frame
        {
            Frame instance = (Frame)Activator.CreateInstance(typeof(T));
            frames.Add(instance.Id, typeof(T));
        }

        protected void RegisterEngineMessage<T>() where T : EngineMessage
        {
            EngineMessage instance = (EngineMessage)Activator.CreateInstance(typeof(T));

            // Remove if the user message is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (engineMessages.ContainsKey(instance.Id))
            {
                engineMessages.Remove(instance.Id);
            }

            engineMessages.Add(instance.Id, typeof(T));
        }

        protected void RegisterUserMessage<T>() where T : UserMessage
        {
            UserMessage instance = (UserMessage)Activator.CreateInstance(typeof(T));

            // Remove if the user message is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (userMessages.ContainsKey(instance.Name))
            {
                userMessages.Remove(instance.Name);
            }

            userMessages.Add(instance.Name, typeof(T));
        }
    }
}
