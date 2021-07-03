using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;
using System.Linq;

namespace Desu
{
    public class SoldCombathandler : GenericCombatHandler
    {
        private Menu _menu;
        public SoldCombathandler()
        {

            //DmgPerks
            RegisterPerkProcessor(PerkHash.SupressiveHorde, DamagePerk);
            RegisterPerkProcessor(PerkHash.Energize, DamagePerk);
            RegisterPerkProcessor(PerkHash.ReinforceSlugs, DamagePerk);
            RegisterPerkProcessor(PerkHash.Violence, Violence);


            //Debuffs
            RegisterPerkProcessor(PerkHash.Tracer, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.TriangulateTarget, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.LaserPaintTarget, TargetedDamagePerk);

            //AI Perks
            RegisterPerkProcessor(PerkHash.LaserPaintTarget, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Fuzz, DamagePerk);
            RegisterPerkProcessor(PerkHash.FireFrenzy, DamagePerk);
            RegisterPerkProcessor(PerkHash.Clipfever, DamagePerk);
            RegisterPerkProcessor(PerkHash.MuzzleOverload, DamagePerk);
            RegisterPerkProcessor(PerkHash.BotConfinement, DamagePerk);
            RegisterPerkProcessor(PerkHash.BotConfinement, DamagePerk);
            RegisterPerkProcessor(PerkHash.NanoFeast, DamagePerk);

            //Shadow Dmg
            RegisterPerkProcessor(PerkHash.WeaponBash, DamagePerk);
            RegisterPerkProcessor(PerkHash.NapalmSpray, DamagePerk);
            RegisterPerkProcessor(PerkHash.ContainedBurst, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerVolley, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerShock, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerBlast, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerCombo, DamagePerk);
            RegisterPerkProcessor(PerkHash.JarringBurst, DamagePerk);
            RegisterPerkProcessor(PerkHash.SolidSlug, DamagePerk);
            RegisterPerkProcessor(PerkHash.NeutroniumSlug, DamagePerk);
            RegisterPerkProcessor(PerkHash.EasyShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.PointBlank, DamagePerk);
            RegisterPerkProcessor(PerkHash.Collapser, DamagePerk);
            RegisterPerkProcessor(PerkHash.Implode, DamagePerk);
            RegisterPerkProcessor(PerkHash.DrawBlood, DamagePerk);

            //Procs
            RegisterPerkProcessor(PerkHash.LEProcSoldierFuseBodyArmor, LEProc , CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierFuriousAmmunition, LEProc ,  CombatActionPriority.Medium);

            //Spells
          //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TotalMirrorShield).OrderByStackingOrder(), AugmentedMirrorShieldMKV);
          //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DrainHeal).OrderByStackingOrder(), DrainHeal);
            RegisterSpellProcessor(RelevantNanos.DontFeartheReaper, DontFearTheReaper);
            RegisterSpellProcessor(RelevantNanos.Distinctvictim, SingleTargetTaunt);
            RegisterSpellProcessor(RelevantNanos.Fat, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.OffensiveSteamroller, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.AbsorptionShield, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.RiotControl, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.PreNullitySphere, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.GazumpFight, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedPrecognition, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedSoldierClipJunkie, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedTotalFocus, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.TotalCombatSurvival, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ArtOfWar, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.AugmentedMirrorShieldMKIII, AugmentedMirrorShield);


            //Items
            RegisterItemProcessor(RelevantItems.DreadlochEnduranceBoosterNanomageEdition, RelevantItems.DreadlochEnduranceBoosterNanomageEdition, EnduranceBooster, CombatActionPriority.High);

            _menu = new Menu("CombatHandler.Sold", "CombatHandler.Sold");
            _menu.AddItem(new MenuBool("useTaunt", "Use Taunt", false));
            OptionPanel.AddMenu(_menu);
        }

        private bool EnduranceBooster(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                return false;


            if (DynelManager.LocalPlayer.HealthPercent > 40)
                return false;

 
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
              return false;

            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            return true;
        }

        private bool Violence(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null && fightingTarget.HealthPercent < 40)
                return true;
            return false;
        }

        private bool SingleTargetTaunt(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!_menu.GetBool("useTaunt") || !DynelManager.LocalPlayer.IsAttacking || fightingtarget == null || DynelManager.LocalPlayer.Nano < spell.Cost)
                return false;

            return true;
        }

        private bool DontFearTheReaper(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            //if (!DynelManager.LocalPlayer.IsAttacking || fightingtarget == null)
            //    return false;

            if (DynelManager.LocalPlayer.HealthPercent <= 40)
                return true;

            return false;
        }

        private bool AugmentedMirrorShield(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!DynelManager.LocalPlayer.IsAttacking || fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.HealthPercent <= 60 && spell.IsReady)
                return true;

            return true;
        }
        private static class RelevantNanos
        {
            public const int AdrenalineRush = 301897;
            public const int Distinctvictim = 223205;
            public const int Fat = 270248;
            public const int AbsorptionShield = 75401;
            public const int CompositeHeavyArtillery = 269482;
            public const int RiotControl = 29251;
            public const int GazumpFight = 223199;
            public const int TotalCombatSurvival = 273398;
            public const int OffensiveSteamroller = 29240;
            public const int ImprovedPrecognition = 275844;
            public const int PreNullitySphere = 233033;
            public const int ImprovedTotalFocus = 270806;
            public const int ImprovedSoldierClipJunkie = 273402;
            public const int ArtOfWar = 275027;
            public const int AugmentedMirrorShieldMKIII = 223183;
            public const int DontFeartheReaper = 29241;


        }

        private static class RelevantItems
        {
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        }
    }
}
