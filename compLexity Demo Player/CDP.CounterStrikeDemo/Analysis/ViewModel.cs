using System;
using System.Linq;
using System.Windows.Media;

namespace CDP.CounterStrikeDemo.Analysis
{
    public class ViewModel : HalfLifeDemo.Analysis.ViewModel
    {
        public ViewModel(Demo demo) : base(demo)
        {
            demo.AddMessageCallback<HalfLifeDemo.Messages.SvcEventReliable>(MessageEventReliable);

            // TextMsg
            titles.Add("#Target_Bombed", "Target Succesfully Bombed!");
            titles.Add("#VIP_Escaped", "The VIP has escaped!");
            titles.Add("#VIP_Assassinated", "VIP has been assassinated!");
            titles.Add("#Terrorists_Escaped", "The terrorists have escaped!");
            titles.Add("#CTs_PreventEscape", "The CTs have prevented most of the terrorists from escaping!");
            titles.Add("#Escaping_Terrorists_Neutralized", "Escaping terrorists have all been neutralized!");
            titles.Add("#Bomb_Defused", "The bomb has been defused!");
            titles.Add("#CTs_Win", "Counter-Terrorists Win!");
            titles.Add("#Terrorists_Win", "Terrorists Win!");
            titles.Add("#Round_Draw", "Round Draw!");
            titles.Add("#All_Hostages_Rescued", "All Hostages have been rescued!");
            titles.Add("#Target_Saved", "Target has been saved!");
            titles.Add("#Hostages_Not_Rescued", "Hostages have not been rescued!");
            titles.Add("#Terrorists_Not_Escaped", "Terrorists have not escaped!");
            titles.Add("#VIP_Not_Escaped", "VIP has not escaped!");
            titles.Add("#Terrorist_Escaped", "A terrorist has escaped!");
            titles.Add("#Bomb_Planted", "The bomb has been planted!");
            titles.Add("#Game_will_restart_in", "The game will restart in %s1 %s2");
            titles.Add("#Game_bomb_drop", "%s1 dropped the bomb");
            titles.Add("#Game_bomb_pickup", "%s1 picked up the bomb");
            titles.Add("#Game_connected", "%s1 connected");
            titles.Add("#Game_disconnected", "%s1 has left the game");
            titles.Add("#Game_join_ct", "%s1 is joining the Counter-Terrorist force");
            titles.Add("#Game_join_ct_auto", "%s1 is joining the Counter-Terrorist force (auto)");
            titles.Add("#Game_join_terrorist", "%s1 is joining the Terrorist force");
            titles.Add("#Game_join_terrorist_auto", "%s1 is joining the Terrorist force (auto)");
            titles.Add("#Game_kicked", "Kicked %s1");
            titles.Add("#Game_teammate_attack", "%s1 attacked a teammate");

            // SayText
            titles.Add("#Cstrike_Name_Change", "* %s1 changed name to %s2");
            titles.Add("#Cstrike_Chat_CT", "(Counter-Terrorist) ");
            titles.Add("#Cstrike_Chat_T", "(Terrorist) ");
            titles.Add("#Cstrike_Chat_CT_Dead", "*DEAD*(Counter-Terrorist) ");
            titles.Add("#Cstrike_Chat_T_Dead", "*DEAD*(Terrorist) ");
            titles.Add("#Cstrike_Chat_Spec", "(Spectator) ");
            titles.Add("#Cstrike_Chat_All", "");
            titles.Add("#Cstrike_Chat_AllDead", "*DEAD* ");
            titles.Add("#Cstrike_Chat_AllSpec", "*SPEC* ");
        }

        protected override SolidColorBrush GetTeamColour(string teamName)
        {
            if (teamName == "CT")
            {
                return Brushes.Blue;
            }
            else if (teamName == "TERRORIST")
            {
                return Brushes.Red;
            }

            return base.GetTeamColour(teamName);
        }

        #region Message handlers
        private void MessageEventReliable(HalfLifeDemo.Messages.SvcEventReliable message)
        {
            // TODO: should be 29 for 1.6, 26 for everything else.
            if (message.Index == 29)
            {
                NewRound();
            }
        }
        #endregion
    }
}
