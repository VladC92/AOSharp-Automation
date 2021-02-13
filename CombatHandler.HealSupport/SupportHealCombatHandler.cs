using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Desu
{
    class SupportHealCombatHandler : GenericCombatHandler
    {
        private Menu _menu;
        public SupportHealCombatHandler()
        {

            //This needs work 
            RegisterSpellProcessor(RelevantNanos.Ubt, Ubt);
            RegisterSpellProcessor(RelevantNanos.CompleteHealing, CompleteHeal , CombatActionPriority.High);
           RegisterSpellProcessor(RelevantNanos.LifegivingElixir, LifegivingElixir);
           RegisterSpellProcessor(RelevantNanos.BODILY_INV, BI, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.FountainOfLife, FountainOfLife  , CombatActionPriority.Medium);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SingleTargetHealing).OrderByStackingOrder(), SingleTargetHeal, CombatActionPriority.Medium);


            _menu = new Menu("CombatHandler.SUpportHeal", "CombatHandler.SupportHeal");
            _menu.AddItem(new MenuBool("UseUBT", "UBT Debuffing", true));
            OptionPanel.AddMenu(_menu);
        }
        private bool Ubt(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseUBT"))
                return false;
            //Check the remaining time on debuffs. On the enemy target
            foreach (Buff buff in fightingTarget.Buffs.AsEnumerable())
            {
                //Chat.WriteLine(buff.Name);
                if (buff.Name == spell.Name && buff.RemainingTime > 1)
                    return false;

            }

            return true;
        }
        private bool FountainOfLife(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 40)
            {
                actiontarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 40)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actiontarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }

        private bool SingleTargetHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.MissingHealth > 800) 
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 80)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return true;
                }

            }
            return false;
        }

         
        
        private bool LifegivingElixir(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 85)
            {
                actiontarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 85)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actiontarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }
        private bool BI(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 80)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 80)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }
        


        private bool CompleteHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 50)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 50)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }


        private static class RelevantNanos
        {
            public const int Ubt = 99577;
            public const int Bullseye = 275823;
            public const int CompleteHealing = 28650;
            public const int LifegivingElixir = 43878;
            public const int BODILY_INV = 223299;
            public const int FountainOfLife = 302907;

        }
    }
}
