using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;

namespace CDP.IdTech3
{
    public class Demo : Core.Demo
    {
        private class CommandCallback
        {
            public CommandIds Id { get; set; }
            public object Delegate { get; set; }

            public void Fire(Command command)
            {
                MethodInfo methodInfo = Delegate.GetType().GetMethod("Invoke");
                methodInfo.Invoke(Delegate, new object[] { command });
            }
        }

        public class Player
        {
            public string Name { get; set; }
            public string HeadModel { get; set; }
            public string Model { get; set; }
        }

        public override string GameName
        {
            get { return gameName; }
        }

        public string ModFolder { get; protected set; }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get
            {
                return new string[]
                {
                    fileSystem.PathCombine(settings.ProgramPath, "icons", "idtech3", gameName + ".ico"),
                    fileSystem.PathCombine(settings.ProgramPath, "icons", "idtech3.ico")
                };
            }
        }

        public override string MapThumbnailRelativePath
        {
            get { return fileSystem.PathCombine("idtech3", MapName + ".jpg"); }
        }

        public override bool CanPlay
        {
            get { return false; }
        }

        public override bool CanAnalyse
        {
            get { return false; }
        }

        protected Plugin plugin;

        public override Core.Plugin Plugin
        {
            get { return plugin; }
            set { plugin = (Plugin)value; }
        }

        public virtual ConvertTargets ConvertTarget
        {
            get { return ConvertTargets.None; }
        }

        public Protocols Protocol { get; private set; }
        public Protocols ConvertTargetProtocol
        {
            get
            {
                if (ConvertTarget == ConvertTargets.Protocol68)
                {
                    return Protocols.Protocol68;
                }
                else if (ConvertTarget == ConvertTargets.Protocol73)
                {
                    return Protocols.Protocol73;
                }

                return Protocol;
            }
        }

        protected string gameName;
        private bool isLoaded = false;

        private readonly List<CommandCallback> commandCallbacks = new List<CommandCallback>();
        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();

        public Demo()
        {
            Players = new ArrayList();
        }

        public override void Load()
        {
            try
            {
                if (isLoaded)
                {
                    throw new ApplicationException("Demo has already been loaded.");
                }
                else
                {
                    isLoaded = true;
                }

                AddCommandCallback<Commands.SvcConfigString>(SetProtocol_ConfigString);
                AddCommandCallback<Commands.SvcConfigString>(Load_ConfigString);
                GuessProtocol();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    Message message = new Message();
                    message.Read(stream, Protocol);
                    ReadCommandsUntilEof(message.Reader, null, null, false);
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }
            finally
            {
                commandCallbacks.Clear();
            }

            OnOperationComplete();
        }

        public override void Read()
        {
            try
            {
                ResetOperationCancelledState();
                ResetProgress();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    while (true)
                    {
                        Message message = new Message();
                        message.Read(stream, Protocol);

                        if (message.Length == -1)
                        {
                            break;
                        }

                        ReadCommandsUntilEof(message.Reader, null, null, false);
                        UpdateProgress(stream.Position, stream.Length);

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }
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
                commandCallbacks.Clear();
            }

            OnOperationComplete();
        }

        public override void Write(string destinationFileName)
        {
            const int bufferSize = 4096;

            try
            {
                ResetOperationCancelledState();
                ResetProgress();
                AddCommandCallback<Commands.SvcConfigString>(Write_ConfigString);

                using (Core.FastFileStream inputStream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (Core.FastFileStream outputStream = new Core.FastFileStream(destinationFileName, Core.FastFileAccess.Write))
                {
                    while (true)
                    {
                        if (ConvertTarget == ConvertTargets.None)
                        {
                            int bytesToRead = bufferSize;

                            if (inputStream.BytesLeft < bytesToRead)
                            {
                                bytesToRead = (int)inputStream.BytesLeft;

                                if (bytesToRead == 0)
                                {
                                    break;
                                }
                            }

                            outputStream.WriteBytes(inputStream.ReadBytes(bytesToRead));
                        }
                        else
                        {
                            Message message = new Message();
                            message.Read(inputStream, Protocol);

                            if (message.Length == -1)
                            {
                                message.Write(outputStream);
                                break;
                            }

                            BitWriter writer = new BitWriter(true);
                            writer.WriteInt(message.ReliableAck);
                            ReadAndWriteCommandsUntilEof(message.Reader, writer, false);
                            byte[] messageData = writer.ToArray();
                            message.Length = messageData.Length;
                            message.Write(outputStream);
                            outputStream.WriteBytes(messageData);
                        }

                        UpdateProgress(inputStream.Position, inputStream.Length);

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }
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
                commandCallbacks.Clear();
            }

            OnOperationComplete();
        }

        public void RunDiagnostic(string logFileName, IEnumerable<CommandIds> commandsToLog)
        {
            try
            {
                AddCommandCallback<Commands.SvcConfigString>(SetProtocol_ConfigString);
                ResetOperationCancelledState();
                ResetProgress();
                GuessProtocol();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (StreamWriter log = new StreamWriter(logFileName))
                {
                    while (true)
                    {
                        Message message = new Message();
                        message.Read(stream, Protocol);

                        if (message.Length == -1)
                        {
                            log.WriteLine("=== END OF DEMO ===");
                            break;
                        }

                        message.Log(log);
                        ReadCommandsUntilEof(message.Reader, log, commandsToLog, false);
                        UpdateProgress(stream.Position, stream.Length);

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }
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
                commandCallbacks.Clear();
            }

            OnOperationComplete();
        }

        private CommandIds ReadCommandId(BitReader buffer, StreamWriter log)
        {
            CommandIds commandId = (CommandIds)buffer.ReadByte();

            if (log != null)
            {
                log.WriteLine("\nCommand {0} [{1}]", commandId, (byte)commandId);
            }

            return commandId;
        }

        private void ReadCommandsUntilEof(BitReader buffer, StreamWriter log, IEnumerable<CommandIds> commandsToLog, bool parsingSubCommands)
        {
            while (true)
            {
                CommandIds commandId = ReadCommandId(buffer, log);

                if (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol48)
                {
                    if (parsingSubCommands && (byte)commandId == 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (commandId == CommandIds.svc_eof)
                    {
                        break;
                    }
                }

                Command command = plugin.CreateCommand(commandId);

                if (command == null)
                {
                    throw new ApplicationException(string.Format("Unknown command \'{0}\'.", commandId));
                }

                bool writeToLog = (log == null ? false : commandsToLog.Contains(commandId));

                command.Demo = this;

                try
                {
                    command.Read(buffer);
                }
                catch (Exception ex)
                {
                    if (log != null)
                    {
                        log.WriteLine("\n*** ERROR ***\n");
                        log.WriteLine(ex.ToString());
                        log.WriteLine();
                        command.Log(log);
                    }

                    throw;
                }

                if (writeToLog)
                {
                    command.Log(log);
                }

                foreach (CommandCallback callback in FindCommandCallbacks(command))
                {
                    callback.Fire(command);
                }

                if (command.ContainsSubCommands)
                {
                    ReadCommandsUntilEof(buffer, log, commandsToLog, true);

                    if (command.HasFooter)
                    {
                        command.ReadFooter(buffer);

                        if (writeToLog)
                        {
                            command.LogFooter(log);
                        }
                    }
                }

                if (!parsingSubCommands && (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol48))
                {
                    buffer.SeekRemainingBitsInCurrentByte();

                    if (buffer.BitsLeft == 0)
                    {
                        return;
                    }
                }
            }
        }

        private void ReadAndWriteCommandsUntilEof(BitReader reader, BitWriter writer, bool parsingSubCommands)
        {
            while (true)
            {
                CommandIds commandId = (CommandIds)reader.ReadByte();

                if (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol48)
                {
                    // Command ID 0 marks the end of sub commands.
                    if (parsingSubCommands && (byte)commandId == 0)
                    {
                        // Insert svc_eof instead.
                        writer.WriteByte((byte)CommandIds.svc_eof);
                        break;
                    }
                }
                else
                {
                    if (commandId == CommandIds.svc_eof)
                    {
                        writer.WriteByte((byte)commandId);

                        if (!parsingSubCommands)
                        {
                            writer.WriteByte(0);
                        }

                        break;
                    }
                }

                if (commandId == CommandIds.svc_nop)
                {
                    writer.WriteByte((byte)commandId);
                    continue;
                }

                Command command = plugin.CreateCommand(commandId);

                if (command == null)
                {
                    throw new ApplicationException(string.Format("Unknown command \'{0}\'.", commandId));
                }

                writer.WriteByte((byte)commandId);
                command.Demo = this;
                command.Read(reader);

                foreach (CommandCallback callback in FindCommandCallbacks(command))
                {
                    callback.Fire(command);
                }

                command.Write(writer);

                if (command.ContainsSubCommands)
                {
                    ReadAndWriteCommandsUntilEof(reader, writer, true);

                    if (command.HasFooter)
                    {
                        command.ReadFooter(reader);
                        command.WriteFooter(writer);
                    }
                }

                if (!parsingSubCommands && (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol48))
                {
                    reader.SeekRemainingBitsInCurrentByte();

                    if (reader.BitsLeft == 0)
                    {
                        writer.WriteByte((byte)CommandIds.svc_eof);
                        writer.WriteByte(0);
                        return;
                    }
                }
            }
        }

        private void GuessProtocol()
        {
            Protocol = plugin.GuessProtocol(fileSystem.GetExtension(FileName));
        }

        /// <summary>
        /// Set the protocol, while checking to see if it is valid.
        /// </summary>
        private void SetProtocol(int protocol)
        {
            if (!Enum.IsDefined(typeof(Protocols), protocol))
            {
                throw new ApplicationException(string.Format("Unknown protocol \'{0}\'.", protocol));
            }

            Protocol = (Protocols)protocol;
        }

        private void SetProtocol_ConfigString(Commands.SvcConfigString command)
        {
            if (command.KeyValuePairs != null && command.KeyValuePairs.ContainsKey("protocol"))
            {
                SetProtocol(int.Parse(command.KeyValuePairs["protocol"]));
            }
        }

        private void Load_ConfigString(Commands.SvcConfigString command)
        {
            if (command.KeyValuePairs == null)
            {
                return;
            }

            if (command.KeyValuePairs.ContainsKey("version"))
            {
                AddDetail(Strings.DemoDetailVersion, command.KeyValuePairs["version"]);
            }

            if (command.KeyValuePairs.ContainsKey("mapname"))
            {
                MapName = command.KeyValuePairs["mapname"].ToLower();
                AddDetail(Strings.DemoDetailMap, MapName);
            }

            if (command.KeyValuePairs.ContainsKey("gamename"))
            {
                gameName = command.KeyValuePairs["gamename"].ToLower();
                ModFolder = gameName;
            }

            if (command.KeyValuePairs.ContainsKey("sv_maxclients"))
            {
                AddDetail(Strings.DemoDetailServerSlots, command.KeyValuePairs["sv_maxclients"]);
            }

            if (command.KeyValuePairs.ContainsKey("sv_hostname"))
            {
                AddDetail(Strings.DemoDetailServerName, command.KeyValuePairs["sv_hostname"]);
            }

            if (command.KeyValuePairs.ContainsKey("protocol"))
            {
                AddDetail(Strings.DemoDetailProtocol, command.KeyValuePairs["protocol"]);
            }

            if (command.IsPlayer)
            {
                Players.Add(new Player
                {
                    Name = command.KeyValuePairs["n"],
                    HeadModel = command.KeyValuePairs["hmodel"],
                    Model = command.KeyValuePairs["model"]
                });
            }
        }

        private void Write_ConfigString(Commands.SvcConfigString command)
        {
            if (command.KeyValuePairs == null || command.IsPlayer)
            {
                return;
            }

            if (command.KeyValuePairs.ContainsKey("protocol"))
            {
                command.KeyValuePairs["protocol"] = ((int)ConvertTargetProtocol).ToString();
            }
        }

        #region Command callbacks
        private List<CommandCallback> FindCommandCallbacks(Command command)
        {
            return commandCallbacks.FindAll(cc => cc.Id == command.Id);
        }

        public void AddCommandCallback<T>(Action<T> method) where T : Command
        {
            CommandCallback callback = new CommandCallback
            {
                Delegate = method
            };

            // Instantiate the frame type to get the ID.
            Command command = (Command)Activator.CreateInstance(typeof(T));
            callback.Id = command.Id;
            commandCallbacks.Add(callback);
        }
        #endregion
    }
}
