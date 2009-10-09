using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    // HL SDK cdll_int.h
    /*
        typedef struct client_data_s
        {
            // fields that cannot be modified  (ie. have no effect if changed)
            vec3_t origin;

            // fields that can be changed by the cldll
            vec3_t viewangles;
            int		iWeaponBits;
            float	fov;	// field of view
        } client_data_t;
    */
    public class ClientData : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.ClientData; }
        }

        public Core.Vector Origin { get; set; }
        public Core.Vector ViewAngles { get; set; }
        public uint WeaponBitmask { get; set; }
        public float Fov { get; set; }

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(32, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            Origin = new Core.Vector();
            Origin.X = stream.ReadFloat();
            Origin.Y = stream.ReadFloat();
            Origin.Z = stream.ReadFloat();
            ViewAngles = new Core.Vector();
            ViewAngles.X = stream.ReadFloat();
            ViewAngles.Y = stream.ReadFloat();
            ViewAngles.Z = stream.ReadFloat();
            WeaponBitmask = stream.ReadUInt();
            Fov = stream.ReadFloat();
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteFloat(Origin.X);
            stream.WriteFloat(Origin.Y);
            stream.WriteFloat(Origin.Z);
            stream.WriteFloat(ViewAngles.X);
            stream.WriteFloat(ViewAngles.Y);
            stream.WriteFloat(ViewAngles.Z);
            stream.WriteUInt(WeaponBitmask);
            stream.WriteFloat(Fov);
        }
    }
}
