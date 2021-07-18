
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
    public class TL5AgentCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public TL5AgentCombatHandler()
        {
            //Perks

            RegisterPerkProcessor(PerkHash.ArmorPiercingShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ConcussiveShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.SnipeShot1, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Fuzz, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.FindTheFlaw, DamagePerk);





            RegisterSpellProcessor(RelevantNanos.AssassinsGrin, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Phase3, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.SteadyNerves, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgilityBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CriticalIncreaseBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgentEscapeNanos).OrderByStackingOrder(), EscapeNano);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Challenger).OrderByStackingOrder(), Challenger);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HPBuff).OrderByStackingOrder(), EnfBuffs);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Rage).OrderByStackingOrder(), EnfBuffs);
            RegisterSpellProcessor(RelevantNanos.FormofTheExecutioner, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CoruscatingScreen, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.FailingImpregnability, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.WavesofJarring, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.JarringShock, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompleteHealing, CompleteHeal);
            RegisterSpellProcessor(RelevantNanos.LifegivingElixir, LifegivingElixir);
            RegisterSpellProcessor(RelevantNanos.FpEnf, EnfBuffs);
            RegisterSpellProcessor(RelevantNanos.FpDoc, DocBuffs);
            RegisterSpellProcessor(RelevantNanos.ImprovedInstinctiveControl, DocBuffs);
            RegisterSpellProcessor(RelevantNanos.ImprovedNanoRepulsor, DocBuffs);
            RegisterSpellProcessor(RelevantNanos.ContinuousReconstruction, DocBuffs);
            //RegisterSpellProcessor(RelevantNanos.EnhancedSureshot, TeamBuff);


            // RegisterSpellProcessor(RelevantNanos.SuperiorOmniMedEnhancement, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.LifeChanneler, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CompleteHealingLine).OrderByStackingOrder(), CompleteHeal);







            //Spells
            RegisterSpellProcessor(RelevantNanos.Ubt, Ubt, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.Bullseye, AgentDebuff, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.AbsoluteConcentration, Concentration, CombatActionPriority.High);



            _menu = new Menu("Tl5 Agent", "Tl5 Agent");
            _menu.AddItem(new MenuBool("UseDebuff", "Agent Debuffing", false));
            _menu.AddItem(new MenuBool("UseUBT", "Agent Ubting", false));
            _menu.AddItem(new MenuBool("UseRage", "Using Rage", true));
            _menu.AddItem(new MenuBool("UseFpEnf", "Using FpEnf", false));
            _menu.AddItem(new MenuBool("UseFpDoc", "Using FpDoc", false));


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
        private bool EscapeNano(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null)
                return false;


            return true;
        }
        private bool Challenger(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {
            if (fightingTarget == null)
            {
                return false;
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return false;

            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                    return false;

                //Don't cast if greater than 10% time remaining

                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                    return false;

                if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU))
                    return false;
            }
            else
            {
                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU)
                    return false;
            }
            return true;

        }
        private bool Concentration(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {

            if (fightingTarget == null)

                return false;


            return true;
        }

        protected bool EnfBuffs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null && !_menu.GetBool("UseFpEnf"))

                return false;

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return false;

            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                    return false;

                //Don't cast if greater than 10% time remaining

                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                    return false;

                if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU))
                    return false;
            }
            else
            {
                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU)
                    return false;
            }

            actionTarget.ShouldSetTarget = true;
            return true;
        }
        protected bool DocBuffs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null && !_menu.GetBool("UseFpDoc"))

                return false;

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return false;

            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                    return false;

                //Don't cast if greater than 10% time remaining

                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                    return false;

                if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU))
                    return false;
            }
            else
            {
                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU)
                    return false;
            }

            actionTarget.ShouldSetTarget = true;
            return true;
        }

        private bool EnfRage(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

        {

            if (fightingTarget == null || _menu.GetBool("UseRage"))
                return true;

            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                    return false;

                //Don't cast if greater than 10% time remaining

                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                    return false;


            }
            return false;
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
            public static readonly int FpEnf = 117217;
            public static readonly int WayofTheExecutioner = 275822;
            public static readonly int Phase3 = 32061;
            public static readonly int JarringShock = 226396;
            public static readonly int WavesofJarring = 226413;
            public static readonly int AbsoluteConcentration = 160712;
            public static readonly int ImprovedInstinctiveControl = 222856;
            public static readonly int ImprovedNanoRepulsor = 222823;
            public static readonly int ContinuousReconstruction = 222824;
            public static readonly int SuperiorOmniMedEnhancement = 95709;
            public static readonly int LifeChanneler = 96247;
            public static readonly int EnhancedSureshot = 160791;
            public static readonly int SteadyNerves = 160795;
            public static readonly int FormofTheExecutioner = 160828;
            public static readonly int AssassinsGrin = 81856;
            public static readonly int FailingImpregnability = 117686;
            public static readonly int CoruscatingScreen = 55751;
            public static readonly int EssenceofBehemoth = 95708;

        }
    }
}


