using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Combat;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;

namespace Desu
{
    public class MACombatHandler : GenericCombatHandler
    {
        private Menu _menu;
        public MACombatHandler() : base()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.Moonmist, Moonmist);
            RegisterPerkProcessor(PerkHash.Dragonfire, DamagePerk);
            RegisterPerkProcessor(PerkHash.ChiConductor, DamagePerk);
            RegisterPerkProcessor(PerkHash.Incapacitate, DamagePerk);
            RegisterPerkProcessor(PerkHash.TremorHand, DamagePerk);
            RegisterPerkProcessor(PerkHash.FleshQuiver, DamagePerk);
            RegisterPerkProcessor(PerkHash.Obliterate, DamagePerk);
            RegisterPerkProcessor(PerkHash.FollowupSmash, DamagePerk);
            RegisterPerkProcessor(PerkHash.Crave, DamagePerk);
            RegisterPerkProcessor(PerkHash.BlindsideBlow, DamagePerk);
            RegisterPerkProcessor(PerkHash.LEProcMartialArtistAbsoluteFist, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcMartialArtistDebilitatingStrike, LEProc);
            RegisterPerkProcessor(PerkHash.RedDawn, RedDawnPerk, CombatActionPriority.High);

            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SingleTargetHealing).OrderByStackingOrder(), SingleTargetHeal, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.FistsOfTheWinterFlame, FistsOfTheWinterFlameNano);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MartialArtsBuff), GenericBuff);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MartialArtistZazenStance), MartialArtistZazen);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.StrengthBuff), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SingedFists), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeBuffs), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.BrawlBuff), GenericBuff);


            RegisterSpellProcessor(RelevantItems.FistsofStellarHarmony, GenericBuff);
            RegisterSpellProcessor(RelevantItems.CompositeMartialProwessExpertise, GenericBuff);
            RegisterSpellProcessor(RelevantItems.CompositePhysicalSpecialExpertise, GenericBuff);
            RegisterSpellProcessor(RelevantItems.LimboMastery, GenericBuff);
            RegisterSpellProcessor(RelevantItems.FormofRisan, GenericBuff);
            RegisterSpellProcessor(RelevantItems.MuscleBooster, GenericBuff);
            RegisterSpellProcessor(RelevantItems.UnnoticedStrike, GenericBuff);
            RegisterSpellProcessor(RelevantItems.DirtyFighter, GenericBuff);
            RegisterSpellProcessor(RelevantItems.StutterStep, GenericBuff);
            RegisterSpellProcessor(RelevantItems.MarkofPeril, TeamBuff);

            //RegisterItemProcessor(RelevantItems.HackedBoostedGraftEnhancedSenses, RelevantItems.HackedBoostedGraftEnhancedSenses, EnhancedSenceGraft);

            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CriticalIncreaseBuff), GenericBuff);
            //https://aoitems.com/item/95527/universal-vulnerability-compendium/
            RegisterSpellProcessor(95527, GenericBuff);
            //Items
            RegisterItemProcessor(RelevantItems.TheWizdomOfHuzzum, RelevantItems.TheWizdomOfHuzzum, MartialArtsTeamHealAttack);

            _menu = new Menu("CombatHandler.MA", "CombatHandler.MA");

            _menu.AddItem(new MenuBool("UseZazen", "UseZazen", false));

            OptionPanel.AddMenu(_menu);
        }
        protected virtual bool EnhancedSenceGraft(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Enhanced Senses = 25988
            if (!DynelManager.LocalPlayer.Buffs.Contains(25988))
                return DamageItem(item, fightingTarget, ref actionTarget);

            return false;
        }

        private bool MartialArtsTeamHealAttack(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)))
                return false;

            int healingReceived = item.LowId == RelevantItems.TreeOfEnlightenment ? 290 : 1200;

            if (DynelManager.LocalPlayer.MissingHealth < healingReceived * 2)
                return false;

            return true;
        }
        private bool MartialArtistZazen(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null || !_menu.GetBool("UseZazen"))

                return false;
            if (DynelManager.LocalPlayer.IsInTeam())

                return true;
            return false;
        }



        protected override bool ShouldUseSpecialAttack(SpecialAttack specialAttack)
        {
            return specialAttack != SpecialAttack.Dimach;
        }

        private bool FistsOfTheWinterFlameNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            actiontarget.ShouldSetTarget = false;
            return fightingtarget != null && fightingtarget.HealthPercent > 50;
        }

        private bool RedDawnPerk(PerkAction perkAction, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            return DynelManager.LocalPlayer.MissingHealth > 2000;
        }

        private bool SingleTargetHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.MissingHealth > 800) //TODO: Some kind of healing check to calc an optimal missing health value
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

        private bool Moonmist(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;

            if (fightingTarget == null || (fightingTarget.HealthPercent < 90 && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) < 2))
                return false;

            return true;
        }

        private bool Obliterate(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || fightingTarget.HealthPercent > 15)
                return false;

            return true;
        }

        private Stat GetSkillLockStat(Item item)
        {
            switch (item.HighId)
            {
                case RelevantItems.TheWizdomOfHuzzum:
                case RelevantItems.TreeOfEnlightenment:
                    return Stat.Dimach;
                default:
                    throw new Exception($"No skill lock stat defined for item id {item.HighId}");
            }
        }

        private static class RelevantNanos
        {
            public const int FistsOfTheWinterFlame = 269470;
        }

        private static class RelevantItems
        {
            public const int TheWizdomOfHuzzum = 303056;
            public const int TreeOfEnlightenment = 204607;
            public const int FistsofStellarHarmony = 81827;
            public const int CompositeMartialProwessExpertise = 302158;
            public const int CompositePhysicalSpecialExpertise = 215264;
            public const int CompositeNanoExpertise = 223380;
            public const int LimboMastery = 28894;
            public const int FormofRisan = 275700;
            public const int MuscleBooster = 28898;
            public const int UnnoticedStrike = 273372;
            public const int DirtyFighter = 28870;
            public const int StutterStep = 218070;
            public const int MarkofPeril = 160574;
            public const int HackedBoostedGraftEnhancedSenses = 125883;







        }
    }
}
