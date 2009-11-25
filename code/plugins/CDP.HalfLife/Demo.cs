using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using CDP.Core.Extensions;

namespace CDP.HalfLife
{
    public class Demo : Core.Demo
    {
        public class MessageException : Exception
        {
            private readonly string message;

            public override string Message
            {
                get { return message; }
            }

            public MessageException(IEnumerable<IMessage> messageHistory, Exception innerException) : base(null, innerException)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Last frame messages:\n");

                foreach (IMessage m in messageHistory)
                {
                    sb.AppendFormat("({0}) {1} [{2}]\n", m.Offset, m.Name, m.Id);
                }

                message = sb.ToString();
            }
        }

        private class MessageCallback
        {
            public byte EngineMessageId { get; set; }
            public string UserMessageName { get; set; }
            public object Delegate { get; set; }

            public void Fire(IMessage message)
            {
                MethodInfo methodInfo = Delegate.GetType().GetMethod("Invoke");
                methodInfo.Invoke(Delegate, new object[] { message });
            }
        }

        private class FrameCallback
        {
            public byte FrameId { get; set; }
            public object Delegate { get; set; }

            public void Fire(Frame frame)
            {
                MethodInfo methodInfo = Delegate.GetType().GetMethod("Invoke");
                methodInfo.Invoke(Delegate, new object[] { frame });
            }
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
                return Game.Name + (version == null ? string.Empty : " " + version);
            }
        }

        public Core.SteamGame Game { get; private set; }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
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
        public string ServerName { get; private set; }
        public bool IsHltv { get; private set; }
        public bool IsCorrupt { get; private set; }

        /// <summary>
        /// During a Read operation, the timestamp of the frame currently being read.
        /// </summary>
        public float CurrentTimestamp { get; private set; }

        private Handler handler;
        protected string clientDllChecksum;
        private readonly List<MessageCallback> messageCallbacks;
        private readonly List<FrameCallback> frameCallbacks;
        private readonly Dictionary<string, DeltaStructure> readDeltaStructures;
        private readonly Dictionary<string, DeltaStructure> writeDeltaStructures;
        private readonly List<UserMessageDefinition> userMessageDefinitions;
        private readonly Core.CyclicQueue<IMessage> messageHistory = new Core.CyclicQueue<IMessage>(16);
        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();

        // Pre-write.
        private int currentFrameIndex;
        private int firstFrameToWriteIndex;

        // Writing.
        private readonly List<IMessage> messagesToInsert = new List<IMessage>();
        private bool haveInsertedSvcDirectorMessage;
        private bool haveParsedSvcDirectorMessage;

        /// <summary>The offset of the last message block that was read.</summary>
        /// <remarks>Used to calculate a message's offset, which in turn is used for diagnostics and error messages.</remarks>
        private long currentMessageBlockOffset;

        public Demo()
        {
            Perspective = "POV";
            IsHltv = false;
            IsCorrupt = false;
            Players = new ArrayList();
            messageCallbacks = new List<MessageCallback>();
            frameCallbacks = new List<FrameCallback>();
            readDeltaStructures = new Dictionary<string, DeltaStructure>();
            writeDeltaStructures = new Dictionary<string, DeltaStructure>();
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

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    // Read header.
                    // TODO: sanity check on demo and network protocol; friendly error message.
                    Header header = new Header();
                    header.Read(stream.ReadBytes(Header.SizeInBytes));
                    NetworkProtocol = header.NetworkProtocol;
                    MapName = header.MapName;
                    GameFolderName = header.GameFolderName;
                    MapChecksum = header.MapChecksum;
                    AddDetail(Strings.DemoDetailDemoProtocol, header.DemoProtocol);
                    AddDetail(Strings.DemoDetailNetworkProtocol, NetworkProtocol);
                    AddDetail(Strings.DemoDetailGameFolder, GameFolderName);
                    AddDetail(Strings.DemoDetailMapChecksum, MapChecksum);
                    AddDetail(Strings.DemoDetailMapName, MapName);
                    Game = handler.FindGame(GameFolderName);

                    // Read directory entries.
                    try
                    {
                        ReadDirectoryEntries(header.DirectoryEntriesOffset, stream);
                    }
                    catch (Exception)
                    {
                        IsCorrupt = true;
                    }

                    AddDetail(Strings.DemoDetailStatus, IsCorrupt ? Strings.DemoStatusCorruptDirectoryEntries : Strings.DemoStatusOk);

                    // Read frames.
                    stream.Seek(Header.SizeInBytes, SeekOrigin.Begin);

                    while (true)
                    {
                        Frame frame = ReadFrame(stream);

                        if (frame.Id == (byte)FrameIds.PlaybackSegmentStart)
                        {
                            break;
                        }
                        else if (frame.HasMessages)
                        {
                            ReadMessagesInMessageBlock(ReadMessageBlock(stream));
                        }

                        UpdateProgress(stream.Position, stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }
            finally
            {
                messageHistory.Clear();
                messageCallbacks.Clear();
                frameCallbacks.Clear();
            }

            OnOperationComplete();
        }

        public override void Read()
        {
            long lastFrameOffset = 0;
            CurrentTimestamp = 0.0f;

            try
            {
                ResetOperationCancelledState();
                ResetProgress();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    Header header = new Header();
                    header.Read(stream.ReadBytes(Header.SizeInBytes));

                    while (true)
                    {
                        lastFrameOffset = stream.Position;
                        Frame frame = ReadFrame(stream);
                        CurrentTimestamp = frame.Timestamp;

                        if (frame.HasMessages)
                        {
                            ReadMessagesInMessageBlock(ReadMessageBlock(stream));
                        }

                        if ((!IsCorrupt && stream.Position >= header.DirectoryEntriesOffset) || stream.Position == stream.Length)
                        {
                            break;
                        }

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }

                        UpdateProgress(stream.Position, stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsOperationCancelled())
                {
                    OnOperationError(FileName, ex);
                    return;
                }
            }
            finally
            {
                messageHistory.Clear();
                messageCallbacks.Clear();
                frameCallbacks.Clear();
            }

            if (IsOperationCancelled())
            {
                OnOperationCancelled();
            }
            else
            {
                OnOperationComplete();
            }
        }

        public override void Write(string destinationFileName)
        {
            Write(destinationFileName, true);
        }

        public void Write(string destinationFileName, bool canSkipMessages)
        {
            // Clear state (Write may be called on the same demo multiple times).
            writeDeltaStructures.Clear();
            writeDeltaStructures.AddRange(readDeltaStructures);
            long lastFrameOffset = 0;
            CurrentTimestamp = 0.0f;
            currentFrameIndex = 0;
            firstFrameToWriteIndex = 0;
            haveInsertedSvcDirectorMessage = false;
            haveParsedSvcDirectorMessage = false;

            try
            {
                ResetOperationCancelledState();
                ResetProgress();

                using (Core.FastFileStream inputStream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (Core.FastFileStream outputStream = new Core.FastFileStream(destinationFileName, Core.FastFileAccess.Write))
                {
                    // Before writing, read the loading segment and do some processing.
                    // We don't want message and frame callbacks added before this call to Write to fire when reading the loading segment, so save them now and restore them just before writing starts.
                    List<MessageCallback> savedMessageCallbacks = new List<MessageCallback>(messageCallbacks);
                    List<FrameCallback> savedFrameCallbacks = new List<FrameCallback>(frameCallbacks);
                    messageCallbacks.Clear();
                    frameCallbacks.Clear();
                    inputStream.Seek(Header.SizeInBytes, SeekOrigin.Begin);
                    AddMessageCallback<Messages.SvcServerInfo>(msg => firstFrameToWriteIndex = currentFrameIndex);

                    while (true)
                    {
                        Frame frame = ReadFrame(inputStream);
                        CurrentTimestamp = frame.Timestamp;

                        if (frame.Id == (byte)FrameIds.PlaybackSegmentStart)
                        {
                            break;
                        }
                        else if (frame.HasMessages)
                        {
                            ReadMessagesInMessageBlock(ReadMessageBlock(inputStream));
                        }

                        currentFrameIndex++;
                    }

                    // Restore saved message and frame callbacks.
                    messageCallbacks.Clear();
                    frameCallbacks.Clear();
                    messageCallbacks.AddRange(savedMessageCallbacks);
                    frameCallbacks.AddRange(savedFrameCallbacks);

                    // For a HLTV demo, svc_director messages should be inserted into the first frame of the playback segment if no svc_director messages were encountered during the loading segment.
                    if (Perspective == "HLTV")
                    {
                        AddMessageCallback<Messages.SvcDirector>(Write_Director);
                        AddFrameCallback<Frames.Playback>(Write_Playback);
                    }

                    // Read header, write empty header.
                    // The directory entries offset must be known before the header can be written, which means the entire demo must be parsed first.
                    inputStream.Seek(0, SeekOrigin.Begin);
                    Header header = new Header();
                    header.Read(inputStream.ReadBytes(Header.SizeInBytes));
                    outputStream.WriteBytes(new byte[Header.SizeInBytes]);

                    bool foundPlaybackSegment = false;
                    uint playbackSegmentOffset = 0;
                    int nPlaybackFrames = 0;
                    currentFrameIndex = 0;
                    AddMessageCallback<Messages.SvcServerInfo>(Write_ServerInfo);
                    AddFrameCallback<Frames.ClientCommand>(Write_ClientCommand);

                    while (true)
                    {
                        // Workaround for a Half-Life engine bug.
                        // Skip writing frames until the last frame that contains a svc_serverinfo message.
                        if (currentFrameIndex < firstFrameToWriteIndex)
                        {
                            Frame frameToSkip = ReadFrame(inputStream);
                            
                            if (frameToSkip.HasMessages)
                            {
                                ReadMessageBlock(inputStream);
                            }

                            currentFrameIndex++;
                            continue;
                        }

                        // Read and write frame.
                        lastFrameOffset = outputStream.Position;
                        Frame frame = ReadAndWriteFrame(inputStream, outputStream);
                        CurrentTimestamp = frame.Timestamp;

                        // Remember the offset of the start of the playback segment.
                        if (!foundPlaybackSegment && frame is Frames.PlaybackSegmentStart)
                        {
                            foundPlaybackSegment = true;
                            playbackSegmentOffset = (uint)lastFrameOffset;
                        }

                        // Handle a frame's message block.
                        if (frame.HasMessages)
                        {
                            if (foundPlaybackSegment)
                            {
                                nPlaybackFrames++;
                            }

                            ReadAndWriteMessagesInMessageBlock(ReadMessageBlock(inputStream), outputStream, canSkipMessages);
                        }

                        if ((!IsCorrupt && inputStream.Position >= header.DirectoryEntriesOffset) || inputStream.Position == inputStream.Length)
                        {
                            break;
                        }

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }

                        UpdateProgress(inputStream.Position, inputStream.Length);
                    }

                    if (!foundPlaybackSegment)
                    {
                        throw new ApplicationException("Demo frames have been written, but no playback segment offset has been found.");
                    }

                    // Write directory entries.
                    uint directoryEntriesOffset = (uint)outputStream.Position;
                    outputStream.WriteInt(2); // Number of dir. entries (always 2).

                    outputStream.WriteBytes(new DirectoryEntry
                    {
                        Title = "LOADING",
                        Offset = Header.SizeInBytes,
                        Length = (int)(playbackSegmentOffset - Header.SizeInBytes)
                    }.Write());

                    outputStream.WriteBytes(new DirectoryEntry
                    {
                        Title = "Playback",
                        Number = 1,
                        Offset = (int)playbackSegmentOffset,
                        Length = (int)(directoryEntriesOffset - playbackSegmentOffset),
                        NumFrames = nPlaybackFrames,
                        Duration = CurrentTimestamp
                    }.Write());

                    // Write header.
                    header.DirectoryEntriesOffset = directoryEntriesOffset;
                    header.NetworkProtocol = Demo.NewestNetworkProtocol;
                    header.GameFolderName = Game.ModFolder;
                    outputStream.Seek(0, SeekOrigin.Begin);
                    outputStream.WriteBytes(header.Write());
                }
            }
            catch (Exception ex)
            {
                if (!IsOperationCancelled())
                {
                    OnOperationError(FileName, ex);
                    return;
                }
            }
            finally
            {
                messageHistory.Clear();
                messageCallbacks.Clear();
                frameCallbacks.Clear();
            }

            if (IsOperationCancelled())
            {
                OnOperationCancelled();
            }
            else
            {
                OnOperationComplete();
            }
        }

        public void RunDiagnostic(IEnumerable<byte> engineMessageIds, IEnumerable<string> userMessages, string logFileName)
        {
            Func<byte, bool> shouldLogMessage = id =>
            {
                if (id <= MaxEngineMessageId)
                {
                    if (engineMessageIds != null)
                    {
                        return engineMessageIds.Contains(id);
                    }
                }
                else
                {
                    if (userMessages != null)
                    {
                        UserMessageDefinition userMessageDefinition = userMessageDefinitions.SingleOrDefault(umd => umd.Id == id);

                        if (userMessageDefinition != null)
                        {
                            return userMessages.Contains(userMessageDefinition.Name);
                        }
                    }
                }

                return false;
            };


            try
            {
                AddMessageCallback<Messages.SvcServerInfo>(Diagnostic_ServerInfo);
                AddMessageCallback<Messages.SvcDeltaDescription>(Diagnostic_DeltaDescription);
                AddMessageCallback<Messages.SvcNewUserMessage>(Diagnostic_NewUserMessage);
                AddMessageCallback<Messages.SvcHltv>(Diagnostic_Hltv);
                ResetOperationCancelledState();
                ResetProgress();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (StreamWriter log = new StreamWriter(logFileName))
                {
                    try
                    {
                        // Header.
                        Header header = new Header();
                        header.Read(stream.ReadBytes(Header.SizeInBytes));
                        NetworkProtocol = header.NetworkProtocol;
                        GameFolderName = header.GameFolderName;
                        Game = handler.FindGame(GameFolderName);
                        header.Log(log);
                        log.WriteLine();

                        // Frames
                        while (true)
                        {
                            long frameOffset = stream.Position;
                            Frame frame = ReadFrame(stream);
                            log.WriteLine("Frame {0}. Number: {1}, Timestamp: {2}, Offset: {3}", frame.Id, frame.Number, frame.Timestamp, frameOffset);

                            if (frame.HasMessages)
                            {
                                try
                                {
                                    byte[] messageBlock = ReadMessageBlock(stream);

                                    if (messageBlock != null)
                                    {
                                        BitReader messageReader = new BitReader(messageBlock);

                                        while (messageReader.CurrentByte < messageReader.Length)
                                        {
                                            try
                                            {
                                                byte id = messageReader.ReadByte();
                                                ReadMessage(messageReader, id, !shouldLogMessage(id));
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new MessageException(messageHistory, ex);
                                            }
                                        }
                                    }
                                }
                                catch (MessageException ex)
                                {
                                    log.WriteLine("\n*** Error processing message block ***\n");
                                    log.WriteLine(ex.ToString());
                                    log.WriteLine();
                                }

                                foreach (IMessage message in messageHistory)
                                {
                                    log.WriteLine("{0} [{1}] Offset: {2}", message.Name, message.Id, message.Offset);

                                    if (shouldLogMessage(message.Id))
                                    {
                                        message.Log(log);
                                        log.WriteLine();
                                    }
                                }

                                log.WriteLine();
                            }

                            if ((stream.Position >= header.DirectoryEntriesOffset) || stream.Position == stream.Length)
                            {
                                break;
                            }

                            if (IsOperationCancelled())
                            {
                                OnOperationCancelled();
                                return;
                            }

                            UpdateProgress(stream.Position, stream.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine(ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }
            finally
            {
                messageHistory.Clear();
                messageCallbacks.Clear();
                frameCallbacks.Clear();
            }

            OnOperationComplete();
        }

        #region Reading and writing
        private void ReadDirectoryEntries(long offset, Core.FastFileStream stream)
        {
            // offset + nEntries + 2 directory entries.
            // Don't check if there's anything following the directory entries, it could be used for meta-data in the future.
            if (stream.Length < offset + 4 + DirectoryEntry.SizeInBytes * 2)
            {
                goto corrupt;
            }

            stream.Seek(offset, SeekOrigin.Begin);

            int nEntries = stream.ReadInt();

            if (nEntries != 2)
            {
                goto corrupt;
            }

            DirectoryEntry loading = new DirectoryEntry();
            loading.Read(stream.ReadBytes(DirectoryEntry.SizeInBytes));
            DirectoryEntry playback = new DirectoryEntry();
            playback.Read(stream.ReadBytes(DirectoryEntry.SizeInBytes));

            // Verify loading segment offset. It immediately follows the header, so it should be equal to the header size.
            if (loading.Offset != Header.SizeInBytes)
            {
                goto corrupt;
            }

            // Verify loading segment length. Offset + length should be equal to playback segment offset.
            if (loading.Offset + loading.Length != playback.Offset)
            {
                goto corrupt;
            }

            // Verify first byte at playback segment offset (should be frame ID '2', playback segment start).
            stream.Seek(playback.Offset, SeekOrigin.Begin);

            if (stream.ReadByte() != (byte)FrameIds.PlaybackSegmentStart)
            {
                goto corrupt;
            }

            Duration = TimeSpan.FromSeconds(Math.Abs(playback.Duration));
            AddDetail(Strings.DemoDetailDuration, Duration);
            return;

            corrupt:
            IsCorrupt = true;
        }

        private Frame ReadFrameHeader(Core.FastFileStream stream)
        {
            // New frame, clear the message history.
            messageHistory.Clear();

            byte id = stream.ReadByte();
            Frame frame = handler.CreateFrame(id);

            if (frame == null)
            {
                throw new ApplicationException(string.Format("Unknown frame type \"{0}\" at offset \"{1}\"", id, stream.Position - 1));
            }

            frame.Demo = this;
            frame.ReadHeader(stream, NetworkProtocol);
            return frame;
        }

        private Frame ReadFrame(Core.FastFileStream stream)
        {
            Frame frame = ReadFrameHeader(stream);
            List<FrameCallback> frameCallbacks = FindFrameCallbacks(frame);

            if (frameCallbacks.Count == 0 && frame.CanSkip)
            {
                frame.Skip(stream);
            }
            else
            {
                frame.Read(stream);
            }

            foreach (FrameCallback frameCallback in frameCallbacks)
            {
                frameCallback.Fire(frame);
            }

            return frame;
        }

        private Frame ReadAndWriteFrame(Core.FastFileStream inputStream, Core.FastFileStream outputStream)
        {
            Frame frame = ReadFrameHeader(inputStream);
            List<FrameCallback> frameCallbacks = FindFrameCallbacks(frame);

            if (frameCallbacks.Count == 0 && frame.CanSkip)
            {
                long frameStartOffset = inputStream.Position;
                frame.Skip(inputStream);
                int frameLength = (int)(inputStream.Position - frameStartOffset);
                frame.WriteHeader(outputStream);

                if (frameLength > 0)
                {
                    inputStream.Seek(frameStartOffset, SeekOrigin.Begin);
                    outputStream.WriteBytes(inputStream.ReadBytes(frameLength));
                }
            }
            else
            {
                frame.Read(inputStream);

                foreach (FrameCallback frameCallback in frameCallbacks)
                {
                    frameCallback.Fire(frame);
                }

                if (!frame.Remove)
                {
                    frame.WriteHeader(outputStream);
                    frame.Write(outputStream);
                }
            }

            return frame;
        }

        private byte[] ReadMessageBlock(Core.FastFileStream stream)
        {
            uint length = stream.ReadUInt();

            if (length > (1<<16))
            {
                throw new ApplicationException("Message data length too large.");
            }

            currentMessageBlockOffset = stream.Position;
            byte[] data = null;

            if (length > 0)
            {
                data = stream.ReadBytes((int)length);
            }

            return data;
        }

        private void ReadMessagesInMessageBlock(byte[] messageBlock)
        {
            if (messageBlock == null)
            {
                return;
            }

            BitReader messageReader = new BitReader(messageBlock);

            while (messageReader.CurrentByte < messageReader.Length)
            {
                try
                {
                    ReadMessage(messageReader, null, true);
                }
                catch (Exception ex)
                {
                    throw new MessageException(messageHistory, ex);
                }
            }
        }

        private void ReadAndWriteMessagesInMessageBlock(byte[] messageBlock, Core.FastFileStream outputStream, bool canSkipMessages)
        {
            if (messageBlock != null && messageBlock.Length != 0)
            {
                BitReader messageReader = new BitReader(messageBlock);
                BitWriter messageWriter = new BitWriter();

                while (messageReader.CurrentByte < messageReader.Length)
                {
                    try
                    {
                        ReadAndWriteMessage(messageReader, messageWriter, canSkipMessages);
                    }
                    catch (Exception ex)
                    {
                        throw new MessageException(messageHistory, ex);
                    }
                }

                // Append any extra messages.
                foreach (IMessage message in messagesToInsert)
                {
                    UserMessage userMessage = message as UserMessage;

                    if (userMessage == null)
                    {
                        messageWriter.WriteByte(message.Id);
                    }
                    else
                    {
                        messageWriter.WriteByte(GetUserMessageId(message.Name));
                    }

                    if (userMessage != null)
                    {
                        UserMessageDefinition definition = userMessageDefinitions.Single(umd => umd.Id == message.Id);

                        if (definition.Length == -1)
                        {
                            messageWriter.WriteByte(userMessage.Length);
                        }
                    }

                    message.Write(messageWriter);
                }

                messagesToInsert.Clear();

                // Write the message block.
                messageBlock = messageWriter.ToArray();
                outputStream.WriteUInt((uint)messageBlock.Length);
                outputStream.WriteBytes(messageBlock);
            }
            else
            {
                outputStream.WriteUInt(0);
            }
        }

        private IMessage ReadMessageHeader(BitReader buffer, byte? id)
        {
            long messageOffset = currentMessageBlockOffset + buffer.CurrentByte;

            if (id == null)
            {
                id = buffer.ReadByte();
            }
            else
            {
                // Assume the buffer position is one byte after where the ID was read.
                messageOffset--;
            }

            IMessage message = null;

            if (id <= Demo.MaxEngineMessageId)
            {
                message = handler.CreateEngineMessage(id.Value);
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

                    if (userMessageDefinition.Length == -1)
                    {
                        // -1 is a marker for "dynamic length message", which just means the next byte contains the message length.
                        userMessage.Length = buffer.ReadByte();
                    }
                    else
                    {
                        userMessage.Length = (byte)userMessageDefinition.Length;
                    }

                    message = userMessage;
                }
            }

            if (message == null)
            {
                throw new ApplicationException(string.Format("Unknown message type \"{0}\".", id));
            }

            message.Demo = this;
            message.Offset = messageOffset;
            messageHistory.Enqueue(message);
            return message;
        }

        private IMessage ReadMessage(BitReader buffer, byte? id, bool canSkip)
        {
            IMessage message = ReadMessageHeader(buffer, id);
            List<MessageCallback> messageCallbacks = FindMessageCallbacks(message);

            try
            {
                if (canSkip && messageCallbacks.Count == 0)
                {
                    message.Skip(buffer);
                }
                else
                {
                    message.Read(buffer);
                }
            }
            finally
            {
                buffer.Endian = BitReader.Endians.Little;
            }

            foreach (MessageCallback messageCallback in messageCallbacks)
            {
                messageCallback.Fire(message);
            }

            return message;
        }

        private IMessage ReadAndWriteMessage(BitReader reader, BitWriter writer, bool canSkip)
        {
            IMessage message = ReadMessageHeader(reader, null);
            UserMessage userMessage = message as UserMessage;

            // User message IDs are changed to match the current version of the specific mod.
            byte newMessageId = message.Id;

            if (userMessage != null)
            {
                newMessageId = GetUserMessageId(message.Name);
            }

            List<MessageCallback> messageCallbacks = FindMessageCallbacks(message);
            int startPosition = reader.CurrentByte;
            bool wasSkipped;

            try
            {
                if (canSkip && messageCallbacks.Count == 0 && message.CanSkipWhenWriting)
                {
                    message.Skip(reader);
                    wasSkipped = true;
                }
                else
                {
                    message.Read(reader);
                    wasSkipped = false;
                }
            }
            finally
            {
                reader.Endian = BitReader.Endians.Little;
            }

            foreach (MessageCallback messageCallback in messageCallbacks)
            {
                messageCallback.Fire(message);
            }

            // Write the message if a message callback hasn't flagged it for removal.
            if (!message.Remove)
            {
                writer.WriteByte(newMessageId);

                if (userMessage != null)
                {
                    UserMessageDefinition definition = userMessageDefinitions.Single(umd => umd.Id == message.Id);

                    if (definition.Length == -1)
                    {
                        writer.WriteByte(userMessage.Length);
                    }
                }

                if (wasSkipped)
                {
                    int length = reader.CurrentByte - startPosition;

                    if (length > 0)
                    {
                        writer.WriteBytes(reader.Buffer, startPosition, length);
                    }
                }
                else
                {
                    message.Write(writer);
                }
            }

            return message;
        }

        /// <summary>
        /// Provides a new user message ID to use when writing a demo.
        /// </summary>
        /// <remarks>
        /// In order to play a demo after a listen server has been started, the demo user message ID should be changed to match the current mod version's; the Half-Life engine doesn't support re-registering of user messages (new messages are ignored).
        /// 
        /// svc_newusermsg messages will also have to be re-written to match the new IDs. This is the responsiblity of the individual mod.
        /// </remarks>
        /// <param name="userMessageName">The user message name.</param>
        /// <returns>A new user message ID.</returns>
        protected virtual byte GetUserMessageId(string userMessageName)
        {
            // Provide defaults.
            return userMessageDefinitions.First(umd => umd.Name == userMessageName).Id;
        }
        #endregion

        #region Message callbacks
        private List<MessageCallback> FindMessageCallbacks(IMessage message)
        {
            if (message.Id <= Demo.MaxEngineMessageId)
            {
                return messageCallbacks.FindAll(mc => mc.EngineMessageId == message.Id);
            }
            
            return messageCallbacks.FindAll(mc => mc.UserMessageName == message.Name);
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
        #endregion

        #region Frame callbacks
        private List<FrameCallback> FindFrameCallbacks(Frame frame)
        {
            return frameCallbacks.FindAll(fc => fc.FrameId == frame.Id);
        }

        public void AddFrameCallback<T>(Action<T> method) where T : Frame
        {
            FrameCallback callback = new FrameCallback
            {
                Delegate = method
            };

            // Instantiate the frame type to get the ID.
            Frame frame = (Frame)Activator.CreateInstance(typeof(T));
            callback.FrameId = frame.Id;
            frameCallbacks.Add(callback);
        }
        #endregion

        private void AddDeltaStructure(DeltaStructure structure)
        {
            // Remove the decoder if it already exists, e.g. duplicate svc_deltadescription message (http://www.gotfrag.com/cs/demos/6/).
            Action<Dictionary<string, DeltaStructure>> addDeltaStructure = dict =>
            {
                if (dict.ContainsKey(structure.Name))
                {
                    dict.Remove(structure.Name);
                }

                dict.Add(structure.Name, structure);
            };

            addDeltaStructure(readDeltaStructures);
            addDeltaStructure(writeDeltaStructures);
        }

        protected void AddWriteDeltaStructure(DeltaStructure structure)
        {
            // For handling svc_deltadescription message changes when writing, so deltas are written using the new structures.
            if (writeDeltaStructures.ContainsKey(structure.Name))
            {
                writeDeltaStructures.Remove(structure.Name);
            }

            writeDeltaStructures.Add(structure.Name, structure);
        }

        public DeltaStructure FindReadDeltaStructure(string name)
        {
            if (!readDeltaStructures.ContainsKey(name))
            {
                throw new ApplicationException("Delta structure \"" + name + "\" not found.");
            }

            return readDeltaStructures[name];
        }

        public DeltaStructure FindWriteDeltaStructure(string name)
        {
            if (!writeDeltaStructures.ContainsKey(name))
            {
                throw new ApplicationException("Delta structure \"" + name + "\" not found.");
            }

            return writeDeltaStructures[name];
        }

        public void AddUserMessage(byte id, sbyte length, string name)
        {
            // Some demos contain duplicate user message definitions.
            // Example: http://www.gotfrag.com/cs/demos/13
            // Some poorly written server mods have many copies of the same user message registered with different IDs.
            UserMessageDefinition userMessageDefinition = userMessageDefinitions.SingleOrDefault(umd => umd.Id == id);

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
            AddDetail(Strings.DemoDetailServerSlots, MaxClients);
            ServerName = message.ServerName;
            AddDetail(Strings.DemoDetailServerName, message.ServerName);
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
            AddDetail(Strings.DemoDetailPerspective, Perspective);
        }
        #endregion

        #region Write callbacks
        private void Write_Director(Messages.SvcDirector message)
        {
            haveParsedSvcDirectorMessage = true;
        }

        private void Write_Playback(Frames.Playback frame)
        {
            if (haveParsedSvcDirectorMessage || haveInsertedSvcDirectorMessage)
            {
                return;
            }
            
            messagesToInsert.Add(new Messages.SvcDirector
            {
                Data = new byte[] { Messages.SvcDirector.DRC_CMD_START }
            });

            messagesToInsert.Add(new Messages.SvcDirector
            {
                Data = new byte[]
                {
                    Messages.SvcDirector.DRC_CMD_MODE,
                    Messages.SvcDirector.OBS_IN_EYE
                }
            });

            haveInsertedSvcDirectorMessage = true;
        }

        private void Write_ServerInfo(Messages.SvcServerInfo message)
        {
            message.NetworkProtocol = Demo.NewestNetworkProtocol;
            message.GameFolder = Game.ModFolder;
        }

        private void Write_ClientCommand(Frames.ClientCommand frame)
        {
            if (Perspective == "POV" && (bool)settings["HlRemoveShowscores"] && (frame.Command == "+showscores" || frame.Command == "-showscores"))
            {
                frame.Remove = true;
            }
        }
        #endregion

        #region Diagnostic callbacks
        private void Diagnostic_ServerInfo(Messages.SvcServerInfo message)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < message.ClientDllChecksum.Length; i++)
            {
                sb.Append(message.ClientDllChecksum[i].ToString("X2"));
            }

            clientDllChecksum = sb.ToString();
            MaxClients = message.MaxClients;
        }

        private void Diagnostic_DeltaDescription(Messages.SvcDeltaDescription message)
        {
            AddDeltaStructure(message.Structure);
        }

        private void Diagnostic_NewUserMessage(Messages.SvcNewUserMessage message)
        {
            AddUserMessage(message.UserMessageId, message.UserMessageLength, message.UserMessageName);
        }

        private void Diagnostic_Hltv(Messages.SvcHltv message)
        {
            IsHltv = true;
        }
        #endregion
    }
}