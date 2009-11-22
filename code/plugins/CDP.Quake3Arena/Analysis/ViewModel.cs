using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using CDP.Core;
using CDP.Core.Extensions;
using CDP.IdTech3;
using CDP.IdTech3.Commands;

namespace CDP.Quake3Arena.Analysis
{
    public class ViewModel : Core.ViewModelBase
    {
        public FlowDocument GameLogDocument { get; private set; }
        private readonly Demo demo;
        private readonly Core.IFlowDocumentWriter gameLog = Core.ObjectCreator.Get<Core.IFlowDocumentWriter>();
        private readonly List<string> playerNames = new List<string>();
        private readonly string chatServerCommandMarker = "chat ";
        private readonly string printServerCommandMarker = "print ";
        private const char colourStringEscape = '^';

        // Out of order or duplicate server commands are ignored.
        private int serverCommandSequence = 0;

        private readonly SolidColorBrush[] stringColours =
        {
            Brushes.Black,
            Brushes.Red,
            Brushes.Green,
            Brushes.Yellow,
            Brushes.Blue,
            Brushes.Cyan,
            Brushes.Magenta,
            Brushes.White
        };

        public ViewModel(Demo demo)
        {
            this.demo = demo;
            demo.AddCommandCallback<SvcConfigString>(ConfigString);
            demo.AddCommandCallback<SvcServerCommand>(ServerCommand);
            demo.AddCommandCallback<SvcSnapshot>(Snapshot);
            GameLogDocument = new FlowDocument();
        }

        public override void OnNavigateComplete()
        {
            gameLog.Save(GameLogDocument);
            OnPropertyChanged("GameLogDocument");
        }

        /// <summary>
        /// Checks if a string contains at least one colour escape sequence.
        /// </summary>
        /// <returns>True if the string contains at least one colour escape sequence, otherwise false.</returns>
        private bool IsColourString(string s)
        {
            if (s.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] == colourStringEscape && s[i + 1] != colourStringEscape)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Writes a string to the game log that uses escaped sequences to indicate text colours.
        /// </summary>
        private void GameLogWriteColourString(string s)
        {
            if (IsColourString(s))
            {
                int i = 0;
                int colour = 7; // Default: white.
                int runStartIndex = 0; // Start index of the current run of a specific colour.

                while (true)
                {
                    // e.g. ^1
                    if (s[i] == colourStringEscape && i + 1 < s.Length && s[i + 1] != colourStringEscape)
                    {
                        // Write the run leading up to this colour change.
                        int runLength = i - runStartIndex;

                        if (runLength > 0)
                        {
                            gameLog.Write(s.Substring(runStartIndex, runLength), stringColours[colour]);
                        }

                        // Set the new colour and skip this escape sequence
                        colour = (s[i + 1] - '0') & 7;
                        i += 2;

                        // Start a new run.
                        runStartIndex = i;
                    }
                    else
                    {
                        i++;
                    }

                    if (i >= s.Length)
                    {
                        // End of string, write the current run up to the end.
                        if (runStartIndex < s.Length - 1)
                        {
                            gameLog.Write(s.Substring(runStartIndex), stringColours[colour]);
                        }

                        break;
                    }
                }
            }
            else
            {
                gameLog.Write(s, Brushes.White);
            }

            gameLog.WriteLine();
        }

        /// <summary>
        /// Calculate an obituary message for a player's death.
        /// </summary>
        /// <param name="mod">The player's means of death.</param>
        /// <param name="killerIndex">The player index of the killer.</param>
        /// <param name="victimIndex">The player index of the victim.</param>
        /// <returns>An obituary message.</returns>
        private string CalculateObituary(uint mod, uint killerIndex, uint victimIndex)
        {
            LookupTable_uint modTable = GameConstants.MeansOfDeath_Protocol48;

            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                // Convert mod to protocol 48.
                mod = GameConstants.MeansOfDeath_Protocol43.Convert(mod, modTable);
            }

            string message = null;

            if (mod == modTable["MOD_SUICIDE"])
                message = "suicides";
            else if (mod == modTable["MOD_FALLING"]) 
                message = "cratered";
            else if (mod == modTable["MOD_CRUSH"])
                message = "was squished";
            else if (mod == modTable["MOD_WATER"])
                message = "sank like a rock";
            else if (mod == modTable["MOD_SLIME"])
                message = "melted";
            else if (mod == modTable["MOD_LAVA"])
                message = "does a back flip into the lava";
            else if (mod == modTable["MOD_TARGET_LASER"])
                message = "saw the light";
            else if (mod == modTable["MOD_TRIGGER_HURT"])
                message = "was in the wrong place";

            if (killerIndex == victimIndex)
            {
                // TODO: get correct gender.
                Genders gender = Genders.Male;
                string genderIdentifier = CalculateGenderIdentifier(gender);

                if (mod == modTable["MOD_KAMIKAZE"])
                    message = "goes out with a bang";
                else if (mod == modTable["MOD_GRENADE_SPLASH"])
                    message = string.Format("tripped on {0} own grenade", genderIdentifier);
                else if (mod == modTable["MOD_ROCKET_SPLASH"])
                    message = string.Format("blew {0}self up", genderIdentifier);
                else if (mod == modTable["MOD_PLASMA_SPLASH"])
                    message = string.Format("melted {0}self", genderIdentifier);
                else if (mod == modTable["MOD_BFG_SPLASH"])
                    message = "should have used a smaller gun";
                else if (mod == modTable["MOD_PROXIMITY_MINE"])
                    message = string.Format("found {0} prox mine", gender == Genders.Neutral ? "it's" : genderIdentifier);
                else
                    message = string.Format("killed {0}self", genderIdentifier);
            }

            if (message != null)
            {
                return string.Format("{0} {1}.", playerNames[(int)victimIndex], message);
            }

            if (killerIndex >= playerNames.Count)
            {
                return string.Format("{0} died.", playerNames[(int)victimIndex]);
            }

            string message2 = string.Empty;

            if (mod == modTable["MOD_GRAPPLE"])
	            message = "was caught by";
            else if (mod == modTable["MOD_GAUNTLET"])
	            message = "was pummeled by";
            else if (mod == modTable["MOD_MACHINEGUN"])
	            message = "was machinegunned by";
            else if (mod == modTable["MOD_SHOTGUN"])
	            message = "was gunned down by";
            else if (mod == modTable["MOD_GRENADE"])
            {
	            message = "ate";
	            message2 = "'s grenade";
	        }
            else if (mod == modTable["MOD_GRENADE_SPLASH"])
            {
	            message = "was shredded by";
	            message2 = "'s shrapnel";
	        }
            else if (mod == modTable["MOD_ROCKET"])
            {
	            message = "ate";
	            message2 = "'s rocket";
	        }
            else if (mod == modTable["MOD_ROCKET_SPLASH"])
            {
	            message = "almost dodged";
	            message2 = "'s rocket";
	        }
            else if (mod == modTable["MOD_PLASMA"])
            {
	            message = "was melted by";
	            message2 = "'s plasmagun";
	        }
            else if (mod == modTable["MOD_PLASMA_SPLASH"])
            {
	            message = "was melted by";
	            message2 = "'s plasmagun";
	        }
            else if (mod == modTable["MOD_RAILGUN"])
            {
	            message = "was railed by";
	        }
            else if (mod == modTable["MOD_LIGHTNING"])
            {
	            message = "was electrocuted by";
	        }
            else if (mod == modTable["MOD_BFG"] || mod == modTable["MOD_BFG_SPLASH"])
            {
	            message = "was blasted by";
	            message2 = "'s BFG";
	        }
            else if (mod == modTable["MOD_NAIL"])
	            message = "was nailed by";
            else if (mod == modTable["MOD_CHAINGUN"])
            {
	            message = "got lead poisoning from";
	            message2 = "'s Chaingun";
	        }
            else if (mod == modTable["MOD_PROXIMITY_MINE"])
            {
	            message = "was too close to";
	            message2 = "'s Prox Mine";
	        }
            else if (mod == modTable["MOD_KAMIKAZE"])
            {
	            message = "falls to";
	            message2 = "'s Kamikaze blast";
	        }
            else if (mod == modTable["MOD_JUICED"])
	            message = "was juiced by";
            else if (mod == modTable["MOD_TELEFRAG"])
            {
	            message = "tried to invade";
	            message2 = "'s personal space";
	        }
	        else
		        message = "was killed by";

            // Set string colour back to default (white) after each player name.
            return string.Format("{0}^7 {1} {2}^7{3}.", playerNames[(int)victimIndex], message, playerNames[(int)killerIndex], message2);
        }

        /// <summary>
        /// Calculates a gender string identifier for a particular gender (e.g. "her" for female).
        /// </summary>
        private string CalculateGenderIdentifier(Genders gender)
        {
            if (gender == Genders.Male)
            {
                return "him";
            }
            else if (gender == Genders.Female)
            {
                return "her";
            }
            else if (gender == Genders.Neutral)
            {
                return "it";
            }

            throw new ApplicationException("Unknown gender.");
        }

        /// <summary>
        /// Write a server command to the game log, stripping the command marker and trimming leading and trailing quotes and newline characters.
        /// </summary>
        /// <param name="commandMarker"></param>
        /// <param name="command"></param>
        private void GameLogWriteServerCommand(string commandMarker, string command)
        {
            GameLogWriteColourString(command.Remove(0, commandMarker.Length).Trim('"', '\n'));
        }

        /// <summary>
        /// Is the specified entity eType an event, and if it is, is the event an obituary.
        /// </summary>
        /// <param name="eType">The entity eType.</param>
        private bool IsObituary(uint eType)
        {
            LookupTable_uint eTypesTable;
            LookupTable_uint entityEventsTable;

            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                eTypesTable = GameConstants.EntityTypes_Protocol43;
                entityEventsTable = GameConstants.EntityEvents_Protocol43;
            }
            else
            {
                eTypesTable = GameConstants.EntityTypes_Protocol48;
                entityEventsTable = GameConstants.EntityEvents_Protocol48;
            }

            if (eType < eTypesTable["ET_EVENTS"])
            {
                return false;
            }

            return (eType - eTypesTable["ET_EVENTS"] == entityEventsTable["EV_OBITUARY"]);
        }

        #region Command callbacks
        private void ConfigString(SvcConfigString command)
        {
            if (command.IsPlayer)
            {
                playerNames.Add(command.KeyValuePairs["n"]);
            }
        }

        private void ServerCommand(SvcServerCommand command)
        {
            if (command.Sequence > serverCommandSequence)
            {
                serverCommandSequence = command.Sequence;
            }
            else
            {
                return;
            }

            if (command.Command.StartsWith(chatServerCommandMarker))
            {
                GameLogWriteServerCommand(chatServerCommandMarker, command.Command);
            }
            else if (command.Command.StartsWith(printServerCommandMarker))
            {
                GameLogWriteServerCommand(printServerCommandMarker, command.Command);
            }
        }

        private void Snapshot(SvcSnapshot command)
        {
            foreach (Entity entity in command.Entities)
            {
                if (entity["eType"] == null)
                {
                    continue;
                }

                if (IsObituary((uint)entity["eType"]))
                {
                    uint killerIndex = (uint?)entity["otherEntityNum2"] ?? 0;
                    uint victimIndex = (uint?)entity["otherEntityNum"] ?? 0;
                    GameLogWriteColourString(CalculateObituary((uint)entity["eventParm"], killerIndex, victimIndex));
                }
            }
        }
        #endregion
    }
}