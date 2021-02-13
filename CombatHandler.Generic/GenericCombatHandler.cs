using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;

namespace CombatHandler.Generic
{
    public class GenericCombatHandler : AOSharp.Core.Combat.CombatHandler
    {
        private double _lastCombatTime = double.MinValue;
        public int EvadeCycleTimeoutSeconds = 180;
        private Dictionary<PerkLine, int> _perkLineLevels;

        public GenericCombatHandler()
        {
            _perkLineLevels = Perk.GetPerkLineLevels(true);
            Game.OnUpdate += OnUpdate;
            Game.TeleportEnded += TeleportEnded;

            RegisterPerkProcessor(PerkHash.Limber, Limber, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.DanceOfFools, DanceOfFools, CombatActionPriority.High);

            RegisterPerkProcessor(PerkHash.Bore, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Crave, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.NanoFeast, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.BotConfinement, TargetedDamagePerk);

            RegisterPerkProcessor(PerkHash.ForceOpponent, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Purify, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Bluntness, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.RegainNano, RegainNano);
            RegisterPerkProcessor(PerkHash.NanoHeal, RegainNano);
            RegisterPerkProcessor(PerkHash.TapNotumSource, TapNotumSource);
            RegisterPerkProcessor(PerkHash.AccessNotumSource, RegainNano);
            RegisterPerkProcessor(PerkHash.Collapser, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Implode, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Fuzz, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.FireFrenzy, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Bluntness, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.MyOwnFortress, TargetedDamagePerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.WitOfTheAtrox, TargetedDamagePerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.EvasiveStance, SelfDefPerk, CombatActionPriority.Medium);
            RegisterPerkProcessor(PerkHash.DodgeTheBlame, SelfDefPerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.BodyTackle, TargetedDamagePerk);



            RegisterItemProcessor(RelevantItems.FlurryOfBlowsLow, RelevantItems.FlurryOfBlowsLow, DamageItem);
            RegisterItemProcessor(RelevantItems.FlurryOfBlowsHigh, RelevantItems.FlurryOfBlowsHigh, DamageItem);
            RegisterItemProcessor(RelevantItems.StrengthOfTheImmortal, RelevantItems.StrengthOfTheImmortal, DamageItem);
            RegisterItemProcessor(RelevantItems.MightOfTheRevenant, RelevantItems.MightOfTheRevenant, DamageItem);
            RegisterItemProcessor(RelevantItems.BarrowStrength, RelevantItems.BarrowStrength, DamageItem);
            RegisterItemProcessor(RelevantItems.MeteoriteSpikes, RelevantItems.MeteoriteSpikes, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.LavaCapsule, RelevantItems.LavaCapsule, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.KizzermoleGumboil, RelevantItems.KizzermoleGumboil, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.SteamingHotCupOfEnhancedCoffee, RelevantItems.SteamingHotCupOfEnhancedCoffee, Coffee);
            RegisterItemProcessor(RelevantItems.GnuffsEternalRiftCrystal, RelevantItems.GnuffsEternalRiftCrystal, DamageItem);
            RegisterItemProcessor(RelevantItems.UponAWaveOfSummerLow, RelevantItems.UponAWaveOfSummerHigh, TargetedDamageItem);

            RegisterItemProcessor(RelevantItems.DreadlochEnduranceBooster, RelevantItems.DreadlochEnduranceBooster, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.DreadlochEnduranceBoosterNanomageEdition, RelevantItems.DreadlochEnduranceBoosterNanomageEdition, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.HealthAndNanoStimLow, RelevantItems.HealthAndNanoStimHigh, HealthAndNanoStim, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.FlowerOfLifeLow, RelevantItems.FlowerOfLifeHigh, FlowerOfLife);




            RegisterSpellProcessor(RelevantNanos.FountainOfLife, FountainOfLife);
            RegisterSpellProcessor(RelevantNanos.CompositeAttributes, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRanged, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRangedSpecial, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRangedExpertise, GenericBuff);
            RegisterItemProcessor(RelevantItems.ExperienceStim, RelevantItems.ExperienceStim, ExperienceStim);

            RegisterItemProcessor(RelevantItems.PremSitKit, RelevantItems.PremSitKit, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit1, RelevantItems.SitKit100, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit100, RelevantItems.SitKit200, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit200, RelevantItems.SitKit300, SitKit);


            switch (DynelManager.LocalPlayer.Breed)
            {
                case Breed.Solitus:
                    break;
                case Breed.Opifex:
                    //Opening
                    //Derivate
                    //Blinded by delights
                    //Dizzying Heights
                    break;
                case Breed.Nanomage:
                    break;
                case Breed.Atrox:
                    break;
            }
        }

        protected bool GenericBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null)
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
        protected virtual bool StarfallPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (PerkAction.Find(PerkHash.Combust, out PerkAction combust) && !combust.IsAvailable)
                return false;

            return TargetedDamagePerk(perkAction, fightingTarget, ref actionTarget);
        }
        private bool RegainNano(PerkAction perkaction, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.MaxNano < 1200)
                return DynelManager.LocalPlayer.NanoPercent < 50;

            actiontarget.Target = DynelManager.LocalPlayer;
            return DynelManager.LocalPlayer.MissingNano > 1200;
        }
        private bool TapNotumSource(PerkAction perkaction, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.NanoPercent < 1200)
                return DynelManager.LocalPlayer.NanoPercent < 30;

            actiontarget.Target = DynelManager.LocalPlayer;
            return DynelManager.LocalPlayer.MissingNano > 1200;
        }
        private void TeleportEnded(object sender, EventArgs e)
        {
            _lastCombatTime = double.MinValue;
        }

        private void OnUpdate(object sender, float e)
        {
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0)
            {
                _lastCombatTime = Time.NormalTime;
            }
        }

 
    protected bool LEProc(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
        {
            if (buff.Name == perkAction.Name)
            {
                return false;
            }
        }
        return true;
    }

    public bool HasBuff(Spell spell, SimpleChar target)
    {
        foreach (Buff buff in target.Buffs.AsEnumerable())
        {
            if (spell.Nanoline == buff.Nanoline && spell.StackingOrder <= buff.StackingOrder)
            {
                return true;
            }
        }
        return false;
    }

    public bool TeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
            return false;

        if (DynelManager.LocalPlayer.IsInTeam())
        {
            SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                .Where(c => !HasBuff(spell, c))
                .FirstOrDefault();
            if (teamMemberWithoutBuff != null)
            {
                int currentNCU = teamMemberWithoutBuff.GetStat(Stat.CurrentNCU);
                int maxNCU = teamMemberWithoutBuff.GetStat(Stat.MaxNCU);

                // MaxNCU is bugged, for a 12 NCU it gives -459 NCU, we need to adapt to calc exact NCU

                int missingNCU = 459+255;

                int baseNCU = 12;

                maxNCU += missingNCU + baseNCU;
                int remainingNCU = maxNCU - currentNCU;

                Chat.WriteLine(teamMemberWithoutBuff.Name + " is missing " + spell.Name);
                //if (remainingNCU > Math.Abs(spell.NCU))
                //{
                    actionTarget.Target = teamMemberWithoutBuff;
                    return true;
                //}
            }
        }

        return false;
    }

    private bool FlowerOfLife(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
    {
        if (fightingtarget == null)
            return false;

        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)))
            return false;

        int approximateHealing = item.QualityLevel * 10;

        return DynelManager.LocalPlayer.MissingHealth > approximateHealing;
    }

    private bool HealthAndNanoStim(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
    {
        if (fightingtarget == null)
            return false;

        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)))
            return false;

        actiontarget.ShouldSetTarget = true;
        actiontarget.Target = DynelManager.LocalPlayer;

        int approximateHealing = item.QualityLevel * 10;

        return DynelManager.LocalPlayer.MissingHealth > approximateHealing || DynelManager.LocalPlayer.MissingNano > approximateHealing;
    }
    private bool ExperienceStim(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid))
            return false;

        actionTarget.Target = DynelManager.LocalPlayer;
        actionTarget.ShouldSetTarget = false;
        return true;

    }
    private bool SitKit(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            return false;

        if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
            return false;

        actionTarget.Target = DynelManager.LocalPlayer;
        actionTarget.ShouldSetTarget = true;
        return true;

    }



    private bool FountainOfLife(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
    {
        // Prioritize keeping ourself alive
        if (DynelManager.LocalPlayer.HealthPercent <= 30)
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
                .Where(c => c.HealthPercent < 30)
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
    private bool SelfDefPerk(PerkAction perkaction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (DynelManager.LocalPlayer.HealthPercent <= 50)

            return true;

        return false;
    }

    private bool EnduranceBooster(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
    {
        // don't use if skill is locked (we will add this dynamically later)
        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
            return false;

        // don't use if we're above 40%
        if (DynelManager.LocalPlayer.HealthPercent > 40)
            return false;

        // don't use if nothing is fighting us
        if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
            return false;

        // don't use if we have another major absorb running
        // we could check remaining absorb stat to be slightly more effective
        if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
            return false;
            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.Pets.Attack(fightingtarget.Identity);

            return true;
    }

    protected virtual bool TargetedDamagePerk(PerkAction perkaction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        actionTarget.ShouldSetTarget = true;
        return DamagePerk(perkaction, fightingTarget, ref actionTarget);
    }

    protected virtual bool DamagePerk(PerkAction perkaction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (fightingTarget == null)
            return false;

        if (fightingTarget.Health > 50000)
            return true;

        if (fightingTarget.HealthPercent < 5)
            return false;

        return true;
    }
    protected virtual bool Sacrifice(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (fightingTarget == null)
            return false;

        if (fightingTarget.Health > 1000000 && fightingTarget.HealthPercent <= 30)
            return true;

        if (fightingTarget.HealthPercent < 5)
            return false;

        return false;
    }

    protected virtual bool TargetedDamageItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        actionTarget.ShouldSetTarget = true;
        return DamageItem(item, fightingTarget, ref actionTarget);
    }

    protected virtual bool DamageItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (fightingTarget == null)
            return false;

        if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)))
            return false;

        return true;
    }

    protected virtual bool Coffee(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (!DynelManager.LocalPlayer.Buffs.Contains(NanoLine.FoodandDrinkBuffs))
            return DamageItem(item, fightingTarget, ref actionTarget);

        return false;
    }

    private bool Limber(PerkAction perkaction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (fightingTarget == null)
            return false;
        if (DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.DanceOfFools, out Buff dof) && dof.RemainingTime > 12.5f)
            return false;

        // stop cycling if we haven't fought anything for over 10 minutes
        if (Time.NormalTime - _lastCombatTime > EvadeCycleTimeoutSeconds)
            return false;

        return true;
    }

    private bool DanceOfFools(PerkAction perkaction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
    {
        if (fightingTarget == null)
            return false;
        if (!DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.Limber, out Buff limber) || limber.RemainingTime > 12.5f)
            return false;

        // stop cycling if we haven't fought anything for over 10 minutes
        if (Time.NormalTime - _lastCombatTime > EvadeCycleTimeoutSeconds)
            return false;

        return true;
    }

    // This will eventually be done dynamically but for now I will implement
    // it statically so we can have it functional
    private Stat GetSkillLockStat(Item item)
    {
        switch (item.HighId)
        {
            case RelevantItems.UponAWaveOfSummerLow:
            case RelevantItems.UponAWaveOfSummerHigh:
                return Stat.Riposte;
            case RelevantItems.FlowerOfLifeLow:
            case RelevantItems.FlowerOfLifeHigh:
                return Stat.MartialArts;
            case RelevantItems.HealthAndNanoStimLow:
            case RelevantItems.HealthAndNanoStimHigh:
                return Stat.FirstAid;
            case RelevantItems.FlurryOfBlowsLow:
            case RelevantItems.FlurryOfBlowsHigh:
                return Stat.AggDef;
            case RelevantItems.StrengthOfTheImmortal:
            case RelevantItems.MightOfTheRevenant:
            case RelevantItems.BarrowStrength:
                return Stat.Strength;
            case RelevantItems.MeteoriteSpikes:
            case RelevantItems.LavaCapsule:
            case RelevantItems.KizzermoleGumboil:
                return Stat.SharpObject;
            case RelevantItems.SteamingHotCupOfEnhancedCoffee:
                return Stat.RunSpeed;
            case RelevantItems.GnuffsEternalRiftCrystal:
                return Stat.MapNavigation;
            case RelevantItems.Xpcan:
                return Stat.XP;
            default:
                throw new Exception($"No skill lock stat defined for item id {item.HighId}");
        }
    }

    private static class RelevantItems
    {
        public const int FlurryOfBlowsLow = 85907;
        public const int FlurryOfBlowsHigh = 85908;
        public const int StrengthOfTheImmortal = -1;
        public const int MightOfTheRevenant = 206013;
        public const int BarrowStrength = 204653;
        public const int LavaCapsule = 245990;
        public const int KizzermoleGumboil = 245323;
        public const int SteamingHotCupOfEnhancedCoffee = 157296;
        public const int DreadlochEnduranceBooster = 267168;
        public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        public const int MeteoriteSpikes = 244204;
        public const int FlowerOfLifeLow = 70614;
        public const int FlowerOfLifeHigh = 204326;
        public const int UponAWaveOfSummerLow = 205405;
        public const int UponAWaveOfSummerHigh = 205406;
        public const int GnuffsEternalRiftCrystal = 303179;
        public const int HealthAndNanoStimLow = 291043;
        public const int HealthAndNanoStimHigh = 291044;
        public const int Xpcan = 288771;
        public const int Xpcan1 = 288772;
        public const int ExperienceStim = 288769;
        public const int PremSitKit = 297274;
        public const int SitKit1 = 291082;
        public const int SitKit100 = 291083;
        public const int SitKit200 = 291084;
        public const int SitKit300 = 293296;


    }


    private static class RelevantNanos
    {
        public const int FountainOfLife = 302907;
        public const int DanceOfFools = 210159;
        public const int Limber = 210158;
        public const int CompositeRangedExpertise = 223348;
        public static readonly int CompositeAttributes = 223372;
        public static readonly int CompositeNano = 223380;
        public static readonly int CompositeRanged = 223364;
        public static readonly int CompositeRangedSpecial = 223348;
    }
}
}
