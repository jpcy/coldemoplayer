using System;
using System.Linq;
using System.Collections.Generic;

namespace CDP.HalfLifeDemo.Analysis.Scoreboard
{
    public class Round
    {
        public int Number { get; private set; }
        public float Timestamp { get; private set; }
        public List<Team> Teams { get; private set; }
        public List<Player> Players { get; private set; }

        public Round(int number, float timestamp)
        {
            Number = number;
            Timestamp = timestamp;
            Teams = new List<Team>();
            Players = new List<Player>();
        }
    }
}
