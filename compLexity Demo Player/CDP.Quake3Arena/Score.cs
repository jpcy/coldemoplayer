using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Quake3Arena
{
    internal class ClientScore
    {
        public int Client { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }
        public int Time { get; set; }
        public int ScoreFlags { get; set; }
        public int Powerups { get; set; }

        // New in protocol 48.
        public int Accuracy { get; set; }
        public int ImpressiveCount { get; set; }
        public int ExcellentCount { get; set; }
        public int GuantletCount { get; set; }
        public int DefendCount { get; set; }
        public int AssistCount { get; set; }
        public int Captures { get; set; }
        public int Perfect { get; set; } // qboolean
    }
}
