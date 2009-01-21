using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

namespace compLexity_Demo_Player
{
    public class HalfLifeGameState : NotifyPropertyChangedItem
    {
        public class Team : NotifyPropertyChangedItem
        {
            public Int32 Score { get; set; }

            public String Name
            {
                get
                {
                    return name;
                }
            }

            private String name;

            public Team(String teamName, Int32 score)
            {
                this.name = teamName;
                Score = score;
            }
        }

        public class Player : NotifyPropertyChangedItem, ICloneable
        {
            public String Name { get; set; }
            public String TeamName { get; set; }
            public Int32 Frags { get; set; }
            public Int32 Deaths { get; set; }

            public Byte Slot
            {
                get
                {
                    return slot;
                }
            }

            private Byte slot;

            public Player(Byte slot)
            {
                this.slot = slot;
            }

            public Object Clone()
            {
                return new Player(slot) { TeamName = TeamName, Name = Name, Frags = Frags, Deaths = Deaths };
            }
        }

        public class Round : NotifyPropertyChangedItem
        {
            public Int32 Number { get; set; }
            public Single Timestamp { get; set; }
            public ObservableCollection<Team> Teams
            {
                get
                {
                    return teams;
                }
            }
            public ObservableCollection<Player> Players
            {
                get
                {
                    return players;
                }
            }

            private ObservableCollection<Team> teams = new ObservableCollection<Team>();
            private ObservableCollection<Player> players = new ObservableCollection<Player>();

            public Round(Int32 number, Single timestamp)
            {
                Number = number;
                Timestamp = timestamp;
            }

            public Team FindTeam(String name)
            {
                return Common.FirstOrDefault<Team>(teams, t => t.Name == name);
            }

            public void SetTeamScore(String teamName, Int32 score)
            {
                // replace team score if it already exists
                // TeamScore message is sent at beginning of each round and when a team wins, e.g. "CT's Win!",
                // we want the end of round score

                Team t = FindTeam(teamName);

                if (t == null)
                {
                    t = new Team(teamName, score);
                    teams.Add(t);
                }
                else
                {
                    t.Score = score;
                }
            }

            public Player FindPlayer(Byte slot)
            {
                return Common.FirstOrDefault<Player>(players, p => p.Slot == slot);
            }

            public void SetPlayerScore(Byte slot, String name, Int32 frags, Int32 deaths)
            {
                Player p = FindPlayer(slot);

                // create a Player object if it doesn't exist
                if (p == null)
                {
                    p = new Player(slot);
                    players.Add(p);
                }

                p.Name = name;
                p.Frags = frags;
                p.Deaths = deaths;
            }

            public void SetPlayerTeam(Byte slot, String name, String team)
            {
                Player p = FindPlayer(slot);

                // create a Player object if it doesn't exist
                if (p == null)
                {
                    p = new Player(slot);
                    players.Add(p);
                }

                p.Name = name;
                p.TeamName = team;
            }
        }

        public ObservableCollection<Round> Rounds
        {
            get
            {
                return rounds;
            }
        }

        private ObservableCollection<Round> rounds = new ObservableCollection<Round>();

        public HalfLifeGameState()
        {
        }

        public Round AddRound(Single timestamp)
        {
            // get the current round
            Round currentRound = GetCurrentRound();

            // add the new round
            Round newRound = new Round(rounds.Count + 1, timestamp);
            rounds.Add(newRound);

            // carry over all connected player's player scores from the previous rounds to this new round
            if (currentRound != null)
            {
                // TODO: detect when a player is disconnected and don't carry over their score
                foreach (Player p in currentRound.Players)
                {
                    newRound.Players.Add((Player)p.Clone());
                }
            }

            return newRound;
        }

        public Round GetCurrentRound()
        {
            if (rounds.Count == 0)
            {
                return null; // only happens on first AddRound call, not need to check for null anywhere else
            }

            return (Round)rounds[rounds.Count - 1];
        }
    }
}
