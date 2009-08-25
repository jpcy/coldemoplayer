using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

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

        public override Core.DemoHandler Handler
        {
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

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override IList<Detail> Details { get; protected set; }

        public uint NetworkProtocol { get; private set; }
        public string GameFolderName { get; private set; }
        public uint MapChecksum { get; private set; }
        public byte MaxClients { get; private set; }
        public bool IsHltv { get; private set; }
        public bool IsCorrupt { get; private set; }

        private Handler handler;
        private readonly List<MessageCallback> messageCallbacks;
        private readonly Dictionary<string, DeltaStructure> deltaStructures;
        private readonly List<UserMessageDefinition> userMessageDefinitions;

        public Demo()
        {
            Perspective = "POV";
            IsHltv = false;
            IsCorrupt = false;
            Details = new List<Detail>();
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
                AddMessageCallback<Messages.SvcDeltaDescription>(Load_DeltaDescription);
                AddMessageCallback<Messages.SvcNewUserMessage>(Load_NewUserMessage);
                AddMessageCallback<Messages.SvcHltv>(Load_Hltv);
                int progress = 0;

                using (FileStream stream = File.OpenRead(FileName))
                using (BinaryReader br = new BinaryReader(stream))
                using (StreamWriter log = new StreamWriter(string.Format("E:\\Counter-Strike Demos\\col demo player test\\logs\\{0}.txt", Name)))
                {
                    // Read header.
                    Header header = new Header();
                    header.Read(br.ReadBytes(Header.SizeInBytes));
                    NetworkProtocol = header.NetworkProtocol;
                    MapName = header.MapName;
                    GameFolderName = header.GameFolderName;
                    MapChecksum = header.MapChecksum;
                    AddDetail("Demo Protocol", header.DemoProtocol);
                    AddDetail("Network Protocol", NetworkProtocol);
                    AddDetail("Game Folder", GameFolderName);
                    AddDetail("Map Checksum", MapChecksum);
                    AddDetail("Map Name", MapName);

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
                            IMessage message = null;
                            message = ReadMessage(messageReader);
                            log.WriteLine("{0} [{1}]", message.Name, message.Id);
                            message.Log(log);
                            log.WriteLine("----------------");
                            log.Flush();
                        }
                    }

                    // Update progress.
                    int newProgress = (int)(stream.Position / stream.Length * 100);
                    
                    if (newProgress > progress)
                    {
                        progress = newProgress;
                        OnProgressChanged(progress);
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(null, ex);
                return;
            }

            OnOperationComplete();
        }

        public override void Read()
        {
            throw new NotImplementedException();
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
            MaxClients = message.MaxClients;
            AddDetail("Server Slots", MaxClients);
            AddDetail("Server Name", message.ServerName);
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