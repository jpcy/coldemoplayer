using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
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
            MeansOfDeath _mod = (MeansOfDeath)mod;

            // Minor conversion. Only really necessary for grappling hook mods that use MOD_GRAPPLE.
            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                if (_mod == MeansOfDeath.MOD_NAIL)
                {
                    _mod = (MeansOfDeath)MeansOfDeath_Protocol43and45.MOD_GRAPPLE;
                }
            }

            string message = null;

            switch (_mod)
            {
                case MeansOfDeath.MOD_SUICIDE:
                    message = "suicides";
                    break;
                case MeansOfDeath.MOD_FALLING:
                    message = "cratered";
                    break;
                case MeansOfDeath.MOD_CRUSH:
                    message = "was squished";
                    break;
                case MeansOfDeath.MOD_WATER:
                    message = "sank like a rock";
                    break;
                case MeansOfDeath.MOD_SLIME:
                    message = "melted";
                    break;
                case MeansOfDeath.MOD_LAVA:
                    message = "does a back flip into the lava";
                    break;
                case MeansOfDeath.MOD_TARGET_LASER:
                    message = "saw the light";
                    break;
                case MeansOfDeath.MOD_TRIGGER_HURT:
                    message = "was in the wrong place";
                    break;
            }

            if (killerIndex == victimIndex)
            {
                // TODO: get correct gender.
                Genders gender = Genders.Male;
                string genderIdentifier = CalculateGenderIdentifier(gender);

                switch (_mod)
                {
                    case MeansOfDeath.MOD_KAMIKAZE:
                        message = "goes out with a bang";
                        break;

                    case MeansOfDeath.MOD_GRENADE_SPLASH:
                        message = string.Format("tripped on {0} own grenade", genderIdentifier);
                        break;

                    case MeansOfDeath.MOD_ROCKET_SPLASH:
                        message = string.Format("blew {0}self up", genderIdentifier);
                        break;

                    case MeansOfDeath.MOD_PLASMA_SPLASH:
                        message = string.Format("melted {0}self", genderIdentifier);
                        break;

                    case MeansOfDeath.MOD_BFG_SPLASH:
                        message = "should have used a smaller gun";
                        break;

                    case MeansOfDeath.MOD_PROXIMITY_MINE:
                        message = string.Format("found {0} prox mine", gender == Genders.Neutral ? "it's" : genderIdentifier);
                        break;

                    default:
                        message = string.Format("killed {0}self", genderIdentifier);
                        break;
                }
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

            switch (_mod) 
            {
	            case MeansOfDeath.MOD_GRAPPLE:
		            message = "was caught by";
		            break;
	            case MeansOfDeath.MOD_GAUNTLET:
		            message = "was pummeled by";
		            break;
	            case MeansOfDeath.MOD_MACHINEGUN:
		            message = "was machinegunned by";
		            break;
	            case MeansOfDeath.MOD_SHOTGUN:
		            message = "was gunned down by";
		            break;
	            case MeansOfDeath.MOD_GRENADE:
		            message = "ate";
		            message2 = "'s grenade";
		            break;
	            case MeansOfDeath.MOD_GRENADE_SPLASH:
		            message = "was shredded by";
		            message2 = "'s shrapnel";
		            break;
	            case MeansOfDeath.MOD_ROCKET:
		            message = "ate";
		            message2 = "'s rocket";
		            break;
	            case MeansOfDeath.MOD_ROCKET_SPLASH:
		            message = "almost dodged";
		            message2 = "'s rocket";
		            break;
	            case MeansOfDeath.MOD_PLASMA:
		            message = "was melted by";
		            message2 = "'s plasmagun";
		            break;
	            case MeansOfDeath.MOD_PLASMA_SPLASH:
		            message = "was melted by";
		            message2 = "'s plasmagun";
		            break;
	            case MeansOfDeath.MOD_RAILGUN:
		            message = "was railed by";
		            break;
	            case MeansOfDeath.MOD_LIGHTNING:
		            message = "was electrocuted by";
		            break;
	            case MeansOfDeath.MOD_BFG:
	            case MeansOfDeath.MOD_BFG_SPLASH:
		            message = "was blasted by";
		            message2 = "'s BFG";
		            break;
	            case MeansOfDeath.MOD_NAIL:
		            message = "was nailed by";
		            break;
	            case MeansOfDeath.MOD_CHAINGUN:
		            message = "got lead poisoning from";
		            message2 = "'s Chaingun";
		            break;
	            case MeansOfDeath.MOD_PROXIMITY_MINE:
		            message = "was too close to";
		            message2 = "'s Prox Mine";
		            break;
	            case MeansOfDeath.MOD_KAMIKAZE:
		            message = "falls to";
		            message2 = "'s Kamikaze blast";
		            break;
	            case MeansOfDeath.MOD_JUICED:
		            message = "was juiced by";
		            break;
	            case MeansOfDeath.MOD_TELEFRAG:
		            message = "tried to invade";
		            message2 = "'s personal space";
		            break;
	            default:
		            message = "was killed by";
		            break;
	        }

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
            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                if ((EntityTypes_Protocol43and45)eType < EntityTypes_Protocol43and45.ET_EVENTS)
                {
                    return false;
                }

                return (EntityEvents_Protocol43and45)(eType - (uint)EntityTypes_Protocol43and45.ET_EVENTS) == EntityEvents_Protocol43and45.EV_OBITUARY;
            }
            else
            {
                if ((EntityTypes)eType < EntityTypes.ET_EVENTS)
                {
                    return false;
                }

                return (EntityEvents)(eType - (uint)EntityTypes.ET_EVENTS) == EntityEvents.EV_OBITUARY;
            }
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