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

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get
            {
                return new string[] { fileSystem.PathCombine(settings.ProgramPath, "icons", "idtech3", gameName + ".ico") };
            }
        }

        public override string MapImagesRelativePath
        {
            get { return null; }
        }

        public override bool CanPlay
        {
            get { return false; }
        }

        public override bool CanAnalyse
        {
            get { return false; }
        }

        protected Handler handler;

        public override Core.DemoHandler Handler
        {
            get { return handler; }
            set { handler = (Handler)value; }
        }

        public int Protocol { get; private set; }
        private string gameName; // TEMP: remove me

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
                AddCommandCallback<Commands.SvcConfigString>(Load_ConfigString);
                CalculateProtocol();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    Message message = new Message();
                    message.Read(stream);
                    ReadCommandsUntilEof(message.Reader, null, null);
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
            throw new NotImplementedException();
        }

        public override void Write(string destinationFileName)
        {
            throw new NotImplementedException();
        }

        public void RunDiagnostic(string logFileName, IEnumerable<CommandIds> commandsToLog)
        {
            try
            {
                ResetProgress();
                CalculateProtocol();

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (StreamWriter log = new StreamWriter(logFileName))
                {
                    while (true)
                    {
                        Message message = new Message();
                        message.Read(stream);

                        if (message.Length == -1)
                        {
                            log.WriteLine("=== END OF DEMO ===");
                            break;
                        }

                        message.Log(log);
                        ReadCommandsUntilEof(message.Reader, log, commandsToLog);
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

        private void ReadCommandsUntilEof(BitReader buffer, StreamWriter log, IEnumerable<CommandIds> commandsToLog)
        {
            while (true)
            {
                CommandIds commandId = ReadCommandId(buffer, log);

                if (commandId == CommandIds.svc_eof)
                {
                    break;
                }

                Command command = handler.CreateCommand(commandId);

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
                catch (Exception)
                {
                    if (log != null)
                    {
                        log.WriteLine("\n*** ERROR ***\n");
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
                    ReadCommandsUntilEof(buffer, log, commandsToLog);

                    if (command.HasFooter)
                    {
                        command.ReadFooter(buffer);

                        if (writeToLog)
                        {
                            command.LogFooter(log);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the protocol based on the filename extension.
        /// </summary>
        private void CalculateProtocol()
        {
            string extension = fileSystem.GetExtension(FileName).ToLower();

            if (extension == "dm3")
            {
                Protocol = 3;
            }
            else
            {
                int protocol = 0;

                if (int.TryParse(extension.Replace("dm_", string.Empty), out protocol))
                {
                    Protocol = protocol;
                }
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
                AddDetail("Version", command.KeyValuePairs["version"]);
            }

            if (command.KeyValuePairs.ContainsKey("mapname"))
            {
                MapName = command.KeyValuePairs["mapname"];
                AddDetail("Map", MapName);
            }

            if (command.KeyValuePairs.ContainsKey("gamename"))
            {
                gameName = command.KeyValuePairs["gamename"];
            }

            if (command.KeyValuePairs.ContainsKey("sv_maxclients"))
            {
                AddDetail("Server Slots", command.KeyValuePairs["sv_maxclients"]);
            }

            if (command.KeyValuePairs.ContainsKey("sv_hostname"))
            {
                AddDetail("Server Name", command.KeyValuePairs["sv_hostname"]);
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
