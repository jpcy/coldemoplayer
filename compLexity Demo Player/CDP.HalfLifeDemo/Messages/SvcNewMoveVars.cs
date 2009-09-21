using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcNewMoveVars : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_newmovevars; }
        }

        public override string Name
        {
            get { return "svc_newmovevars"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public float Gravity { get; set; }
        public float StopSpeed { get; set; }
        public float MaxSpeed { get; set; }
        public float SpectatorMaxSpeed { get; set; }
        public float Accelerate { get; set; }
        public float AirAccelerate { get; set; }
        public float WaterAccelerate { get; set; }
        public float Friction { get; set; }
        public float EdgeFriction { get; set; }
        public float WaterFriction { get; set; }
        public float EntityGravity { get; set; }
        public float Bounce { get; set; }
        public float StepSize { get; set; }
        public float MaxVelocity { get; set; }
        public float ZMax { get; set; }
        public float WaveHeight { get; set; }
        public bool Footsteps { get; set; }
        public float RollAngle { get; set; }
        public float RollSpeed { get; set; }
        public float SkyColourRed { get; set; }
        public float SkyColourGreen { get; set; }
        public float SkyColourBlue { get; set; }
        public Core.Vector SkyVector { get; set; }
        public string SkyName { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(97);
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Gravity = buffer.ReadFloat();
            StopSpeed = buffer.ReadFloat();
            MaxSpeed = buffer.ReadFloat();
            SpectatorMaxSpeed = buffer.ReadFloat();
            Accelerate = buffer.ReadFloat();
            AirAccelerate = buffer.ReadFloat();
            WaterAccelerate = buffer.ReadFloat();
            Friction = buffer.ReadFloat();
            EdgeFriction = buffer.ReadFloat();
            WaterFriction = buffer.ReadFloat();
            EntityGravity = buffer.ReadFloat();
            Bounce = buffer.ReadFloat();
            StepSize = buffer.ReadFloat();
            MaxVelocity = buffer.ReadFloat();
            ZMax = buffer.ReadFloat();
            WaveHeight = buffer.ReadFloat();
            Footsteps = buffer.ReadByte() == 1;
            RollAngle = buffer.ReadFloat();
            RollSpeed = buffer.ReadFloat();
            SkyColourRed = buffer.ReadFloat();
            SkyColourGreen = buffer.ReadFloat();
            SkyColourBlue = buffer.ReadFloat();
            SkyVector = new Core.Vector();
            SkyVector.X = buffer.ReadFloat();
            SkyVector.Y = buffer.ReadFloat();
            SkyVector.Z = buffer.ReadFloat();
            SkyName = buffer.ReadString();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteFloat(Gravity);
            buffer.WriteFloat(StopSpeed);
            buffer.WriteFloat(MaxSpeed);
            buffer.WriteFloat(SpectatorMaxSpeed);
            buffer.WriteFloat(Accelerate);
            buffer.WriteFloat(AirAccelerate);
            buffer.WriteFloat(WaterAccelerate);
            buffer.WriteFloat(Friction);
            buffer.WriteFloat(EdgeFriction);
            buffer.WriteFloat(WaterFriction);
            buffer.WriteFloat(EntityGravity);
            buffer.WriteFloat(Bounce);
            buffer.WriteFloat(StepSize);
            buffer.WriteFloat(MaxVelocity);
            buffer.WriteFloat(ZMax);
            buffer.WriteFloat(WaveHeight);
            buffer.WriteByte((byte)(Footsteps ? 1 : 0));
            buffer.WriteFloat(RollAngle);
            buffer.WriteFloat(RollSpeed);
            buffer.WriteFloat(SkyColourRed);
            buffer.WriteFloat(SkyColourGreen);
            buffer.WriteFloat(SkyColourBlue);
            buffer.WriteFloat(SkyVector.X);
            buffer.WriteFloat(SkyVector.Y);
            buffer.WriteFloat(SkyVector.Z);
            buffer.WriteString(SkyName);
            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Gravity: {0}", Gravity);
            log.WriteLine("StopSpeed: {0}", StopSpeed);
            log.WriteLine("MaxSpeed: {0}", MaxSpeed);
            log.WriteLine("SpectatorMaxSpeed: {0}", SpectatorMaxSpeed);
            log.WriteLine("Accelerate: {0}", Accelerate);
            log.WriteLine("AirAccelerate: {0}", AirAccelerate);
            log.WriteLine("WaterAccelerate: {0}", WaterAccelerate);
            log.WriteLine("Friction: {0}", Friction);
            log.WriteLine("EdgeFriction: {0}", EdgeFriction);
            log.WriteLine("WaterFriction: {0}", WaterFriction);
            log.WriteLine("EntityGravity: {0}", EntityGravity);
            log.WriteLine("Bounce: {0}", Bounce);
            log.WriteLine("StepSize: {0}", StepSize);
            log.WriteLine("MaxVelocity: {0}", MaxVelocity);
            log.WriteLine("ZMax: {0}", ZMax);
            log.WriteLine("WaveHeight: {0}", WaveHeight);
            log.WriteLine("Footsteps: {0}", Footsteps);
            log.WriteLine("RollAngle: {0}", RollAngle);
            log.WriteLine("RollSpeed: {0}", RollSpeed);
            log.WriteLine("SkyColourRed: {0}", SkyColourRed);
            log.WriteLine("SkyColourGreen: {0}", SkyColourGreen);
            log.WriteLine("SkyColourBlue: {0}", SkyColourBlue);

            if (SkyVector != null)
            {
                log.WriteLine("SkyVector.X: {0}", SkyVector.X);
                log.WriteLine("SkyVector.Y: {0}", SkyVector.Y);
                log.WriteLine("SkyVector.Z: {0}", SkyVector.Z);
            }

            log.WriteLine("SkyName: {0}", SkyName);
        }
    }
}
