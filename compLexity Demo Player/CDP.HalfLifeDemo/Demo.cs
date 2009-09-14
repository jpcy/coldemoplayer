using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;

namespace CDP.HalfLifeDemo
{
    public class Demo : Core.Demo
    {
        private class MessageCallback
        {
            public byte EngineMessageId { get; set; }
            public string UserMessageName { get; set; }
            public object Delegate { get; set; }
        }

        private class UserMessageDefinition
        {
            public byte Id { get; set; }
            public sbyte Length { get; set; }
            public string Name { get; set; }
        }

        private class Player
        {
            public byte Slot { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public float UpdateRate { get; set; }
            public float Rate { get; set; }
            public string SteamId { get; set; }
        }

        public override Core.DemoHandler Handler
        {
            get { return handler; }
            set { handler = (Handler)value; }
        }

        public static uint NewestNetworkProtocol
        {
            get { return 48; }
        }

        public static byte MaxEngineMessageId
        {
            get { return 58; }
        }

        public override string GameName
        {
            get
            {
                if (Game == null)
                {
                    return string.Format("Unknown ({0})", GameFolderName);
                }

                string version = Game.GetVersionName(clientDllChecksum);
                return Game.Name + version ?? string.Empty;
            }
        }

        public Core.SteamGame Game { get; private set; }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override IList<Detail> Details { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get
            {
                if (Game == null)
                {
                    return null;
                }

                List<string> fileNames = new List<string>();

                // Locally stored icon: "icons\engine\game folder.ico"
                fileNames.Add(fileSystem.PathCombine(settings.ProgramPath, "icons", "goldsrc", Game.ModFolder + ".ico"));

                // Check that Steam.exe path is set.
                if (File.Exists((string)settings["SteamExeFullPath"]))
                {
                    string steamFolder = Path.GetDirectoryName((string)settings["SteamExeFullPath"]);

                    // Steam\steam\games\x.ico, where x is the game name. e.g. counter-strike.
                    fileNames.Add(fileSystem.PathCombine(steamFolder, "steam", "games", Game.AppFolder + ".ico"));

                    // e.g. counter-strike\cstrike\game.ico
                    fileNames.Add(fileSystem.PathCombine(steamFolder, "SteamApps", (string)settings["SteamAccountName"], Game.AppFolder, Game.ModFolder, "game.ico"));

                    // e.g. counter-strike\cstrike\resource\game.ico.
                    fileNames.Add(fileSystem.PathCombine(steamFolder, "SteamApps", (string)settings["SteamAccountName"], Game.AppFolder, Game.ModFolder, "resource", "game.ico"));
                }

                return fileNames.ToArray();
            }
        }

        public override string MapImagesRelativePath
        {
            get
            {
                return fileSystem.PathCombine("goldsrc", GameFolderName, MapName + ".jpg");
            }
        }

        public override bool CanPlay
        {
            get { return (Game != null); }
        }

        public override bool CanAnalyse
        {
            get { return false; }
        }

        public uint NetworkProtocol { get; private set; }
        public string GameFolderName { get; private set; }
        public uint MapChecksum { get; private set; }
        public byte MaxClients { get; private set; }
        public bool IsHltv { get; private set; }
        public bool IsCorrupt { get; private set; }

        private Handler handler;
        private uint directoryEntriesOffset;
        private string clientDllChecksum;
        private readonly List<MessageCallback> messageCallbacks;
        private readonly Dictionary<string, DeltaStructure> deltaStructures;
        private readonly List<UserMessageDefinition> userMessageDefinitions;

        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();

        public Demo()
        {
            Perspective = "POV";
            IsHltv = false;
            IsCorrupt = false;
            Details = new List<Detail>();
            Players = new ArrayList();
            messageCallbacks = new List<MessageCallback>();
            deltaStructures = new Dictionary<string, DeltaStructure>();
            userMessageDefinitions = new List<UserMessageDefinition>();

            // Add delta_description_t delta structure. Yes, it's a delta structure even though delta compression is never used and the parameters are always the same. Blame Valve.
            DeltaStructure deltaDescription = new DeltaStructure("delta_description_t");
            deltaDescription.AddEntry("flags", 32, 1.0f, DeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("name", 8, 1.0f, DeltaStructure.EntryFlags.String);
            deltaDescription.AddEntry("offset", 16, 1.0f, DeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("size", 8, 1.0f, DeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("nBits", 8, 1.0f, DeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("divisor", 32, 4000.0f, DeltaStructure.EntryFlags.Float);
            deltaDescription.AddEntry("preMultiplier", 32, 4000.0f, DeltaStructure.EntryFlags.Float);
            AddDeltaStructure(deltaDescription);
        }

        public override void Load()
        {
            try
            {
                AddMessageCallback<Messages.SvcServerInfo>(Load_ServerInfo);
                AddMessageCallback<Messages.SvcUpdateUserInfo>(Load_UpdateUserInfo);
                AddMessageCallback<Messages.SvcDeltaDescription>(Load_DeltaDescription);
                AddMessageCallback<Messages.SvcNewUserMessage>(Load_NewUserMessage);
                AddMessageCallback<Messages.SvcHltv>(Load_Hltv);
                ResetProgress();

                using (FileStream stream = File.OpenRead(FileName))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    // Read header.
                    // TODO: sanity check on demo and network protocol; friendly error message.
                    Header header = new Header();
                    header.Read(br.ReadBytes(Header.SizeInBytes));
                    NetworkProtocol = header.NetworkProtocol;
                    MapName = header.MapName;
                    GameFolderName = header.GameFolderName;
                    MapChecksum = header.MapChecksum;
                    directoryEntriesOffset = header.DirectoryEntriesOffset;
                    AddDetail("Demo Protocol", header.DemoProtocol);
                    AddDetail("Network Protocol", NetworkProtocol);
                    AddDetail("Game Folder", GameFolderName);
                    AddDetail("Map Checksum", MapChecksum);
                    AddDetail("Map Name", MapName);
                    Game = handler.FindGame(GameFolderName);

                    // Read directory entries.
                    try
                    {
                        ReadDirectoryEntries(header.DirectoryEntriesOffset, br);
                    }
                    catch (Exception)
                    {
                        IsCorrupt = true;
                    }

                    AddDetail("Status", IsCorrupt ? "Corrupt directory entries" : "OK");

                    // Read frames.
                    stream.Seek(Header.SizeInBytes, SeekOrigin.Begin);

                    while (true)
                    {
                        Frame frame = ReadFrame(br);

                        if (frame.Id == (byte)FrameIds.PlaybackSegmentStart)
                        {
                            break;
                        }
                        else if (!frame.HasMessages)
                        {
                            continue;
                        }

                        Core.BitReader messageReader = new Core.BitReader(((MessageFrame)frame).MessageData);

                        while (messageReader.CurrentByte < messageReader.Length)
                        {
                            ReadMessage(messageReader);
                        }

                        UpdateProgress(stream.Position, stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(null, ex);
                return;
            }
            finally
            {
                messageCallbacks.Clear();
            }

            OnOperationComplete();
        }

        public override void Read()
        {
            try
            {
                ResetOperationCancelledState();
                ResetProgress();

                using (FileStream stream = File.OpenRead(FileName))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    stream.Seek(Header.SizeInBytes, SeekOrigin.Begin);

                    while (true)
                    {
                        Frame frame = ReadFrame(br);

                        if (!frame.HasMessages)
                        {
                            continue;
                        }

                        Core.BitReader messageReader = new Core.BitReader(((MessageFrame)frame).MessageData);

                        while (messageReader.CurrentByte < messageReader.Length)
                        {
                            ReadMessage(messageReader);
                        }

                        if (stream.Position >= directoryEntriesOffset || stream.Position == stream.Length)
                        {
                            break;
                        }

                        if (IsOperationCancelled())
                        {
                            return;
                        }

                        UpdateProgress(stream.Position, stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsOperationCancelled())
                {
                    return;
                }

                OnOperationError(null, ex);
                return;
            }
            finally
            {
                messageCallbacks.Clear();
            }

            if (!IsOperationCancelled())
            {
                OnOperationComplete();
            }
        }

        public override void Write(string destinationFileName)
        {
            throw new NotImplementedException();
        }

        private void ReadDirectoryEntries(long offset, BinaryReader br)
        {
            // offset + nEntries + 2 directory entries.
            // Don't check if there's anything following the directory entries, it could be used for meta-data in the future.
            if (br.BaseStream.Length < offset + 4 + DirectoryEntry.SizeInBytes * 2)
            {
                IsCorrupt = true;
                return;
            }

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int nEntries = br.ReadInt32();

            if (nEntries != 2)
            {
                IsCorrupt = true;
                return;
            }

            // Skip "LOADING".
            br.BaseStream.Seek(DirectoryEntry.SizeInBytes, SeekOrigin.Current);

            DirectoryEntry playback = new DirectoryEntry();
            playback.Read(br.ReadBytes(DirectoryEntry.SizeInBytes));
            Duration = TimeSpan.FromSeconds(Math.Abs(playback.Duration));
            AddDetail("Duration", Duration);
        }

        private Frame ReadFrame(BinaryReader br)
        {
            byte id = br.ReadByte();
            Frame frame = handler.CreateFrame(id);

            if (frame == null)
            {
                throw new ApplicationException(string.Format("Unknown frame type \"{0}\".", id));
            }

            frame.Read(br, NetworkProtocol);
            return frame;
        }

        private IMessage ReadMessage(Core.BitReader reader)
        {
            byte id = reader.ReadByte();
            IMessage message = null;

            if (id <= Demo.MaxEngineMessageId)
            {
                message = handler.CreateEngineMessage(id);
            }
            else
            {
                if (userMessageDefinitions.Count == 0)
                {
                    throw new ApplicationException("Tried to read a user message before any svc_newusermsg messages were parsed.");
                }

                UserMessageDefinition userMessageDefinition = userMessageDefinitions.SingleOrDefault(umd => umd.Id == id);

                if (userMessageDefinition != null)
                {
                    UserMessage userMessage = handler.CreateUserMessage(userMessageDefinition.Name);
                    userMessage.Id = userMessageDefinition.Id;
                    userMessage.Length = userMessageDefinition.Length;

                    if (userMessage.Length == -1)
                    {
                        // -1 is a marker for "dynamic length message", which just means the next signed byte contains the message length.
                        userMessage.Length = reader.ReadSByte();
                    }

                    message = userMessage;
                }
            }

            if (message == null)
            {
                throw new ApplicationException(string.Format("Unknown message type \"{0}\".", id));
            }

            message.Demo = this;
            message.Read(reader);
            FireMessageCallbacks(message);
            return message;
        }

        private void FireMessageCallbacks(IMessage message)
        {
            List<MessageCallback> callbacks = null;

            if (message.Id <= Demo.MaxEngineMessageId)
            {
                callbacks = messageCallbacks.FindAll(mc => mc.EngineMessageId == message.Id);
            }
            else
            {
                callbacks = messageCallbacks.FindAll(mc => mc.UserMessageName == message.Name);
            }

            foreach (MessageCallback callback in callbacks)
            {
                MethodInfo methodInfo = callback.Delegate.GetType().GetMethod("Invoke");
                methodInfo.Invoke(callback.Delegate, new object[] { message });
            }
        }

        public void AddMessageCallback<T>(Action<T> method) where T : IMessage
        {
            MessageCallback callback = new MessageCallback
            {
                Delegate = method
            };

            // Instantiate the message type to get the message ID (for engine messages) or name (for user messages).
            IMessage message = (IMessage)Activator.CreateInstance(typeof(T));

            if (message.Id > 0 && message.Id <= Demo.MaxEngineMessageId)
            {
                callback.EngineMessageId = message.Id;
            }
            else
            {
                callback.UserMessageName = message.Name;
            }

            messageCallbacks.Add(callback);
        }

        private void AddDeltaStructure(DeltaStructure structure)
        {
            // Remove the decoder if it already exists (duplicate svc_deltadescription message).
            // Example: http://www.gotfrag.com/cs/demos/6/
            if (deltaStructures.ContainsKey(structure.Name))
            {
                deltaStructures.Remove(structure.Name);
            }

            deltaStructures.Add(structure.Name, structure);
        }

        public DeltaStructure FindDeltaStructure(string name)
        {
            if (!deltaStructures.ContainsKey(name))
            {
                throw new ApplicationException("Delta structure \"" + name + "\" not found.");
            }

            return deltaStructures[name];
        }

        public void AddUserMessage(byte id, sbyte length, string name)
        {
            // Some demos contain duplicate user message definitions.
            // Example: http://www.gotfrag.com/cs/demos/13
            // Some poorly written server mods have many copies of the same user message registered with different id's, so always remove a message definition if one with a matching name already exists.
            UserMessageDefinition userMessageDefinition = userMessageDefinitions.SingleOrDefault(umd => umd.Name == name);

            if (userMessageDefinition != null)
            {
                userMessageDefinitions.Remove(userMessageDefinition);
            }

            userMessageDefinitions.Add(new UserMessageDefinition
            {
                Id = id,
                Length = length,
                Name = name
            });
        }

        private void AddDetail(string name, object value)
        {
            Detail detail = Details.FirstOrDefault(d => d.Name == name);

            if (detail == null)
            {
                detail = new Detail
                {
                    Name = name
                };

                Details.Add(detail);
            }

            detail.Value = value;
        }

        #region Load message callbacks
        private void Load_ServerInfo(Messages.SvcServerInfo message)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < message.ClientDllChecksum.Length; i++)
            {
                sb.Append(message.ClientDllChecksum[i].ToString("X2"));
            }

            clientDllChecksum = sb.ToString();
            MaxClients = message.MaxClients;
            AddDetail("Server Slots", MaxClients);
            AddDetail("Server Name", message.ServerName);
        }

        private void Load_UpdateUserInfo(Messages.SvcUpdateUserInfo message)
        {
            Player player = null;

            // Find a player with a matching ID.
            foreach (Player p in Players)
            {
                if (p.Id == message.EntityId)
                {
                    player = p;
                    break;
                }
            }

            if (message.Info.Length == 0)
            {
                // 0 length text means a player just left and another player's slot is being changed.
                if (player != null)
                {
                    player.Slot = message.Slot;
                }

                return;
            }

            // Create a player if one doesn't exist.
            if (player == null)
            {
                player = new Player { Slot = message.Slot, Id = message.EntityId };
                Players.Add(player);
            }

            // Parse info string.
            string s = message.Info.Remove(0, 1); // trim leading slash
            string[] infoKeyTokens = s.Split('\\');

            if (infoKeyTokens.Length % 2 != 0)
            {
                throw new ApplicationException("User information string contains an odd number of tokens.");
            }

            for (int i = 0; i < infoKeyTokens.Length; i += 2)
            {
                string key = infoKeyTokens[i];
                string value = infoKeyTokens[i + 1];

                if (key == "name")
                {
                    player.Name = value;
                }
                else if (key == "cl_updaterate")
                {
                    player.UpdateRate = float.Parse(value);
                }
                else if (key == "rate")
                {
                    player.Rate = float.Parse(value);
                }
                else if (key == "*sid")
                {
                    ulong sid;

                    if (ulong.TryParse(value, out sid) && sid != 0)
                    {
                        ulong authId = sid - 76561197960265728;
                        int serverId = ((authId % 2) == 0 ? 0 : 1);
                        authId = (authId - (ulong)serverId) / 2;
                        player.SteamId = string.Format("STEAM_0:{0}:{1}", serverId, authId);
                    }
                }
            }
        }

        private void Load_DeltaDescription(Messages.SvcDeltaDescription message)
        {
            AddDeltaStructure(message.Structure);
        }

        private void Load_NewUserMessage(Messages.SvcNewUserMessage message)
        {
            AddUserMessage(message.UserMessageId, message.UserMessageLength, message.UserMessageName);
        }

        private void Load_Hltv(Messages.SvcHltv message)
        {
            IsHltv = true;
            Perspective = "HLTV";
            AddDetail("Perspective", Perspective);
        }
        #endregion
    }
}