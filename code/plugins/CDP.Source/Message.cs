using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CDP.Source
{
    public abstract class Message
    {
        public abstract MessageIds Id { get; }
        public abstract MessageIds_Protocol36 Id_Protocol36 { get; }
        public abstract string Name { get; }
        
        public virtual bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public Demo Demo { protected get; set; }
        public bool Remove { get; set; }
        public long Offset { get; set; }

        public abstract void Skip(Core.BitReader buffer);
        public abstract void Read(Core.BitReader buffer);
        public abstract void Write(Core.BitWriter buffer);
        public abstract void Log(StreamWriter log);
    }

    public enum MessageIds_Protocol36 : byte
    {
        Nop = 0,
        Customization = 2, // FIXME: does this exist in protocol 36 and higher?
        NET_SplitScreenUser = 3,
        NET_Tick = 4,
        NET_StringCmd = 5,
        NET_SetConVar = 6,
        NET_SignonState = 7,
        SVC_ServerInfo = 8, // no change
        SVC_SendTable = 9, // ???
        SVC_ClassInfo = 10, // no change
        SVC_SetPause = 11, // no change
        SVC_CreateStringTable = 12, // no change
        SVC_UpdateStringTable = 13, // no change
        SVC_VoiceInit = 14, // no change
        SVC_VoiceData = 15, // no change
        SVC_Print = 16,
        SVC_Sounds = 17, // no change
        SVC_SetView = 18, // no change
        SVC_FixAngle = 19, // no change
        SVC_CrosshairAngle = 20, // no change
        SVC_BSPDecal = 21, // no change
        SVC_SplitScreen = 22,
        SVC_UserMessage = 23, // no change
        SVC_EntityMessage = 24,
        SVC_GameEvent = 25, // no change
        SVC_PacketEntities = 26, // no change
        SVC_TempEntities = 27, // no change
        SVC_Prefetch = 28, // no change
        SVC_Menu = 29, // ???
        SVC_GameEventList = 30, // no change
        SVC_GetCvarValue = 31 // no change
    }

    public enum MessageIds : byte
    {
        Nop = 0,
        Customization = 2, // Only encountered in demo protocol 2 so far.
        NET_Tick = 3,
        NET_StringCmd = 4,
        NET_SetConVar = 5,
        NET_SignonState = 6,
        SVC_Print = 7,
        SVC_ServerInfo = 8,
        SVC_ClassInfo = 10,
        SVC_SetPause = 11,
        SVC_CreateStringTable = 12,
        SVC_UpdateStringTable = 13,
        SVC_VoiceInit = 14,
        SVC_VoiceData = 15,
        SVC_Sounds = 17,
        SVC_SetView = 18,
        SVC_FixAngle = 19,
        SVC_CrosshairAngle = 20,
        SVC_BSPDecal = 21,
        SVC_UserMessage = 23,
        SVC_EntityMessage = 24,
        SVC_GameEvent = 25,
        SVC_PacketEntities = 26,
        SVC_TempEntities = 27,
        SVC_Prefetch = 28,
        SVC_GameEventList = 30,
        SVC_GetCvarValue = 31
    }
}
