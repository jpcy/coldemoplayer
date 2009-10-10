using System;

namespace CDP.HalfLife.Analysis.Scoreboard
{
    public class Team : ICloneable
    {
        public string Name { get; private set; }
        public int Score { get; set; }

        public Team(string name, int score)
        {
            Name = name;
            Score = score;
        }

        public object Clone()
        {
            return new Team(Name, Score);
        }
    }
}
