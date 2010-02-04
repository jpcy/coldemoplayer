using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDP.IdTech3;
using CDP.Core.Extensions;

namespace CDP.Quake3Arena
{
    internal class Scores
    {
        private readonly string scoresServerCommandMarker = "scores ";
        private readonly char separator = ' ';

        private int teamScores1;
        private int teamScores2;
        private ClientScore[] clientScores;
        private Protocols protocol;

        public Scores(Protocols protocol)
        {
            this.protocol = protocol;
        }

        public void Parse(string serverCommand)
        {
            if (string.IsNullOrEmpty(serverCommand))
            {
                throw new ArgumentException("Parameter cannot be null or empty", "serverCommand");
            }

            if (!serverCommand.StartsWith(scoresServerCommandMarker))
            {
                throw new ArgumentException("String does not start with \'{0}\'".Args(scoresServerCommandMarker), "serverCommand");
            }

            // Strip the server command marker.
            serverCommand = serverCommand.Remove(0, scoresServerCommandMarker.Length);

            // Split and parse each element as integers.
            string[] split = serverCommand.Split(separator);
            int[] values = split.Select(s => int.Parse(s)).ToArray();

            // Parse values.
            int nScores = values[0];
            teamScores1 = values[1];
            teamScores2 = values[2];

            if (nScores > 0)
            {
                clientScores = new ClientScore[nScores];
            }

            int vi = 3;

            for (int i = 0; i < nScores; i++)
            {
                clientScores[i] = new ClientScore();
                clientScores[i].Client = values[vi++];
                clientScores[i].Score = values[vi++];
                clientScores[i].Ping = values[vi++];
                clientScores[i].Time = values[vi++];
                clientScores[i].ScoreFlags = values[vi++];
                clientScores[i].Powerups = values[vi++];

                if (protocol >= Protocols.Protocol48)
                {
                    clientScores[i].Accuracy = values[vi++];
                    clientScores[i].ImpressiveCount = values[vi++];
                    clientScores[i].ExcellentCount = values[vi++];
                    clientScores[i].GuantletCount = values[vi++];
                    clientScores[i].DefendCount = values[vi++];
                    clientScores[i].AssistCount = values[vi++];
                    clientScores[i].Perfect = values[vi++];
                    clientScores[i].Captures = values[vi++];
                }
            }
        }

        public string Compose()
        {
            List<int> values = new List<int>();

            if (clientScores == null)
            {
                values.Add(0);
            }
            else
            {
                values.Add(clientScores.Length);
            }

            values.Add(teamScores1);
            values.Add(teamScores2);

            if (clientScores != null)
            {
                foreach (ClientScore clientScore in clientScores)
                {
                    values.Add(clientScore.Client);
                    values.Add(clientScore.Score);
                    values.Add(clientScore.Ping);
                    values.Add(clientScore.Time);
                    values.Add(clientScore.ScoreFlags);
                    values.Add(clientScore.Powerups);
                    values.Add(clientScore.Accuracy);
                    values.Add(clientScore.ImpressiveCount);
                    values.Add(clientScore.ExcellentCount);
                    values.Add(clientScore.GuantletCount);
                    values.Add(clientScore.DefendCount);
                    values.Add(clientScore.AssistCount);
                    values.Add(clientScore.Perfect);
                    values.Add(clientScore.Captures);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(scoresServerCommandMarker);

            for (int i = 0; i < values.Count; i++)
            {
                if (i != 0)
                {
                    sb.Append(" ");
                }

                sb.Append(values[i]);
            }

            return sb.ToString();
        }
    }
}
