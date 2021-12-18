using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System.Linq;
using AOSharp.Core.UI;
using System;

namespace Desu
{
    public class TraderCombatHandler : GenericCombatHandler
    {
        private Menu _menu;
        public TraderCombatHandler()
        {
            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcTraderUnforgivenDebts, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcTraderRefinanceLoans, LEProc);

            //Distill Life
            RegisterPerkProcessor(PerkHash.ReapLife, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Bloodletting, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ReapLife, TargetedDamagePerk);

            //Trader huge dmg boost perk to whole team
            RegisterPerkProcessor(PerkHash.Sacrifice, Sacrifice);

            //Shotgun Mastery
            RegisterPerkProcessor(PerkHash.LegShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.EasyShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.PointBlank, TargetedDamagePerk);



            //Power Up
            RegisterPerkProcessor(PerkHash.Energize, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.PowerVolley, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.PowerShock, TargetedDamagePerk);



            //Self Buffs
            RegisterSpellProcessor(RelevantNanos.ImprovedQuantumUncertanity, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.UnstoppableKiller, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.UmbralWranglerPremium, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.NanobotAegis, NanobotAegis);
            RegisterSpellProcessor(RelevantNanos.SubprimeVitalityMortgage, SubprimeVitalityMortgage);

            //Team Buffs
            RegisterSpellProcessor(RelevantNanos.QuantumUncertanity, TeamBuff);

            //Team Nano heal (Rouse Outfit nanoline)
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoPointHeals).OrderByStackingOrder(), TeamNanoHeal);

            //AC Drains/Nanoline
            //   RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderDebuffACNanos).OrderByStackingOrder(), TraderACDrain);
            //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Draw).OrderByStackingOrder(), TraderACDrain);
            //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Siphon).OrderByStackingOrder(), TraderACDrain);
            //   RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DebuffNanoACHeavy).OrderByStackingOrder(), TraderACDrain);

            //AAO/AAD/Damage Drains
            // RegisterSpellProcessor(RelevantNanos.DivestDamage, LEDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAADDrain).OrderByStackingOrder(), LEDrainAAD);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAAODrain).OrderByStackingOrder(), LEDrainAAO);

            //Deprive/Ransack Drains
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Deprive), DepriveDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Ransack), RansackDrain);

            //GTH/Your Enemy Drains
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDrain_LineB), Debuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HealthandNanoOverTimeDrain), Debuff);

            _menu = new Menu("CombatHandler.Trader", "CombatHandler.Trader");
            _menu.AddItem(new MenuBool("UseACDrainsDebuffs", "Use AC debuff/drain nano lines", true));
            _menu.AddItem(new MenuBool("UseLEDrains", "Use AAO'/AAD drain nano lines", true));
            OptionPanel.AddMenu(_menu);
        }

        private static class RelevantNanos
        {
            public const int QuantumUncertanity = 30745;
            public const int ImprovedQuantumUncertanity = 270808;
            public const int UnstoppableKiller = 275846;
            public const int DivestDamage = 273407;
            public const int UmbralWranglerPremium = 235291;
            public const int NanobotAegis = 302074;
            public const int SubprimeVitalityMortgage = 302401;
        }

        private static class RelevantItems
        {
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        }
        private bool NanobotAegis(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;
            return DynelManager.LocalPlayer.HealthPercent < 60;
        }
        private bool SubprimeVitalityMortgage(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 35)
            {
                actiontarget.Target = DynelManager.LocalPlayer;
                return true;

            }
            return false;
        }
        private bool RansackDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }
            foreach (Buff buff in fightingTarget.Buffs)
            {
                if (spell.Nanoline == buff.Nanoline && fightingTarget.Buffs.Contains(NanoLine.TraderSkillTransferTargetDebuff_Ransack) && buff.RemainingTime > 1)
                {
                    return false;
                }

            }
            return true;
        }

        private bool DepriveDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }
            foreach (Buff buff in fightingTarget.Buffs)
            {
                if (spell.Nanoline == buff.Nanoline && fightingTarget.Buffs.Contains(NanoLine.TraderSkillTransferTargetDebuff_Deprive) && buff.RemainingTime > 1)
                {
                    return false;
                }

            }
            return true;
        }

        private bool LEDrainAAD(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null)
                return false;

            foreach (Buff buff in fightingTarget.Buffs)
            {
                if (spell.Nanoline == buff.Nanoline && fightingTarget.Buffs.Contains(NanoLine.TraderAADDrain) && buff.RemainingTime > 1)
                {
                    return false;
                }

            }
            return true;
        }
        private bool LEDrainAAO(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null)
                return false;
            foreach (Buff buff in fightingTarget.Buffs)
            {
                if (spell.Nanoline == buff.Nanoline && fightingTarget.Buffs.Contains(NanoLine.TraderAAODrain) && buff.RemainingTime > 1)
                {
                    return false;
                }

            }
            return true;

        }

        private bool TeamNanoHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if (buff.Nanoline == NanoLine.NanoPointHeals)
                {
                    return false;
                }
            }

            // Cast when any team mate is lower than 30% of nano
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar lowNanoTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.NanoPercent <= 30)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (lowNanoTeamMember != null)
                {
                    actionTarget.Target = lowNanoTeamMember;
                    return true;
                }
            }

            return false;
        }

        private bool Debuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }

            //Check the remaining time on debuffs. On the enemy target
            foreach (Buff buff in fightingTarget.Buffs.AsEnumerable())
            {
                //Chat.WriteLine(buff.Name);
                if (buff.Name == spell.Name && buff.RemainingTime > 1)
                    return false;
            }

            return true;
        }
    }
}
