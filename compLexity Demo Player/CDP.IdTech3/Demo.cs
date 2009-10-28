using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace CDP.IdTech3
{
    public class Demo : Core.Demo
    {
        public override string GameName
        {
            get { throw new NotImplementedException(); }
        }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override IList<Core.Demo.Detail> Details { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get { throw new NotImplementedException(); }
        }

        public override string MapImagesRelativePath
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanPlay
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanAnalyse
        {
            get { throw new NotImplementedException(); }
        }

        protected Handler handler;

        public override Core.DemoHandler Handler
        {
            get { return handler; }
            set { handler = (Handler)value; }
        }

        public int Protocol { get; private set; }

        private readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();

        public override void Load()
        {
            throw new NotImplementedException();
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
            }

            OnOperationComplete();
        }

        private CommandIds ReadCommandId(BitReader buffer, StreamWriter log)
        {
            CommandIds commandId = (CommandIds)buffer.ReadByte();
            log.WriteLine("\nCommand {0} [{1}]", commandId, (byte)commandId);
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

                bool writeToLog = commandsToLog.Contains(commandId);

                command.Demo = this;

                try
                {
                    command.Read(buffer);
                }
                catch (Exception)
                {
                    log.WriteLine("\n*** ERROR ***\n");
                    command.Log(log);
                    throw;
                }

                if (writeToLog)
                {
                    command.Log(log);
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
    }
}
