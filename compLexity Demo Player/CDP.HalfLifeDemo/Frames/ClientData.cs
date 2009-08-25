using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        protected override void ReadContent(BinaryReader br)
        {
            Origin.X = br.ReadSingle();
            Origin.Y = br.ReadSingle();
            Origin.Z = br.ReadSingle();
            ViewAngles.X = br.ReadSingle();
            ViewAngles.Y = br.ReadSingle();
            ViewAngles.Z = br.ReadSingle();
            WeaponBitmask = br.ReadUInt32();
            Fov = br.ReadSingle();
        }

        protected override byte[] WriteContent()
        {
            // TODO
            return null;
        }
    }
}
