
using System.Linq;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System.Collections.Generic;
using AOSharp.Core.UI;
using System;

namespace Desu
{
    public class AgentCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public AgentCombatHandler()
        {
            //Perks

            RegisterPerkProcessor(PerkHash.ArmorPiercingShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ConcussiveShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.FindTheFlaw, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.TheShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DeathStrike, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.PinpointStrike, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.SoftenUp, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Assassinate, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Tranquilizer, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ShadowBullet, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.NightKiller, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.SilentPlague, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Recalibrate, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Fuzz, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.FireFrenzy, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.LEProcAgentLaserAim, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcAgentGrimReaper,LEProc);




            RegisterSpellProcessor(RelevantNanos.AssassinGrin, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.GnatsWIng, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Phase4, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.AssassinAimedShot, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.LesserPredator, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.WayofTheExecutioner, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompleteHealing, CompleteHeal);
            RegisterSpellProcessor(RelevantNanos.LifegivingElixir, LifegivingElixir);
            RegisterSpellProcessor(RelevantNanos.FpDoc, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedInstinctiveControl, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedNanoRepulsor, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ContinuousReconstruction, GenericBuff);
            //RegisterSpellProcessor(RelevantNanos.EnhancedSureshot, TeamBuff);


            // RegisterSpellProcessor(RelevantNanos.SuperiorOmniMedEnhancement, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.LifeChanneler, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CompleteHealingLine).OrderByStackingOrder(), CompleteHeal);







            //Spells
            RegisterSpellProcessor(RelevantNanos.Ubt, Ubt, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.Bullseye, AgentDebuff, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.AbsoluteConcentration, AgentDebuff, CombatActionPriority.Low);



            _menu = new Menu("CombatHandler.Agent", "CombatHandler.Agent");
            _menu.AddItem(new MenuBool("UseDebuff", "Agent Debuffing", true));
            _menu.AddItem(new MenuBool("UseUBT", "Agent Ubting", true));
            _menu.AddItem(new MenuBool("BuffTeamMembers", "BuffTeamMembers?", false));

            OptionPanel.AddMenu(_menu);

        }
        private bool AgentDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
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


        private bool CompleteHeal(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
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

                if (DynelManager.LocalPlayer.NanoPercent < 20)

                    return false;

                else if (dyingTeamMember != null)
                {
                    actiontarget.Target = dyingTeamMember;

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
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, dyingTeamMember.Position, DebuggingColor.Red);
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



            // Buffs

            public static readonly int FpDoc = 117210;
            public static readonly int WayofTheExecutioner = 275822;
            public static readonly int AssassinGrin = 81856;
            public static readonly int GnatsWIng = 273293;
            public static readonly int Phase4 = 273296;
            public static readonly int AssassinAimedShot = 275007;
            public static readonly int LesserPredator = 263240;
            public static readonly int AbsoluteConcentration = 160712;
            public static readonly int ImprovedInstinctiveControl = 222856;
            public static readonly int ImprovedNanoRepulsor = 222823;
            public static readonly int ContinuousReconstruction = 222824;
            public static readonly int SuperiorOmniMedEnhancement = 95709;
            public static readonly int LifeChanneler = 96247;
            public static readonly int EnhancedSureshot = 160791;

        }
    }
}


