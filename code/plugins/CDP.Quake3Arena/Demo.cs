using System;
using System.Collections.Generic;
using System.Linq;
using CDP.Core;
using CDP.IdTech3.Commands;
using CDP.IdTech3;

namespace CDP.Quake3Arena
{
    public class Demo : IdTech3.Demo
    {
        public override string GameName
        {
            get
            {
                if (mod == null)
                {
                    return string.Format("Quake III Arena ({0})", gameName);
                }
                else
                {
                    return mod.Name;
                }
            }
        }

        public override bool CanAnalyse
        {
            get { return mod == null ? false : mod.CanAnalyse; }
        }

        public override bool CanPlay
        {
            get { return true; }
        }

        public bool CanConvert
        {
            get { return Protocol <= Protocols.Protocol48; }
        }

        public override ConvertTargets ConvertTarget
        {
            get
            {
                if ((bool)settings["Quake3ConvertProtocol"] && CanConvert)
                {
                    return ConvertTargets.Protocol68;
                }

                return ConvertTargets.None;
            }
        }

        private Mod mod;
        private readonly string scoresServerCommandMarker = "scores ";
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();

        public Demo()
        {
        }

        public override void Load()
        {
            base.Load();
            mod = ((Plugin)plugin).FindMod(ModFolder);
        }

        public override void Write(string destinationFileName)
        {
            AddCommandCallback<SvcConfigString>(Write_ConfigString);
            AddCommandCallback<SvcServerCommand>(Write_ServerCommand);
            AddCommandCallback<SvcSnapshot>(Write_Snapshot);
            base.Write(destinationFileName);
        }

        private void Write_ConfigString(SvcConfigString command)
        {
            ConvertConfigString(command);
        }

        private void Write_ServerCommand(SvcServerCommand command)
        {
            if (command.Command.StartsWith(scoresServerCommandMarker))
            {
                Scores scores = new Scores(Protocol);
                scores.Parse(command.Command);
                command.Command = scores.Compose();
            }
        }

        private void Write_Snapshot(SvcSnapshot command)
        {
            command.Player.Persistant = ConvertPlayerPersistant(command.Player.Persistant);

            foreach (Entity entity in command.Entities)
            {
                ConvertEntity(entity);
            }
        }

        protected virtual void ConvertConfigString(SvcConfigString configString)
        {
            if (ConvertTarget == ConvertTargets.None)
            {
                return;
            }

            LookupTable_short from;

            if (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol45)
            {
                from = GameConstants.ConfigStrings_Protocol43;
            }
            else if (Protocol == Protocols.Protocol48)
            {
                from = GameConstants.ConfigStrings_Protocol48;
            }
            else
            {
                from = GameConstants.ConfigStrings_Protocol66;
            }

            LookupTable_short to = GameConstants.ConfigStrings_Protocol66;

            if (from == to)
            {
                return;
            }

            configString.Index = from.Convert(configString.Index, to);
        }

        protected virtual short[] ConvertPlayerPersistant(short[] persistant)
        {
            if (ConvertTarget == ConvertTargets.None || Protocol >= Protocols.Protocol48)
            {
                return persistant;
            }

            short[] result = new short[persistant.Length];

            for (int i = 0; i < persistant.Length; i++)
            {
                int newIndex = GameConstants.PlayerPersistant_Protocol43.Convert(i, GameConstants.PlayerPersistant_Protocol48);

                if (newIndex != GameConstants.PlayerPersistant_Protocol43.ErrorValue)
                {
                    result[newIndex] = persistant[i];
                }
            }

            return result;
        }

        protected virtual void ConvertEntity(Entity entity)
        {
            if (ConvertTarget == ConvertTargets.None || entity["eType"] == null)
            {
                return;
            }

            LookupTable_uint from;

            if (Protocol >= Protocols.Protocol43 && Protocol <= Protocols.Protocol45)
            {
                from = GameConstants.EntityTypes_Protocol43;
            }
            else
            {
                from = GameConstants.EntityTypes_Protocol48;
            }

            LookupTable_uint to = GameConstants.EntityTypes_Protocol48;

            if (from == to)
            {
                return;
            }

            uint eType = from.Convert((uint)entity["eType"], to);

            if (eType >= to["ET_EVENTS"])
            {
                uint entityEvent = eType - to["ET_EVENTS"];
                ConvertEntityEvent(ref entityEvent);

                if (entityEvent == GameConstants.EntityEvents_Protocol48["EV_OBITUARY"])
                {
                    uint mod = (uint)entity["eventParm"];
                    ConvertMeansOfDeath(ref mod);
                    entity["eventParm"] = mod;
                }

                eType = to["ET_EVENTS"] + entityEvent;
            }

            entity["eType"] = eType;
        }

        protected virtual void ConvertEntityEvent(ref uint entityEvent)
        {
            if (Protocol != Protocols.Protocol43 && Protocol != Protocols.Protocol45)
            {
                return;
            }

            entityEvent = GameConstants.EntityEvents_Protocol43.Convert(entityEvent, GameConstants.EntityEvents_Protocol48);
        }

        protected virtual void ConvertMeansOfDeath(ref uint meansOfDeath)
        {
            if (Protocol > Protocols.Protocol45)
            {
                return;
            }

            meansOfDeath = GameConstants.MeansOfDeath_Protocol43.Convert(meansOfDeath, GameConstants.MeansOfDeath_Protocol48);
        }
    }
}
