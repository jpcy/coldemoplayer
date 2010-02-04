using System;
using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using CDP.Core.Extensions;

namespace CDP.IdTech3
{
    public class Plugin : Core.Plugin
    {
        private class RegisteredCommand
        {
            public CommandIds Id { get; private set; }
            public Type Type { get; private set; }

            public RegisteredCommand(Type type)
            {
                Type = type;
                Command instance = CreateInstance();
                Id = instance.Id;

                if (instance.IsSubCommand && instance.ContainsSubCommands)
                {
                    throw new ApplicationException("A command cannot contain subcommands and be a subcommand as well.");
                }

                if (instance.HasFooter && !instance.ContainsSubCommands)
                {
                    throw new ApplicationException("A command can only have a footer if it contains subcommands.");
                }
            }

            public Command CreateInstance()
            {
                return (Command)Activator.CreateInstance(Type);
            }
        }

        public override string FullName
        {
            get { return "id Tech 3"; }
        }

        public override string Name
        {
            get { return "idtech3"; }
        }

        public override uint Priority
        {
            get { return 0; }
        }

        public override string[] FileExtensions
        {
            get { return new string[] { "dm3", "dm_48", "dm_66", "dm_67", "dm_68", "dm_73" }; }
        }

        public override PlayerColumn[] PlayerColumns
        {
            get
            {
                return new PlayerColumn[]
                {
                    new PlayerColumn(Strings.PlayerNameColumnHeader, "Name"),
                    new PlayerColumn(Strings.PlayerHeadModelColumnHeader, "HeadModel"),
                    new PlayerColumn(Strings.PlayerModelColumnHeader, "Model")
                };
            }
        }

        public override Core.Setting[] Settings
        {
            get { return new Core.Setting[] { }; }
        }

        private readonly List<RegisteredCommand> registeredCommands = new List<RegisteredCommand>();

        public Plugin()
        {
            RegisterCommand<Commands.SvcBaseline>();
            RegisterCommand<Commands.SvcConfigString>();
            RegisterCommand<Commands.SvcGameState>();
            RegisterCommand<Commands.SvcServerCommand>();
            RegisterCommand<Commands.SvcSnapshot>();
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (stream.Length < 8)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Begin); // Skip sequence number.
            int messageLength = stream.ReadInt();

            if (messageLength > Message.MAX_MSGLEN)
            {
                return false;
            }

            if (stream.BytesLeft < messageLength)
            {
                return false;
            }

            return true;
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            throw new NotImplementedException();
        }

        public override UserControl CreateAnalysisView()
        {
            throw new NotImplementedException();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            throw new NotImplementedException();
        }

        public override UserControl CreateSettingsView(Core.Demo demo)
        {
            throw new NotImplementedException();
        }

        public Command CreateCommand(CommandIds id)
        {
            RegisteredCommand command = registeredCommands.FirstOrDefault(c => id == c.Id);

            if (command == null)
            {
                return null;
            }

            return command.CreateInstance();
        }

        protected void RegisterCommand<T>() where T : Command
        {
            RegisteredCommand command = registeredCommands.FirstOrDefault(c => c.Type == typeof(T));

            // Remove if the command is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (command != null)
            {
                registeredCommands.Remove(command);
            }

            command = new RegisteredCommand(typeof(T));

            // Make sure this ID isn't already registered by another command type.
            if (registeredCommands.FirstOrDefault(c => c.Id == command.Id) != null)
            {
                throw new ApplicationException("Duplicate command ID.");
            }

            registeredCommands.Add(command);
        }

        /// <summary>
        /// Guess the protocol based on the filename extension. Once a svc_configstring command containing the protocol key/value pair is read, the precise protocol should be assigned to the property "Protocol".
        /// </summary>
        public Protocols GuessProtocol(string fileExtension)
        {
            if (fileExtension == "dm3")
            {
                // Could also be 45 or possibly 46 (patch was never released to the public?), but parsing is the same up until the the correct protocol can be read, so it's OK to assume it's 43.
                return Protocols.Protocol43;
            }
            else
            {
                int protocol = int.Parse(fileExtension.Replace("dm_", string.Empty));

                if (!Enum.IsDefined(typeof(Protocols), protocol))
                {
                    throw new ApplicationException("Unknown protocol \'{0}\'.".Args(protocol));
                }

                return (Protocols)protocol;
            }
        }

        /// <summary>
        /// Reads the game name from a demo.
        /// </summary>
        /// <param name="stream">The demo file stream.</param>
        /// <param name="fileExtension">The demo file extension.</param>
        /// <returns>The game name in lower case, or null if it cannot be read.</returns>
        /// <remarks>IsValidDemo helper method for plugins that derive from IdTech3.</remarks>
        protected string ReadGameName(Core.FastFileStreamBase stream, string fileExtension)
        {
            stream.Seek(4, SeekOrigin.Begin); // Skip sequence number.
            int messageLength = stream.ReadInt();

            if (messageLength > Message.MAX_MSGLEN)
            {
                return null;
            }

            if (stream.BytesLeft < messageLength)
            {
                return null;
            }

            Protocols protocol = GuessProtocol(fileExtension);
            BitReader reader = new BitReader(stream.ReadBytes(messageLength), protocol >= Protocols.Protocol66);

            if (protocol >= Protocols.Protocol48)
            {
                reader.ReadInt(); // Reliable ACK.
            }

            // First command should be svc_gamestate.
            if (reader.ReadByte() != (byte)CommandIds.svc_gamestate)
            {
                return null;
            }

            reader.ReadInt(); // Server command sequence.

            // First sub-command of svc_gamestate should be svc_configstring.
            if (reader.ReadByte() != (byte)CommandIds.svc_configstring)
            {
                return null;
            }

            // Read the first svc_configstring message. It should contain the game name.
            Commands.SvcConfigString configString = new Commands.SvcConfigString();
            configString.Read(reader);

            if (configString.KeyValuePairs == null)
            {
                return null;
            }

            string gameName = configString.KeyValuePairs["gamename"];

            if (string.IsNullOrEmpty(gameName))
            {
                return null;
            }

            return gameName.ToLower();
        }
    }
}
