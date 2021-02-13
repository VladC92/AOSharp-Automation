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

            //Team Buffs
            RegisterSpellProcessor(RelevantNanos.QuantumUncertanity, TeamBuff);

            //Team Nano heal (Rouse Outfit nanoline)
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoPointHeals).OrderByStackingOrder(), TeamNanoHeal);

            //AC Drains/Nanoline
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderDebuffACNanos).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Draw).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Siphon).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DebuffNanoACHeavy).OrderByStackingOrder(), TraderACDrain);

            //AAO/AAD/Damage Drains
            RegisterSpellProcessor(RelevantNanos.DivestDamage, LEDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAADDrain).OrderByStackingOrder(), LEDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAAODrain).OrderByStackingOrder(), LEDrain);

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
        }

        private static class RelevantItems
        {
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        }

        private bool RansackDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if(fightingTarget == null)
            {
                return false;
            }

            if(DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Ransack, out Buff buff))
            {
                if(buff.RemainingTime > 5)
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

            if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Deprive, out Buff buff))
            {
                if (buff.RemainingTime > 2)
                {
                    return false;
                }
            }
            return true;
        }

        private bool LEDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseLEDrains"))
                return false;

            return Debuff(spell, fightingTarget, ref actionTarget);
        }

        private bool TeamNanoHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach(Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if(buff.Nanoline == NanoLine.NanoPointHeals)
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

        private bool TraderACDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseACDrainsDebuffs"))
                return false;

            return Debuff(spell, fightingTarget, ref actionTarget);
        }

        private bool Debuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if(fightingTarget == null)
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
