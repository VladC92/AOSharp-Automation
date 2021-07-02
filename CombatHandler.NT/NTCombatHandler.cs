using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;
using System.Linq;



namespace Desu
{
    public class NTCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public NTCombatHandler()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.HostileTakeover, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ChaoticAssumption, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.FlimFocus, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Utilize, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DazzleWithLights, StarfallPerk);
            RegisterPerkProcessor(PerkHash.Combust, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ThermalDetonation, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Supernova, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.BreachDefenses, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ProgramOverload, TargetedDamagePerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.BreachDefenses, DamagePerk);
            RegisterPerkProcessor(PerkHash.NotumOverflow, DamagePerk);







            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NullitySphereNano).OrderByStackingOrder(), NullitySphere, CombatActionPriority.High);
            //   RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AbsorbACBuff).OrderByStackingOrder(), GenericBuff, CombatActionPriority.High);
            //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoResistanceDebuff_LineA).OrderByStackingOrder(), ConstantBarrage, CombatActionPriority.Low);


            RegisterSpellProcessor(RelevantNanos.NanobotAegis, NanobotAegis);
            RegisterSpellProcessor(RelevantNanos.IzgimmersWealth, IzgimmersWealth);
            RegisterSpellProcessor(RelevantNanos.Garuk, SingleTargetNuke);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DOTNanotechnicianStrainA).OrderByStackingOrder(), AiDotNuke);


            //
            //
            // AOE NUKE
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AOENuke).OrderByStackingOrder(), AoeNuke);
            //RegisterSpellProcessor(RelevantNanos.MildToxicSpill, SingleTargetNuke);
            //RegisterSpellProcessor(RelevantNanos.CircleOfWinter, SingleTargetNuke, CombatActionPriority.High);
            //RegisterSpellProcessor(RelevantNanos.CircleScythe, SingleTargetNuke, CombatActionPriority.High);








            RegisterSpellProcessor(RelevantNanos.TacticalNuke, SingleTargetNuke);
            RegisterSpellProcessor(RelevantNanos.IzgimmersUltimatum, SingleTargetNuke);
            RegisterSpellProcessor(RelevantNanos.DefilementofBeing, SingleTargetNuke);
            RegisterSpellProcessor(RelevantNanos.IzgimmerCorrosiveTear, SingleTargetNuke);
            RegisterSpellProcessor(RelevantNanos.OpticPlague, Blind, CombatActionPriority.High);



            RegisterSpellProcessor(RelevantNanos.NanobotShelter, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.NanoBuffs, GenericBuff);
            // RegisterSpellProcessor(RelevantNanos.NotumOverload, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Psy_IntBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Psy_IntBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoOverTime_LineA).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDamageMultiplierBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NPCostBuff).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MatCreaBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MajorEvasionBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Fortify).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(RelevantNanos.NanobotShelter, GenericBuff);

            _menu = new Menu("CombatHandler.NT", "CombatHandler.NT");

            _menu.AddItem(new MenuBool("UseAoeNuke", "Use AI DoT", true));
            _menu.AddItem(new MenuBool("UseAIDot", "Use AI DoT", false));
            _menu.AddItem(new MenuBool("UseBlind", "Use Blind", false));

            OptionPanel.AddMenu(_menu);
        }


        private bool NanobotAegis(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;
            return DynelManager.LocalPlayer.HealthPercent < 50 && !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.NullitySphereNano);
        }

        private bool NullitySphere(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;
            return DynelManager.LocalPlayer.HealthPercent < 75 && !DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.NanobotAegis);
        }

        private bool SingleTargetNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            return true;
        }




        private bool IzgimmersWealth(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;

            if (fightingTarget == null)
                return false;

            if (DynelManager.LocalPlayer.MissingNano < 20000 && DynelManager.LocalPlayer.NanoPercent > 5)
                return false;

            return true;
        }

        private bool AiDotNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("UseAIDot"))
                return false;

            if (fightingTarget == null)
                return false;

            if (fightingTarget.Health < 80000)
                return false;

            if (fightingTarget.Buffs.Find(spell.Identity.Instance, out Buff buff) && buff.RemainingTime > 5)
                return false;

            return true;
        }


        private bool Blind(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("UseBlind"))
                return false;

            if (fightingTarget == null)
                return false;

            if (fightingTarget.Health > 80000)
                return true;

            if (fightingTarget.Buffs.Find(275697, out Buff buff) && buff.RemainingTime > 5)
                return false;

            return true;
        }

        private static class RelevantNanos
        {
            public const int NanobotAegis = 302074;
            public const int IzgimmersWealth = 275024;
            public const int IzgimmersUltimatum = 218168;
            public const int IzgimmerCorrosiveTear = 218158;
            public const int DefilementofBeing = 218150;
            public const int Garuk = 275692;
            public const int TacticalNuke = 266287;
            public const int OpticPlague = 275697;
            public const int BriefPoisonFog = 45943;
            public const int MildToxicSpill = 45894;
            public const int CircleOfWinter = 28599;
            public const int CircleScythe = 45937;



            //Buffs
            public static readonly int[] NanobotShelter = { 273388, 263265 };
            public static readonly int CompositeAttribute = 223372;
            public static readonly int CompositeNano = 223380;
            public static readonly int[] NanoBuffs = { 95417, 273386, 150631, 270802, 90406, 95443 };

        }
    }
}
