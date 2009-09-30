using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo
{
    public abstract class EngineMessage : IMessage
    {
        public abstract byte Id { get; }
        public abstract string Name { get; }
        public abstract bool CanSkipWhenWriting { get; }
        public Demo Demo
        {
            set { demo = value; }
        }

        protected Demo demo;

        public abstract void Skip(Core.BitReader buffer);
        public abstract void Read(Core.BitReader buffer);
        public abstract void Write(Core.BitWriter buffer);
        public abstract void Log(StreamWriter log);
    }

    public enum EngineMessageIds : byte
    {
        svc_nop = 1,
        svc_disconnect = 2,
        svc_event = 3,
        svc_version = 4,
        svc_setview = 5,
        svc_sound = 6,
        svc_time = 7,
        svc_print = 8,
        svc_stufftext = 9,
        svc_setangle = 10,
        svc_serverinfo = 11,
        svc_lightstyle = 12,
        svc_updateuserinfo = 13,
        svc_deltadescription = 14,
        svc_clientdata = 15,
        svc_stopsound = 16,
        svc_pings = 17,
        svc_particle = 18,
        //svc_damage = 19,
        svc_spawnstatic = 20,
        svc_event_reliable = 21,
        svc_spawnbaseline = 22,
        svc_tempentity = 23,
        svc_setpause = 24,
        svc_signonnum = 25,
        svc_centerprint = 26,
        //svc_killedmonster = 27,
        //svc_foundsecret = 28,
        svc_spawnstaticsound = 29,
        svc_intermission = 30,
        svc_finale = 31,
        svc_cdtrack = 32,
        //svc_restore = 33, // TEST ME!!! something to do with loading/saving
        //svc_cutscene = 34,
        svc_weaponanim = 35,
        //svc_decalname = 36,
        svc_roomtype = 37,
        svc_addangle = 38,
        svc_newusermsg = 39,
        svc_packetentities = 40,
        svc_deltapacketentities = 41,
        svc_choke = 42,
        svc_resourcelist = 43,
        svc_newmovevars = 44,
        svc_resourcerequest = 45,
        svc_customization = 46,
        svc_crosshairangle = 47,
        svc_soundfade = 48,
        svc_filetxferfailed = 49,
        svc_hltv = 50,
        svc_director = 51,
        svc_voiceinit = 52,
        svc_voicedata = 53,
        svc_sendextrainfo = 54,
        svc_timescale = 55,
        svc_resourcelocation = 56,
        svc_sendcvarvalue = 57,
        svc_sendcvarvalue2 = 58
    }
}
