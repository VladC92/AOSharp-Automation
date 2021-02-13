using System;
using System.Linq;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;

namespace Desu
{
    public static class RelevantNanos
    {
        public const int CompositeAttribute = 223372;
        public const int CompositeNano = 223380;
        public const int CompositeUtility = 287046;
        public const int CompositeRanged = 223348;
        public const int CompositeRangedSpec = 223364;
        public const int CourageOfTheJustTeam = 279379;
        public const int CourageOfTheJust = 279378;
        public const int AdaptiveAmbientRenewal = 273362;
        public const int ImprovedGuardianofMight = 273365;
        public const int FervoroftheDevotee = 210323;
        public const int CompositeMartialProwessExpertise = 302158;
        public const int GiantofSwords = 275838;
        public const int CompositeMeleeExpertise = 223360;
        public const int CompositePhysicalSpecialExpertise = 215264;
        public const int DefenderPoise = 210311;
        public const int ImprovedVengeanceoftheImmaculate = 273359;
        public const int PunisheroftheWicked = 301602;
        public const int EludePain = 211170;
        public const int SaintedSanctifier = 222981;
        public const int ImminenceofExtermination = 275837;
        public const int WardenWard = 223022;




    }
    public class KeeperCombatHandler : GenericCombatHandler
    {
        private Menu _menu;
        public KeeperCombatHandler()
        {

            //DmgPerks
            RegisterPerkProcessor(PerkHash.Insight, DamagePerk);
            RegisterPerkProcessor(PerkHash.BladeWhirlwind, DamagePerk);
            RegisterPerkProcessor(PerkHash.ReinforceSlugs, DamagePerk);

            RegisterPerkProcessor(PerkHash.Cleave, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Transfix, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.PainLance, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.SliceAndDice, TargetedDamagePerk);


            //Debuffs
            RegisterPerkProcessor(PerkHash.MarkOfSufferance, TeamHealPerk);
            RegisterPerkProcessor(PerkHash.MarkOfTheUnclean, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.MarkOfVengeance, TargetedDamagePerk);

            //Shadow Dmg
            RegisterPerkProcessor(PerkHash.DeepCuts, DamagePerk);
            RegisterPerkProcessor(PerkHash.SeppukuSlash, DamagePerk);
            RegisterPerkProcessor(PerkHash.HonoringTheAncients, DamagePerk);

            //Heal Perks
            RegisterPerkProcessor(PerkHash.BioRejuvenation, TeamHealPerk);
            RegisterPerkProcessor(PerkHash.BioRegrowth, TeamHealPerk);
            RegisterPerkProcessor(PerkHash.LayOnHands, TeamHealPerk);
            RegisterPerkProcessor(PerkHash.LayOnHands, TeamHealPerk);
            RegisterPerkProcessor(PerkHash.BioShield, SelfHealPerk);
            RegisterPerkProcessor(PerkHash.BioCocoon, SelfHealPerk);//TODO: Write independent logic for this

            //Buffs
            RegisterSpellProcessor(RelevantNanos.CompositeAttribute, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeUtility, GenericBuff);
            //RegisterSpellProcessor(RelevantNanos.CompositeRanged, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CourageOfTheJust, CourageOfTheJust);


            RegisterSpellProcessor(RelevantNanos.AdaptiveAmbientRenewal, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedGuardianofMight, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.FervoroftheDevotee, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeMartialProwessExpertise, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.GiantofSwords, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeMeleeExpertise, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositePhysicalSpecialExpertise, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.DefenderPoise, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedVengeanceoftheImmaculate, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.PunisheroftheWicked, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.EludePain, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.SaintedSanctifier, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImminenceofExtermination, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.WardenWard, GenericBuff);
            

            _menu = new Menu("CombatHandler.Keeper", "CombatHandler.Keeper");
         
            OptionPanel.AddMenu(_menu);
        }


        private bool SelfHealPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.HealthPercent <= 30)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }
            return false;
        }

        private bool TeamHealPerk(PerkAction perkAction, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 40)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 50)
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
        //TODO: Rework
        private bool CourageOfTheJust(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //279378 is the parent nano 
            //279379 is the aura casted to the team
            //if (fightingTarget != null)
            //    return false;

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

            actionTarget.ShouldSetTarget = false;
            return true;
        }

    }
}
