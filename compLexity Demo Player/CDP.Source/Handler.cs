using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.Source
{
    public class Handler : Core.DemoHandler
    {
        public class RegisteredFrame
        {
            public FrameIds Id { get; private set; }
            public FrameIds_Protocol36 Id_Protocol36 { get; private set; }
            public Type Type { get; private set; }

            public RegisteredFrame(Type type)
            {
                Type = type;
                Frame instance = CreateInstance();
                Id = instance.Id;
                Id_Protocol36 = instance.Id_Protocol36;
            }

            public Frame CreateInstance()
            {
                return (Frame)Activator.CreateInstance(Type);
            }
        }

        public class RegisteredMessage
        {
            public MessageIds Id { get; private set; }
            public MessageIds_Protocol36 Id_Protocol36 { get; private set; }
            public Type Type { get; private set; }

            public RegisteredMessage(Type type)
            {
                Type = type;
                Message instance = CreateInstance();
                Id = instance.Id;
                Id_Protocol36 = instance.Id_Protocol36;
            }

            public Message CreateInstance()
            {
                return (Message)Activator.CreateInstance(Type);
            }
        }

        public override string FullName
        {
            get { return "Source Engine"; }
        }

        public override string Name
        {
            get { return "source"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dem" }; }
        }

        public override Core.DemoHandler.PlayerColumn[] PlayerColumns
        {
            get { throw new NotImplementedException(); }
        }

        public override Core.Setting[] Settings
        {
            get { return new Core.Setting[] { }; }
        }

        public override System.Windows.Controls.UserControl SettingsView
        {
            get { throw new NotImplementedException(); }
        }

        private readonly byte[] magic = { 0x48, 0x4C, 0x32, 0x44, 0x45, 0x4D, 0x4F }; // HL2DEMO
        private readonly List<RegisteredFrame> registeredFrames = new List<RegisteredFrame>();
        private readonly List<RegisteredMessage> registeredMessages = new List<RegisteredMessage>();

        public Handler()
        {
            RegisterMessages();
        }

        protected virtual void RegisterMessages()
        {
            // Register frames.
            RegisterFrame<Frames.ConsoleCommand>();
            RegisterFrame<Frames.DataTables>();
            RegisterFrame<Frames.Signon>();
            RegisterFrame<Frames.Packet>();
            RegisterFrame<Frames.Stop>();
            RegisterFrame<Frames.StringTables>();
            RegisterFrame<Frames.Synctick>();
            RegisterFrame<Frames.UserCommand>();

            // Register messages.
            RegisterMessage<Messages.Customization>();
            RegisterMessage<Messages.NetSetConVar>();
            RegisterMessage<Messages.NetSignonState>();
            RegisterMessage<Messages.NetStringCmd>();
            RegisterMessage<Messages.NetTick>();
            RegisterMessage<Messages.Nop>();
            RegisterMessage<Messages.SvcBspDecal>();
            RegisterMessage<Messages.SvcClassInfo>();
            RegisterMessage<Messages.SvcCreateStringTable>();
            RegisterMessage<Messages.SvcCrosshairAngle>();
            RegisterMessage<Messages.SvcEntityMessage>();
            RegisterMessage<Messages.SvcFixAngle>();
            RegisterMessage<Messages.SvcGameEvent>();
            RegisterMessage<Messages.SvcGameEventList>();
            RegisterMessage<Messages.SvcPacketEntities>();
            RegisterMessage<Messages.SvcPrefetch>();
            RegisterMessage<Messages.SvcPrint>();
            RegisterMessage<Messages.SvcServerInfo>();
            RegisterMessage<Messages.SvcSetView>();
            RegisterMessage<Messages.SvcSounds>();
            RegisterMessage<Messages.SvcTempEntities>();
            RegisterMessage<Messages.SvcUpdateStringTable>();
            RegisterMessage<Messages.SvcUserMessage>();
            RegisterMessage<Messages.SvcVoiceData>();
            RegisterMessage<Messages.SvcVoiceInit>();
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream)
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
            throw new NotImplementedException();
        }

        public override System.Windows.Controls.UserControl CreateAnalysisView()
        {
            throw new NotImplementedException();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(CDP.Core.Demo demo)
        {
            throw new NotImplementedException();
        }

        public Frame CreateFrame(byte id, int networkProtocol)
        {
            Func<RegisteredFrame, bool> predicate = null;

            if (networkProtocol >= 36)
            {
                predicate = m => id == (byte)m.Id_Protocol36;
            }
            else
            {
                predicate = m => id == (byte)m.Id;
            }

            RegisteredFrame frame = registeredFrames.FirstOrDefault(predicate);

            if (frame == null)
            {
                return null;
            }

            return frame.CreateInstance();
        }

        public Message CreateMessage(byte id, int networkProtocol)
        {
            Func<RegisteredMessage, bool> predicate = null;

            if (networkProtocol >= 36)
            {
                predicate = m => id == (byte)m.Id_Protocol36; 
            }
            else
            {
                predicate = m => id == (byte)m.Id;
            }

            RegisteredMessage message = registeredMessages.FirstOrDefault(predicate);

            if (message == null)
            {
                return null;
            }

            return message.CreateInstance();
        }

        private void RegisterFrame<T>() where T : Frame
        {
            RegisteredFrame frame = new RegisteredFrame(typeof(T));

            // Make sure this ID isn't already registered by another message type.
            if (registeredFrames.FirstOrDefault(f => f.Id == frame.Id) != null || registeredFrames.FirstOrDefault(f => f.Id_Protocol36 == frame.Id_Protocol36) != null)
            {
                throw new ApplicationException("Duplicate frame ID.");
            }

            registeredFrames.Add(frame);
        }

        protected void RegisterMessage<T>() where T : Message
        {
            RegisteredMessage message = registeredMessages.FirstOrDefault(m => m.Type == typeof(T));

            // Remove if the message is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (message != null)
            {
                registeredMessages.Remove(message);
            }

            message = new RegisteredMessage(typeof(T));

            // Make sure this ID isn't already registered by another message type.
            if (registeredMessages.FirstOrDefault(m => m.Id == message.Id) != null || registeredMessages.FirstOrDefault(m => m.Id_Protocol36 == message.Id_Protocol36) != null)
            {
                throw new ApplicationException("Duplicate message ID.");
            }

            registeredMessages.Add(message);
        }
    }
}
