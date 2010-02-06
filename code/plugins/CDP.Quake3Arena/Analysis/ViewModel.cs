using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using CDP.Core;
using CDP.Core.Extensions;
using CDP.IdTech3;
using CDP.IdTech3.Commands;

namespace CDP.Quake3Arena.Analysis
{
    public class ViewModel : ViewModelBase
    {
        public FlowDocument GameLogDocument { get; private set; }
        public FlowDocument ConsoleLogDocument { get; private set; }
        private readonly Demo demo;
        private readonly IFlowDocumentWriter gameLog = ObjectCreator.Get<IFlowDocumentWriter>();
        private readonly IFlowDocumentWriter consoleLog = ObjectCreator.Get<IFlowDocumentWriter>();
        private readonly List<string> playerNames = new List<string>();
        private readonly string chatServerCommandMarker = "chat ";
        private readonly string printServerCommandMarker = "print ";
        private const char colourStringEscape = '^';

        // Out of order or duplicate server commands are ignored.
        private int serverCommandSequence = 0;

        private readonly SolidColorBrush[] stringColours =
        {
            Brushes.White, // Should be black, but due to colour clashes the background is set to black.
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
            ConsoleLogDocument = new FlowDocument();
        }

        public override void OnNavigateComplete()
        {
            gameLog.Save(GameLogDocument);
            consoleLog.Save(ConsoleLogDocument);
            OnPropertyChanged("GameLogDocument");
            OnPropertyChanged("ConsoleLogDocument");
        }

        #region Document writing
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
        /// Writes a string that uses escaped sequences to indicate text colours to a flow document.
        /// </summary>
        /// <param name="document">The document to write to.</param>
        /// <param name="s">The string to write.</param>
        private void WriteColouredString(IFlowDocumentWriter document, string s)
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
                            document.Write(s.Substring(runStartIndex, runLength), stringColours[colour]);
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
                            document.Write(s.Substring(runStartIndex), stringColours[colour]);
                        }

                        break;
                    }
                }
            }
            else
            {
                document.Write(s, Brushes.White);
            }

            document.WriteLine();
        }

        /// <summary>
        /// Write a server command to a flow document, stripping the command marker and trimming leading and trailing quotes and newline characters.
        /// </summary>
        /// <param name="document">The document to write to.</param>
        /// <param name="commandMarker">The command marker to strip from the command (e.g. 'chat ').</param>
        /// <param name="command">The command.</param>
        private void WriteServerCommand(IFlowDocumentWriter document, string commandMarker, string command)
        {
            WriteColouredString(document, command.Remove(0, commandMarker.Length).Trim('"', '\n'));
        }
        #endregion

        #region Player obituary
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
                    message = "tripped on {0} own grenade".Args(genderIdentifier);
                else if (mod == modTable["MOD_ROCKET_SPLASH"])
                    message = "blew {0}self up".Args(genderIdentifier);
                else if (mod == modTable["MOD_PLASMA_SPLASH"])
                    message = "melted {0}self".Args(genderIdentifier);
                else if (mod == modTable["MOD_BFG_SPLASH"])
                    message = "should have used a smaller gun";
                else if (mod == modTable["MOD_PROXIMITY_MINE"])
                    message = "found {0} prox mine".Args(gender == Genders.Neutral ? "it's" : genderIdentifier);
                else
                    message = "killed {0}self".Args(genderIdentifier);
            }

            if (message != null)
            {
                return "{0} {1}.".Args(GetPlayerName(victimIndex), message);
            }

            if (killerIndex >= playerNames.Count)
            {
                return "{0} died.".Args(GetPlayerName(victimIndex));
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
            return "{0}^7 {1} {2}^7{3}.".Args(GetPlayerName(victimIndex), message, GetPlayerName(killerIndex), message2);
        }

        /// <summary>
        /// Calculate an simplified obituary message for a player's death.
        /// </summary>
        /// <param name="mod">The player's means of death.</param>
        /// <param name="killerIndex">The player index of the killer.</param>
        /// <param name="victimIndex">The player index of the victim.</param>
        /// <returns>An obituary message.</returns>
        private string CalculateSimpleObituary(uint mod, uint killerIndex, uint victimIndex)
        {
            LookupTable_uint modTable = GameConstants.MeansOfDeath_Protocol48;

            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                // Convert mod to protocol 48.
                mod = GameConstants.MeansOfDeath_Protocol43.Convert(mod, modTable);
            }

            string message = null;

            // Suicide against world.
            if (mod == modTable["MOD_SUICIDE"])
                message = "suicide";
            else if (mod == modTable["MOD_FALLING"])
                message = "cratered";
            else if (mod == modTable["MOD_CRUSH"])
                message = "crushed";
            else if (mod == modTable["MOD_WATER"])
                message = "drowned";
            else if (mod == modTable["MOD_SLIME"])
                message = "slime";
            else if (mod == modTable["MOD_LAVA"])
                message = "lava";
            else if (mod == modTable["MOD_TARGET_LASER"])
                message = "laser";
            else if (mod == modTable["MOD_TRIGGER_HURT"])
                message = "hurt";

            if (message != null)
            {
                return "{0} died ({1}).".Args(GetPlayerName(victimIndex), message);
            }

            // Suicide using own weapon.
            if (killerIndex == victimIndex)
            {
                // TODO: get correct gender.
                Genders gender = Genders.Male;
                string genderIdentifier = CalculateGenderIdentifier(gender);

                if (mod == modTable["MOD_KAMIKAZE"])
                    message = "kamikaze";
                else if (mod == modTable["MOD_GRENADE_SPLASH"])
                    message = "grenade".Args(genderIdentifier);
                else if (mod == modTable["MOD_ROCKET_SPLASH"])
                    message = "rocket".Args(genderIdentifier);
                else if (mod == modTable["MOD_PLASMA_SPLASH"])
                    message = "plasma".Args(genderIdentifier);
                else if (mod == modTable["MOD_BFG_SPLASH"])
                    message = "bfg";
                else if (mod == modTable["MOD_PROXIMITY_MINE"])
                    message = "proximity mine";
                else
                    message = "unknown";
            }

            if (message != null)
            {
                return "{0} suicided ({1}).".Args(GetPlayerName(victimIndex), message);
            }

            // Died (unknown killer).
            if (killerIndex >= playerNames.Count)
            {
                return "{0} died.".Args(GetPlayerName(victimIndex));
            }

            // Killed.
            string weapon = null;

            if (mod == modTable["MOD_GRAPPLE"])
                weapon = "Grappling Hook";
            else if (mod == modTable["MOD_GAUNTLET"])
                weapon = "Gauntlet";
            else if (mod == modTable["MOD_MACHINEGUN"])
                weapon = "Machinegun";
            else if (mod == modTable["MOD_SHOTGUN"])
                weapon = "Shotgun";
            else if (mod == modTable["MOD_GRENADE"])
                weapon = "Grenade Launcher";
            else if (mod == modTable["MOD_GRENADE_SPLASH"])
                message = "Grenade Launcher (splash damage)";
            else if (mod == modTable["MOD_ROCKET"])
                weapon = "Rocket Launcher";
            else if (mod == modTable["MOD_ROCKET_SPLASH"])
                weapon = "Rocket Launcher (splash damage)";
            else if (mod == modTable["MOD_PLASMA"])
                weapon = "Plasmagun";
            else if (mod == modTable["MOD_PLASMA_SPLASH"])
                weapon = "Plasmagun (splash damage)";
            else if (mod == modTable["MOD_RAILGUN"])
                weapon = "Railgun";
            else if (mod == modTable["MOD_LIGHTNING"])
                weapon = "Lightning Gun";
            else if (mod == modTable["MOD_BFG"] || mod == modTable["MOD_BFG_SPLASH"])
                weapon = "BFG";
            else if (mod == modTable["MOD_NAIL"])
                weapon = "Nailgun";
            else if (mod == modTable["MOD_CHAINGUN"])
                weapon = "Chaingun";
            else if (mod == modTable["MOD_PROXIMITY_MINE"])
                weapon = "Prox Mine";
            else if (mod == modTable["MOD_KAMIKAZE"])
                weapon = "Kamikaze blast";
            else if (mod == modTable["MOD_JUICED"])
                weapon = "Juice?";
            else if (mod == modTable["MOD_TELEFRAG"])
                weapon = "Telefrag";
            else
                message = "Unknown Weapon";

            // Set string colour back to default (white) after each player name.
            return "{0}^7 killed {1}^7 with {2}.".Args(GetPlayerName(killerIndex), GetPlayerName(victimIndex), weapon);
        }

        /// <summary>
        /// Gets a player's name.
        /// </summary>
        /// <param name="index">The player's index number.</param>
        /// <returns>The player's name if the index is valid, otherwise "*unknown*".</returns>
        private string GetPlayerName(uint index)
        {
            if (index >= playerNames.Count)
                return "*unknown*";

            return playerNames[(int)index];
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
        #endregion

        /// <summary>
        /// Is the specified entity type an event, and does the event match the specified event name.
        /// </summary>
        /// <param name="eType">The entity type.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if the entity type is an event and the event matches the specified event name, otherwise false.</returns>
        private bool IsEvent(uint eType, string eventName)
        {
            LookupTable_uint eTypesTable;
            LookupTable_uint entityEventsTable;

            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                eTypesTable = GameConstants.EntityTypes_Protocol43;
                entityEventsTable = GameConstants.EntityEvents_Protocol43;
            }
            else if (demo.Protocol == Protocols.Protocol73)
            {
                eTypesTable = GameConstants.EntityTypes_Protocol48;
                entityEventsTable = GameConstants.EntityEvents_Protocol73;
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

            return (eType - eTypesTable["ET_EVENTS"] == entityEventsTable[eventName]);
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
                WriteServerCommand(gameLog, chatServerCommandMarker, command.Command);
                WriteServerCommand(consoleLog, chatServerCommandMarker, command.Command);
            }
            else if (command.Command.StartsWith(printServerCommandMarker))
            {
                WriteServerCommand(consoleLog, printServerCommandMarker, command.Command);
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

                uint eType = (uint)entity["eType"];

                if (IsEvent(eType, "EV_OBITUARY"))
                {
                    uint killerIndex = (uint?)entity["otherEntityNum2"] ?? 0;
                    uint victimIndex = (uint?)entity["otherEntityNum"] ?? 0;
                    WriteColouredString(gameLog, CalculateSimpleObituary((uint)entity["eventParm"], killerIndex, victimIndex));
                    WriteColouredString(consoleLog, CalculateObituary((uint)entity["eventParm"], killerIndex, victimIndex));
                }
            }
        }
        #endregion
    }
}