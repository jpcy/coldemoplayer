using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Source
{
    public abstract class Frame
    {
        public abstract FrameIds Id { get; }
        public abstract FrameIds_Protocol36 Id_Protocol36 { get; }
        public int Tick { get; private set; }
        public byte Unknown1 { get; private set; }

        public virtual bool HasMessages
        {
            get { return false; }
        }

        public virtual bool CanSkip
        {
            get { return true; }
        }

        public Demo Demo { protected get; set; }

        public void ReadHeader(FastFileStream stream)
        {
            if ((Demo.NetworkProtocol >= 36 && Id_Protocol36 == FrameIds_Protocol36.Stop) || 
                (Demo.NetworkProtocol < 36 && Id == FrameIds.Stop))
            {
                // Last byte of Stop tick isn't written. Weird.
                Tick = stream.ReadByte() << 32 + stream.ReadByte() << 16 + stream.ReadByte() << 8;
            }
            else
            {
                Tick = stream.ReadInt();

                if (Demo.NetworkProtocol >= 36)
                {
                    Unknown1 = stream.ReadByte();
                }
            }
        }

        public void WriteHeader(FastFileStream stream)
        {
            throw new NotImplementedException();
        }

        public abstract void Skip(FastFileStream stream);
        public abstract void Read(FastFileStream stream);
        public abstract void Write(FastFileStream stream);
        public abstract void Log(StreamWriter log);
    }

    public enum FrameIds : byte
    {
        Signon = 1,
        Packet,
        Synctick,
        ConsoleCommand,
        UserCommand,
        DataTables,
        Stop,
        StringTables, // For network protocols 14 and 15.
    }

    public enum FrameIds_Protocol36 : byte
    {
        Signon = 1,
        Packet,
        Synctick,
        ConsoleCommand,
        UserCommand,
        DataTables,
        Stop,
        CustomData,
        StringTables,
    }
}
