using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;
using System.Linq;

namespace Desu
{
    public class FixerCombathandler : GenericCombatHandler
    {
        private Menu _menu;
        public FixerCombathandler()
        {

            //DmgPerks
            RegisterPerkProcessor(PerkHash.NeutroniumSlug, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.SolidSlug, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.JarringBurst, DamagePerk);
            RegisterPerkProcessor(PerkHash.ReinforceSlugs, DamagePerk);
            RegisterPerkProcessor(PerkHash.Energize, DamagePerk);
            RegisterPerkProcessor(PerkHash.BodyTackle, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerBolt, DamagePerk);
            RegisterPerkProcessor(PerkHash.Numb, DamagePerk);
            RegisterPerkProcessor(PerkHash.Cripple, DamagePerk);
            RegisterPerkProcessor(PerkHash.EatBullets, DamagePerk);
            RegisterPerkProcessor(PerkHash.TriggerHappy, DamagePerk);

            //Procs
            RegisterPerkProcessor(PerkHash.LEProcFixerBackyardBandages, LEProc);
            //RegisterPerkProcessor(PerkHash.LEProcFixerContaminatedBullets, LEProc);

            //Shadow Dmg
            RegisterPerkProcessor(PerkHash.WeaponBash, DamagePerk);
            RegisterPerkProcessor(PerkHash.NapalmSpray, DamagePerk);
            RegisterPerkProcessor(PerkHash.ContainedBurst, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerVolley, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerShock, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerBlast, DamagePerk);
            RegisterPerkProcessor(PerkHash.PowerCombo, DamagePerk);
            RegisterPerkProcessor(PerkHash.EasyShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.PointBlank, DamagePerk);
            RegisterPerkProcessor(PerkHash.Collapser, DamagePerk);
            RegisterPerkProcessor(PerkHash.Implode, DamagePerk);

            //Spells
                 
                          
            RegisterSpellProcessor(RelevantNanos.SlipofMind, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.SemiSentientAugmentationCloud, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedSemiSentientAugmentationCloud, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedFrenzyofShells, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.BlessedByShadow, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.RefactorNCUMatrix, GenericBuff);
       
                        
            RegisterSpellProcessor(RelevantNanos.LucksImprovedCapriciousConsequence, FixerDebuff);
           
           // RegisterSpellProcessor(RelevantNanos.SuperiorInsuranceHack, HealDot);
                 
                    
            _menu = new Menu("CombatHandler.Fixer", "CombatHandler.Fixer");
            _menu.AddItem(new MenuBool("useDebuff", "Use Debuff", true));
            OptionPanel.AddMenu(_menu);
        }

        private bool FixerDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)

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


        private static class RelevantNanos
        {
            public static readonly int SemiSentientAugmentationCloud = 81879;
            public static readonly int ImprovedSemiSentientAugmentationCloud = 222838;
            public static readonly int SlipofMind = 227398;
            public static readonly int ImprovedFrenzyofShells = 273355;
            public static readonly int SuperiorInsuranceHack = 273352;
            public static readonly int BlessedByShadow = 223125;
            public const int LucksImprovedCapriciousConsequence = 273357;
            public static readonly int RefactorNCUMatrix = 275680;
            public static readonly int PreservationMatrix = 275679;
            public static readonly int GSF = 93132;

        }

        private static class RelevantItems
        {
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        }
    }
}
