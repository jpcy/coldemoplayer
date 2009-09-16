using System;

namespace CDP.HalfLifeDemo.Analysis
{
    public class Player : ICloneable
    {
        public byte Slot { get; private set; }
        public string Name { get; set; }
        public string TeamName { get; set; }
        public int Frags { get; set; }
        public int Deaths { get; set; }
        public bool IsConnected { get; set; }

        public Player(byte slot)
        {
            Slot = slot;
            IsConnected = true;
            TeamName = "SPECTATOR";
        }

        public virtual object Clone()
        {
            return new Player(Slot)
            {
                Name = Name,
                TeamName = TeamName,
                Frags = Frags,
                Deaths = Deaths
            };
        }
    }
}
